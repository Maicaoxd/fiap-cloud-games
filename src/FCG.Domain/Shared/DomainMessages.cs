namespace FCG.Domain.Shared
{
    public static class DomainMessages
    {
        public static class Entity
        {
            public const string ResponsibleForChangeRequired = "Responsável pela alteração é obrigatório.";
        }

        public static class User
        {
            public const string NameRequired = "Nome é obrigatório.";
        }

        public static class Email
        {
            public const string Required = "E-mail é obrigatório.";
            public const string InvalidFormat = "E-mail deve estar em um formato válido.";
        }

        public static class Password
        {
            public const string Required = "Senha é obrigatória.";
            public const string WhiteSpaceNotAllowed = "Senha não deve conter espaços em branco.";
            public const string MinimumLength = "Senha deve ter no mínimo 8 caracteres.";
            public const string LetterRequired = "Senha deve conter pelo menos uma letra.";
            public const string NumberRequired = "Senha deve conter pelo menos um número.";
            public const string SpecialCharacterRequired = "Senha deve conter pelo menos um caractere especial.";
        }

        public static class PasswordHash
        {
            public const string Required = "Hash da senha é obrigatório.";
        }

        public static class Game
        {
            public const string TitleRequired = "Título do jogo é obrigatório.";
            public const string DescriptionRequired = "Descrição do jogo é obrigatória.";
            public const string PriceCannotBeNegative = "Preço do jogo não pode ser negativo.";
        }
    }
}
