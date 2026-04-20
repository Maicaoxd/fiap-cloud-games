using FCG.Domain.Users.ValueObjects;

namespace FCG.Tests.Domain.ValueObjects;

public sealed class PasswordTests
{
    [Fact]
    public void Deve_Criar_Senha_Quando_Valor_For_Forte()
    {
        // Arrange
        const string valor = "Senha@123";

        // Act
        var senha = Password.Create(valor);

        // Assert
        Assert.NotNull(senha);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    public void Deve_Lancar_Excecao_Quando_Senha_For_Obrigatoria_E_Nao_For_Informada(string? valor)
    {
        // Arrange
        Action acao = () => Password.Create(valor!);

        // Act
        var excecao = Assert.Throws<ArgumentException>(acao);

        // Assert
        Assert.Equal("Senha é obrigatória.", excecao.Message);
    }

    [Fact]
    public void Deve_Lancar_Excecao_Quando_Senha_Tiver_Menos_De_Oito_Caracteres()
    {
        // Arrange
        const string valor = "Senha@1";

        // Act
        Action acao = () => Password.Create(valor);

        // Assert
        var excecao = Assert.Throws<ArgumentException>(acao);
        Assert.Equal("Senha deve ter no mínimo 8 caracteres.", excecao.Message);
    }

    [Fact]
    public void Deve_Lancar_Excecao_Quando_Senha_Nao_Tiver_Letras()
    {
        // Arrange
        const string valor = "12345678@";

        // Act
        Action acao = () => Password.Create(valor);

        // Assert
        var excecao = Assert.Throws<ArgumentException>(acao);
        Assert.Equal("Senha deve conter pelo menos uma letra.", excecao.Message);
    }

    [Fact]
    public void Deve_Lancar_Excecao_Quando_Senha_Nao_Tiver_Numeros()
    {
        // Arrange
        const string valor = "Senha@@@";

        // Act
        Action acao = () => Password.Create(valor);

        // Assert
        var excecao = Assert.Throws<ArgumentException>(acao);
        Assert.Equal("Senha deve conter pelo menos um número.", excecao.Message);
    }

    [Fact]
    public void Deve_Lancar_Excecao_Quando_Senha_Nao_Tiver_Caractere_Especial()
    {
        // Arrange
        const string valor = "Senha123";

        // Act
        Action acao = () => Password.Create(valor);

        // Assert
        var excecao = Assert.Throws<ArgumentException>(acao);
        Assert.Equal("Senha deve conter pelo menos um caractere especial.", excecao.Message);
    }

    [Fact]
    public void Deve_Lancar_Excecao_Quando_Senha_Tiver_Espaco_Em_Branco()
    {
        // Arrange
        const string valor = "Senha 123";

        // Act
        Action acao = () => Password.Create(valor);

        // Assert
        var excecao = Assert.Throws<ArgumentException>(acao);
        Assert.Equal("Senha não deve conter espaços em branco.", excecao.Message);
    }
}
