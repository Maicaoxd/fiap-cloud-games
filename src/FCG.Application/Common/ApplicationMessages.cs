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
        }
    }
}
