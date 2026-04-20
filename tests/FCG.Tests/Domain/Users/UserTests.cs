using FCG.Domain.Users;
using FCG.Domain.Users.ValueObjects;

namespace FCG.Tests.Domain.Users;

public sealed class UserTests
{
    [Fact]
    public void Deve_Criar_Usuario_Quando_Dados_Forem_Validos()
    {
        // Arrange
        const string nome = "Maicon Guedes";
        var email = Email.Create("maicon@email.com");
        var passwordHash = PasswordHash.Create("$2a$11$hashfakeparatestes");

        // Act
        var usuario = User.Create(nome, email, passwordHash);

        // Assert
        Assert.Equal(nome, usuario.Name);
        Assert.Equal(email, usuario.Email);
        Assert.Equal(passwordHash, usuario.PasswordHash);
        Assert.Equal(UserRole.User, usuario.Role);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    public void Deve_Lancar_Excecao_Quando_Nome_For_Obrigatorio_E_Nao_For_Informado(string? nome)
    {
        // Arrange
        var email = Email.Create("maicon@email.com");
        var passwordHash = PasswordHash.Create("$2a$11$hashfakeparatestes");
        Action acao = () => User.Create(nome!, email, passwordHash);

        // Act
        var excecao = Assert.Throws<ArgumentException>(acao);

        // Assert
        Assert.Equal("Nome é obrigatório.", excecao.Message);
    }

    [Fact]
    public void Deve_Lancar_Excecao_Quando_Email_For_Obrigatorio_E_Nao_For_Informado()
    {
        // Arrange
        var passwordHash = PasswordHash.Create("$2a$11$hashfakeparatestes");
        Action acao = () => User.Create("Maicon Silva", null!, passwordHash);

        // Act
        var excecao = Assert.Throws<ArgumentException>(acao);

        // Assert
        Assert.Equal("E-mail é obrigatório.", excecao.Message);
    }

    [Fact]
    public void Deve_Lancar_Excecao_Quando_PasswordHash_For_Obrigatorio_E_Nao_For_Informado()
    {
        // Arrange
        var email = Email.Create("maicon@email.com");
        Action acao = () => User.Create("Maicon Guedes", email, null!);

        // Act
        var excecao = Assert.Throws<ArgumentException>(acao);

        // Assert
        Assert.Equal("Hash da senha é obrigatório.", excecao.Message);
    }
}
