namespace FCG.Domain.Users.ValueObjects
{
    public sealed class Password
    {
        public string Value { get; }

        private Password(string value)
        {
            Value = value;
        }

        public static Password Create(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Senha é obrigatória.");

            if (value.Any(char.IsWhiteSpace))
                throw new ArgumentException("Senha não deve conter espaços em branco.");

            if (value.Length < 8)
                throw new ArgumentException("Senha deve ter no mínimo 8 caracteres.");

            if (!value.Any(char.IsLetter))
                throw new ArgumentException("Senha deve conter pelo menos uma letra.");

            if (!value.Any(char.IsNumber))
                throw new ArgumentException("Senha deve conter pelo menos um número.");

            if (!value.Any(character => !char.IsLetterOrDigit(character)))
                throw new ArgumentException("Senha deve conter pelo menos um caractere especial.");

            return new Password(value);
        }
    }
}
