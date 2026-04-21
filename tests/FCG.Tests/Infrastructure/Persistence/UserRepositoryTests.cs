using FCG.Domain.Users;
using FCG.Domain.Users.ValueObjects;
using FCG.Infrastructure.Persistence;
using FCG.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FCG.Tests.Infrastructure.Persistence;

[Trait("Category", "Integration")]
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

    [Fact]
    public async Task GetByEmailAsync_QuandoEmailEstiverCadastrado_DeveRetornarUsuario()
    {
        // Arrange
        var email = Email.Create("maicon@email.com");
        var passwordHash = PasswordHash.Create("$2a$11$hashfakeparatestes");
        var user = User.Create("Maicon Guedes", email, passwordHash);

        await using var dbContext = CreateDbContext();
        var repository = new UserRepository(dbContext);

        await repository.AddAsync(user);

        // Act
        var foundUser = await repository.GetByEmailAsync(email);

        // Assert
        foundUser.ShouldNotBeNull();
        foundUser!.Id.ShouldBe(user.Id);
        foundUser.Name.ShouldBe("Maicon Guedes");
        foundUser.Email.ShouldBe(email);
        foundUser.PasswordHash.ShouldBe(passwordHash);
        foundUser.Role.ShouldBe(UserRole.User);
        foundUser.IsActive.ShouldBeTrue();
    }

    [Fact]
    public async Task GetByEmailAsync_QuandoEmailForInformadoComEspacosEMaiusculas_DeveRetornarUsuario()
    {
        // Arrange
        var email = Email.Create("maicon@email.com");
        var passwordHash = PasswordHash.Create("$2a$11$hashfakeparatestes");
        var user = User.Create("Maicon Guedes", email, passwordHash);

        await using var dbContext = CreateDbContext();
        var repository = new UserRepository(dbContext);

        await repository.AddAsync(user);

        // Act
        var foundUser = await repository.GetByEmailAsync(Email.Create("  MAICON@EMAIL.COM  "));

        // Assert
        foundUser.ShouldNotBeNull();
        foundUser!.Id.ShouldBe(user.Id);
        foundUser.Email.ShouldBe(email);
    }

    [Fact]
    public async Task GetByEmailAsync_QuandoEmailNaoEstiverCadastrado_DeveRetornarNulo()
    {
        // Arrange
        await using var dbContext = CreateDbContext();
        var repository = new UserRepository(dbContext);

        // Act
        var foundUser = await repository.GetByEmailAsync(Email.Create("maicon@email.com"));

        // Assert
        foundUser.ShouldBeNull();
    }

    private FcgDbContext CreateDbContext()
    {
        return new FcgDbContext(_dbContextOptions);
    }
}
