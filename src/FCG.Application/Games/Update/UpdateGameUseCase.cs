using FCG.Application.Abstractions.Persistence;
using FCG.Application.Common.Exceptions;

namespace FCG.Application.Games.Update
{
    public sealed class UpdateGameUseCase
    {
        private readonly IGameRepository _gameRepository;

        public UpdateGameUseCase(IGameRepository gameRepository)
        {
            _gameRepository = gameRepository;
        }

        public async Task ExecuteAsync(
            UpdateGameCommand command,
            CancellationToken cancellationToken = default)
        {
            var game = await _gameRepository.GetByIdAsync(command.GameId, cancellationToken);

            if (game is null)
                throw new GameNotFoundException();

            if (await _gameRepository.ExistsByTitleForAnotherGameAsync(
                    command.Title,
                    command.GameId,
                    cancellationToken))
                throw new GameTitleAlreadyRegisteredException();

            game.Update(
                command.Title,
                command.Description,
                command.Price,
                command.UpdatedBy);

            await _gameRepository.UpdateAsync(game, cancellationToken);
        }
    }
}
