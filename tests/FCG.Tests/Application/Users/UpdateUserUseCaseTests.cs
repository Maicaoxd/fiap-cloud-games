using FCG.Application.Abstractions.Persistence;
using FCG.Application.Common;
using FCG.Application.Common.Exceptions;
using FCG.Application.Users.Update;
using FCG.Domain.Users;
using FCG.Domain.Users.ValueObjects;
using NSubstitute;

namespace FCG.Tests.Application.Users;

[Trait("Category", "Unit")]
public sealed class UpdateUserUseCaseTests
{
    [Fact]
    public async Task Deve_Atualizar_Usuario_Quando_Dados_Forem_Validos()
    {
        // Arrange
        var adminId = Guid.NewGuid();
        var user = CreateUser();
        var userRepository = Substitute.For<IUserRepository>();

        userRepository
            .GetByIdAsync(user.Id, Arg.Any<CancellationToken>())
            .Returns(user);
        userRepository
            .GetByEmailAsync(Email.Create("maicon.guedes@email.com"), Arg.Any<CancellationToken>())
            .Returns((User?)null);

        var useCase = new UpdateUserUseCase(userRepository);
        var command = new UpdateUserCommand(
            user.Id,
            "Maicon Guedes",
            "maicon.guedes@email.com",
            adminId);

        // Act
        await useCase.ExecuteAsync(command);

        // Assert
        user.Name.ShouldBe("Maicon Guedes");
        user.Email.ShouldBe(Email.Create("maicon.guedes@email.com"));
        user.UpdatedBy.ShouldBe(adminId);
        user.UpdatedAt.ShouldNotBeNull();
        await userRepository.Received(1).UpdateAsync(user, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Deve_Permitir_Atualizar_Usuario_Inativo()
    {
        // Arrange
        var adminId = Guid.NewGuid();
        var user = CreateUser();
        user.Deactivate(Guid.NewGuid());
        var userRepository = Substitute.For<IUserRepository>();

        userRepository
            .GetByIdAsync(user.Id, Arg.Any<CancellationToken>())
            .Returns(user);
        userRepository
            .GetByEmailAsync(Email.Create("maicon.guedes@email.com"), Arg.Any<CancellationToken>())
            .Returns((User?)null);

        var useCase = new UpdateUserUseCase(userRepository);
        var command = new UpdateUserCommand(
            user.Id,
            "Maicon Guedes",
            "maicon.guedes@email.com",
            adminId);

        // Act
        await useCase.ExecuteAsync(command);

        // Assert
        user.Name.ShouldBe("Maicon Guedes");
        user.Email.ShouldBe(Email.Create("maicon.guedes@email.com"));
        user.IsActive.ShouldBeFalse();
        user.UpdatedBy.ShouldBe(adminId);
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

        var useCase = new UpdateUserUseCase(userRepository);
        var command = new UpdateUserCommand(
            user.Id,
            "Maicon Guedes",
            user.Email.Value,
            Guid.NewGuid());

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

        var useCase = new UpdateUserUseCase(userRepository);
        var command = new UpdateUserCommand(
            userId,
            "Maicon Guedes",
            "maicon@email.com",
            Guid.NewGuid());

        // Act
        var exception = await Should.ThrowAsync<UserNotFoundException>(() => useCase.ExecuteAsync(command));

        // Assert
        exception.Message.ShouldBe(ApplicationMessages.User.NotFound);
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

        var useCase = new UpdateUserUseCase(userRepository);
        var command = new UpdateUserCommand(
            user.Id,
            "Maicon Guedes",
            "outro@email.com",
            Guid.NewGuid());

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
