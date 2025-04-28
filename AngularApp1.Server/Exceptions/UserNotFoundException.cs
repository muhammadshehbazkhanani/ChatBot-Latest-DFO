namespace AngularApp1.Server.Exceptions
{
    public sealed class UserNotFoundException : Exception
    {
        public UserNotFoundException() { }

        public UserNotFoundException(string message)
            : base(message) { }

        public UserNotFoundException(string message, Exception innerException)
            : base(message, innerException) { }
    }

}