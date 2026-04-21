using FCG.Api.Users;
using FCG.Application.Users.Register;
using Microsoft.AspNetCore.Mvc;

namespace FCG.Api.Controllers
{
    [ApiController]
    [Route("api/users")]
    public sealed class UsersController : ControllerBase
    {
        private readonly RegisterUserUseCase _registerUserUseCase;

        public UsersController(RegisterUserUseCase registerUserUseCase)
        {
            _registerUserUseCase = registerUserUseCase;
        }

        [HttpPost]
        [ProducesResponseType(typeof(RegisterUserResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
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
    }
}
