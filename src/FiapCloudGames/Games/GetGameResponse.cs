namespace FCG.Api.Games
{
    public sealed record GetGameResponse(
        Guid GameId,
        string Title,
        string Description,
        decimal Price);
}
