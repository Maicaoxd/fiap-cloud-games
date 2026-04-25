namespace FCG.Application.Common
{
    public static class ApplicationMessages
    {
        public static class User
        {
            public const string PasswordConfirmationDoesNotMatch = "As senhas não conferem.";
            public const string EmailAlreadyRegistered = "E-mail já cadastrado.";
        }

        public static class Authentication
        {
            public const string InvalidCredentials = "E-mail ou senha inválidos.";
            public const string InactiveUser = "Usuário inativo.";
        }

        public static class Game
        {
            public const string TitleAlreadyRegistered = "Jogo já cadastrado com este título.";
            public const string NotFound = "Jogo não encontrado.";
            public const string InactiveCannotBeAcquired = "Jogo inativo não pode ser adquirido.";
        }

        public static class Library
        {
            public const string GameAlreadyOwned = "Jogo já está na biblioteca do usuário.";
        }

        public static class Conflict
        {
            public const string UniqueConstraintViolation = "Já existe um registro com os mesmos dados.";
        }
    }
}
