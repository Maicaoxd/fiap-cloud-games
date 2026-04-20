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
        Assert.NotEqual(Guid.Empty, result.UserId);
        Assert.NotNull(addedUser);
        Assert.Equal(result.UserId, addedUser.Id);
        Assert.Equal("Maicon Guedes", addedUser.Name);
        Assert.Equal(Email.Create("maicon@email.com"), addedUser.Email);
        Assert.Equal(passwordHash, addedUser.PasswordHash);
        Assert.Equal(UserRole.User, addedUser.Role);
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
        var excecao = await Assert.ThrowsAsync<ArgumentException>(() => useCase.ExecuteAsync(command));

        // Assert
        Assert.Equal("As senhas não conferem.", excecao.Message);
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
        var excecao = await Assert.ThrowsAsync<InvalidOperationException>(() => useCase.ExecuteAsync(command));

        // Assert
        Assert.Equal("E-mail já cadastrado.", excecao.Message);
        passwordHasher.DidNotReceive().Hash(Arg.Any<Password>());
        await userRepository.DidNotReceive().AddAsync(Arg.Any<User>(), Arg.Any<CancellationToken>());
    }
}
