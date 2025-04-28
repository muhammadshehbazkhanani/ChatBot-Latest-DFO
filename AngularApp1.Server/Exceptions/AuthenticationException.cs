namespace AngularApp1.Server.Exceptions

{
    public class AuthenticationException : Exception
    {
        public AuthenticationException(string message) : base(message) { }
    }
}