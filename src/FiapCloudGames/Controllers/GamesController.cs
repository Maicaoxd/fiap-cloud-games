using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using FCG.Api.Games;
using FCG.Application.Common.Exceptions;
using FCG.Application.Games.Create;
using FCG.Application.Games.List;
using FCG.Domain.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FCG.Api.Controllers
{
    [ApiController]
    [Route("api/games")]
    public sealed class GamesController : ControllerBase
    {
        private readonly CreateGameUseCase _createGameUseCase;
        private readonly ListGamesUseCase _listGamesUseCase;

        public GamesController(
            CreateGameUseCase createGameUseCase,
            ListGamesUseCase listGamesUseCase)
        {
            _createGameUseCase = createGameUseCase;
            _listGamesUseCase = listGamesUseCase;
        }

        [HttpGet]
        [Authorize]
        [ProducesResponseType(typeof(IReadOnlyCollection<ListGameResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IReadOnlyCollection<ListGameResponse>>> ListAsync(
            CancellationToken cancellationToken)
        {
            var result = await _listGamesUseCase.ExecuteAsync(cancellationToken);
            var response = result
                .Select(game => new ListGameResponse(
                    game.GameId,
                    game.Title,
                    game.Description,
                    game.Price))
                .ToList();

            return Ok(response);
        }

        [HttpPost]
        [Authorize(Roles = nameof(UserRole.Administrator))]
        [ProducesResponseType(typeof(CreateGameResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<CreateGameResponse>> CreateAsync(
            CreateGameRequest request,
            CancellationToken cancellationToken)
        {
            var createdBy = GetAuthenticatedUserId(User);

            var command = new CreateGameCommand(
                request.Title,
                request.Description,
                request.Price,
                createdBy);

            var result = await _createGameUseCase.ExecuteAsync(command, cancellationToken);
            var response = new CreateGameResponse(result.GameId);

            return Created($"/api/games/{response.GameId}", response);
        }

        private static Guid GetAuthenticatedUserId(ClaimsPrincipal user)
        {
            var userId = user.FindFirstValue(JwtRegisteredClaimNames.Sub);

            if (!Guid.TryParse(userId, out var parsedUserId))
                throw new InvalidCredentialsException();

            return parsedUserId;
        }
    }
}
