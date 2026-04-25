using FCG.Application.Abstractions.Persistence;
using FCG.Application.Common.Exceptions;
using FCG.Domain.Users;

namespace FCG.Application.Libraries.List
{
    public sealed class ListLibraryGamesUseCase
    {
        private readonly IUserRepository _userRepository;
        private readonly ILibraryRepository _libraryRepository;

        public ListLibraryGamesUseCase(
            IUserRepository userRepository,
            ILibraryRepository libraryRepository)
        {
            _userRepository = userRepository;
            _libraryRepository = libraryRepository;
        }

        public async Task<IReadOnlyCollection<ListLibraryGameResult>> ExecuteAsync(
            Guid userId,
            CancellationToken cancellationToken = default)
        {
            var user = await _userRepository.GetByIdAsync(userId, cancellationToken);

            user = ResolveExistingUser(user);
            EnsureUserIsActive(user);

            var libraryGames = await _libraryRepository.ListGamesByUserIdAsync(userId, cancellationToken);

            return libraryGames
                .Select(libraryGame => new ListLibraryGameResult(
                    libraryGame.LibraryId,
                    libraryGame.GameId,
                    libraryGame.Title,
                    libraryGame.Description,
                    libraryGame.Price,
                    libraryGame.IsActive,
                    libraryGame.AcquiredAt))
                .ToList();
        }

        private static User ResolveExistingUser(User? user)
        {
            if (user is null)
                throw new InvalidCredentialsException();

            return user;
        }

        private static void EnsureUserIsActive(User user)
        {
            if (!user.IsActive)
                throw new InactiveUserException();
        }
    }
}
