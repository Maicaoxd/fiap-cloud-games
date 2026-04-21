using FCG.Api.Authentication;
using FCG.Api.Controllers;
using FCG.Application.Abstractions.Persistence;
using FCG.Application.Abstractions.Security;
using FCG.Application.Users.Authenticate;
using FCG.Domain.Users;
using FCG.Domain.Users.ValueObjects;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace FCG.Tests.Api.Controllers;

public sealed class AuthenticationControllerTests
{
    [Trait("Category", "Unit")]
    [Fact]
    public async Task LoginAsync_QuandoCredenciaisForemValidas_DeveRetornarOkComAccessToken()
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
            .Verify(Arg.Any<Password>(), passwordHash)
            .Returns(true);

        accessTokenGenerator
            .Generate(user)
            .Returns("access-token");

        var useCase = new AuthenticateUserUseCase(
            userRepository,
            passwordHasher,
            accessTokenGenerator);

        var controller = new AuthenticationController(useCase);
        var request = new LoginRequest(
            "maicon@email.com",
            "Senha@123");

        // Act
        var actionResult = await controller.LoginAsync(request, CancellationToken.None);

        // Assert
        var okResult = actionResult.Result.ShouldBeOfType<OkObjectResult>();
        var response = okResult.Value.ShouldBeOfType<LoginResponse>();

        response.AccessToken.ShouldBe("access-token");
        await userRepository.Received(1).GetByEmailAsync(email, Arg.Any<CancellationToken>());
        passwordHasher.Received(1).Verify(Arg.Any<Password>(), passwordHash);
        accessTokenGenerator.Received(1).Generate(user);
    }

    [Trait("Category", "Unit")]
    [Fact]
    public void LoginAsync_DeveDocumentarRespostasEsperadasNoSwagger()
    {
        // Arrange
        var method = typeof(AuthenticationController)
            .GetMethod(nameof(AuthenticationController.LoginAsync));

        // Act
        var responseTypes = method!
            .GetCustomAttributes(typeof(ProducesResponseTypeAttribute), inherit: false)
            .Cast<ProducesResponseTypeAttribute>()
            .ToDictionary(attribute => attribute.StatusCode);

        // Assert
        responseTypes[StatusCodes.Status200OK].Type.ShouldBe(typeof(LoginResponse));
        responseTypes[StatusCodes.Status400BadRequest].Type.ShouldBe(typeof(ValidationProblemDetails));
        responseTypes[StatusCodes.Status401Unauthorized].Type.ShouldBe(typeof(ProblemDetails));
        responseTypes[StatusCodes.Status403Forbidden].Type.ShouldBe(typeof(ProblemDetails));
        responseTypes[StatusCodes.Status500InternalServerError].Type.ShouldBe(typeof(ProblemDetails));
    }
}
