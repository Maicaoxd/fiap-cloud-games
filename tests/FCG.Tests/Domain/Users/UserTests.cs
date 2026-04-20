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
        Assert.Equal(usuario.Id, usuario.CreatedBy);
        Assert.Null(usuario.UpdatedAt);
        Assert.Null(usuario.UpdatedBy);
    }

    [Fact]
    public void Deve_Criar_Usuario_Com_Auditoria_Quando_Criado_Por_Outro_Usuario()
    {
        // Arrange
        var criadoPor = Guid.NewGuid();
        var email = Email.Create("maicon@email.com");
        var passwordHash = PasswordHash.Create("$2a$11$hashfakeparatestes");

        // Act
        var usuario = User.Create("Maicon Guedes", email, passwordHash, criadoPor);

        // Assert
        Assert.Equal(criadoPor, usuario.CreatedBy);
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
        Action acao = () => User.Create("Maicon Guedes", null!, passwordHash);

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
        var desativadoPor = Guid.NewGuid();
        var email = Email.Create("maicon@email.com");
        var passwordHash = PasswordHash.Create("$2a$11$hashfakeparatestes");
        var usuario = User.Create("Maicon Guedes", email, passwordHash);

        // Act
        usuario.Deactivate(desativadoPor);

        // Assert
        Assert.False(usuario.IsActive);
        Assert.NotNull(usuario.UpdatedAt);
        Assert.Equal(desativadoPor, usuario.UpdatedBy);
    }

    [Fact]
    public void Deve_Reativar_Usuario_Quando_Usuario_Estiver_Inativo()
    {
        // Arrange
        var desativadoPor = Guid.NewGuid();
        var ativadoPor = Guid.NewGuid();
        var email = Email.Create("maicon@email.com");
        var passwordHash = PasswordHash.Create("$2a$11$hashfakeparatestes");
        var usuario = User.Create("Maicon Guedes", email, passwordHash);
        usuario.Deactivate(desativadoPor);

        // Act
        usuario.Activate(ativadoPor);

        // Assert
        Assert.True(usuario.IsActive);
        Assert.NotNull(usuario.UpdatedAt);
        Assert.Equal(ativadoPor, usuario.UpdatedBy);
    }

    [Fact]
    public void Deve_Lancar_Excecao_Quando_Desativar_Usuario_Sem_Responsavel_Valido()
    {
        // Arrange
        var email = Email.Create("maicon@email.com");
        var passwordHash = PasswordHash.Create("$2a$11$hashfakeparatestes");
        var usuario = User.Create("Maicon Guedes", email, passwordHash);
        Action acao = () => usuario.Deactivate(Guid.Empty);

        // Act
        var excecao = Assert.Throws<ArgumentException>(acao);

        // Assert
        Assert.Equal("Responsável pela alteração é obrigatório.", excecao.Message);
    }

    [Fact]
    public void Deve_Lancar_Excecao_Quando_Reativar_Usuario_Sem_Responsavel_Valido()
    {
        // Arrange
        var desativadoPor = Guid.NewGuid();
        var email = Email.Create("maicon@email.com");
        var passwordHash = PasswordHash.Create("$2a$11$hashfakeparatestes");
        var usuario = User.Create("Maicon Guedes", email, passwordHash);
        usuario.Deactivate(desativadoPor);
        Action acao = () => usuario.Activate(Guid.Empty);

        // Act
        var excecao = Assert.Throws<ArgumentException>(acao);

        // Assert
        Assert.Equal("Responsável pela alteração é obrigatório.", excecao.Message);
    }

    [Fact]
    public void Deve_Alterar_Nome_Quando_Nome_For_Valido()
    {
        // Arrange
        var atualizadoPor = Guid.NewGuid();
        var email = Email.Create("maicon@email.com");
        var passwordHash = PasswordHash.Create("$2a$11$hashfakeparatestes");
        var usuario = User.Create("Maicon Alves", email, passwordHash);

        // Act
        usuario.ChangeName("Maicon Guedes", atualizadoPor);

        // Assert
        Assert.Equal("Maicon Guedes", usuario.Name);
        Assert.NotNull(usuario.UpdatedAt);
        Assert.Equal(atualizadoPor, usuario.UpdatedBy);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    public void Deve_Lancar_Excecao_Quando_Alterar_Nome_Para_Valor_Obrigatorio_E_Nao_Informado(string? nome)
    {
        // Arrange
        var atualizadoPor = Guid.NewGuid();
        var email = Email.Create("maicon@email.com");
        var passwordHash = PasswordHash.Create("$2a$11$hashfakeparatestes");
        var usuario = User.Create("Maicon Guedes", email, passwordHash);
        Action acao = () => usuario.ChangeName(nome!, atualizadoPor);

        // Act
        var excecao = Assert.Throws<ArgumentException>(acao);

        // Assert
        Assert.Equal("Nome é obrigatório.", excecao.Message);
    }

    [Fact]
    public void Deve_Lancar_Excecao_Quando_Alterar_Nome_Sem_Responsavel_Valido()
    {
        // Arrange
        var email = Email.Create("maicon@email.com");
        var passwordHash = PasswordHash.Create("$2a$11$hashfakeparatestes");
        var usuario = User.Create("Maicon Guedes", email, passwordHash);
        Action acao = () => usuario.ChangeName("Maicon Guedes", Guid.Empty);

        // Act
        var excecao = Assert.Throws<ArgumentException>(acao);

        // Assert
        Assert.Equal("Responsável pela alteração é obrigatório.", excecao.Message);
    }

    [Fact]
    public void Deve_Alterar_Email_Quando_Email_For_Valido()
    {
        // Arrange
        var atualizadoPor = Guid.NewGuid();
        var email = Email.Create("maicon@email.com");
        var novoEmail = Email.Create("novo@email.com");
        var passwordHash = PasswordHash.Create("$2a$11$hashfakeparatestes");
        var usuario = User.Create("Maicon Guedes", email, passwordHash);

        // Act
        usuario.ChangeEmail(novoEmail, atualizadoPor);

        // Assert
        Assert.Equal(novoEmail, usuario.Email);
        Assert.NotNull(usuario.UpdatedAt);
        Assert.Equal(atualizadoPor, usuario.UpdatedBy);
    }

    [Fact]
    public void Deve_Lancar_Excecao_Quando_Alterar_Email_Para_Valor_Obrigatorio_E_Nao_Informado()
    {
        // Arrange
        var atualizadoPor = Guid.NewGuid();
        var email = Email.Create("maicon@email.com");
        var passwordHash = PasswordHash.Create("$2a$11$hashfakeparatestes");
        var usuario = User.Create("Maicon Guedes", email, passwordHash);
        Action acao = () => usuario.ChangeEmail(null!, atualizadoPor);

        // Act
        var excecao = Assert.Throws<ArgumentException>(acao);

        // Assert
        Assert.Equal("E-mail é obrigatório.", excecao.Message);
    }

    [Fact]
    public void Deve_Lancar_Excecao_Quando_Alterar_Email_Sem_Responsavel_Valido()
    {
        // Arrange
        var email = Email.Create("maicon@email.com");
        var novoEmail = Email.Create("novo@email.com");
        var passwordHash = PasswordHash.Create("$2a$11$hashfakeparatestes");
        var usuario = User.Create("Maicon Guedes", email, passwordHash);
        Action acao = () => usuario.ChangeEmail(novoEmail, Guid.Empty);

        // Act
        var excecao = Assert.Throws<ArgumentException>(acao);

        // Assert
        Assert.Equal("Responsável pela alteração é obrigatório.", excecao.Message);
    }

    [Fact]
    public void Deve_Alterar_Senha_Quando_PasswordHash_For_Valido()
    {
        // Arrange
        var atualizadoPor = Guid.NewGuid();
        var email = Email.Create("maicon@email.com");
        var passwordHash = PasswordHash.Create("$2a$11$hashfakeparatestes");
        var novoPasswordHash = PasswordHash.Create("$2a$11$novohashfakeparatestes");
        var usuario = User.Create("Maicon Guedes", email, passwordHash);

        // Act
        usuario.ChangePassword(novoPasswordHash, atualizadoPor);

        // Assert
        Assert.Equal(novoPasswordHash, usuario.PasswordHash);
        Assert.NotNull(usuario.UpdatedAt);
        Assert.Equal(atualizadoPor, usuario.UpdatedBy);
    }

    [Fact]
    public void Deve_Lancar_Excecao_Quando_Alterar_Senha_Para_PasswordHash_Obrigatorio_E_Nao_Informado()
    {
        // Arrange
        var atualizadoPor = Guid.NewGuid();
        var email = Email.Create("maicon@email.com");
        var passwordHash = PasswordHash.Create("$2a$11$hashfakeparatestes");
        var usuario = User.Create("Maicon Guedes", email, passwordHash);
        Action acao = () => usuario.ChangePassword(null!, atualizadoPor);

        // Act
        var excecao = Assert.Throws<ArgumentException>(acao);

        // Assert
        Assert.Equal("Hash da senha é obrigatório.", excecao.Message);
    }

    [Fact]
    public void Deve_Lancar_Excecao_Quando_Alterar_Senha_Sem_Responsavel_Valido()
    {
        // Arrange
        var email = Email.Create("maicon@email.com");
        var passwordHash = PasswordHash.Create("$2a$11$hashfakeparatestes");
        var novoPasswordHash = PasswordHash.Create("$2a$11$novohashfakeparatestes");
        var usuario = User.Create("Maicon Guedes", email, passwordHash);
        Action acao = () => usuario.ChangePassword(novoPasswordHash, Guid.Empty);

        // Act
        var excecao = Assert.Throws<ArgumentException>(acao);

        // Assert
        Assert.Equal("Responsável pela alteração é obrigatório.", excecao.Message);
    }
}
