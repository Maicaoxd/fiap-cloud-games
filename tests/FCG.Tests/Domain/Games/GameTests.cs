using FCG.Domain.Games;
using FCG.Domain.Shared;

namespace FCG.Tests.Domain.Games;

[Trait("Category", "Unit")]
public sealed class GameTests
{
    [Fact]
    public void Deve_Criar_Jogo_Quando_Dados_Forem_Validos()
    {
        // Arrange
        var criadoPor = Guid.NewGuid();

        // Act
        var jogo = Game.Create(
            "Stardew Valley",
            "Simulador de fazenda e vida no campo.",
            24.90m,
            criadoPor);

        // Assert
        jogo.Id.ShouldNotBe(Guid.Empty);
        jogo.Title.ShouldBe("Stardew Valley");
        jogo.Description.ShouldBe("Simulador de fazenda e vida no campo.");
        jogo.Price.ShouldBe(24.90m);
        jogo.IsActive.ShouldBeTrue();
        jogo.CreatedAt.ShouldNotBe(default);
        jogo.CreatedBy.ShouldBe(criadoPor);
        jogo.UpdatedAt.ShouldBeNull();
        jogo.UpdatedBy.ShouldBeNull();
    }

    [Fact]
    public void Deve_Criar_Jogo_Gratuito_Quando_Preco_For_Zero()
    {
        // Arrange
        var criadoPor = Guid.NewGuid();

        // Act
        var jogo = Game.Create(
            "FIAP Quest",
            "Jogo gratuito para alunos.",
            0,
            criadoPor);

        // Assert
        jogo.Price.ShouldBe(0);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    public void Deve_Lancar_Excecao_Quando_Titulo_For_Obrigatorio_E_Nao_For_Informado(string? titulo)
    {
        // Arrange
        var criadoPor = Guid.NewGuid();
        Action acao = () => Game.Create(
            titulo!,
            "Descrição válida.",
            10,
            criadoPor);

        // Act
        var excecao = Should.Throw<ArgumentException>(acao);

        // Assert
        excecao.Message.ShouldBe(DomainMessages.Game.TitleRequired);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    public void Deve_Lancar_Excecao_Quando_Descricao_For_Obrigatoria_E_Nao_For_Informada(string? descricao)
    {
        // Arrange
        var criadoPor = Guid.NewGuid();
        Action acao = () => Game.Create(
            "Stardew Valley",
            descricao!,
            10,
            criadoPor);

        // Act
        var excecao = Should.Throw<ArgumentException>(acao);

        // Assert
        excecao.Message.ShouldBe(DomainMessages.Game.DescriptionRequired);
    }

    [Fact]
    public void Deve_Lancar_Excecao_Quando_Preco_For_Negativo()
    {
        // Arrange
        var criadoPor = Guid.NewGuid();
        Action acao = () => Game.Create(
            "Stardew Valley",
            "Descrição válida.",
            -1,
            criadoPor);

        // Act
        var excecao = Should.Throw<ArgumentException>(acao);

        // Assert
        excecao.Message.ShouldBe(DomainMessages.Game.PriceCannotBeNegative);
    }

    [Fact]
    public void Deve_Lancar_Excecao_Quando_Criar_Jogo_Sem_Responsavel_Valido()
    {
        // Arrange
        Action acao = () => Game.Create(
            "Stardew Valley",
            "Descrição válida.",
            10,
            Guid.Empty);

        // Act
        var excecao = Should.Throw<ArgumentException>(acao);

        // Assert
        excecao.Message.ShouldBe(DomainMessages.Entity.ResponsibleForChangeRequired);
    }

    [Fact]
    public void Deve_Atualizar_Jogo_Quando_Dados_Forem_Validos()
    {
        // Arrange
        var criadoPor = Guid.NewGuid();
        var atualizadoPor = Guid.NewGuid();
        var jogo = Game.Create(
            "Stardew Valley",
            "Simulador de fazenda.",
            24.90m,
            criadoPor);

        // Act
        jogo.Update(
            "Stardew Valley Deluxe",
            "Simulador de fazenda com conteúdo extra.",
            39.90m,
            atualizadoPor);

        // Assert
        jogo.Title.ShouldBe("Stardew Valley Deluxe");
        jogo.Description.ShouldBe("Simulador de fazenda com conteúdo extra.");
        jogo.Price.ShouldBe(39.90m);
        jogo.UpdatedAt.ShouldNotBeNull();
        jogo.UpdatedBy.ShouldBe(atualizadoPor);
    }

    [Fact]
    public void Deve_Desativar_Jogo_Quando_Jogo_Estiver_Ativo()
    {
        // Arrange
        var criadoPor = Guid.NewGuid();
        var desativadoPor = Guid.NewGuid();
        var jogo = Game.Create(
            "Stardew Valley",
            "Simulador de fazenda.",
            24.90m,
            criadoPor);

        // Act
        jogo.Deactivate(desativadoPor);

        // Assert
        jogo.IsActive.ShouldBeFalse();
        jogo.UpdatedAt.ShouldNotBeNull();
        jogo.UpdatedBy.ShouldBe(desativadoPor);
    }

    [Fact]
    public void Deve_Reativar_Jogo_Quando_Jogo_Estiver_Inativo()
    {
        // Arrange
        var criadoPor = Guid.NewGuid();
        var desativadoPor = Guid.NewGuid();
        var ativadoPor = Guid.NewGuid();
        var jogo = Game.Create(
            "Stardew Valley",
            "Simulador de fazenda.",
            24.90m,
            criadoPor);

        jogo.Deactivate(desativadoPor);

        // Act
        jogo.Activate(ativadoPor);

        // Assert
        jogo.IsActive.ShouldBeTrue();
        jogo.UpdatedAt.ShouldNotBeNull();
        jogo.UpdatedBy.ShouldBe(ativadoPor);
    }
}
