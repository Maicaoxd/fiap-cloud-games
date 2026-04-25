namespace FCG.Api.Libraries
{
    public sealed record ListLibraryGameResponse(
        Guid LibraryId,
        Guid GameId,
        string Title,
        string Description,
        decimal Price,
        bool IsActive,
        DateTime AcquiredAt);
}
