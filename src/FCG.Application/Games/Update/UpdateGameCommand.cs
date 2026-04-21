namespace FCG.Application.Games.Update
{
    public sealed record UpdateGameCommand(
        Guid GameId,
        string Title,
        string Description,
        decimal Price,
        Guid UpdatedBy);
}
