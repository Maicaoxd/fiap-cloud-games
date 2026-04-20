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
        Assert.NotEqual(Guid.Empty, usuario.Id);
        Assert.Equal(nome, usuario.Name);
        Assert.Equal(email, usuario.Email);
        Assert.Equal(passwordHash, usuario.PasswordHash);
        Assert.Equal(UserRole.User, usuario.Role);
        Assert.True(usuario.IsActive);
        Assert.NotEqual(default, usuario.CreatedAt);
        Assert.Null(usuario.UpdatedAt);
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

    [Fact]
    public void Deve_Desativar_Usuario_Quando_Usuario_Estiver_Ativo()
    {
        // Arrange
        var email = Email.Create("maicon@email.com");
        var passwordHash = PasswordHash.Create("$2a$11$hashfakeparatestes");
        var usuario = User.Create("Maicon Guedes", email, passwordHash);

        // Act
        usuario.Deactivate();

        // Assert
        Assert.False(usuario.IsActive);
        Assert.NotNull(usuario.UpdatedAt);
    }
}
