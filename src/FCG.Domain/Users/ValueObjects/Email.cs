using System.Text.RegularExpressions;

namespace FCG.Domain.Users.ValueObjects
{
    public sealed class Email
    {
        private static readonly Regex EmailRegex = new(@"^[^\s@]+@[^\s@]+\.[^\s@]{2,}$");

        public string Value { get; }

        private Email(string value)
        {
            Value = value;
        }

        public static Email Create(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("E-mail é obrigatório.");

            value = Normalize(value);

            EnsureValidFormat(value);

            return new Email(value);
        }

        private static string Normalize(string value)
        {
            return value.Trim().ToLowerInvariant();
        }

        private static void EnsureValidFormat(string value)
        {
            if (!EmailRegex.IsMatch(value))
                throw new ArgumentException("E-mail deve estar em um formato válido.");
        }
    }
}
