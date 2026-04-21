using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using FCG.Api.Controllers;
using FCG.Api.Games;
using FCG.Application.Abstractions.Persistence;
using FCG.Application.Games.Create;
using FCG.Domain.Games;
using FCG.Domain.Users;
using FCG.Infrastructure.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace FCG.Tests.Api.Controllers;

[Trait("Category", "Unit")]
public sealed class GamesControllerTests
{
    [Fact]
    public async Task CreateAsync_QuandoAdminAutenticadoEJogoValido_DeveRetornarCreatedComGameId()
    {
        // Arrange
        var adminId = Guid.NewGuid();
        var gameRepository = Substitute.For<IGameRepository>();
        Game? addedGame = null;

        gameRepository
            .ExistsByTitleAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(false);

        gameRepository
            .AddAsync(Arg.Any<Game>(), Arg.Any<CancellationToken>())
            .Returns(callInfo =>
            {
                addedGame = callInfo.ArgAt<Game>(0);

                return Task.CompletedTask;
            });

        var useCase = new CreateGameUseCase(gameRepository);
        var controller = new GamesController(useCase)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = CreateHttpContext(adminId)
            }
        };

        var request = new CreateGameRequest(
            "Stardew Valley",
            "Simulador de fazenda e vida no campo.",
            24.90m);

        // Act
        var actionResult = await controller.CreateAsync(request, CancellationToken.None);

        // Assert
        var createdResult = actionResult.Result.ShouldBeOfType<CreatedResult>();
        var response = createdResult.Value.ShouldBeOfType<CreateGameResponse>();

        response.GameId.ShouldNotBe(Guid.Empty);
        createdResult.Location.ShouldBe($"/api/games/{response.GameId}");
        addedGame.ShouldNotBeNull();
        addedGame!.CreatedBy.ShouldBe(adminId);
        addedGame.Title.ShouldBe("Stardew Valley");
        addedGame.Description.ShouldBe("Simulador de fazenda e vida no campo.");
        addedGame.Price.ShouldBe(24.90m);
        await gameRepository.Received(1).AddAsync(Arg.Any<Game>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public void CreateAsync_DeveExigirRoleAdministrator()
    {
        // Arrange
        var method = typeof(GamesController)
            .GetMethod(nameof(GamesController.CreateAsync));

        // Act
        var authorizeAttribute = method!
            .GetCustomAttributes(typeof(AuthorizeAttribute), inherit: false)
            .Cast<AuthorizeAttribute>()
            .Single();

        // Assert
        authorizeAttribute.Roles.ShouldBe(nameof(UserRole.Administrator));
    }

    [Fact]
    public void CreateAsync_DeveDocumentarRespostasEsperadasNoSwagger()
    {
        // Arrange
        var method = typeof(GamesController)
            .GetMethod(nameof(GamesController.CreateAsync));

        // Act
        var responseTypes = method!
            .GetCustomAttributes(typeof(ProducesResponseTypeAttribute), inherit: false)
            .Cast<ProducesResponseTypeAttribute>()
            .ToDictionary(attribute => attribute.StatusCode);

        // Assert
        responseTypes[StatusCodes.Status201Created].Type.ShouldBe(typeof(CreateGameResponse));
        responseTypes[StatusCodes.Status400BadRequest].Type.ShouldBe(typeof(ValidationProblemDetails));
        responseTypes[StatusCodes.Status401Unauthorized].Type.ShouldBe(typeof(ProblemDetails));
        responseTypes[StatusCodes.Status403Forbidden].Type.ShouldBe(typeof(ProblemDetails));
        responseTypes[StatusCodes.Status409Conflict].Type.ShouldBe(typeof(ProblemDetails));
        responseTypes[StatusCodes.Status500InternalServerError].Type.ShouldBe(typeof(ProblemDetails));
    }

    private static DefaultHttpContext CreateHttpContext(Guid userId)
    {
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new Claim(JwtClaimNames.Role, nameof(UserRole.Administrator))
        };

        return new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity(claims, "Test"))
        };
    }
}
