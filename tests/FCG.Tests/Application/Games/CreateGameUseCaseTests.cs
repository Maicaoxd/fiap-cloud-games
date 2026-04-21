using FCG.Application.Abstractions.Persistence;
using FCG.Application.Common;
using FCG.Application.Common.Exceptions;
using FCG.Application.Games.Create;
using FCG.Domain.Games;
using NSubstitute;

namespace FCG.Tests.Application.Games;

[Trait("Category", "Unit")]
public sealed class CreateGameUseCaseTests
{
    [Fact]
    public async Task Deve_Cadastrar_Jogo_Quando_Dados_Forem_Validos()
    {
        // Arrange
        var createdBy = Guid.NewGuid();
        var gameRepository = Substitute.For<IGameRepository>();
        Game? addedGame = null;

        gameRepository
            .ExistsByTitleAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(false);

        gameRepository
            .AddAsync(Arg.Any<Game>(), Arg.Any<CancellationToken>())
            .Returns(callInfo =>
            {
                addedGame = callInfo.ArgAt<Game>(0);

                return Task.CompletedTask;
            });

        var useCase = new CreateGameUseCase(gameRepository);
        var command = new CreateGameCommand(
            "Stardew Valley",
            "Simulador de fazenda e vida no campo.",
            24.90m,
            createdBy);

        // Act
        var result = await useCase.ExecuteAsync(command);

        // Assert
        result.GameId.ShouldNotBe(Guid.Empty);
        addedGame.ShouldNotBeNull();
        addedGame!.Id.ShouldBe(result.GameId);
        addedGame.Title.ShouldBe("Stardew Valley");
        addedGame.Description.ShouldBe("Simulador de fazenda e vida no campo.");
        addedGame.Price.ShouldBe(24.90m);
        addedGame.CreatedBy.ShouldBe(createdBy);
        addedGame.IsActive.ShouldBeTrue();
        await gameRepository.Received(1).AddAsync(Arg.Any<Game>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Deve_Lancar_Excecao_Quando_Titulo_Ja_Estiver_Cadastrado()
    {
        // Arrange
        var gameRepository = Substitute.For<IGameRepository>();

        gameRepository
            .ExistsByTitleAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(true);

        var useCase = new CreateGameUseCase(gameRepository);
        var command = new CreateGameCommand(
            "Stardew Valley",
            "Simulador de fazenda e vida no campo.",
            24.90m,
            Guid.NewGuid());

        // Act
        var excecao = await Should.ThrowAsync<GameTitleAlreadyRegisteredException>(() => useCase.ExecuteAsync(command));

        // Assert
        excecao.Message.ShouldBe(ApplicationMessages.Game.TitleAlreadyRegistered);
        await gameRepository.DidNotReceive().AddAsync(Arg.Any<Game>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Deve_Validar_Dominio_Antes_De_Consultar_Duplicidade_De_Titulo()
    {
        // Arrange
        var gameRepository = Substitute.For<IGameRepository>();
        var useCase = new CreateGameUseCase(gameRepository);
        var command = new CreateGameCommand(
            "",
            "Simulador de fazenda e vida no campo.",
            24.90m,
            Guid.NewGuid());

        // Act
        var excecao = await Should.ThrowAsync<ArgumentException>(() => useCase.ExecuteAsync(command));

        // Assert
        excecao.Message.ShouldBe(FCG.Domain.Shared.DomainMessages.Game.TitleRequired);
        await gameRepository
            .DidNotReceive()
            .ExistsByTitleAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
        await gameRepository.DidNotReceive().AddAsync(Arg.Any<Game>(), Arg.Any<CancellationToken>());
    }
}
