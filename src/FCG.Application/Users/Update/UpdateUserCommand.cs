namespace FCG.Application.Users.Update
{
    public sealed record UpdateUserCommand(
        Guid UserId,
        string Name,
        string Email,
        Guid UpdatedBy);
}
