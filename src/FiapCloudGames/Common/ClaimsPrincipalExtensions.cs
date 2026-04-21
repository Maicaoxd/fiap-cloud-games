using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using FCG.Application.Common.Exceptions;

namespace FCG.Api.Common
{
    public static class ClaimsPrincipalExtensions
    {
        public static Guid GetRequiredUserId(this ClaimsPrincipal user)
        {
            var userId = user.FindFirstValue(JwtRegisteredClaimNames.Sub);

            if (!Guid.TryParse(userId, out var parsedUserId))
                throw new InvalidCredentialsException();

            return parsedUserId;
        }
    }
}
