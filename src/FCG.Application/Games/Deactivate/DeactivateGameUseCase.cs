using FCG.Application.Abstractions.Persistence;
using FCG.Application.Common.Exceptions;

namespace FCG.Application.Games.Deactivate
{
    public sealed class DeactivateGameUseCase
    {
        private readonly IGameRepository _gameRepository;

        public DeactivateGameUseCase(IGameRepository gameRepository)
        {
            _gameRepository = gameRepository;
        }

        public async Task ExecuteAsync(
            DeactivateGameCommand command,
            CancellationToken cancellationToken = default)
        {
            var game = await _gameRepository.GetByIdAsync(command.GameId, cancellationToken);

            if (game is null)
                throw new GameNotFoundException();

            if (!game.IsActive)
                return;

            game.Deactivate(command.DeactivatedBy);

            await _gameRepository.UpdateAsync(game, cancellationToken);
        }
    }
}
