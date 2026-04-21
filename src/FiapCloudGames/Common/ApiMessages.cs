namespace FCG.Api.Common
{
    public static class ApiMessages
    {
        public static class Validation
        {
            public const string Title = "Erro de validação.";
            public const string InvalidFields = "Um ou mais campos são inválidos.";
            public const string RequestBodyRequired = "O corpo da requisição é obrigatório.";
        }

        public static class User
        {
            public const string NameRequired = "Nome é obrigatório.";
            public const string EmailRequired = "E-mail é obrigatório.";
            public const string PasswordRequired = "Senha é obrigatória.";
            public const string ConfirmPasswordRequired = "Confirmação de senha é obrigatória.";
        }

        public static class Conflict
        {
            public const string Title = "Conflito.";
        }

        public static class InternalServerError
        {
            public const string Title = "Erro interno.";
            public const string Detail = "Ocorreu um erro inesperado.";
        }
    }
}
