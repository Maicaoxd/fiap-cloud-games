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
            throw new NotImplementedException();
        }
    }
}
