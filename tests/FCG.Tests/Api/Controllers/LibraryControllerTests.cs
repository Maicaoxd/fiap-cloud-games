using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using FCG.Api.Controllers;
using FCG.Api.Libraries;
using FCG.Application.Abstractions.Persistence;
using FCG.Application.Libraries.Acquire;
using FCG.Application.Libraries.List;
using FCG.Domain.Users;
using FCG.Infrastructure.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace FCG.Tests.Api.Controllers;

[Trait("Category", "Unit")]
public sealed class LibraryControllerTests
{
    [Fact]
    public async Task ListAsync_QuandoUsuarioAutenticado_DeveRetornarOkComJogosDaBiblioteca()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var acquiredAt = DateTime.UtcNow;
        var userRepository = Substitute.For<IUserRepository>();
        var gameRepository = Substitute.For<IGameRepository>();
        var libraryRepository = Substitute.For<ILibraryRepository>();

        userRepository
            .GetByIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns(CreateUser(userId));

        libraryRepository
            .ListGamesByUserIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns(new[]
            {
                new LibraryGameReadModel(
                    Guid.NewGuid(),
                    Guid.NewGuid(),
                    "Hades",
                    "Roguelike de ação.",
                    49.90m,
                    true,
                    acquiredAt),
                new LibraryGameReadModel(
                    Guid.NewGuid(),
                    Guid.NewGuid(),
                    "Half-Life",
                    "FPS clássico.",
                    29.90m,
                    false,
                    acquiredAt.AddMinutes(-10))
            });

        var acquireUseCase = new AcquireGameUseCase(userRepository, gameRepository, libraryRepository);
        var listUseCase = new ListLibraryGamesUseCase(userRepository, libraryRepository);
        var controller = new LibraryController(acquireUseCase, listUseCase)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = CreateHttpContext(userId, nameof(UserRole.User))
            }
        };

        // Act
        var actionResult = await controller.ListAsync(CancellationToken.None);

        // Assert
        var okResult = actionResult.Result.ShouldBeOfType<OkObjectResult>();
        var response = okResult.Value.ShouldBeOfType<List<ListLibraryGameResponse>>();

        response.Count.ShouldBe(2);
        response[0].Title.ShouldBe("Hades");
        response[0].IsActive.ShouldBeTrue();
        response[1].Title.ShouldBe("Half-Life");
        response[1].IsActive.ShouldBeFalse();
        await libraryRepository.Received(1).ListGamesByUserIdAsync(userId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public void ListAsync_DeveExigirUsuarioAutenticado()
    {
        // Arrange
        var method = typeof(LibraryController)
            .GetMethod(nameof(LibraryController.ListAsync));

        // Act
        var authorizeAttribute = method!
            .GetCustomAttributes(typeof(AuthorizeAttribute), inherit: false)
            .Cast<AuthorizeAttribute>()
            .Single();

        // Assert
        authorizeAttribute.Roles.ShouldBeNull();
    }

    [Fact]
    public void ListAsync_DeveDocumentarRespostasEsperadasNoSwagger()
    {
        // Arrange
        var method = typeof(LibraryController)
            .GetMethod(nameof(LibraryController.ListAsync));

        // Act
        var responseTypes = method!
            .GetCustomAttributes(typeof(ProducesResponseTypeAttribute), inherit: false)
            .Cast<ProducesResponseTypeAttribute>()
            .ToDictionary(attribute => attribute.StatusCode);

        // Assert
        responseTypes[StatusCodes.Status200OK].Type.ShouldBe(typeof(IReadOnlyCollection<ListLibraryGameResponse>));
        responseTypes[StatusCodes.Status401Unauthorized].Type.ShouldBe(typeof(ProblemDetails));
        responseTypes[StatusCodes.Status403Forbidden].Type.ShouldBe(typeof(ProblemDetails));
        responseTypes[StatusCodes.Status500InternalServerError].Type.ShouldBe(typeof(ProblemDetails));
    }

    private static DefaultHttpContext CreateHttpContext(Guid userId, string role)
    {
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new Claim(JwtClaimNames.Role, role)
        };

        return new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity(claims, "Test"))
        };
    }

    private static FCG.Domain.Users.User CreateUser(Guid userId)
    {
        var email = FCG.Domain.Users.ValueObjects.Email.Create("maicon@email.com");
        var passwordHash = FCG.Domain.Users.ValueObjects.PasswordHash.Create("$2a$11$hashfakeparatestes");

        return FCG.Domain.Users.User.Create("Maicon Guedes", email, passwordHash, userId);
    }
}
