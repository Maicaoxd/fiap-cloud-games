namespace FCG.Application.Libraries.Acquire
{
    public sealed record AcquireGameCommand(
        Guid UserId,
        Guid GameId);
}
