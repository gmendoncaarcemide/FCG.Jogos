using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using FCG.Jogos.Domain.Jogos.Entities;

namespace FCG.Jogos.Application.Jogos.ViewModels;

public class CompraRequest : IValidatableObject
{
    [Required(ErrorMessage = "ID do usuário é obrigatório")]
    public Guid UsuarioId { get; set; }

    [Required(ErrorMessage = "ID do jogo é obrigatório")]
    public Guid JogoId { get; set; }

    public string? Observacoes { get; set; }

    // 1=Cartão, 2=PIX, 3=Boleto (opcional: se ausente, será inferido pelos dados presentes)
    public int? TipoPagamento { get; set; }

    public DadosCartaoRequest? DadosCartao { get; set; }
    public DadosPixRequest? DadosPIX { get; set; }
    public DadosBoletoRequest? DadosBoleto { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        var results = new List<ValidationResult>();
        // Inferência do tipo caso não informado
        var tipo = TipoPagamento;
        if (!tipo.HasValue)
        {
            if (DadosCartao != null) tipo = 1;
            else if (DadosPIX != null) tipo = 2;
            else if (DadosBoleto != null) tipo = 3;
        }

        if (tipo == 1)
        {
            if (DadosCartao == null)
            {
                results.Add(new ValidationResult("Dados do cartão são obrigatórios para pagamento com cartão.", new[] { nameof(DadosCartao) }));
            }
            else
            {
                if (string.IsNullOrWhiteSpace(DadosCartao.NumeroCartao))
                    results.Add(new ValidationResult("Número do cartão é obrigatório.", new[] { nameof(DadosCartao.NumeroCartao) }));
                if (string.IsNullOrWhiteSpace(DadosCartao.DataValidade))
                    results.Add(new ValidationResult("Data de validade é obrigatória.", new[] { nameof(DadosCartao.DataValidade) }));
                if (string.IsNullOrWhiteSpace(DadosCartao.Cvv))
                    results.Add(new ValidationResult("CVV é obrigatório.", new[] { nameof(DadosCartao.Cvv) }));
                if (!DadosCartao.Parcelas.HasValue)
                    results.Add(new ValidationResult("Parcelas é obrigatório.", new[] { nameof(DadosCartao.Parcelas) }));
            }
        }
        else if (tipo == 3)
        {
            if (DadosPIX == null)
            {
                results.Add(new ValidationResult("Dados do PIX são obrigatórios para pagamento via PIX.", new[] { nameof(DadosPIX) }));
            }
            else if (string.IsNullOrWhiteSpace(DadosPIX.ChavePIX))
            {
                results.Add(new ValidationResult("Chave PIX é obrigatória.", new[] { nameof(DadosPIX.ChavePIX) }));
            }
        }
        else if (tipo == 4)
        {
            if (DadosBoleto == null)
            {
                results.Add(new ValidationResult("Dados do boleto são obrigatórios para pagamento via boleto.", new[] { nameof(DadosBoleto) }));
            }
            else
            {
                if (string.IsNullOrWhiteSpace(DadosBoleto.CpfCnpj))
                    results.Add(new ValidationResult("CPF/CNPJ é obrigatório.", new[] { nameof(DadosBoleto.CpfCnpj) }));
                if (string.IsNullOrWhiteSpace(DadosBoleto.NomePagador))
                    results.Add(new ValidationResult("Nome do pagador é obrigatório.", new[] { nameof(DadosBoleto.NomePagador) }));
                if (string.IsNullOrWhiteSpace(DadosBoleto.Endereco))
                    results.Add(new ValidationResult("Endereço é obrigatório.", new[] { nameof(DadosBoleto.Endereco) }));
                if (string.IsNullOrWhiteSpace(DadosBoleto.Cep))
                    results.Add(new ValidationResult("CEP é obrigatório.", new[] { nameof(DadosBoleto.Cep) }));
                if (string.IsNullOrWhiteSpace(DadosBoleto.Cidade))
                    results.Add(new ValidationResult("Cidade é obrigatória.", new[] { nameof(DadosBoleto.Cidade) }));
                if (string.IsNullOrWhiteSpace(DadosBoleto.Estado))
                    results.Add(new ValidationResult("Estado é obrigatório.", new[] { nameof(DadosBoleto.Estado) }));
            }
        }
        else if (!tipo.HasValue)
        {
            results.Add(new ValidationResult("Tipo de pagamento não informado e não foi possível inferir pelos dados enviados.", new[] { nameof(TipoPagamento) }));
        }

        return results;
    }
}

public class CompraResponse
{
    public Guid Id { get; set; }
    public Guid UsuarioId { get; set; }
    public Guid JogoId { get; set; }
    public decimal PrecoPago { get; set; }
    public DateTime DataCompra { get; set; }
    public StatusCompra Status { get; set; }
    public string? CodigoAtivacao { get; set; }
    public DateTime? DataAtivacao { get; set; }
    public string? Observacoes { get; set; }
    public DateTime DataCriacao { get; set; }
    public DateTime? DataAtualizacao { get; set; }
}

// DTOs de pagamento para repassar ao serviço externo
public class DadosCartaoRequest
{
    [RegularExpression(@"^\d{13,19}$", ErrorMessage = "Número do cartão deve ter entre 13 e 19 dígitos")]
    public string? NumeroCartao { get; set; }

    public string? NomeTitular { get; set; }

    [RegularExpression(@"^(0[1-9]|1[0-2])\/\d{2}$", ErrorMessage = "Data de validade deve estar no formato MM/YY")]
    public string? DataValidade { get; set; }

    [RegularExpression(@"^\d{3,4}$", ErrorMessage = "CVV deve ter entre 3 e 4 dígitos")]
    public string? Cvv { get; set; }

    [Range(1, 12, ErrorMessage = "Parcelas deve estar entre 1 e 12")]
    public int? Parcelas { get; set; }
}

public class DadosPixRequest
{
    public string? ChavePIX { get; set; }
    public string? NomeBeneficiario { get; set; }
}

public class DadosBoletoRequest
{
    public string? CpfCnpj { get; set; }
    public string? NomePagador { get; set; }
    public string? Endereco { get; set; }
    [RegularExpression(@"^\d{5}-\d{3}$", ErrorMessage = "CEP deve estar no formato 00000-000")]
    public string? Cep { get; set; }
    public string? Cidade { get; set; }
    [StringLength(2, MinimumLength = 2, ErrorMessage = "Estado deve ter 2 caracteres")]
    public string? Estado { get; set; }
}
 