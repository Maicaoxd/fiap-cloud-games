using System.Text.RegularExpressions;

namespace FCG.Domain.Users.ValueObjects
{
    public sealed class Email
    {
        private const string RequiredMessage = "E-mail é obrigatório.";
        private const string InvalidFormatMessage = "E-mail deve estar em um formato válido.";

        private static readonly Regex EmailRegex = new(@"^[^\s@]+@[^\s@]+\.[^\s@]{2,}$");

        public string Value { get; }

        private Email(string value)
        {
            Value = value;
        }

        public static Email Create(string value)
        {
            EnsureIsRequired(value);

            var normalizedValue = Normalize(value);

            EnsureValidFormat(normalizedValue);

            return new Email(normalizedValue);
        }

        private static void EnsureIsRequired(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException(RequiredMessage);
        }

        private static string Normalize(string value)
        {
            return value.Trim().ToLowerInvariant();
        }

        private static void EnsureValidFormat(string value)
        {
            if (!EmailRegex.IsMatch(value))
                throw new ArgumentException(InvalidFormatMessage);
        }
    }
}
