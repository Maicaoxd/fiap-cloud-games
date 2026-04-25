using FCG.Application.Abstractions.Persistence;
using FCG.Application.Common;
using FCG.Application.Common.Exceptions;
using FCG.Application.Libraries.Acquire;
using FCG.Domain.Games;
using FCG.Domain.Libraries;
using FCG.Domain.Users;
using FCG.Domain.Users.ValueObjects;
using NSubstitute;

namespace FCG.Tests.Application.Libraries;

[Trait("Category", "Unit")]
public sealed class AcquireGameUseCaseTests
{
    [Fact]
    public async Task Deve_Adquirir_Jogo_Quando_Usuario_E_Jogo_Forem_Validos()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var gameId = Guid.NewGuid();
        var user = CreateUser(userId);
        var game = CreateGame(gameId, isActive: true);
        var userRepository = Substitute.For<IUserRepository>();
        var gameRepository = Substitute.For<IGameRepository>();
        var libraryRepository = Substitute.For<ILibraryRepository>();
        Library? addedLibrary = null;

        userRepository
            .GetByIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns(user);

        gameRepository
            .GetByIdAsync(gameId, Arg.Any<CancellationToken>())
            .Returns(game);

        libraryRepository
            .ExistsByUserAndGameAsync(userId, gameId, Arg.Any<CancellationToken>())
            .Returns(false);

        libraryRepository
            .AddAsync(Arg.Any<Library>(), Arg.Any<CancellationToken>())
            .Returns(callInfo =>
            {
                addedLibrary = callInfo.ArgAt<Library>(0);

                return Task.CompletedTask;
            });

        var useCase = new AcquireGameUseCase(
            userRepository,
            gameRepository,
            libraryRepository);

        var command = new AcquireGameCommand(userId, gameId);

        // Act
        var result = await useCase.ExecuteAsync(command);

        // Assert
        result.LibraryId.ShouldNotBe(Guid.Empty);
        addedLibrary.ShouldNotBeNull();
        addedLibrary!.UserId.ShouldBe(userId);
        addedLibrary.GameId.ShouldBe(gameId);
        await libraryRepository.Received(1)
            .AddAsync(Arg.Any<Library>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Deve_Lancar_Excecao_Quando_Usuario_Nao_Existir()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var gameId = Guid.NewGuid();
        var userRepository = Substitute.For<IUserRepository>();
        var gameRepository = Substitute.For<IGameRepository>();
        var libraryRepository = Substitute.For<ILibraryRepository>();

        userRepository
            .GetByIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns((User?)null);

        var useCase = new AcquireGameUseCase(
            userRepository,
            gameRepository,
            libraryRepository);

        var command = new AcquireGameCommand(userId, gameId);

        // Act
        var exception = await Should.ThrowAsync<InvalidCredentialsException>(() => useCase.ExecuteAsync(command));

        // Assert
        exception.Message.ShouldBe(ApplicationMessages.Authentication.InvalidCredentials);
        await gameRepository.DidNotReceive().GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await libraryRepository.DidNotReceive()
            .AddAsync(Arg.Any<Library>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Deve_Lancar_Excecao_Quando_Usuario_Estiver_Inativo()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var gameId = Guid.NewGuid();
        var user = CreateUser(userId, isActive: false);
        var userRepository = Substitute.For<IUserRepository>();
        var gameRepository = Substitute.For<IGameRepository>();
        var libraryRepository = Substitute.For<ILibraryRepository>();

        userRepository
            .GetByIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns(user);

        var useCase = new AcquireGameUseCase(
            userRepository,
            gameRepository,
            libraryRepository);

        var command = new AcquireGameCommand(userId, gameId);

        // Act
        var exception = await Should.ThrowAsync<InactiveUserException>(() => useCase.ExecuteAsync(command));

        // Assert
        exception.Message.ShouldBe(ApplicationMessages.Authentication.InactiveUser);
        await gameRepository.DidNotReceive().GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Deve_Lancar_Excecao_Quando_Jogo_Nao_Existir()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var gameId = Guid.NewGuid();
        var user = CreateUser(userId);
        var userRepository = Substitute.For<IUserRepository>();
        var gameRepository = Substitute.For<IGameRepository>();
        var libraryRepository = Substitute.For<ILibraryRepository>();

        userRepository
            .GetByIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns(user);

        gameRepository
            .GetByIdAsync(gameId, Arg.Any<CancellationToken>())
            .Returns((Game?)null);

        var useCase = new AcquireGameUseCase(
            userRepository,
            gameRepository,
            libraryRepository);

        var command = new AcquireGameCommand(userId, gameId);

        // Act
        var exception = await Should.ThrowAsync<GameNotFoundException>(() => useCase.ExecuteAsync(command));

        // Assert
        exception.Message.ShouldBe(ApplicationMessages.Game.NotFound);
        await libraryRepository.DidNotReceive()
            .AddAsync(Arg.Any<Library>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Deve_Lancar_Excecao_Quando_Jogo_Estiver_Inativo()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var gameId = Guid.NewGuid();
        var user = CreateUser(userId);
        var game = CreateGame(gameId, isActive: false);
        var userRepository = Substitute.For<IUserRepository>();
        var gameRepository = Substitute.For<IGameRepository>();
        var libraryRepository = Substitute.For<ILibraryRepository>();

        userRepository
            .GetByIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns(user);

        gameRepository
            .GetByIdAsync(gameId, Arg.Any<CancellationToken>())
            .Returns(game);

        var useCase = new AcquireGameUseCase(
            userRepository,
            gameRepository,
            libraryRepository);

        var command = new AcquireGameCommand(userId, gameId);

        // Act
        var exception = await Should.ThrowAsync<GameUnavailableException>(() => useCase.ExecuteAsync(command));

        // Assert
        exception.Message.ShouldBe(ApplicationMessages.Game.InactiveCannotBeAcquired);
    }

    [Fact]
    public async Task Deve_Lancar_Excecao_Quando_Usuario_Ja_Possuir_O_Jogo()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var gameId = Guid.NewGuid();
        var user = CreateUser(userId);
        var game = CreateGame(gameId, isActive: true);
        var userRepository = Substitute.For<IUserRepository>();
        var gameRepository = Substitute.For<IGameRepository>();
        var libraryRepository = Substitute.For<ILibraryRepository>();

        userRepository
            .GetByIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns(user);

        gameRepository
            .GetByIdAsync(gameId, Arg.Any<CancellationToken>())
            .Returns(game);

        libraryRepository
            .ExistsByUserAndGameAsync(userId, gameId, Arg.Any<CancellationToken>())
            .Returns(true);

        var useCase = new AcquireGameUseCase(
            userRepository,
            gameRepository,
            libraryRepository);

        var command = new AcquireGameCommand(userId, gameId);

        // Act
        var exception = await Should.ThrowAsync<GameAlreadyOwnedException>(() => useCase.ExecuteAsync(command));

        // Assert
        exception.Message.ShouldBe(ApplicationMessages.Library.GameAlreadyOwned);
        await libraryRepository.DidNotReceive()
            .AddAsync(Arg.Any<Library>(), Arg.Any<CancellationToken>());
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

    private static Game CreateGame(Guid gameId, bool isActive)
    {
        var game = Game.Create(
            "Hades",
            "Roguelike de ação.",
            49.90m,
            gameId);

        if (!isActive)
            game.Deactivate(gameId);

        return game;
    }
}
