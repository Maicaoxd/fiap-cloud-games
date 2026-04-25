using FCG.Application.Abstractions.Persistence;
using FCG.Application.Common;
using FCG.Application.Common.Exceptions;
using FCG.Application.Users.UpdateCurrent;
using FCG.Domain.Users;
using FCG.Domain.Users.ValueObjects;
using NSubstitute;

namespace FCG.Tests.Application.Users;

[Trait("Category", "Unit")]
public sealed class UpdateCurrentUserUseCaseTests
{
    [Fact]
    public async Task Deve_Atualizar_Usuario_Quando_Dados_Forem_Validos()
    {
        // Arrange
        var user = CreateUser();
        var userRepository = Substitute.For<IUserRepository>();

        userRepository
            .GetByIdAsync(user.Id, Arg.Any<CancellationToken>())
            .Returns(user);
        userRepository
            .GetByEmailAsync(Email.Create("maicon.guedes@email.com"), Arg.Any<CancellationToken>())
            .Returns((User?)null);

        var useCase = new UpdateCurrentUserUseCase(userRepository);
        var command = new UpdateCurrentUserCommand(
            user.Id,
            "Maicon Guedes",
            "maicon.guedes@email.com");

        // Act
        await useCase.ExecuteAsync(command);

        // Assert
        user.Name.ShouldBe("Maicon Guedes");
        user.Email.ShouldBe(Email.Create("maicon.guedes@email.com"));
        user.UpdatedBy.ShouldBe(user.Id);
        user.UpdatedAt.ShouldNotBeNull();
        await userRepository.Received(1).UpdateAsync(user, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Deve_Permitir_Atualizar_Quando_Email_Pertencer_Ao_Proprio_Usuario()
    {
        // Arrange
        var user = CreateUser();
        var userRepository = Substitute.For<IUserRepository>();

        userRepository
            .GetByIdAsync(user.Id, Arg.Any<CancellationToken>())
            .Returns(user);
        userRepository
            .GetByEmailAsync(user.Email, Arg.Any<CancellationToken>())
            .Returns(user);

        var useCase = new UpdateCurrentUserUseCase(userRepository);
        var command = new UpdateCurrentUserCommand(
            user.Id,
            "Maicon Guedes",
            user.Email.Value);

        // Act
        await useCase.ExecuteAsync(command);

        // Assert
        user.Name.ShouldBe("Maicon Guedes");
        user.Email.ShouldBe(Email.Create("maicon@email.com"));
        await userRepository.Received(1).UpdateAsync(user, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Deve_Lancar_Excecao_Quando_Usuario_Nao_Existir()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var userRepository = Substitute.For<IUserRepository>();

        userRepository
            .GetByIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns((User?)null);

        var useCase = new UpdateCurrentUserUseCase(userRepository);
        var command = new UpdateCurrentUserCommand(
            userId,
            "Maicon Guedes",
            "maicon@email.com");

        // Act
        var exception = await Should.ThrowAsync<InvalidCredentialsException>(() => useCase.ExecuteAsync(command));

        // Assert
        exception.Message.ShouldBe(ApplicationMessages.Authentication.InvalidCredentials);
        await userRepository.DidNotReceive().UpdateAsync(Arg.Any<User>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Deve_Lancar_Excecao_Quando_Usuario_Estiver_Inativo()
    {
        // Arrange
        var user = CreateUser();
        user.Deactivate(Guid.NewGuid());
        var userRepository = Substitute.For<IUserRepository>();

        userRepository
            .GetByIdAsync(user.Id, Arg.Any<CancellationToken>())
            .Returns(user);

        var useCase = new UpdateCurrentUserUseCase(userRepository);
        var command = new UpdateCurrentUserCommand(
            user.Id,
            "Maicon Guedes",
            "maicon@email.com");

        // Act
        var exception = await Should.ThrowAsync<InactiveUserException>(() => useCase.ExecuteAsync(command));

        // Assert
        exception.Message.ShouldBe(ApplicationMessages.Authentication.InactiveUser);
        await userRepository.DidNotReceive().UpdateAsync(Arg.Any<User>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Deve_Lancar_Excecao_Quando_Email_Pertencer_A_Outro_Usuario()
    {
        // Arrange
        var user = CreateUser();
        var anotherUser = User.Create(
            "Outro Usuário",
            Email.Create("outro@email.com"),
            PasswordHash.Create("$2a$11$outrohashfakeparatestes"));
        var userRepository = Substitute.For<IUserRepository>();

        userRepository
            .GetByIdAsync(user.Id, Arg.Any<CancellationToken>())
            .Returns(user);
        userRepository
            .GetByEmailAsync(Email.Create("outro@email.com"), Arg.Any<CancellationToken>())
            .Returns(anotherUser);

        var useCase = new UpdateCurrentUserUseCase(userRepository);
        var command = new UpdateCurrentUserCommand(
            user.Id,
            "Maicon Guedes",
            "outro@email.com");

        // Act
        var exception = await Should.ThrowAsync<EmailAlreadyRegisteredException>(() => useCase.ExecuteAsync(command));

        // Assert
        exception.Message.ShouldBe(ApplicationMessages.User.EmailAlreadyRegistered);
        await userRepository.DidNotReceive().UpdateAsync(Arg.Any<User>(), Arg.Any<CancellationToken>());
    }

    private static User CreateUser()
    {
        var email = Email.Create("maicon@email.com");
        var passwordHash = PasswordHash.Create("$2a$11$hashfakeparatestes");

        return User.Create("Maicon Alves", email, passwordHash);
    }
}
