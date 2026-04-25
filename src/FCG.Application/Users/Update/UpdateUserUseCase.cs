using FCG.Application.Abstractions.Persistence;
using FCG.Application.Common.Exceptions;
using FCG.Domain.Users.ValueObjects;

namespace FCG.Application.Users.Update
{
    public sealed class UpdateUserUseCase
    {
        private readonly IUserRepository _userRepository;

        public UpdateUserUseCase(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task ExecuteAsync(
            UpdateUserCommand command,
            CancellationToken cancellationToken = default)
        {
            var user = await _userRepository.GetByIdAsync(command.UserId, cancellationToken);

            if (user is null)
                throw new UserNotFoundException();

            var email = Email.Create(command.Email);
            var userWithSameEmail = await _userRepository.GetByEmailAsync(email, cancellationToken);

            if (userWithSameEmail is not null && userWithSameEmail.Id != user.Id)
                throw new EmailAlreadyRegisteredException();

            user.UpdateProfile(command.Name, email, command.UpdatedBy);

            await _userRepository.UpdateAsync(user, cancellationToken);
        }
    }
}
