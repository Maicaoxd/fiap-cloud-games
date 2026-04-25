using FCG.Api.Common;
using FCG.Api.Libraries;
using FCG.Application.Libraries.Acquire;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FCG.Api.Controllers
{
    [ApiController]
    [Route("api/library")]
    public sealed class LibraryController : ControllerBase
    {
        private readonly AcquireGameUseCase _acquireGameUseCase;

        public LibraryController(AcquireGameUseCase acquireGameUseCase)
        {
            _acquireGameUseCase = acquireGameUseCase;
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
