using FCG.Application.Abstractions.Persistence;
using FCG.Application.Common;
using FCG.Application.Common.Exceptions;
using FCG.Application.Games.Update;
using FCG.Domain.Games;
using NSubstitute;

namespace FCG.Tests.Application.Games;

[Trait("Category", "Unit")]
public sealed class UpdateGameUseCaseTests
{
    [Fact]
    public async Task Deve_Atualizar_Jogo_Quando_Dados_Forem_Validos()
    {
        // Arrange
        var game = Game.Create(
            "Stardew Valley",
            "Simulador de fazenda.",
            24.90m,
            Guid.NewGuid());
        var updatedBy = Guid.NewGuid();
        var gameRepository = Substitute.For<IGameRepository>();

        gameRepository
            .GetByIdAsync(game.Id, Arg.Any<CancellationToken>())
            .Returns(game);
        gameRepository
            .ExistsByTitleForAnotherGameAsync(Arg.Any<string>(), game.Id, Arg.Any<CancellationToken>())
            .Returns(false);

        var useCase = new UpdateGameUseCase(gameRepository);
        var command = new UpdateGameCommand(
            game.Id,
            "Stardew Valley Deluxe",
            "Simulador de fazenda com conteúdo extra.",
            39.90m,
            updatedBy);

        // Act
        await useCase.ExecuteAsync(command);

        // Assert
        game.Title.ShouldBe("Stardew Valley Deluxe");
        game.Description.ShouldBe("Simulador de fazenda com conteúdo extra.");
        game.Price.ShouldBe(39.90m);
        game.UpdatedBy.ShouldBe(updatedBy);
        game.UpdatedAt.ShouldNotBeNull();
        await gameRepository.Received(1).UpdateAsync(game, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Deve_Lancar_Excecao_Quando_Jogo_Nao_Existir()
    {
        // Arrange
        var gameRepository = Substitute.For<IGameRepository>();
        var gameId = Guid.NewGuid();

        gameRepository
            .GetByIdAsync(gameId, Arg.Any<CancellationToken>())
            .Returns((Game?)null);

        var useCase = new UpdateGameUseCase(gameRepository);
        var command = new UpdateGameCommand(
            gameId,
            "Stardew Valley",
            "Simulador de fazenda.",
            24.90m,
            Guid.NewGuid());

        // Act
        var excecao = await Should.ThrowAsync<GameNotFoundException>(() => useCase.ExecuteAsync(command));

        // Assert
        excecao.Message.ShouldBe(ApplicationMessages.Game.NotFound);
        await gameRepository
            .DidNotReceive()
            .ExistsByTitleForAnotherGameAsync(Arg.Any<string>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await gameRepository.DidNotReceive().UpdateAsync(Arg.Any<Game>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Deve_Lancar_Excecao_Quando_Titulo_Pertencer_A_Outro_Jogo()
    {
        // Arrange
        var game = Game.Create(
            "Stardew Valley",
            "Simulador de fazenda.",
            24.90m,
            Guid.NewGuid());
        var gameRepository = Substitute.For<IGameRepository>();

        gameRepository
            .GetByIdAsync(game.Id, Arg.Any<CancellationToken>())
            .Returns(game);
        gameRepository
            .ExistsByTitleForAnotherGameAsync("Hades", game.Id, Arg.Any<CancellationToken>())
            .Returns(true);

        var useCase = new UpdateGameUseCase(gameRepository);
        var command = new UpdateGameCommand(
            game.Id,
            "Hades",
            "Roguelike de ação.",
            49.90m,
            Guid.NewGuid());

        // Act
        var excecao = await Should.ThrowAsync<GameTitleAlreadyRegisteredException>(() => useCase.ExecuteAsync(command));

        // Assert
        excecao.Message.ShouldBe(ApplicationMessages.Game.TitleAlreadyRegistered);
        await gameRepository.DidNotReceive().UpdateAsync(Arg.Any<Game>(), Arg.Any<CancellationToken>());
    }
}
