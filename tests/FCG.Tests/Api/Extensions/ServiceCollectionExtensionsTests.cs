using System.IdentityModel.Tokens.Jwt;
using FCG.Api.Extensions;
using FCG.Infrastructure.Security;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace FCG.Tests.Api.Extensions;

public sealed class ServiceCollectionExtensionsTests
{
    [Trait("Category", "Unit")]
    [Fact]
    public async Task AddApiPresentation_DeveConfigurarJwtBearerComoEsquemaPadrao()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = CreateConfiguration();

        // Act
        services.AddApiPresentation(configuration);

        // Assert
        await using var serviceProvider = services.BuildServiceProvider();
        var schemeProvider = serviceProvider.GetRequiredService<IAuthenticationSchemeProvider>();
        var authenticateScheme = await schemeProvider.GetDefaultAuthenticateSchemeAsync();
        var challengeScheme = await schemeProvider.GetDefaultChallengeSchemeAsync();

        authenticateScheme.ShouldNotBeNull();
        challengeScheme.ShouldNotBeNull();
        authenticateScheme!.Name.ShouldBe(JwtBearerDefaults.AuthenticationScheme);
        challengeScheme!.Name.ShouldBe(JwtBearerDefaults.AuthenticationScheme);
    }

    [Trait("Category", "Unit")]
    [Fact]
    public void AddApiPresentation_DeveConfigurarValidacaoJwtParaRoles()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = CreateConfiguration();

        // Act
        services.AddApiPresentation(configuration);

        // Assert
        using var serviceProvider = services.BuildServiceProvider();
        var jwtBearerOptions = serviceProvider
            .GetRequiredService<IOptionsMonitor<JwtBearerOptions>>()
            .Get(JwtBearerDefaults.AuthenticationScheme);

        jwtBearerOptions.MapInboundClaims.ShouldBeFalse();
        jwtBearerOptions.TokenValidationParameters.ValidateIssuer.ShouldBeTrue();
        jwtBearerOptions.TokenValidationParameters.ValidateAudience.ShouldBeTrue();
        jwtBearerOptions.TokenValidationParameters.ValidateIssuerSigningKey.ShouldBeTrue();
        jwtBearerOptions.TokenValidationParameters.ValidateLifetime.ShouldBeTrue();
        jwtBearerOptions.TokenValidationParameters.NameClaimType.ShouldBe(JwtRegisteredClaimNames.Sub);
        jwtBearerOptions.TokenValidationParameters.RoleClaimType.ShouldBe(JwtClaimNames.Role);
        jwtBearerOptions.TokenValidationParameters.ClockSkew.ShouldBe(TimeSpan.Zero);
    }

    private static IConfiguration CreateConfiguration()
    {
        return new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Issuer"] = "FiapCloudGames",
                ["Jwt:Audience"] = "FiapCloudGames",
                ["Jwt:Secret"] = "maicon-guedes-dotnet-architect-level-99-cloud-games-key",
                ["Jwt:ExpirationMinutes"] = "60"
            })
            .Build();
    }
}
