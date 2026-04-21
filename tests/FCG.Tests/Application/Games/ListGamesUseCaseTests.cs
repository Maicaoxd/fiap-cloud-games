using FCG.Application.Abstractions.Persistence;
using FCG.Application.Games.List;
using FCG.Domain.Games;
using NSubstitute;

namespace FCG.Tests.Application.Games;

[Trait("Category", "Unit")]
public sealed class ListGamesUseCaseTests
{
    [Fact]
    public async Task Deve_Listar_Jogos_Ativos()
    {
        // Arrange
        var firstGame = Game.Create(
            "Hades",
            "Roguelike de ação.",
            49.90m,
            Guid.NewGuid());
        var secondGame = Game.Create(
            "Stardew Valley",
            "Simulador de fazenda e vida no campo.",
            24.90m,
            Guid.NewGuid());

        var gameRepository = Substitute.For<IGameRepository>();
        gameRepository
            .ListActiveAsync(Arg.Any<CancellationToken>())
            .Returns(new[] { firstGame, secondGame });

        var useCase = new ListGamesUseCase(gameRepository);

        // Act
        var result = await useCase.ExecuteAsync();

        // Assert
        result.Count.ShouldBe(2);
        result.ShouldContain(game => game.GameId == firstGame.Id);
        result.ShouldContain(game => game.GameId == secondGame.Id);
        result.ShouldContain(game =>
            game.Title == "Hades" &&
            game.Description == "Roguelike de ação." &&
            game.Price == 49.90m);
        await gameRepository.Received(1).ListActiveAsync(Arg.Any<CancellationToken>());
    }
}
