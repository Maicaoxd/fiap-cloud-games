using System.ComponentModel.DataAnnotations;
using FCG.Api.Common;

namespace FCG.Api.Contracts.Authentication.Login
{
    public sealed record LoginRequest(
        [Required(ErrorMessage = ApiMessages.User.EmailRequired)]
        string Email,
        [Required(ErrorMessage = ApiMessages.User.PasswordRequired)]
        string Password);
}
