using FCG.Api.Common;
using FCG.Api.Users;
using FCG.Application.Users.Deactivate;
using FCG.Application.Users.Register;
using FCG.Application.Users.ChangePassword;
using FCG.Application.Users.Update;
using FCG.Application.Users.UpdateCurrent;
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
        private readonly ChangePasswordUseCase _changePasswordUseCase;
        private readonly UpdateUserUseCase _updateUserUseCase;
        private readonly UpdateCurrentUserUseCase _updateCurrentUserUseCase;

        public UsersController(
            DeactivateUserUseCase deactivateUserUseCase,
            RegisterUserUseCase registerUserUseCase,
            ChangePasswordUseCase changePasswordUseCase,
            UpdateUserUseCase updateUserUseCase,
            UpdateCurrentUserUseCase updateCurrentUserUseCase)
        {
            _deactivateUserUseCase = deactivateUserUseCase;
            _registerUserUseCase = registerUserUseCase;
            _changePasswordUseCase = changePasswordUseCase;
            _updateUserUseCase = updateUserUseCase;
            _updateCurrentUserUseCase = updateCurrentUserUseCase;
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
                request.Cpf,
                request.BirthDate!.Value,
                request.Password,
                request.ConfirmPassword);

            var result = await _registerUserUseCase.ExecuteAsync(command, cancellationToken);
            var response = new RegisterUserResponse(result.UserId);

            return Created($"/api/users/{response.UserId}", response);
        }

        [HttpPut("me")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateMeAsync(
            UpdateCurrentUserRequest request,
            CancellationToken cancellationToken)
        {
            var userId = User.GetRequiredUserId();
            var command = new UpdateCurrentUserCommand(
                userId,
                request.Name,
                request.Email,
                request.BirthDate!.Value);

            await _updateCurrentUserUseCase.ExecuteAsync(command, cancellationToken);

            return NoContent();
        }

        [HttpPatch("me/password")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ChangePasswordAsync(
            ChangePasswordRequest request,
            CancellationToken cancellationToken)
        {
            var userId = User.GetRequiredUserId();
            var command = new ChangePasswordCommand(
                userId,
                request.CurrentPassword,
                request.NewPassword,
                request.ConfirmNewPassword);

            await _changePasswordUseCase.ExecuteAsync(command, cancellationToken);

            return NoContent();
        }

        [HttpPut("{userId:guid}")]
        [Authorize(Roles = nameof(UserRole.Administrator))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateAsync(
            Guid userId,
            UpdateUserRequest request,
            CancellationToken cancellationToken)
        {
            var updatedBy = User.GetRequiredUserId();
            var command = new UpdateUserCommand(
                userId,
                request.Name,
                request.Email,
                request.Cpf,
                request.BirthDate!.Value,
                updatedBy);

            await _updateUserUseCase.ExecuteAsync(command, cancellationToken);

            return NoContent();
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
