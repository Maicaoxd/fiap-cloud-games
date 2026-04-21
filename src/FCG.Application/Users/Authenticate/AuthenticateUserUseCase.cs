using FCG.Application.Abstractions.Persistence;
using FCG.Application.Abstractions.Security;
using FCG.Application.Common.Exceptions;
using FCG.Domain.Users;
using FCG.Domain.Users.ValueObjects;

namespace FCG.Application.Users.Authenticate
{
    public sealed class AuthenticateUserUseCase
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IAccessTokenGenerator _accessTokenGenerator;

        public AuthenticateUserUseCase(
            IUserRepository userRepository,
            IPasswordHasher passwordHasher,
            IAccessTokenGenerator accessTokenGenerator)
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
            _accessTokenGenerator = accessTokenGenerator;
        }

        public async Task<AuthenticateUserResult> ExecuteAsync(
            AuthenticateUserCommand command,
            CancellationToken cancellationToken = default)
        {
            var email = Email.Create(command.Email);
            var password = Password.Create(command.Password);
            var user = await _userRepository.GetByEmailAsync(email, cancellationToken);

            user = ResolveExistingUser(user);
            EnsureCredentialsAreValid(password, user.PasswordHash);
            EnsureAccountIsActive(user);

            var accessToken = _accessTokenGenerator.Generate(user);

            return new AuthenticateUserResult(accessToken);
        }

        private static User ResolveExistingUser(User? user)
        {
            if (user is null)
                throw new InvalidCredentialsException();

            return user;
        }

        private void EnsureCredentialsAreValid(Password password, PasswordHash passwordHash)
        {
            if (!_passwordHasher.Verify(password, passwordHash))
                throw new InvalidCredentialsException();
        }

        private static void EnsureAccountIsActive(User user)
        {
            if (!user.IsActive)
                throw new InactiveUserException();
        }
    }
}
