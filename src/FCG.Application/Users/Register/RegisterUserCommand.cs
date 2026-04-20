namespace FCG.Application.Users.Register
{
    public sealed record RegisterUserCommand(
        string Name,
        string Email,
        string Password,
        string ConfirmPassword);
}
