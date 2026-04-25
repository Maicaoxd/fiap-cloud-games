namespace FCG.Application.Common.Exceptions
{
    public sealed class GameUnavailableException : Exception
    {
        public GameUnavailableException()
            : base(ApplicationMessages.Game.InactiveCannotBeAcquired)
        {
        }
    }
}
