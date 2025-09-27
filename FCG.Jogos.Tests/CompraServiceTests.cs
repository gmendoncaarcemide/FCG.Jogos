using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Xunit;
using Moq;
using FluentAssertions;
using FCG.Jogos.Application.Jogos.Services;
using FCG.Jogos.Application.Jogos.Interfaces;
using FCG.Jogos.Domain.Jogos.Interfaces;
using FCG.Jogos.Domain.Jogos.Entities;
using FCG.Jogos.Application.Jogos.ViewModels;

namespace FCG.Jogos.Tests
{
    public class CompraServiceTests
    {
        private readonly Mock<ICompraRepository> _compraRepositoryMock;
        private readonly Mock<IJogoRepository> _jogoRepositoryMock;
        private readonly CompraService _service;

        public CompraServiceTests()
        {
            _compraRepositoryMock = new Mock<ICompraRepository>();
            _jogoRepositoryMock = new Mock<IJogoRepository>();
            _service = new CompraService(_compraRepositoryMock.Object, _jogoRepositoryMock.Object);
        }

        [Fact]
        public async Task CriarCompraAsync_DeveCriarCompraQuandoJogoExisteEHaEstoque()
        {
            var request = new CompraRequest { JogoId = Guid.NewGuid(), UsuarioId = Guid.NewGuid(), PrecoPago = 100 };
            var jogo = new Jogo {
                Id = request.JogoId,
                Estoque = 5,
                Titulo = "Titulo",
                Descricao = "Descricao",
                Desenvolvedor = "Dev",
                Editora = "Editora"
            };
            var compra = new Compra { Id = Guid.NewGuid(), JogoId = request.JogoId, UsuarioId = request.UsuarioId, PrecoPago = request.PrecoPago, Status = StatusCompra.Pendente };

            _jogoRepositoryMock.Setup(r => r.ObterPorIdAsync(request.JogoId)).ReturnsAsync(jogo);
            _compraRepositoryMock.Setup(r => r.AdicionarAsync(It.IsAny<Compra>())).ReturnsAsync(compra);
            _jogoRepositoryMock.Setup(r => r.AtualizarAsync(It.IsAny<Jogo>())).ReturnsAsync(jogo);

            var result = await _service.CriarCompraAsync(request);

            result.Should().NotBeNull();
            result.JogoId.Should().Be(request.JogoId);
            result.UsuarioId.Should().Be(request.UsuarioId);
            _jogoRepositoryMock.Verify(r => r.AtualizarAsync(It.Is<Jogo>(j => j.Estoque == 4)), Times.Once);
        }

        [Fact]
        public async Task CriarCompraAsync_DeveLancarExcecaoQuandoJogoNaoExiste()
        {
            var request = new CompraRequest { JogoId = Guid.NewGuid(), UsuarioId = Guid.NewGuid(), PrecoPago = 100 };
            _jogoRepositoryMock.Setup(r => r.ObterPorIdAsync(request.JogoId)).ReturnsAsync((Jogo)null!);

            await Assert.ThrowsAsync<InvalidOperationException>(() => _service.CriarCompraAsync(request));
        }

        [Fact]
        public async Task CriarCompraAsync_DeveLancarExcecaoQuandoSemEstoque()
        {
            var request = new CompraRequest { JogoId = Guid.NewGuid(), UsuarioId = Guid.NewGuid(), PrecoPago = 100 };
            var jogo = new Jogo {
                Id = request.JogoId,
                Estoque = 0,
                Titulo = "Titulo",
                Descricao = "Descricao",
                Desenvolvedor = "Dev",
                Editora = "Editora"
            };
            _jogoRepositoryMock.Setup(r => r.ObterPorIdAsync(request.JogoId)).ReturnsAsync(jogo);

            await Assert.ThrowsAsync<InvalidOperationException>(() => _service.CriarCompraAsync(request));
        }

        [Fact]
        public async Task CancelarCompraAsync_DeveCancelarCompraERestaurarEstoque()
        {
            var compraId = Guid.NewGuid();
            var jogoId = Guid.NewGuid();
            var compra = new Compra { Id = compraId, JogoId = jogoId, Status = StatusCompra.Pendente };
            var jogo = new Jogo {
                Id = jogoId,
                Estoque = 1,
                Titulo = "Titulo",
                Descricao = "Descricao",
                Desenvolvedor = "Dev",
                Editora = "Editora"
            };
            _compraRepositoryMock.Setup(r => r.ObterPorIdAsync(compraId)).ReturnsAsync(compra);
            _compraRepositoryMock.Setup(r => r.AtualizarAsync(It.IsAny<Compra>())).ReturnsAsync(compra);
            _jogoRepositoryMock.Setup(r => r.ObterPorIdAsync(jogoId)).ReturnsAsync(jogo);
            _jogoRepositoryMock.Setup(r => r.AtualizarAsync(It.IsAny<Jogo>())).ReturnsAsync(jogo);

            await _service.CancelarCompraAsync(compraId);

            _compraRepositoryMock.Verify(r => r.AtualizarAsync(It.Is<Compra>(c => c.Status == StatusCompra.Cancelada)), Times.Once);
            _jogoRepositoryMock.Verify(r => r.AtualizarAsync(It.Is<Jogo>(j => j.Estoque == 2)), Times.Once);
        }

        [Fact]
        public async Task CancelarCompraAsync_DeveLancarExcecaoSeCompraNaoEncontrada()
        {
            var compraId = Guid.NewGuid();
            _compraRepositoryMock.Setup(r => r.ObterPorIdAsync(compraId)).ReturnsAsync((Compra)null);
            await Assert.ThrowsAsync<InvalidOperationException>(() => _service.CancelarCompraAsync(compraId));
        }

        [Fact]
        public async Task CancelarCompraAsync_DeveLancarExcecaoSeCompraJaCancelada()
        {
            var compraId = Guid.NewGuid();
            var compra = new Compra { Id = compraId, Status = StatusCompra.Cancelada };
            _compraRepositoryMock.Setup(r => r.ObterPorIdAsync(compraId)).ReturnsAsync(compra);
            await Assert.ThrowsAsync<InvalidOperationException>(() => _service.CancelarCompraAsync(compraId));
        }

        [Fact]
        public async Task AtualizarStatusAsync_DeveAtualizarStatusDaCompra()
        {
            var compraId = Guid.NewGuid();
            var compra = new Compra { Id = compraId, Status = StatusCompra.Pendente };
            _compraRepositoryMock.Setup(r => r.ObterPorIdAsync(compraId)).ReturnsAsync(compra);
            _compraRepositoryMock.Setup(r => r.AtualizarAsync(It.IsAny<Compra>())).ReturnsAsync(compra);

            var result = await _service.AtualizarStatusAsync(compraId, StatusCompra.Aprovada);

            result.Status.Should().Be(StatusCompra.Aprovada);
            _compraRepositoryMock.Verify(r => r.AtualizarAsync(It.Is<Compra>(c => c.Status == StatusCompra.Aprovada)), Times.Once);
        }

        [Fact]
        public async Task AtualizarStatusAsync_DeveLancarExcecaoSeCompraNaoEncontrada()
        {
            var compraId = Guid.NewGuid();
            _compraRepositoryMock.Setup(r => r.ObterPorIdAsync(compraId)).ReturnsAsync((Compra)null);
            await Assert.ThrowsAsync<InvalidOperationException>(() => _service.AtualizarStatusAsync(compraId, StatusCompra.Aprovada));
        }

        [Fact]
        public async Task GerarCodigoAtivacaoAsync_DeveGerarCodigoSeCompraAprovada()
        {
            var compraId = Guid.NewGuid();
            var compra = new Compra { Id = compraId, Status = StatusCompra.Aprovada };
            _compraRepositoryMock.Setup(r => r.ObterPorIdAsync(compraId)).ReturnsAsync(compra);
            _compraRepositoryMock.Setup(r => r.AtualizarAsync(It.IsAny<Compra>())).ReturnsAsync(compra);

            var codigo = await _service.GerarCodigoAtivacaoAsync(compraId);

            codigo.Should().NotBeNullOrWhiteSpace();
            codigo.Length.Should().Be(16);
            _compraRepositoryMock.Verify(r => r.AtualizarAsync(It.Is<Compra>(c => c.CodigoAtivacao == codigo)), Times.Once);
        }

        [Fact]
        public async Task GerarCodigoAtivacaoAsync_DeveLancarExcecaoSeCompraNaoEncontrada()
        {
            var compraId = Guid.NewGuid();
            _compraRepositoryMock.Setup(r => r.ObterPorIdAsync(compraId)).ReturnsAsync((Compra)null);
            await Assert.ThrowsAsync<InvalidOperationException>(() => _service.GerarCodigoAtivacaoAsync(compraId));
        }

        [Fact]
        public async Task GerarCodigoAtivacaoAsync_DeveLancarExcecaoSeStatusNaoAprovada()
        {
            var compraId = Guid.NewGuid();
            var compra = new Compra { Id = compraId, Status = StatusCompra.Pendente };
            _compraRepositoryMock.Setup(r => r.ObterPorIdAsync(compraId)).ReturnsAsync(compra);
            await Assert.ThrowsAsync<InvalidOperationException>(() => _service.GerarCodigoAtivacaoAsync(compraId));
        }
    }
}