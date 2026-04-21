using FCG.Api.Authentication;
using FCG.Application.Users.Authenticate;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FCG.Api.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public sealed class AuthenticationController : ControllerBase
    {
        private readonly AuthenticateUserUseCase _authenticateUserUseCase;

        public AuthenticationController(AuthenticateUserUseCase authenticateUserUseCase)
        {
            _authenticateUserUseCase = authenticateUserUseCase;
        }

        [HttpPost("login")]
        [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [AllowAnonymous]
        public async Task<ActionResult<LoginResponse>> LoginAsync(
            LoginRequest request,
            CancellationToken cancellationToken)
        {
            var command = new AuthenticateUserCommand(
                request.Email,
                request.Password);

            var result = await _authenticateUserUseCase.ExecuteAsync(command, cancellationToken);
            var response = new LoginResponse(result.AccessToken);

            return Ok(response);
        }
    }
}
