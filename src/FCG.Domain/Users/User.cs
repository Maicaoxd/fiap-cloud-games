using FCG.Domain.Shared;
using FCG.Domain.Users.ValueObjects;

namespace FCG.Domain.Users
{
    public class User : Entity
    {
        private const string RequiredNameMessage = "Nome é obrigatório.";
        private const string RequiredEmailMessage = "E-mail é obrigatório.";
        private const string RequiredPasswordHashMessage = "Hash da senha é obrigatório.";

        public string Name { get; }
        public Email Email { get; }
        public PasswordHash PasswordHash { get; }
        public UserRole Role { get; }

        private User(string name, Email email, PasswordHash passwordHash, UserRole role)
        {
            Name = name;
            Email = email;
            PasswordHash = passwordHash;
            Role = role;
        }

        public static User Create(string name, Email email, PasswordHash passwordHash)
        {
            EnsureNameIsRequired(name);
            EnsureEmailIsRequired(email);
            EnsurePasswordHashIsRequired(passwordHash);

            return new User(name, email, passwordHash, UserRole.User);
        }

        public void Deactivate()
        {
            IsActive = false;
            UpdatedAt = DateTime.UtcNow;
        }

        private static void EnsureNameIsRequired(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException(RequiredNameMessage);
        }

        private static void EnsureEmailIsRequired(Email email)
        {
            if (email is null)
                throw new ArgumentException(RequiredEmailMessage);
        }

        private static void EnsurePasswordHashIsRequired(PasswordHash passwordHash)
        {
            if (passwordHash is null)
                throw new ArgumentException(RequiredPasswordHashMessage);
        }
    }
}
