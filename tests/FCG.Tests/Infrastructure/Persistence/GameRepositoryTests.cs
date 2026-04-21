using FCG.Domain.Games;
using FCG.Infrastructure.Persistence;
using FCG.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FCG.Tests.Infrastructure.Persistence;

[Trait("Category", "Integration")]
public sealed class GameRepositoryTests : IAsyncLifetime
{
    private readonly DbContextOptions<FcgDbContext> _dbContextOptions;

    public GameRepositoryTests()
    {
        var databaseName = $"FiapCloudGamesTests_{Guid.NewGuid():N}";
        var connectionString = $"Server=(localdb)\\MSSQLLocalDB;Database={databaseName};Trusted_Connection=True;TrustServerCertificate=True";

        _dbContextOptions = new DbContextOptionsBuilder<FcgDbContext>()
            .UseSqlServer(connectionString)
            .Options;
    }

    public async Task InitializeAsync()
    {
        await using var dbContext = CreateDbContext();

        await dbContext.Database.EnsureDeletedAsync();
        await dbContext.Database.EnsureCreatedAsync();
    }

    public async Task DisposeAsync()
    {
        await using var dbContext = CreateDbContext();

        await dbContext.Database.EnsureDeletedAsync();
    }

    [Fact]
    public async Task AddAsync_QuandoJogoForValido_DevePersistirJogo()
    {
        // Arrange
        var createdBy = Guid.NewGuid();
        var game = Game.Create(
            "Stardew Valley",
            "Simulador de fazenda e vida no campo.",
            24.90m,
            createdBy);

        await using var dbContext = CreateDbContext();
        var repository = new GameRepository(dbContext);

        // Act
        await repository.AddAsync(game);

        // Assert
        await using var assertionDbContext = CreateDbContext();
        var persistedGame = await assertionDbContext.Games.AsNoTracking().SingleAsync();

        persistedGame.Id.ShouldBe(game.Id);
        persistedGame.Title.ShouldBe("Stardew Valley");
        persistedGame.Description.ShouldBe("Simulador de fazenda e vida no campo.");
        persistedGame.Price.ShouldBe(24.90m);
        persistedGame.CreatedBy.ShouldBe(createdBy);
        persistedGame.IsActive.ShouldBeTrue();
    }

    [Fact]
    public async Task ExistsByTitleAsync_QuandoTituloEstiverCadastrado_DeveRetornarVerdadeiro()
    {
        // Arrange
        var game = Game.Create(
            "Stardew Valley",
            "Simulador de fazenda e vida no campo.",
            24.90m,
            Guid.NewGuid());

        await using var dbContext = CreateDbContext();
        var repository = new GameRepository(dbContext);

        await repository.AddAsync(game);

        // Act
        var exists = await repository.ExistsByTitleAsync("  STARDEW VALLEY  ");

        // Assert
        exists.ShouldBeTrue();
    }

    [Fact]
    public async Task ExistsByTitleAsync_QuandoTituloNaoEstiverCadastrado_DeveRetornarFalso()
    {
        // Arrange
        await using var dbContext = CreateDbContext();
        var repository = new GameRepository(dbContext);

        // Act
        var exists = await repository.ExistsByTitleAsync("Stardew Valley");

        // Assert
        exists.ShouldBeFalse();
    }

    [Fact]
    public async Task ListActiveAsync_QuandoExistiremJogosAtivosEInativos_DeveRetornarApenasAtivosOrdenadosPorTitulo()
    {
        // Arrange
        var createdBy = Guid.NewGuid();
        var inactiveGame = Game.Create(
            "Zelda",
            "Aventura.",
            299.90m,
            createdBy);
        inactiveGame.Deactivate(createdBy);

        var firstActiveGame = Game.Create(
            "Hades",
            "Roguelike de ação.",
            49.90m,
            createdBy);
        var secondActiveGame = Game.Create(
            "Stardew Valley",
            "Simulador de fazenda e vida no campo.",
            24.90m,
            createdBy);

        await using var dbContext = CreateDbContext();
        await dbContext.Games.AddRangeAsync(inactiveGame, secondActiveGame, firstActiveGame);
        await dbContext.SaveChangesAsync();

        var repository = new GameRepository(dbContext);

        // Act
        var games = await repository.ListActiveAsync();

        // Assert
        games.Count.ShouldBe(2);
        games.Select(game => game.Title).ShouldBe(["Hades", "Stardew Valley"]);
        games.ShouldNotContain(game => game.Title == "Zelda");
    }

    [Fact]
    public async Task GetByIdAsync_QuandoJogoExistir_DeveRetornarJogo()
    {
        // Arrange
        var game = Game.Create(
            "Hades",
            "Roguelike de ação.",
            49.90m,
            Guid.NewGuid());

        await using var dbContext = CreateDbContext();
        await dbContext.Games.AddAsync(game);
        await dbContext.SaveChangesAsync();

        var repository = new GameRepository(dbContext);

        // Act
        var foundGame = await repository.GetByIdAsync(game.Id);

        // Assert
        foundGame.ShouldNotBeNull();
        foundGame!.Id.ShouldBe(game.Id);
    }

    [Fact]
    public async Task ExistsByTitleForAnotherGameAsync_QuandoTituloPertencerAoMesmoJogo_DeveRetornarFalso()
    {
        // Arrange
        var game = Game.Create(
            "Hades",
            "Roguelike de ação.",
            49.90m,
            Guid.NewGuid());

        await using var dbContext = CreateDbContext();
        await dbContext.Games.AddAsync(game);
        await dbContext.SaveChangesAsync();

        var repository = new GameRepository(dbContext);

        // Act
        var exists = await repository.ExistsByTitleForAnotherGameAsync(" HADES ", game.Id);

        // Assert
        exists.ShouldBeFalse();
    }

    [Fact]
    public async Task ExistsByTitleForAnotherGameAsync_QuandoTituloPertencerAOutroJogo_DeveRetornarVerdadeiro()
    {
        // Arrange
        var firstGame = Game.Create(
            "Hades",
            "Roguelike de ação.",
            49.90m,
            Guid.NewGuid());
        var secondGame = Game.Create(
            "Stardew Valley",
            "Simulador de fazenda.",
            24.90m,
            Guid.NewGuid());

        await using var dbContext = CreateDbContext();
        await dbContext.Games.AddRangeAsync(firstGame, secondGame);
        await dbContext.SaveChangesAsync();

        var repository = new GameRepository(dbContext);

        // Act
        var exists = await repository.ExistsByTitleForAnotherGameAsync(" HADES ", secondGame.Id);

        // Assert
        exists.ShouldBeTrue();
    }

    [Fact]
    public async Task UpdateAsync_QuandoJogoForAtualizado_DevePersistirAlteracoes()
    {
        // Arrange
        var createdBy = Guid.NewGuid();
        var updatedBy = Guid.NewGuid();
        var game = Game.Create(
            "Stardew Valley",
            "Simulador de fazenda.",
            24.90m,
            createdBy);

        await using var dbContext = CreateDbContext();
        await dbContext.Games.AddAsync(game);
        await dbContext.SaveChangesAsync();

        var repository = new GameRepository(dbContext);
        game.Update(
            "Stardew Valley Deluxe",
            "Simulador de fazenda com conteúdo extra.",
            39.90m,
            updatedBy);

        // Act
        await repository.UpdateAsync(game);

        // Assert
        await using var assertionDbContext = CreateDbContext();
        var persistedGame = await assertionDbContext.Games.AsNoTracking().SingleAsync();

        persistedGame.Title.ShouldBe("Stardew Valley Deluxe");
        persistedGame.Description.ShouldBe("Simulador de fazenda com conteúdo extra.");
        persistedGame.Price.ShouldBe(39.90m);
        persistedGame.UpdatedBy.ShouldBe(updatedBy);
        persistedGame.UpdatedAt.ShouldNotBeNull();
    }

    private FcgDbContext CreateDbContext()
    {
        return new FcgDbContext(_dbContextOptions);
    }
}
