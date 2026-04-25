using System.ComponentModel.DataAnnotations;
using FCG.Api.Common;

namespace FCG.Api.Contracts.Users.Update
{
    public sealed record UpdateUserRequest(
        [Required(ErrorMessage = ApiMessages.User.NameRequired)]
        string Name,
        [Required(ErrorMessage = ApiMessages.User.EmailRequired)]
        string Email,
        [Required(ErrorMessage = ApiMessages.User.CpfRequired)]
        string Cpf,
        [Required(ErrorMessage = ApiMessages.User.BirthDateRequired)]
        DateOnly? BirthDate);
}
