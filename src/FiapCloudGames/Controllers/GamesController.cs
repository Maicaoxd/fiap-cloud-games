using FCG.Api.Common;
using FCG.Api.Games;
using FCG.Application.Games.Create;
using FCG.Application.Games.List;
using FCG.Application.Games.Update;
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
        private readonly UpdateGameUseCase _updateGameUseCase;

        public GamesController(
            CreateGameUseCase createGameUseCase,
            ListGamesUseCase listGamesUseCase,
            UpdateGameUseCase updateGameUseCase)
        {
            _createGameUseCase = createGameUseCase;
            _listGamesUseCase = listGamesUseCase;
            _updateGameUseCase = updateGameUseCase;
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
            var createdBy = User.GetRequiredUserId();

            var command = new CreateGameCommand(
                request.Title,
                request.Description,
                request.Price,
                createdBy);

            var result = await _createGameUseCase.ExecuteAsync(command, cancellationToken);
            var response = new CreateGameResponse(result.GameId);

            return Created($"/api/games/{response.GameId}", response);
        }

        [HttpPut("{gameId:guid}")]
        [Authorize(Roles = nameof(UserRole.Administrator))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateAsync(
            Guid gameId,
            UpdateGameRequest request,
            CancellationToken cancellationToken)
        {
            var updatedBy = User.GetRequiredUserId();

            var command = new UpdateGameCommand(
                gameId,
                request.Title,
                request.Description,
                request.Price,
                updatedBy);

            await _updateGameUseCase.ExecuteAsync(command, cancellationToken);

            return NoContent();
        }
    }
}
