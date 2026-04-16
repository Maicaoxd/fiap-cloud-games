using System.Text.RegularExpressions;

namespace FCG.Domain.Users.ValueObjects
{
    public sealed class Email
    {
        public string Value { get; }

        private Email(string value)
        {
            Value = value;
        }

        public static Email Create(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("E-mail é obrigatório.");

            value = value.Trim().ToLowerInvariant();

            var regex = new Regex(@"^[^\s@]+@[^\s@]+\.[^\s@]{2,}$");

            if (!regex.IsMatch(value))
            {
                throw new ArgumentException("E-mail deve estar em um formato válido.");
            }

            return new Email(value);
        }
    }
}
