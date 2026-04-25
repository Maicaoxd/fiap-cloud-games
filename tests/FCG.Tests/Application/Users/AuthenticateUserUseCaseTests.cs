using FCG.Application.Abstractions.Persistence;
using FCG.Application.Abstractions.Security;
using FCG.Application.Common;
using FCG.Application.Common.Exceptions;
using FCG.Application.Users.Authenticate;
using FCG.Domain.Users;
using FCG.Domain.Users.ValueObjects;
using NSubstitute;

namespace FCG.Tests.Application.Users;

[Trait("Category", "Unit")]
public sealed class AuthenticateUserUseCaseTests
{
    [Fact]
    public async Task Deve_Autenticar_Usuario_Quando_Credenciais_Forem_Validas()
    {
        // Arrange
        var email = Email.Create("maicon@email.com");
        var passwordHash = PasswordHash.Create("$2a$11$hashfakeparatestes");
        var user = User.Create("Maicon Guedes", email, passwordHash);
        var userRepository = Substitute.For<IUserRepository>();
        var passwordHasher = Substitute.For<IPasswordHasher>();
        var accessTokenGenerator = Substitute.For<IAccessTokenGenerator>();

        userRepository
            .GetByEmailAsync(email, Arg.Any<CancellationToken>())
            .Returns(user);

        passwordHasher
            .Verify(Arg.Any<string?>(), passwordHash)
            .Returns(true);

        accessTokenGenerator
            .Generate(user)
            .Returns("access-token");

        var useCase = new AuthenticateUserUseCase(
            userRepository,
            passwordHasher,
            accessTokenGenerator);

        var command = new AuthenticateUserCommand(
            "maicon@email.com",
            "Senha@123");

        // Act
        var result = await useCase.ExecuteAsync(command);

        // Assert
        result.AccessToken.ShouldBe("access-token");
        await userRepository.Received(1).GetByEmailAsync(email, Arg.Any<CancellationToken>());
        passwordHasher.Received(1).Verify("Senha@123", passwordHash);
        accessTokenGenerator.Received(1).Generate(user);
    }

    [Fact]
    public async Task Deve_Lancar_Excecao_Quando_Email_Nao_Estiver_Cadastrado()
    {
        // Arrange
        var email = Email.Create("maicon@email.com");
        var userRepository = Substitute.For<IUserRepository>();
        var passwordHasher = Substitute.For<IPasswordHasher>();
        var accessTokenGenerator = Substitute.For<IAccessTokenGenerator>();

        userRepository
            .GetByEmailAsync(email, Arg.Any<CancellationToken>())
            .Returns((User?)null);

        var useCase = new AuthenticateUserUseCase(
            userRepository,
            passwordHasher,
            accessTokenGenerator);

        var command = new AuthenticateUserCommand(
            "maicon@email.com",
            "Senha@123");

        // Act
        var excecao = await Should.ThrowAsync<InvalidCredentialsException>(() => useCase.ExecuteAsync(command));

        // Assert
        excecao.Message.ShouldBe(ApplicationMessages.Authentication.InvalidCredentials);
        passwordHasher.DidNotReceive().Verify(Arg.Any<string?>(), Arg.Any<PasswordHash>());
        accessTokenGenerator.DidNotReceive().Generate(Arg.Any<User>());
    }

    [Fact]
    public async Task Deve_Lancar_Excecao_Quando_Senha_For_Invalida()
    {
        // Arrange
        var email = Email.Create("maicon@email.com");
        var passwordHash = PasswordHash.Create("$2a$11$hashfakeparatestes");
        var user = User.Create("Maicon Guedes", email, passwordHash);
        var userRepository = Substitute.For<IUserRepository>();
        var passwordHasher = Substitute.For<IPasswordHasher>();
        var accessTokenGenerator = Substitute.For<IAccessTokenGenerator>();

        userRepository
            .GetByEmailAsync(email, Arg.Any<CancellationToken>())
            .Returns(user);

        passwordHasher
            .Verify(Arg.Any<string?>(), passwordHash)
            .Returns(false);

        var useCase = new AuthenticateUserUseCase(
            userRepository,
            passwordHasher,
            accessTokenGenerator);

        var command = new AuthenticateUserCommand(
            "maicon@email.com",
            "Senha@123");

        // Act
        var excecao = await Should.ThrowAsync<InvalidCredentialsException>(() => useCase.ExecuteAsync(command));

        // Assert
        excecao.Message.ShouldBe(ApplicationMessages.Authentication.InvalidCredentials);
        passwordHasher.Received(1).Verify("Senha@123", passwordHash);
        accessTokenGenerator.DidNotReceive().Generate(Arg.Any<User>());
    }

    [Fact]
    public async Task Deve_Lancar_Excecao_Quando_Usuario_Estiver_Inativo()
    {
        // Arrange
        var email = Email.Create("maicon@email.com");
        var passwordHash = PasswordHash.Create("$2a$11$hashfakeparatestes");
        var user = User.Create("Maicon Guedes", email, passwordHash);
        var userRepository = Substitute.For<IUserRepository>();
        var passwordHasher = Substitute.For<IPasswordHasher>();
        var accessTokenGenerator = Substitute.For<IAccessTokenGenerator>();

        user.Deactivate(Guid.NewGuid());

        userRepository
            .GetByEmailAsync(email, Arg.Any<CancellationToken>())
            .Returns(user);

        passwordHasher
            .Verify(Arg.Any<string?>(), passwordHash)
            .Returns(true);

        var useCase = new AuthenticateUserUseCase(
            userRepository,
            passwordHasher,
            accessTokenGenerator);

        var command = new AuthenticateUserCommand(
            "maicon@email.com",
            "Senha@123");

        // Act
        var excecao = await Should.ThrowAsync<InactiveUserException>(() => useCase.ExecuteAsync(command));

        // Assert
        excecao.Message.ShouldBe(ApplicationMessages.Authentication.InactiveUser);
        passwordHasher.Received(1).Verify("Senha@123", passwordHash);
        accessTokenGenerator.DidNotReceive().Generate(Arg.Any<User>());
    }

    [Fact]
    public async Task Deve_Tentar_Verificar_Senha_Fraca_No_Login_Sem_Aplicar_Regra_De_Cadastro()
    {
        // Arrange
        var email = Email.Create("maicon@email.com");
        var passwordHash = PasswordHash.Create("$2a$11$hashfakeparatestes");
        var user = User.Create("Maicon Guedes", email, passwordHash);
        var userRepository = Substitute.For<IUserRepository>();
        var passwordHasher = Substitute.For<IPasswordHasher>();
        var accessTokenGenerator = Substitute.For<IAccessTokenGenerator>();

        userRepository
            .GetByEmailAsync(email, Arg.Any<CancellationToken>())
            .Returns(user);

        passwordHasher
            .Verify("123", passwordHash)
            .Returns(false);

        var useCase = new AuthenticateUserUseCase(
            userRepository,
            passwordHasher,
            accessTokenGenerator);

        var command = new AuthenticateUserCommand(
            "maicon@email.com",
            "123");

        // Act
        var excecao = await Should.ThrowAsync<InvalidCredentialsException>(() => useCase.ExecuteAsync(command));

        // Assert
        excecao.Message.ShouldBe(ApplicationMessages.Authentication.InvalidCredentials);
        passwordHasher.Received(1).Verify("123", passwordHash);
        accessTokenGenerator.DidNotReceive().Generate(Arg.Any<User>());
    }
}
