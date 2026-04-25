using FCG.Application.Abstractions.Persistence;
using FCG.Application.Common.Exceptions;
using FCG.Domain.Libraries;
using FCG.Domain.Users;

namespace FCG.Application.Libraries.Acquire
{
    public sealed class AcquireGameUseCase
    {
        private readonly IUserRepository _userRepository;
        private readonly IGameRepository _gameRepository;
        private readonly ILibraryRepository _libraryRepository;

        public AcquireGameUseCase(
            IUserRepository userRepository,
            IGameRepository gameRepository,
            ILibraryRepository libraryRepository)
        {
            _userRepository = userRepository;
            _gameRepository = gameRepository;
            _libraryRepository = libraryRepository;
        }

        public async Task<AcquireGameResult> ExecuteAsync(
            AcquireGameCommand command,
            CancellationToken cancellationToken = default)
        {
            var user = await _userRepository.GetByIdAsync(command.UserId, cancellationToken);

            user = ResolveExistingUser(user);
            EnsureUserIsActive(user);

            var game = await _gameRepository.GetByIdAsync(command.GameId, cancellationToken);

            if (game is null)
                throw new GameNotFoundException();

            if (!game.IsActive)
                throw new GameUnavailableException();

            if (await _libraryRepository.ExistsByUserAndGameAsync(
                    command.UserId,
                    command.GameId,
                    cancellationToken))
                throw new GameAlreadyOwnedException();

            var library = Library.Create(command.UserId, command.GameId);

            await _libraryRepository.AddAsync(library, cancellationToken);

            return new AcquireGameResult(library.Id);
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
