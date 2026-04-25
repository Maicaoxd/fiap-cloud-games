using FCG.Application.Abstractions.Persistence;
using FCG.Application.Common;
using FCG.Application.Common.Exceptions;
using FCG.Application.Libraries.List;
using FCG.Domain.Users;
using FCG.Domain.Users.ValueObjects;
using NSubstitute;

namespace FCG.Tests.Application.Libraries;

[Trait("Category", "Unit")]
public sealed class ListLibraryGamesUseCaseTests
{
    [Fact]
    public async Task Deve_Listar_Jogos_Da_Biblioteca_Quando_Usuario_For_Valido()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var acquiredAt = DateTime.UtcNow;
        var user = CreateUser(userId);
        var userRepository = Substitute.For<IUserRepository>();
        var libraryRepository = Substitute.For<ILibraryRepository>();

        userRepository
            .GetByIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns(user);

        libraryRepository
            .ListGamesByUserIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns(new[]
            {
                new LibraryGameReadModel(
                    Guid.NewGuid(),
                    Guid.NewGuid(),
                    "Hades",
                    "Roguelike de ação.",
                    49.90m,
                    true,
                    acquiredAt),
                new LibraryGameReadModel(
                    Guid.NewGuid(),
                    Guid.NewGuid(),
                    "Half-Life",
                    "FPS clássico.",
                    29.90m,
                    false,
                    acquiredAt.AddMinutes(-5))
            });

        var useCase = new ListLibraryGamesUseCase(userRepository, libraryRepository);

        // Act
        var result = await useCase.ExecuteAsync(userId);

        // Assert
        var libraryGames = result.ToList();

        libraryGames.Count.ShouldBe(2);
        libraryGames[0].Title.ShouldBe("Hades");
        libraryGames[0].IsActive.ShouldBeTrue();
        libraryGames[0].AcquiredAt.ShouldBe(acquiredAt);
        libraryGames[1].Title.ShouldBe("Half-Life");
        libraryGames[1].IsActive.ShouldBeFalse();
        await libraryRepository.Received(1).ListGamesByUserIdAsync(userId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Deve_Lancar_Excecao_Quando_Usuario_Nao_Existir()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var userRepository = Substitute.For<IUserRepository>();
        var libraryRepository = Substitute.For<ILibraryRepository>();

        userRepository
            .GetByIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns((User?)null);

        var useCase = new ListLibraryGamesUseCase(userRepository, libraryRepository);

        // Act
        var exception = await Should.ThrowAsync<InvalidCredentialsException>(() => useCase.ExecuteAsync(userId));

        // Assert
        exception.Message.ShouldBe(ApplicationMessages.Authentication.InvalidCredentials);
        await libraryRepository.DidNotReceive().ListGamesByUserIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Deve_Lancar_Excecao_Quando_Usuario_Estiver_Inativo()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = CreateUser(userId, isActive: false);
        var userRepository = Substitute.For<IUserRepository>();
        var libraryRepository = Substitute.For<ILibraryRepository>();

        userRepository
            .GetByIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns(user);

        var useCase = new ListLibraryGamesUseCase(userRepository, libraryRepository);

        // Act
        var exception = await Should.ThrowAsync<InactiveUserException>(() => useCase.ExecuteAsync(userId));

        // Assert
        exception.Message.ShouldBe(ApplicationMessages.Authentication.InactiveUser);
        await libraryRepository.DidNotReceive().ListGamesByUserIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    private static User CreateUser(Guid userId, bool isActive = true)
    {
        var email = Email.Create("maicon@email.com");
        var cpf = Cpf.Create("529.982.247-25");
        var passwordHash = PasswordHash.Create("$2a$11$hashfakeparatestes");
        var user = User.Create("Maicon Guedes", email, cpf, new DateOnly(1993, 6, 17), passwordHash, userId);

        if (!isActive)
            user.Deactivate(userId);

        return user;
    }
}
