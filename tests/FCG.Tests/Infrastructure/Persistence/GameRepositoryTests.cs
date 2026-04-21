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

    private FcgDbContext CreateDbContext()
    {
        return new FcgDbContext(_dbContextOptions);
    }
}
