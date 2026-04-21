using System.ComponentModel.DataAnnotations;

namespace FCG.Api.Users
{
    public sealed record RegisterUserRequest(
        [Required(ErrorMessage = "Nome é obrigatório.")]
        string Name,
        [Required(ErrorMessage = "E-mail é obrigatório.")]
        string Email,
        [Required(ErrorMessage = "Senha é obrigatória.")]
        string Password,
        [Required(ErrorMessage = "Confirmação de senha é obrigatória.")]
        string ConfirmPassword);
}
