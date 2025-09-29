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
    public class JogoServiceTest
    {
        private readonly Mock<IJogoRepository> _jogoRepositoryMock;
        private readonly JogoService _jogoService;

        public JogoServiceTest()
        {
            _jogoRepositoryMock = new Mock<IJogoRepository>();
            _jogoService = new JogoService(_jogoRepositoryMock.Object);
        }

        [Fact]
        public async Task CriarAsync_DeveRetornarJogoResponse()
        {
            // Arrange
            var request = new CriarJogoRequest
            {
                Titulo = "Jogo Teste",
                Descricao = "Descricao Teste",
                Preco = 100,
                Estoque = 10,
                Tags = new List<string> { "Ação" },
                Plataformas = new List<string> { "PC" },
                Categoria = CategoriaJogo.Acao
            };
            var jogoCriado = new Jogo
            {
                Id = Guid.NewGuid(),
                Titulo = request.Titulo,
                Descricao = request.Descricao,
                Preco = request.Preco,
                Estoque = request.Estoque,
                Tags = request.Tags,
                Plataformas = request.Plataformas,
                Categoria = request.Categoria,
                AvaliacaoMedia = 0,
                NumeroAvaliacoes = 0,
                DataCriacao = DateTimeOffset.UtcNow,
                DataLancamento = DateTimeOffset.UtcNow,
                Desenvolvedor = "Dev Teste",
                Editora = "Editora Teste"
            };
            _jogoRepositoryMock.Setup(r => r.AdicionarAsync(It.IsAny<Jogo>())).ReturnsAsync(jogoCriado);

            // Act
            var result = await _jogoService.CriarAsync(request);

            // Assert
            result.Should().NotBeNull();
            result.Titulo.Should().Be(request.Titulo);
            result.Descricao.Should().Be(request.Descricao);
        }

        [Fact]
        public async Task ObterPorIdAsync_DeveRetornarJogoResponse_QuandoEncontrado()
        {
            // Arrange
            var id = Guid.NewGuid();
            var jogo = new Jogo
            {
                Id = id,
                Titulo = "Jogo Teste",
                Descricao = "Descricao Teste",
                Preco = 100,
                Estoque = 10,
                Tags = new List<string> { "Ação" },
                Plataformas = new List<string> { "PC" },
                Categoria = CategoriaJogo.Acao,
                AvaliacaoMedia = 0,
                NumeroAvaliacoes = 0,
                DataCriacao = DateTimeOffset.UtcNow,
                DataLancamento = DateTimeOffset.UtcNow,
                Desenvolvedor = "Dev Teste",
                Editora = "Editora Teste"
            };
            _jogoRepositoryMock.Setup(r => r.ObterPorIdAsync(id)).ReturnsAsync(jogo);

            // Act
            var result = await _jogoService.ObterPorIdAsync(id);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(id);
        }

        [Fact]
        public async Task ObterPorIdAsync_DeveRetornarNull_QuandoNaoEncontrado()
        {
            // Arrange
            var id = Guid.NewGuid();
            _jogoRepositoryMock.Setup(r => r.ObterPorIdAsync(id)).ReturnsAsync((Jogo)null);

            // Act
            var result = await _jogoService.ObterPorIdAsync(id);

            // Assert
            result.Should().BeNull();
        }
    }
}