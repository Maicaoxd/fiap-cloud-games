using System.ComponentModel.DataAnnotations;
using FCG.Api.Common;

namespace FCG.Api.Authentication
{
    public sealed record LoginRequest(
        [Required(ErrorMessage = ApiMessages.User.EmailRequired)]
        string Email,
        [Required(ErrorMessage = ApiMessages.User.PasswordRequired)]
        string Password);
}
