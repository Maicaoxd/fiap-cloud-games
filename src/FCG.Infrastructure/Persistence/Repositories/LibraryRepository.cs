using FCG.Application.Abstractions.Persistence;
using FCG.Application.Common.Exceptions;
using FCG.Domain.Libraries;
using Microsoft.EntityFrameworkCore;

namespace FCG.Infrastructure.Persistence.Repositories
{
    public sealed class LibraryRepository : ILibraryRepository
    {
        private const string UniqueOwnershipIndexName = "IX_Libraries_UserId_GameId";

        private readonly FcgDbContext _dbContext;

        public LibraryRepository(FcgDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<bool> ExistsByUserAndGameAsync(
            Guid userId,
            Guid gameId,
            CancellationToken cancellationToken = default)
        {
            return await _dbContext.Libraries
                .AnyAsync(
                    library => library.UserId == userId && library.GameId == gameId,
                    cancellationToken);
        }

        public async Task<IReadOnlyCollection<LibraryGameReadModel>> ListGamesByUserIdAsync(
            Guid userId,
            CancellationToken cancellationToken = default)
        {
            return await _dbContext.Libraries
                .AsNoTracking()
                .Where(library => library.UserId == userId)
                .Join(
                    _dbContext.Games.AsNoTracking(),
                    library => library.GameId,
                    game => game.Id,
                    (library, game) => new LibraryGameReadModel(
                        library.Id,
                        game.Id,
                        game.Title,
                        game.Description,
                        game.Price,
                        game.IsActive,
                        library.CreatedAt))
                .OrderByDescending(libraryGame => libraryGame.AcquiredAt)
                .ToListAsync(cancellationToken);
        }

        public async Task AddAsync(Library library, CancellationToken cancellationToken = default)
        {
            await _dbContext.Libraries.AddAsync(library, cancellationToken);

            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException exception) when (
                SqlServerUniqueConstraintDetector.IsUniqueConstraintViolation(
                    exception,
                    UniqueOwnershipIndexName))
            {
                throw new GameAlreadyOwnedException();
            }
        }
    }
}
