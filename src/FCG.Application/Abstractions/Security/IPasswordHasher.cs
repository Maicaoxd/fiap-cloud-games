using FCG.Domain.Users.ValueObjects;

namespace FCG.Application.Abstractions.Security
{
    public interface IPasswordHasher
    {
        PasswordHash Hash(Password password);

        bool Verify(string? password, PasswordHash passwordHash);
    }
}
