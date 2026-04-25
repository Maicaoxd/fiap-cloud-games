using FCG.Application.Abstractions.Persistence;
using FCG.Application.Common;
using FCG.Application.Common.Exceptions;
using FCG.Application.Games.Get;
using FCG.Domain.Games;
using NSubstitute;

namespace FCG.Tests.Application.Games;

[Trait("Category", "Unit")]
public sealed class GetGameUseCaseTests
{
    [Fact]
    public async Task Deve_Retornar_Jogo_Quando_Jogo_Ativo_Existir()
    {
        // Arrange
        var game = Game.Create(
            "Hades",
            "Roguelike de ação.",
            49.90m,
            Guid.NewGuid());
        var gameRepository = Substitute.For<IGameRepository>();

        gameRepository
            .GetByIdAsync(game.Id, Arg.Any<CancellationToken>())
            .Returns(game);

        var useCase = new GetGameUseCase(gameRepository);

        // Act
        var result = await useCase.ExecuteAsync(game.Id);

        // Assert
        result.GameId.ShouldBe(game.Id);
        result.Title.ShouldBe("Hades");
        result.Description.ShouldBe("Roguelike de ação.");
        result.Price.ShouldBe(49.90m);
        await gameRepository.Received(1).GetByIdAsync(game.Id, Arg.Any<CancellationToken>());
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

        var useCase = new GetGameUseCase(gameRepository);

        // Act
        var excecao = await Should.ThrowAsync<GameNotFoundException>(() => useCase.ExecuteAsync(gameId));

        // Assert
        excecao.Message.ShouldBe(ApplicationMessages.Game.NotFound);
    }

    [Fact]
    public async Task Deve_Lancar_Excecao_Quando_Jogo_Estiver_Inativo()
    {
        // Arrange
        var changedBy = Guid.NewGuid();
        var game = Game.Create(
            "Hades",
            "Roguelike de ação.",
            49.90m,
            changedBy);
        game.Deactivate(changedBy);

        var gameRepository = Substitute.For<IGameRepository>();
        gameRepository
            .GetByIdAsync(game.Id, Arg.Any<CancellationToken>())
            .Returns(game);

        var useCase = new GetGameUseCase(gameRepository);

        // Act
        var excecao = await Should.ThrowAsync<GameNotFoundException>(() => useCase.ExecuteAsync(game.Id));

        // Assert
        excecao.Message.ShouldBe(ApplicationMessages.Game.NotFound);
    }
}
