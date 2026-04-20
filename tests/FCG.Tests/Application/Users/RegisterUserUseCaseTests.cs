using FCG.Application.Abstractions.Persistence;
using FCG.Application.Abstractions.Security;
using FCG.Application.Users.Register;
using FCG.Domain.Users;
using FCG.Domain.Users.ValueObjects;
using NSubstitute;

namespace FCG.Tests.Application.Users;

public sealed class RegisterUserUseCaseTests
{
    [Fact]
    public async Task Deve_Cadastrar_Usuario_Quando_Dados_Forem_Validos()
    {
        // Arrange
        var passwordHash = PasswordHash.Create("$2a$11$hashfakeparatestes");
        var userRepository = Substitute.For<IUserRepository>();
        var passwordHasher = Substitute.For<IPasswordHasher>();
        User? addedUser = null;

        userRepository
            .ExistsByEmailAsync(Arg.Any<Email>(), Arg.Any<CancellationToken>())
            .Returns(false);

        userRepository
            .AddAsync(Arg.Any<User>(), Arg.Any<CancellationToken>())
            .Returns(callInfo =>
            {
                addedUser = callInfo.ArgAt<User>(0);

                return Task.CompletedTask;
            });

        passwordHasher.Hash(Arg.Any<Password>()).Returns(passwordHash);

        var useCase = new RegisterUserUseCase(userRepository, passwordHasher);
        var command = new RegisterUserCommand(
            "Maicon Guedes",
            "maicon@email.com",
            "Senha@123",
            "Senha@123");

        // Act
        var result = await useCase.ExecuteAsync(command);

        // Assert
        result.UserId.ShouldNotBe(Guid.Empty);
        addedUser.ShouldNotBeNull();
        addedUser!.Id.ShouldBe(result.UserId);
        addedUser.Name.ShouldBe("Maicon Guedes");
        addedUser.Email.ShouldBe(Email.Create("maicon@email.com"));
        addedUser.PasswordHash.ShouldBe(passwordHash);
        addedUser.Role.ShouldBe(UserRole.User);
        passwordHasher.Received(1).Hash(Arg.Any<Password>());
        await userRepository.Received(1).AddAsync(Arg.Any<User>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Deve_Lancar_Excecao_Quando_Senha_E_Confirmacao_Nao_Conferirem()
    {
        // Arrange
        var userRepository = Substitute.For<IUserRepository>();
        var passwordHasher = Substitute.For<IPasswordHasher>();
        var useCase = new RegisterUserUseCase(userRepository, passwordHasher);
        var command = new RegisterUserCommand(
            "Maicon Guedes",
            "maicon@email.com",
            "Senha@123",
            "Outra@123");

        // Act
        var excecao = await Should.ThrowAsync<ArgumentException>(() => useCase.ExecuteAsync(command));

        // Assert
        excecao.Message.ShouldBe("As senhas não conferem.");
        passwordHasher.DidNotReceive().Hash(Arg.Any<Password>());
        await userRepository.DidNotReceive().AddAsync(Arg.Any<User>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Deve_Lancar_Excecao_Quando_Email_Ja_Estiver_Cadastrado()
    {
        // Arrange
        var userRepository = Substitute.For<IUserRepository>();
        var passwordHasher = Substitute.For<IPasswordHasher>();

        userRepository
            .ExistsByEmailAsync(Arg.Any<Email>(), Arg.Any<CancellationToken>())
            .Returns(true);

        var useCase = new RegisterUserUseCase(userRepository, passwordHasher);
        var command = new RegisterUserCommand(
            "Maicon Guedes",
            "maicon@email.com",
            "Senha@123",
            "Senha@123");

        // Act
        var excecao = await Should.ThrowAsync<InvalidOperationException>(() => useCase.ExecuteAsync(command));

        // Assert
        excecao.Message.ShouldBe("E-mail já cadastrado.");
        passwordHasher.DidNotReceive().Hash(Arg.Any<Password>());
        await userRepository.DidNotReceive().AddAsync(Arg.Any<User>(), Arg.Any<CancellationToken>());
    }
}
