using FCG.Domain.Games;

namespace FCG.Application.Abstractions.Persistence
{
    public interface IGameRepository
    {
        Task<bool> ExistsByTitleAsync(string title, CancellationToken cancellationToken = default);

        Task<IReadOnlyCollection<Game>> ListActiveAsync(CancellationToken cancellationToken = default);

        Task AddAsync(Game game, CancellationToken cancellationToken = default);
    }
}
