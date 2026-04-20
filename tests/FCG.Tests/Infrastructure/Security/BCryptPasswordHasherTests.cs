using FCG.Domain.Users.ValueObjects;
using FCG.Infrastructure.Security;

namespace FCG.Tests.Infrastructure.Security;

public sealed class BCryptPasswordHasherTests
{
    [Fact]
    public void Deve_Gerar_PasswordHash_Quando_Senha_For_Valida()
    {
        // Arrange
        var password = Password.Create("Senha@123");
        var passwordHasher = new BCryptPasswordHasher();

        // Act
        var passwordHash = passwordHasher.Hash(password);

        // Assert
        string.IsNullOrWhiteSpace(passwordHash.Value).ShouldBeFalse();
        passwordHash.Value.ShouldNotBe(password.Value);
    }

    [Fact]
    public void Deve_Verificar_Senha_Quando_Valor_Original_For_Correto()
    {
        // Arrange
        var password = Password.Create("Senha@123");
        var passwordHasher = new BCryptPasswordHasher();
        var passwordHash = passwordHasher.Hash(password);

        // Act
        var senhaCorreta = passwordHasher.Verify(password, passwordHash);

        // Assert
        senhaCorreta.ShouldBeTrue();
    }

    [Fact]
    public void Deve_Nao_Verificar_Senha_Quando_Valor_Original_For_Incorreto()
    {
        // Arrange
        var password = Password.Create("Senha@123");
        var wrongPassword = Password.Create("Outra@123");
        var passwordHasher = new BCryptPasswordHasher();
        var passwordHash = passwordHasher.Hash(password);

        // Act
        var senhaCorreta = passwordHasher.Verify(wrongPassword, passwordHash);

        // Assert
        senhaCorreta.ShouldBeFalse();
    }
}
