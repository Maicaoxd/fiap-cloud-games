using System.ComponentModel.DataAnnotations;
using FCG.Api.Common;

namespace FCG.Api.Contracts.Users.UpdateCurrent
{
    public sealed record UpdateCurrentUserRequest(
        [Required(ErrorMessage = ApiMessages.User.NameRequired)]
        string Name,
        [Required(ErrorMessage = ApiMessages.User.EmailRequired)]
        string Email,
        [Required(ErrorMessage = ApiMessages.User.BirthDateRequired)]
        DateOnly? BirthDate);
}
