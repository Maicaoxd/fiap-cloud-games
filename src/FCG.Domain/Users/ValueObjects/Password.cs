namespace FCG.Domain.Users.ValueObjects
{
    public sealed class Password
    {
        private const int MinimumLength = 8;
        private const string RequiredMessage = "Senha é obrigatória.";
        private const string WhiteSpaceMessage = "Senha não deve conter espaços em branco.";
        private const string MinimumLengthMessage = "Senha deve ter no mínimo 8 caracteres.";
        private const string LetterMessage = "Senha deve conter pelo menos uma letra.";
        private const string NumberMessage = "Senha deve conter pelo menos um número.";
        private const string SpecialCharacterMessage = "Senha deve conter pelo menos um caractere especial.";

        public string Value { get; }

        private Password(string value)
        {
            Value = value;
        }

        public static Password Create(string value)
        {
            EnsureIsRequired(value);
            EnsureHasNoWhiteSpace(value);
            EnsureMinimumLength(value);
            EnsureHasLetter(value);
            EnsureHasNumber(value);
            EnsureHasSpecialCharacter(value);

            return new Password(value);
        }

        private static void EnsureIsRequired(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException(RequiredMessage);
        }

        private static void EnsureHasNoWhiteSpace(string value)
        {
            if (value.Any(char.IsWhiteSpace))
                throw new ArgumentException(WhiteSpaceMessage);
        }

        private static void EnsureMinimumLength(string value)
        {
            if (value.Length < MinimumLength)
                throw new ArgumentException(MinimumLengthMessage);
        }

        private static void EnsureHasLetter(string value)
        {
            if (!value.Any(char.IsLetter))
                throw new ArgumentException(LetterMessage);
        }

        private static void EnsureHasNumber(string value)
        {
            if (!value.Any(char.IsDigit))
                throw new ArgumentException(NumberMessage);
        }

        private static void EnsureHasSpecialCharacter(string value)
        {
            if (!value.Any(character => !char.IsLetterOrDigit(character)))
                throw new ArgumentException(SpecialCharacterMessage);
        }
    }
}
