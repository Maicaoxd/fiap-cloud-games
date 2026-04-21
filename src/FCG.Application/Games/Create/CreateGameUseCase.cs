using FCG.Application.Abstractions.Persistence;
using FCG.Application.Common.Exceptions;
using FCG.Domain.Games;

namespace FCG.Application.Games.Create
{
    public sealed class CreateGameUseCase
    {
        private readonly IGameRepository _gameRepository;

        public CreateGameUseCase(IGameRepository gameRepository)
        {
            _gameRepository = gameRepository;
        }

        public async Task<CreateGameResult> ExecuteAsync(
            CreateGameCommand command,
            CancellationToken cancellationToken = default)
        {
            if (await _gameRepository.ExistsByTitleAsync(command.Title, cancellationToken))
                throw new GameTitleAlreadyRegisteredException();

            var game = Game.Create(
                command.Title,
                command.Description,
                command.Price,
                command.CreatedBy);

            await _gameRepository.AddAsync(game, cancellationToken);

            return new CreateGameResult(game.Id);
        }
    }
}
