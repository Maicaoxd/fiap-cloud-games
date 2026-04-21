using System.IdentityModel.Tokens.Jwt;
using FCG.Domain.Users;
using FCG.Domain.Users.ValueObjects;
using FCG.Infrastructure.Security;

namespace FCG.Tests.Infrastructure.Security;

public sealed class JwtAccessTokenGeneratorTests
{
    [Trait("Category", "Unit")]
    [Fact]
    public void Generate_QuandoUsuarioForValido_DeveGerarAccessTokenComClaimsDoUsuario()
    {
        // Arrange
        var jwtOptions = new JwtOptions(
            "FiapCloudGames",
            "FiapCloudGames",
            "fiap-cloud-games-test-secret-key-32-chars-minimum",
            60);

        var email = Email.Create("maicon@email.com");
        var passwordHash = PasswordHash.Create("$2a$11$hashfakeparatestes");
        var user = User.Create("Maicon Guedes", email, passwordHash);
        var accessTokenGenerator = new JwtAccessTokenGenerator(jwtOptions);

        // Act
        var accessToken = accessTokenGenerator.Generate(user);

        // Assert
        accessToken.ShouldNotBeNullOrWhiteSpace();

        var token = new JwtSecurityTokenHandler().ReadJwtToken(accessToken);

        token.Issuer.ShouldBe(jwtOptions.Issuer);
        token.Audiences.ShouldContain(jwtOptions.Audience);
        token.Claims.Single(claim => claim.Type == JwtRegisteredClaimNames.Sub).Value.ShouldBe(user.Id.ToString());
        token.Claims.Single(claim => claim.Type == JwtRegisteredClaimNames.Email).Value.ShouldBe(user.Email.Value);
        token.Claims.Single(claim => claim.Type == "role").Value.ShouldBe(UserRole.User.ToString());
        token.ValidTo.ShouldBeGreaterThan(DateTime.UtcNow.AddMinutes(55));
        token.ValidTo.ShouldBeLessThan(DateTime.UtcNow.AddMinutes(65));
    }
}
