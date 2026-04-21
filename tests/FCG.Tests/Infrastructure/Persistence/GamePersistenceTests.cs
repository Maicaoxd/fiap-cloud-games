using FCG.Domain.Games;
using FCG.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FCG.Tests.Infrastructure.Persistence;

[Trait("Category", "Integration")]
public sealed class GamePersistenceTests : IAsyncLifetime
{
    private readonly DbContextOptions<FcgDbContext> _dbContextOptions;

    public GamePersistenceTests()
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
    public async Task Games_QuandoJogoForValido_DevePersistirJogo()
    {
        // Arrange
        var criadoPor = Guid.NewGuid();
        var game = Game.Create(
            "Stardew Valley",
            "Simulador de fazenda e vida no campo.",
            24.90m,
            criadoPor);

        await using var dbContext = CreateDbContext();

        // Act
        await dbContext.Games.AddAsync(game);
        await dbContext.SaveChangesAsync();

        // Assert
        await using var assertionDbContext = CreateDbContext();
        var persistedGame = await assertionDbContext.Games.AsNoTracking().SingleAsync();

        persistedGame.Id.ShouldBe(game.Id);
        persistedGame.Title.ShouldBe("Stardew Valley");
        persistedGame.Description.ShouldBe("Simulador de fazenda e vida no campo.");
        persistedGame.Price.ShouldBe(24.90m);
        persistedGame.IsActive.ShouldBeTrue();
        persistedGame.CreatedAt.ShouldNotBe(default);
        persistedGame.CreatedBy.ShouldBe(criadoPor);
        persistedGame.UpdatedAt.ShouldBeNull();
        persistedGame.UpdatedBy.ShouldBeNull();
    }

    [Fact]
    public async Task Games_QuandoJogoForAtualizado_DevePersistirAuditoria()
    {
        // Arrange
        var criadoPor = Guid.NewGuid();
        var atualizadoPor = Guid.NewGuid();
        var game = Game.Create(
            "Stardew Valley",
            "Simulador de fazenda.",
            24.90m,
            criadoPor);

        await using var dbContext = CreateDbContext();
        await dbContext.Games.AddAsync(game);
        await dbContext.SaveChangesAsync();

        // Act
        game.Update(
            "Stardew Valley Deluxe",
            "Simulador de fazenda com conteúdo extra.",
            39.90m,
            atualizadoPor);

        await dbContext.SaveChangesAsync();

        // Assert
        await using var assertionDbContext = CreateDbContext();
        var persistedGame = await assertionDbContext.Games.AsNoTracking().SingleAsync();

        persistedGame.Title.ShouldBe("Stardew Valley Deluxe");
        persistedGame.Description.ShouldBe("Simulador de fazenda com conteúdo extra.");
        persistedGame.Price.ShouldBe(39.90m);
        persistedGame.UpdatedAt.ShouldNotBeNull();
        persistedGame.UpdatedBy.ShouldBe(atualizadoPor);
    }

    [Fact]
    public async Task Games_QuandoTituloJaExistir_DeveImpedirDuplicidade()
    {
        // Arrange
        var criadoPor = Guid.NewGuid();
        var firstGame = Game.Create(
            "Stardew Valley",
            "Simulador de fazenda.",
            24.90m,
            criadoPor);
        var secondGame = Game.Create(
            "Stardew Valley",
            "Outro jogo com mesmo título.",
            29.90m,
            criadoPor);

        await using var dbContext = CreateDbContext();
        await dbContext.Games.AddAsync(firstGame);
        await dbContext.SaveChangesAsync();

        await dbContext.Games.AddAsync(secondGame);

        // Act
        var acao = () => dbContext.SaveChangesAsync();

        // Assert
        await Should.ThrowAsync<DbUpdateException>(acao);
    }

    private FcgDbContext CreateDbContext()
    {
        return new FcgDbContext(_dbContextOptions);
    }
}
