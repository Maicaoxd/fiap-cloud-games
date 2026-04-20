using FCG.Domain.Users;
using FCG.Domain.Users.ValueObjects;
using FCG.Infrastructure.Persistence;
using FCG.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FCG.Tests.Infrastructure.Persistence;

public sealed class UserRepositoryTests : IAsyncLifetime
{
    private readonly DbContextOptions<FcgDbContext> _dbContextOptions;

    public UserRepositoryTests()
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
    [Trait("Category", "Integration")]
    public async Task AddAsync_QuandoUsuarioForValido_DevePersistirUsuario()
    {
        // Arrange
        var email = Email.Create("maicon@email.com");
        var passwordHash = PasswordHash.Create("$2a$11$hashfakeparatestes");
        var user = User.Create("Maicon Guedes", email, passwordHash);

        await using var dbContext = CreateDbContext();
        var repository = new UserRepository(dbContext);

        // Act
        await repository.AddAsync(user);

        // Assert
        await using var assertionDbContext = CreateDbContext();
        var persistedUser = await assertionDbContext.Users.AsNoTracking().SingleAsync();

        persistedUser.Id.ShouldBe(user.Id);
        persistedUser.Name.ShouldBe("Maicon Guedes");
        persistedUser.Email.ShouldBe(email);
        persistedUser.PasswordHash.ShouldBe(passwordHash);
        persistedUser.Role.ShouldBe(UserRole.User);
        persistedUser.IsActive.ShouldBeTrue();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task ExistsByEmailAsync_QuandoEmailEstiverCadastrado_DeveRetornarVerdadeiro()
    {
        // Arrange
        var email = Email.Create("maicon@email.com");
        var passwordHash = PasswordHash.Create("$2a$11$hashfakeparatestes");
        var user = User.Create("Maicon Guedes", email, passwordHash);

        await using var dbContext = CreateDbContext();
        var repository = new UserRepository(dbContext);

        await repository.AddAsync(user);

        // Act
        var exists = await repository.ExistsByEmailAsync(Email.Create("  MAICON@EMAIL.COM  "));

        // Assert
        exists.ShouldBeTrue();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task ExistsByEmailAsync_QuandoEmailNaoEstiverCadastrado_DeveRetornarFalso()
    {
        // Arrange
        await using var dbContext = CreateDbContext();
        var repository = new UserRepository(dbContext);

        // Act
        var exists = await repository.ExistsByEmailAsync(Email.Create("maicon@email.com"));

        // Assert
        exists.ShouldBeFalse();
    }

    private FcgDbContext CreateDbContext()
    {
        return new FcgDbContext(_dbContextOptions);
    }
}
