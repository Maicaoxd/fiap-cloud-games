using FCG.Application.Abstractions.Persistence;
using FCG.Domain.Games;
using Microsoft.EntityFrameworkCore;

namespace FCG.Infrastructure.Persistence.Repositories
{
    public sealed class GameRepository : IGameRepository
    {
        private readonly FcgDbContext _dbContext;

        public GameRepository(FcgDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<bool> ExistsByTitleAsync(
            string title,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(title))
                return false;

            var normalizedTitle = title.Trim().ToLower();

            return await _dbContext.Games
                .AnyAsync(
                    game => game.Title.Trim().ToLower() == normalizedTitle,
                    cancellationToken);
        }

        public async Task<bool> ExistsByTitleForAnotherGameAsync(
            string title,
            Guid gameId,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(title))
                return false;

            var normalizedTitle = title.Trim().ToLower();

            return await _dbContext.Games
                .AnyAsync(
                    game => game.Id != gameId &&
                            game.Title.Trim().ToLower() == normalizedTitle,
                    cancellationToken);
        }

        public async Task<Game?> GetByIdAsync(
            Guid gameId,
            CancellationToken cancellationToken = default)
        {
            return await _dbContext.Games
                .SingleOrDefaultAsync(game => game.Id == gameId, cancellationToken);
        }

        public async Task<IReadOnlyCollection<Game>> ListActiveAsync(
            CancellationToken cancellationToken = default)
        {
            return await _dbContext.Games
                .AsNoTracking()
                .Where(game => game.IsActive)
                .OrderBy(game => game.Title)
                .ToListAsync(cancellationToken);
        }

        public async Task AddAsync(Game game, CancellationToken cancellationToken = default)
        {
            await _dbContext.Games.AddAsync(game, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task UpdateAsync(Game game, CancellationToken cancellationToken = default)
        {
            _dbContext.Games.Update(game);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
