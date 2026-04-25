namespace FCG.Application.Common.Exceptions
{
    public sealed class InvalidPasswordRecoveryDataException : Exception
    {
        public InvalidPasswordRecoveryDataException()
            : base(ApplicationMessages.PasswordRecovery.InvalidRecoveryData)
        {
        }
    }
}
