using FCG.Api.Common;
using FCG.Api.Contracts.Libraries.Acquire;
using FCG.Api.Contracts.Libraries.List;
using FCG.Application.Libraries.Acquire;
using FCG.Application.Libraries.List;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FCG.Api.Controllers
{
    [ApiController]
    [Route("api/library")]
    public sealed class LibraryController : ControllerBase
    {
        private readonly AcquireGameUseCase _acquireGameUseCase;
        private readonly ListLibraryGamesUseCase _listLibraryGamesUseCase;

        public LibraryController(
            AcquireGameUseCase acquireGameUseCase,
            ListLibraryGamesUseCase listLibraryGamesUseCase)
        {
            _acquireGameUseCase = acquireGameUseCase;
            _listLibraryGamesUseCase = listLibraryGamesUseCase;
        }

        [HttpGet("games")]
        [Authorize]
        [ProducesResponseType(typeof(IReadOnlyCollection<ListLibraryGameResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IReadOnlyCollection<ListLibraryGameResponse>>> ListAsync(
            CancellationToken cancellationToken)
        {
            var userId = User.GetRequiredUserId();
            var result = await _listLibraryGamesUseCase.ExecuteAsync(userId, cancellationToken);
            var response = result
                .Select(libraryGame => new ListLibraryGameResponse(
                    libraryGame.LibraryId,
                    libraryGame.GameId,
                    libraryGame.Title,
                    libraryGame.Description,
                    libraryGame.Price,
                    libraryGame.IsActive,
                    libraryGame.AcquiredAt))
                .ToList();

            return Ok(response);
        }

        [HttpPost("games/{gameId:guid}")]
        [Authorize]
        [ProducesResponseType(typeof(AcquireGameResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<AcquireGameResponse>> AcquireAsync(
            Guid gameId,
            CancellationToken cancellationToken)
        {
            var userId = User.GetRequiredUserId();
            var command = new AcquireGameCommand(userId, gameId);
            var result = await _acquireGameUseCase.ExecuteAsync(command, cancellationToken);
            var response = new AcquireGameResponse(result.LibraryId);

            return Created($"/api/library/{response.LibraryId}", response);
        }
    }
}
