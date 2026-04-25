using FCG.Application.Abstractions.Persistence;
using FCG.Application.Common;
using FCG.Application.Common.Exceptions;
using FCG.Application.Games.Deactivate;
using FCG.Domain.Games;
using NSubstitute;

namespace FCG.Tests.Application.Games;

[Trait("Category", "Unit")]
public sealed class DeactivateGameUseCaseTests
{
    [Fact]
    public async Task Deve_Desativar_Jogo_Quando_Ele_Estiver_Ativo()
    {
        // Arrange
        var deactivatedBy = Guid.NewGuid();
        var game = Game.Create(
            "Stardew Valley",
            "Simulador de fazenda.",
            24.90m,
            Guid.NewGuid());
        var gameRepository = Substitute.For<IGameRepository>();

        gameRepository
            .GetByIdAsync(game.Id, Arg.Any<CancellationToken>())
            .Returns(game);

        var useCase = new DeactivateGameUseCase(gameRepository);
        var command = new DeactivateGameCommand(game.Id, deactivatedBy);

        // Act
        await useCase.ExecuteAsync(command);

        // Assert
        game.IsActive.ShouldBeFalse();
        game.UpdatedBy.ShouldBe(deactivatedBy);
        game.UpdatedAt.ShouldNotBeNull();
        await gameRepository.Received(1).UpdateAsync(game, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Deve_Lancar_Excecao_Quando_Jogo_Nao_Existir()
    {
        // Arrange
        var gameId = Guid.NewGuid();
        var gameRepository = Substitute.For<IGameRepository>();

        gameRepository
            .GetByIdAsync(gameId, Arg.Any<CancellationToken>())
            .Returns((Game?)null);

        var useCase = new DeactivateGameUseCase(gameRepository);
        var command = new DeactivateGameCommand(gameId, Guid.NewGuid());

        // Act
        var exception = await Should.ThrowAsync<GameNotFoundException>(() => useCase.ExecuteAsync(command));

        // Assert
        exception.Message.ShouldBe(ApplicationMessages.Game.NotFound);
        await gameRepository.DidNotReceive().UpdateAsync(Arg.Any<Game>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Deve_Nao_Persistir_Quando_Jogo_Ja_Estiver_Inativo()
    {
        // Arrange
        var game = Game.Create(
            "Stardew Valley",
            "Simulador de fazenda.",
            24.90m,
            Guid.NewGuid());
        game.Deactivate(Guid.NewGuid());

        var updatedAt = game.UpdatedAt;
        var updatedBy = game.UpdatedBy;
        var gameRepository = Substitute.For<IGameRepository>();

        gameRepository
            .GetByIdAsync(game.Id, Arg.Any<CancellationToken>())
            .Returns(game);

        var useCase = new DeactivateGameUseCase(gameRepository);
        var command = new DeactivateGameCommand(game.Id, Guid.NewGuid());

        // Act
        await useCase.ExecuteAsync(command);

        // Assert
        game.IsActive.ShouldBeFalse();
        game.UpdatedAt.ShouldBe(updatedAt);
        game.UpdatedBy.ShouldBe(updatedBy);
        await gameRepository.DidNotReceive().UpdateAsync(Arg.Any<Game>(), Arg.Any<CancellationToken>());
    }
}
