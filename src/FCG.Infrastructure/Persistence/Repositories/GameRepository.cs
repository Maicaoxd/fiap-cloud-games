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
            var normalizedTitle = title.Trim().ToLower();

            return await _dbContext.Games
                .AnyAsync(
                    game => game.Title.Trim().ToLower() == normalizedTitle,
                    cancellationToken);
        }

        public async Task AddAsync(Game game, CancellationToken cancellationToken = default)
        {
            await _dbContext.Games.AddAsync(game, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
