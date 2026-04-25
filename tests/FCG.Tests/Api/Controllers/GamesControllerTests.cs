using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using FCG.Api.Controllers;
using FCG.Api.Games;
using FCG.Application.Abstractions.Persistence;
using FCG.Application.Games.Create;
using FCG.Application.Games.Deactivate;
using FCG.Application.Games.Get;
using FCG.Application.Games.List;
using FCG.Application.Games.Update;
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
        var deactivateUseCase = new DeactivateGameUseCase(gameRepository);
        var getUseCase = new GetGameUseCase(gameRepository);
        var listUseCase = new ListGamesUseCase(gameRepository);
        var updateUseCase = new UpdateGameUseCase(gameRepository);
        var controller = new GamesController(useCase, deactivateUseCase, getUseCase, listUseCase, updateUseCase)
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
    public async Task ListAsync_QuandoUsuarioEstiverAutenticado_DeveRetornarOkComJogos()
    {
        // Arrange
        var firstGame = Game.Create(
            "Hades",
            "Roguelike de ação.",
            49.90m,
            Guid.NewGuid());
        var secondGame = Game.Create(
            "Stardew Valley",
            "Simulador de fazenda e vida no campo.",
            24.90m,
            Guid.NewGuid());

        var gameRepository = Substitute.For<IGameRepository>();
        gameRepository
            .ListActiveAsync(Arg.Any<CancellationToken>())
            .Returns(new[] { firstGame, secondGame });

        var createUseCase = new CreateGameUseCase(gameRepository);
        var deactivateUseCase = new DeactivateGameUseCase(gameRepository);
        var getUseCase = new GetGameUseCase(gameRepository);
        var listUseCase = new ListGamesUseCase(gameRepository);
        var updateUseCase = new UpdateGameUseCase(gameRepository);
        var controller = new GamesController(createUseCase, deactivateUseCase, getUseCase, listUseCase, updateUseCase)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = CreateHttpContext(Guid.NewGuid(), nameof(UserRole.User))
            }
        };

        // Act
        var actionResult = await controller.ListAsync(CancellationToken.None);

        // Assert
        var okResult = actionResult.Result.ShouldBeOfType<OkObjectResult>();
        var response = okResult.Value.ShouldBeOfType<List<ListGameResponse>>();

        response.Count.ShouldBe(2);
        response.ShouldContain(game => game.GameId == firstGame.Id);
        response.ShouldContain(game => game.GameId == secondGame.Id);
        await gameRepository.Received(1).ListActiveAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetByIdAsync_QuandoUsuarioAutenticadoEJogoExistir_DeveRetornarOkComJogo()
    {
        // Arrange
        var game = Game.Create(
            "Hades",
            "Roguelike de ação.",
            49.90m,
            Guid.NewGuid());
        var gameRepository = Substitute.For<IGameRepository>();

        gameRepository
            .GetByIdAsync(game.Id, Arg.Any<CancellationToken>())
            .Returns(game);

        var createUseCase = new CreateGameUseCase(gameRepository);
        var deactivateUseCase = new DeactivateGameUseCase(gameRepository);
        var getUseCase = new GetGameUseCase(gameRepository);
        var listUseCase = new ListGamesUseCase(gameRepository);
        var updateUseCase = new UpdateGameUseCase(gameRepository);
        var controller = new GamesController(createUseCase, deactivateUseCase, getUseCase, listUseCase, updateUseCase)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = CreateHttpContext(Guid.NewGuid(), nameof(UserRole.User))
            }
        };

        // Act
        var actionResult = await controller.GetByIdAsync(game.Id, CancellationToken.None);

        // Assert
        var okResult = actionResult.Result.ShouldBeOfType<OkObjectResult>();
        var response = okResult.Value.ShouldBeOfType<GetGameResponse>();

        response.GameId.ShouldBe(game.Id);
        response.Title.ShouldBe("Hades");
        response.Description.ShouldBe("Roguelike de ação.");
        response.Price.ShouldBe(49.90m);
        await gameRepository.Received(1).GetByIdAsync(game.Id, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateAsync_QuandoAdminAutenticadoEJogoExistir_DeveRetornarNoContent()
    {
        // Arrange
        var adminId = Guid.NewGuid();
        var game = Game.Create(
            "Stardew Valley",
            "Simulador de fazenda.",
            24.90m,
            Guid.NewGuid());
        var gameRepository = Substitute.For<IGameRepository>();

        gameRepository
            .GetByIdAsync(game.Id, Arg.Any<CancellationToken>())
            .Returns(game);
        gameRepository
            .ExistsByTitleForAnotherGameAsync(Arg.Any<string>(), game.Id, Arg.Any<CancellationToken>())
            .Returns(false);

        var createUseCase = new CreateGameUseCase(gameRepository);
        var deactivateUseCase = new DeactivateGameUseCase(gameRepository);
        var getUseCase = new GetGameUseCase(gameRepository);
        var listUseCase = new ListGamesUseCase(gameRepository);
        var updateUseCase = new UpdateGameUseCase(gameRepository);
        var controller = new GamesController(createUseCase, deactivateUseCase, getUseCase, listUseCase, updateUseCase)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = CreateHttpContext(adminId)
            }
        };
        var request = new UpdateGameRequest(
            "Stardew Valley Deluxe",
            "Simulador de fazenda com conteúdo extra.",
            39.90m);

        // Act
        var actionResult = await controller.UpdateAsync(game.Id, request, CancellationToken.None);

        // Assert
        actionResult.ShouldBeOfType<NoContentResult>();
        game.Title.ShouldBe("Stardew Valley Deluxe");
        game.Description.ShouldBe("Simulador de fazenda com conteúdo extra.");
        game.Price.ShouldBe(39.90m);
        game.UpdatedBy.ShouldBe(adminId);
        await gameRepository.Received(1).UpdateAsync(game, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeactivateAsync_QuandoAdminAutenticadoEJogoExistir_DeveRetornarNoContent()
    {
        // Arrange
        var adminId = Guid.NewGuid();
        var game = Game.Create(
            "Stardew Valley",
            "Simulador de fazenda.",
            24.90m,
            Guid.NewGuid());
        var gameRepository = Substitute.For<IGameRepository>();

        gameRepository
            .GetByIdAsync(game.Id, Arg.Any<CancellationToken>())
            .Returns(game);

        var createUseCase = new CreateGameUseCase(gameRepository);
        var deactivateUseCase = new DeactivateGameUseCase(gameRepository);
        var getUseCase = new GetGameUseCase(gameRepository);
        var listUseCase = new ListGamesUseCase(gameRepository);
        var updateUseCase = new UpdateGameUseCase(gameRepository);
        var controller = new GamesController(createUseCase, deactivateUseCase, getUseCase, listUseCase, updateUseCase)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = CreateHttpContext(adminId)
            }
        };

        // Act
        var actionResult = await controller.DeactivateAsync(game.Id, CancellationToken.None);

        // Assert
        actionResult.ShouldBeOfType<NoContentResult>();
        game.IsActive.ShouldBeFalse();
        game.UpdatedBy.ShouldBe(adminId);
        await gameRepository.Received(1).UpdateAsync(game, Arg.Any<CancellationToken>());
    }

    [Fact]
    public void ListAsync_DeveExigirUsuarioAutenticado()
    {
        // Arrange
        var method = typeof(GamesController)
            .GetMethod(nameof(GamesController.ListAsync));

        // Act
        var authorizeAttribute = method!
            .GetCustomAttributes(typeof(AuthorizeAttribute), inherit: false)
            .Cast<AuthorizeAttribute>()
            .Single();

        // Assert
        authorizeAttribute.Roles.ShouldBeNull();
    }

    [Fact]
    public void GetByIdAsync_DeveExigirUsuarioAutenticado()
    {
        // Arrange
        var method = typeof(GamesController)
            .GetMethod(nameof(GamesController.GetByIdAsync));

        // Act
        var authorizeAttribute = method!
            .GetCustomAttributes(typeof(AuthorizeAttribute), inherit: false)
            .Cast<AuthorizeAttribute>()
            .Single();

        // Assert
        authorizeAttribute.Roles.ShouldBeNull();
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
    public void UpdateAsync_DeveExigirRoleAdministrator()
    {
        // Arrange
        var method = typeof(GamesController)
            .GetMethod(nameof(GamesController.UpdateAsync));

        // Act
        var authorizeAttribute = method!
            .GetCustomAttributes(typeof(AuthorizeAttribute), inherit: false)
            .Cast<AuthorizeAttribute>()
            .Single();

        // Assert
        authorizeAttribute.Roles.ShouldBe(nameof(UserRole.Administrator));
    }

    [Fact]
    public void DeactivateAsync_DeveExigirRoleAdministrator()
    {
        // Arrange
        var method = typeof(GamesController)
            .GetMethod(nameof(GamesController.DeactivateAsync));

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

    [Fact]
    public void ListAsync_DeveDocumentarRespostasEsperadasNoSwagger()
    {
        // Arrange
        var method = typeof(GamesController)
            .GetMethod(nameof(GamesController.ListAsync));

        // Act
        var responseTypes = method!
            .GetCustomAttributes(typeof(ProducesResponseTypeAttribute), inherit: false)
            .Cast<ProducesResponseTypeAttribute>()
            .ToDictionary(attribute => attribute.StatusCode);

        // Assert
        responseTypes[StatusCodes.Status200OK].Type.ShouldBe(typeof(IReadOnlyCollection<ListGameResponse>));
        responseTypes[StatusCodes.Status401Unauthorized].Type.ShouldBe(typeof(ProblemDetails));
        responseTypes[StatusCodes.Status500InternalServerError].Type.ShouldBe(typeof(ProblemDetails));
    }

    [Fact]
    public void GetByIdAsync_DeveDocumentarRespostasEsperadasNoSwagger()
    {
        // Arrange
        var method = typeof(GamesController)
            .GetMethod(nameof(GamesController.GetByIdAsync));

        // Act
        var responseTypes = method!
            .GetCustomAttributes(typeof(ProducesResponseTypeAttribute), inherit: false)
            .Cast<ProducesResponseTypeAttribute>()
            .ToDictionary(attribute => attribute.StatusCode);

        // Assert
        responseTypes[StatusCodes.Status200OK].Type.ShouldBe(typeof(GetGameResponse));
        responseTypes[StatusCodes.Status401Unauthorized].Type.ShouldBe(typeof(ProblemDetails));
        responseTypes[StatusCodes.Status404NotFound].Type.ShouldBe(typeof(ProblemDetails));
        responseTypes[StatusCodes.Status500InternalServerError].Type.ShouldBe(typeof(ProblemDetails));
    }

    [Fact]
    public void UpdateAsync_DeveDocumentarRespostasEsperadasNoSwagger()
    {
        // Arrange
        var method = typeof(GamesController)
            .GetMethod(nameof(GamesController.UpdateAsync));

        // Act
        var responseTypes = method!
            .GetCustomAttributes(typeof(ProducesResponseTypeAttribute), inherit: false)
            .Cast<ProducesResponseTypeAttribute>()
            .ToDictionary(attribute => attribute.StatusCode);

        // Assert
        responseTypes[StatusCodes.Status204NoContent].Type.ShouldBe(typeof(void));
        responseTypes[StatusCodes.Status400BadRequest].Type.ShouldBe(typeof(ValidationProblemDetails));
        responseTypes[StatusCodes.Status401Unauthorized].Type.ShouldBe(typeof(ProblemDetails));
        responseTypes[StatusCodes.Status403Forbidden].Type.ShouldBe(typeof(ProblemDetails));
        responseTypes[StatusCodes.Status404NotFound].Type.ShouldBe(typeof(ProblemDetails));
        responseTypes[StatusCodes.Status409Conflict].Type.ShouldBe(typeof(ProblemDetails));
        responseTypes[StatusCodes.Status500InternalServerError].Type.ShouldBe(typeof(ProblemDetails));
    }

    [Fact]
    public void DeactivateAsync_DeveDocumentarRespostasEsperadasNoSwagger()
    {
        // Arrange
        var method = typeof(GamesController)
            .GetMethod(nameof(GamesController.DeactivateAsync));

        // Act
        var responseTypes = method!
            .GetCustomAttributes(typeof(ProducesResponseTypeAttribute), inherit: false)
            .Cast<ProducesResponseTypeAttribute>()
            .ToDictionary(attribute => attribute.StatusCode);

        // Assert
        responseTypes[StatusCodes.Status204NoContent].Type.ShouldBe(typeof(void));
        responseTypes[StatusCodes.Status401Unauthorized].Type.ShouldBe(typeof(ProblemDetails));
        responseTypes[StatusCodes.Status403Forbidden].Type.ShouldBe(typeof(ProblemDetails));
        responseTypes[StatusCodes.Status404NotFound].Type.ShouldBe(typeof(ProblemDetails));
        responseTypes[StatusCodes.Status500InternalServerError].Type.ShouldBe(typeof(ProblemDetails));
    }

    private static DefaultHttpContext CreateHttpContext(Guid userId, string role = nameof(UserRole.Administrator))
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
}
