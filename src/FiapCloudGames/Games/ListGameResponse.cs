namespace FCG.Api.Games
{
    public sealed record ListGameResponse(
        Guid GameId,
        string Title,
        string Description,
        decimal Price);
}
