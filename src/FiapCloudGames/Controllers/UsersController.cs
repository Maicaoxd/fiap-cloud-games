using FCG.Api.Common;
using FCG.Api.Users;
using FCG.Application.Users.Deactivate;
using FCG.Application.Users.Register;
using FCG.Domain.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FCG.Api.Controllers
{
    [ApiController]
    [Route("api/users")]
    public sealed class UsersController : ControllerBase
    {
        private readonly DeactivateUserUseCase _deactivateUserUseCase;
        private readonly RegisterUserUseCase _registerUserUseCase;

        public UsersController(
            DeactivateUserUseCase deactivateUserUseCase,
            RegisterUserUseCase registerUserUseCase)
        {
            _deactivateUserUseCase = deactivateUserUseCase;
            _registerUserUseCase = registerUserUseCase;
        }

        [HttpPost]
        [ProducesResponseType(typeof(RegisterUserResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [AllowAnonymous]
        public async Task<ActionResult<RegisterUserResponse>> RegisterAsync(
            RegisterUserRequest request,
            CancellationToken cancellationToken)
        {
            var command = new RegisterUserCommand(
                request.Name,
                request.Email,
                request.Password,
                request.ConfirmPassword);

            var result = await _registerUserUseCase.ExecuteAsync(command, cancellationToken);
            var response = new RegisterUserResponse(result.UserId);

            return Created($"/api/users/{response.UserId}", response);
        }

        [HttpPatch("{userId:guid}/deactivate")]
        [Authorize(Roles = nameof(UserRole.Administrator))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeactivateAsync(
            Guid userId,
            CancellationToken cancellationToken)
        {
            var deactivatedBy = User.GetRequiredUserId();
            var command = new DeactivateUserCommand(userId, deactivatedBy);

            await _deactivateUserUseCase.ExecuteAsync(command, cancellationToken);

            return NoContent();
        }
    }
}
