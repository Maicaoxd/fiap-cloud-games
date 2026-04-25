using System.ComponentModel.DataAnnotations;
using FCG.Api.Common;

namespace FCG.Api.Contracts.Users.Register
{
    public sealed record RegisterUserRequest(
        [Required(ErrorMessage = ApiMessages.User.NameRequired)]
        string Name,
        [Required(ErrorMessage = ApiMessages.User.EmailRequired)]
        string Email,
        [Required(ErrorMessage = ApiMessages.User.CpfRequired)]
        string Cpf,
        [Required(ErrorMessage = ApiMessages.User.BirthDateRequired)]
        DateOnly? BirthDate,
        [Required(ErrorMessage = ApiMessages.User.PasswordRequired)]
        string Password,
        [Required(ErrorMessage = ApiMessages.User.ConfirmPasswordRequired)]
        string ConfirmPassword);
}
