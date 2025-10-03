using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using FCG.Jogos.Application.Jogos.Interfaces;
using FCG.Jogos.Application.Jogos.ViewModels;
using FCG.Jogos.Domain.Jogos.Entities;
using FCG.Jogos.Domain.Jogos.Interfaces;
using Microsoft.Extensions.Configuration;

namespace FCG.Jogos.Application.Jogos.Services;

public class CompraService : ICompraService
{
    private readonly ICompraRepository _compraRepository;
    private readonly IJogoRepository _jogoRepository;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;

    public CompraService(ICompraRepository compraRepository, IJogoRepository jogoRepository, IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _compraRepository = compraRepository;
        _jogoRepository = jogoRepository;
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
    }

    public async Task<CompraResponse> CriarCompraAsync(CompraRequest request)
    {
        var jogo = await _jogoRepository.ObterPorIdAsync(request.JogoId);
        if (jogo == null)
            throw new InvalidOperationException("Jogo não encontrado");

        if (jogo.Estoque <= 0)
            throw new InvalidOperationException("Jogo sem estoque disponível");

        // 1) Chamar serviço externo de pagamento antes de persistir
        var baseUrl = _configuration.GetValue<string>("Payment:BaseUrl");
        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            throw new InvalidOperationException("Configuração de pagamento ausente: 'Payment:BaseUrl'.");
        }

        var client = _httpClientFactory.CreateClient();
        client.BaseAddress = new Uri(baseUrl.TrimEnd('/') + "/");
        var subscriptionKey = _configuration.GetValue<string>("Payment:SubscriptionKey");
        if (!string.IsNullOrWhiteSpace(subscriptionKey))
        {
            client.DefaultRequestHeaders.Remove("Ocp-Apim-Subscription-Key");
            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", subscriptionKey);
        }

        // Determina o tipo efetivo de pagamento (prefere dados presentes)
        int? effectiveTipo = request.TipoPagamento;
        if (request.DadosCartao != null) effectiveTipo = 1;
        else if (request.DadosPIX != null) effectiveTipo = 3;
        else if (request.DadosBoleto != null) effectiveTipo = 4;

        if (!effectiveTipo.HasValue)
        {
            throw new InvalidOperationException("Tipo de pagamento não informado e não foi possível inferir pelos dados enviados.");
        }

        // Monta o payload esperado pelo serviço de pagamentos
        var payload = new
        {
            usuarioId = request.UsuarioId,
            jogoId = request.JogoId,
            valor = jogo.Preco,
            tipoPagamento = effectiveTipo,
            dadosCartao = request.DadosCartao == null ? null : new
            {
                numeroCartao = request.DadosCartao.NumeroCartao,
                nomeTitular = request.DadosCartao.NomeTitular,
                dataValidade = request.DadosCartao.DataValidade,
                cvv = request.DadosCartao.Cvv,
                parcelas = request.DadosCartao.Parcelas
            },
            dadosPIX = request.DadosPIX == null ? null : new
            {
                chavePIX = request.DadosPIX.ChavePIX,
                nomeBeneficiario = request.DadosPIX.NomeBeneficiario
            },
            dadosBoleto = request.DadosBoleto == null ? null : new
            {
                cpfCnpj = request.DadosBoleto.CpfCnpj,
                nomePagador = request.DadosBoleto.NomePagador,
                endereco = request.DadosBoleto.Endereco,
                cep = request.DadosBoleto.Cep,
                cidade = request.DadosBoleto.Cidade,
                estado = request.DadosBoleto.Estado
            },
            observacoes = request.Observacoes
        };

        var jsonOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web)
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };
        var json = JsonSerializer.Serialize(payload, jsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var resp = await client.PostAsync("api/Transacoes", content);

        if (!resp.IsSuccessStatusCode)
        {
            var body = await resp.Content.ReadAsStringAsync();
            throw new InvalidOperationException($"Pagamento não autorizado: {(int)resp.StatusCode} {resp.ReasonPhrase} - {body}");
        }

        // 2) Pagamento OK -> criar compra aprovada e atualizar estoque
        var compra = new Compra
        {
            UsuarioId = request.UsuarioId,
            JogoId = request.JogoId,
            PrecoPago = jogo.Preco,
            DataCompra = DateTimeOffset.UtcNow,
            Status = StatusCompra.Aprovada,
            Observacoes = request.Observacoes
        };

        var compraCriada = await _compraRepository.AdicionarAsync(compra);

        jogo.Estoque--;
        await _jogoRepository.AtualizarAsync(jogo);

        return MapearParaResponse(compraCriada);
    }

    public async Task<CompraResponse?> ObterPorIdAsync(Guid id)
    {
        var compra = await _compraRepository.ObterPorIdAsync(id);
        return compra != null ? MapearParaResponse(compra) : null;
    }

    public async Task<IEnumerable<CompraResponse>> ObterPorUsuarioAsync(Guid usuarioId)
    {
        var compras = await _compraRepository.ObterPorUsuarioAsync(usuarioId);
        return compras.Select(MapearParaResponse);
    }

    public async Task<IEnumerable<CompraResponse>> ObterPorJogoAsync(Guid jogoId)
    {
        var compras = await _compraRepository.ObterPorJogoAsync(jogoId);
        return compras.Select(MapearParaResponse);
    }

    public async Task<IEnumerable<CompraResponse>> ObterTodosAsync()
    {
        var compras = await _compraRepository.ObterTodosAsync();
        return compras.Select(MapearParaResponse);
    }

    public async Task<CompraResponse> AtualizarStatusAsync(Guid id, StatusCompra status)
    {
        var compra = await _compraRepository.ObterPorIdAsync(id);
        if (compra == null)
            throw new InvalidOperationException("Compra não encontrada");

        compra.Status = status;
        var compraAtualizada = await _compraRepository.AtualizarAsync(compra);
        return MapearParaResponse(compraAtualizada);
    }

    public async Task CancelarCompraAsync(Guid id)
    {
        var compra = await _compraRepository.ObterPorIdAsync(id);
        if (compra == null)
            throw new InvalidOperationException("Compra não encontrada");

        if (compra.Status == StatusCompra.Cancelada)
            throw new InvalidOperationException("Compra já está cancelada");

        compra.Status = StatusCompra.Cancelada;
        await _compraRepository.AtualizarAsync(compra);

        // Restaura o estoque do jogo
        var jogo = await _jogoRepository.ObterPorIdAsync(compra.JogoId);
        if (jogo != null)
        {
            jogo.Estoque++;
            await _jogoRepository.AtualizarAsync(jogo);
        }
    }

    public async Task<string> GerarCodigoAtivacaoAsync(Guid compraId)
    {
        var compra = await _compraRepository.ObterPorIdAsync(compraId);
        if (compra == null)
            throw new InvalidOperationException("Compra não encontrada");

        if (compra.Status != StatusCompra.Aprovada)
            throw new InvalidOperationException("Apenas compras aprovadas podem gerar código de ativação");

        var codigoAtivacao = Guid.NewGuid().ToString("N").Substring(0, 16).ToUpper();
        compra.CodigoAtivacao = codigoAtivacao;
        compra.DataAtivacao = DateTimeOffset.UtcNow;

        await _compraRepository.AtualizarAsync(compra);
        return codigoAtivacao;
    }

    private static CompraResponse MapearParaResponse(Compra compra)
    {
        return new CompraResponse
        {
            Id = compra.Id,
            UsuarioId = compra.UsuarioId,
            JogoId = compra.JogoId,
            PrecoPago = compra.PrecoPago,
            DataCompra = compra.DataCompra.UtcDateTime,
            Status = compra.Status,
            CodigoAtivacao = compra.CodigoAtivacao,
            DataCriacao = compra.DataCriacao.UtcDateTime,
            DataAtualizacao = compra.DataAtualizacao?.UtcDateTime
        };
    }
}