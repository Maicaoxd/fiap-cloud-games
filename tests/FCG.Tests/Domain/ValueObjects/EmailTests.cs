using FCG.Domain.Users.ValueObjects;

namespace FCG.Tests.Domain.ValueObjects;

public sealed class EmailTests
{
    [Fact]
    public void Deve_Criar_Email_Quando_Valor_For_Valido()
    {
        // Arrange
        const string valor = "maicon@email.com";

        // Act
        var email = Email.Create(valor);

        // Assert
        Assert.Equal("maicon@email.com", email.Value);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    public void Deve_Lancar_Excecao_Quando_Email_For_Vazio(string? valor)
    {
        // Arrange
        Action acao = () => Email.Create(valor!);

        // Act
        var excecao = Assert.Throws<ArgumentException>(acao);

        // Assert
        Assert.Equal("E-mail é obrigatório.", excecao.Message);
    }

    [Theory]
    [InlineData("maicon")]
    [InlineData("maicon.email.com")]
    [InlineData("maiconemail.com")]
    [InlineData("@email.com")]
    [InlineData("maicon@")]
    [InlineData("maicon@@email.com")]
    [InlineData("maicon@email")]
    [InlineData("maicon @email.com")]
    public void Deve_Lancar_Excecao_Quando_Formato_Do_Email_For_Invalido(string valor)
    {
        // Arrange
        Action acao = () => Email.Create(valor);

        // Act
        var excecao = Assert.Throws<ArgumentException>(acao);

        // Assert
        Assert.Equal("E-mail deve estar em um formato válido.", excecao.Message);
    }

    [Fact]
    public void Deve_Normalizar_Email_Removendo_Espacos_E_Convertendo_Para_Minusculo()
    {
        // Arrange
        const string valor = "  MAICON@EMAIL.COM  ";

        // Act
        var email = Email.Create(valor);

        // Assert
        Assert.Equal("maicon@email.com", email.Value);
    }

    [Fact]
    public void Deve_Considerar_Emails_Iguais_Quando_Valores_Normalizados_Forem_Iguais()
    {
        // Arrange
        var primeiroEmail = Email.Create("  MAICON@EMAIL.COM  ");
        var segundoEmail = Email.Create("maicon@email.com");

        // Act
        var saoIguais = primeiroEmail.Equals(segundoEmail);

        // Assert
        Assert.True(saoIguais);
    }

    [Fact]
    public void Deve_Considerar_Emails_Diferentes_Quando_Valores_Forem_Diferentes()
    {
        // Arrange
        var primeiroEmail = Email.Create("maicon@email.com");
        var segundoEmail = Email.Create("outro@email.com");

        // Act
        var saoIguais = primeiroEmail.Equals(segundoEmail);

        // Assert
        Assert.False(saoIguais);
    }

    [Fact]
    public void Deve_Gerar_Mesmo_HashCode_Quando_Emails_Forem_Iguais()
    {
        // Arrange
        var primeiroEmail = Email.Create("  MAICON@EMAIL.COM  ");
        var segundoEmail = Email.Create("maicon@email.com");

        // Act
        var primeiroHashCode = primeiroEmail.GetHashCode();
        var segundoHashCode = segundoEmail.GetHashCode();

        // Assert
        Assert.Equal(primeiroHashCode, segundoHashCode);
    }
}
