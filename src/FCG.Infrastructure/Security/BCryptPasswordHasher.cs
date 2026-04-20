using FCG.Application.Abstractions.Security;
using FCG.Domain.Users.ValueObjects;

namespace FCG.Infrastructure.Security
{
    public sealed class BCryptPasswordHasher : IPasswordHasher
    {
        public PasswordHash Hash(Password password)
        {
            var hash = BCrypt.Net.BCrypt.HashPassword(password.Value);

            return PasswordHash.Create(hash);
        }

        public bool Verify(Password password, PasswordHash passwordHash)
        {
            return BCrypt.Net.BCrypt.Verify(password.Value, passwordHash.Value);
        }
    }
}
