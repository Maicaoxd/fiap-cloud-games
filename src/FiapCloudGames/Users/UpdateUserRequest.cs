using System.ComponentModel.DataAnnotations;
using FCG.Api.Common;

namespace FCG.Api.Users
{
    public sealed record UpdateUserRequest(
        [Required(ErrorMessage = ApiMessages.User.NameRequired)]
        string Name,
        [Required(ErrorMessage = ApiMessages.User.EmailRequired)]
        string Email);
}
