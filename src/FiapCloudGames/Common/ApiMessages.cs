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

        public static class Game
        {
            public const string TitleRequired = "Título do jogo é obrigatório.";
            public const string DescriptionRequired = "Descrição do jogo é obrigatória.";
            public const string PriceCannotBeNegative = "Preço do jogo não pode ser negativo.";
        }

        public static class Conflict
        {
            public const string Title = "Conflito.";
        }

        public static class NotFound
        {
            public const string Title = "Recurso não encontrado.";
        }

        public static class Unauthorized
        {
            public const string Title = "Não autorizado.";
            public const string Detail = "A autenticação é necessária para acessar este recurso.";
        }

        public static class Forbidden
        {
            public const string Title = "Acesso negado.";
            public const string Detail = "Você não tem permissão para acessar este recurso.";
        }

        public static class InternalServerError
        {
            public const string Title = "Erro interno.";
            public const string Detail = "Ocorreu um erro inesperado.";
        }
    }
}
