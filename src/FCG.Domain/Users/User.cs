using FCG.Domain.Shared;
using FCG.Domain.Users.ValueObjects;

namespace FCG.Domain.Users
{
    public class User : Entity
    {
        public string Name { get; private set; }
        public Email Email { get; private set; }
        public PasswordHash PasswordHash { get; private set; }
        public UserRole Role { get; }

        private User(string name, Email email, PasswordHash passwordHash, UserRole role, Guid? createdBy)
            : base(createdBy)
        {
            Name = name;
            Email = email;
            PasswordHash = passwordHash;
            Role = role;
        }

        public static User Create(string name, Email email, PasswordHash passwordHash, Guid? createdBy = null)
        {
            EnsureNameIsRequired(name);
            EnsureEmailIsRequired(email);
            EnsurePasswordHashIsRequired(passwordHash);

            return new User(name, email, passwordHash, UserRole.User, createdBy);
        }

        public void Activate(Guid activatedBy)
        {
            MarkAsActivated(activatedBy);
        }

        public void Deactivate(Guid deactivatedBy)
        {
            MarkAsDeactivated(deactivatedBy);
        }

        public void ChangeName(string name, Guid updatedBy)
        {
            EnsureNameIsRequired(name);

            Name = name;
            MarkAsUpdated(updatedBy);
        }

        public void ChangeEmail(Email email, Guid updatedBy)
        {
            EnsureEmailIsRequired(email);

            Email = email;
            MarkAsUpdated(updatedBy);
        }

        public void ChangePassword(PasswordHash passwordHash, Guid updatedBy)
        {
            EnsurePasswordHashIsRequired(passwordHash);

            PasswordHash = passwordHash;
            MarkAsUpdated(updatedBy);
        }

        private static void EnsureNameIsRequired(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException(DomainMessages.User.NameRequired);
        }

        private static void EnsureEmailIsRequired(Email email)
        {
            if (email is null)
                throw new ArgumentException(DomainMessages.Email.Required);
        }

        private static void EnsurePasswordHashIsRequired(PasswordHash passwordHash)
        {
            if (passwordHash is null)
                throw new ArgumentException(DomainMessages.PasswordHash.Required);
        }
    }
}
