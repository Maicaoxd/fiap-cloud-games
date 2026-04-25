namespace FCG.Api.Contracts.Games.Get
{
    public sealed record GetGameResponse(
        Guid GameId,
        string Title,
        string Description,
        decimal Price);
}
