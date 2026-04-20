using FCG.Application.Abstractions.Persistence;
using FCG.Application.Abstractions.Security;
using FCG.Domain.Users;
using FCG.Domain.Users.ValueObjects;

namespace FCG.Application.Users.Register
{
    public sealed class RegisterUserUseCase
    {
        private const string PasswordConfirmationMessage = "As senhas não conferem.";
        private const string EmailAlreadyRegisteredMessage = "E-mail já cadastrado.";

        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher _passwordHasher;

        public RegisterUserUseCase(IUserRepository userRepository, IPasswordHasher passwordHasher)
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
        }

        public async Task<RegisterUserResult> ExecuteAsync(
            RegisterUserCommand command,
            CancellationToken cancellationToken = default)
        {
            EnsurePasswordsMatch(command.Password, command.ConfirmPassword);

            var email = Email.Create(command.Email);
            var password = Password.Create(command.Password);

            if (await _userRepository.ExistsByEmailAsync(email, cancellationToken))
                throw new InvalidOperationException(EmailAlreadyRegisteredMessage);

            var passwordHash = _passwordHasher.Hash(password);
            var user = User.Create(command.Name, email, passwordHash);

            await _userRepository.AddAsync(user, cancellationToken);

            return new RegisterUserResult(user.Id);
        }

        private static void EnsurePasswordsMatch(string password, string confirmPassword)
        {
            if (password != confirmPassword)
                throw new ArgumentException(PasswordConfirmationMessage);
        }
    }
}
