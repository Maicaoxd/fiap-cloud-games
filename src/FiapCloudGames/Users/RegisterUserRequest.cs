namespace FCG.Api.Users
{
    public sealed record RegisterUserRequest(
        string Name,
        string Email,
        string Password,
        string ConfirmPassword);
}
