using FCG.Domain.Shared;

namespace FCG.Domain.Users.ValueObjects
{
    public sealed class PasswordHash : ValueObject
    {
        private const string RequiredMessage = "Hash da senha é obrigatório.";

        public string Value { get; }

        private PasswordHash(string value)
        {
            Value = value;
        }

        public static PasswordHash Create(string value)
        {
            EnsureIsRequired(value);

            return new PasswordHash(value);
        }

        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return Value;
        }

        private static void EnsureIsRequired(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException(RequiredMessage);
        }
    }
}
