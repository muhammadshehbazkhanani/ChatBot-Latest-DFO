namespace AngularApp1.Server.Exceptions
{ 
public sealed class LoginFailedException : Exception
{
    public LoginFailedException() { }
    
    public LoginFailedException(string message) 
        : base(message) { }
        
    public LoginFailedException(string message, Exception innerException) 
        : base(message, innerException) { }
}

}
