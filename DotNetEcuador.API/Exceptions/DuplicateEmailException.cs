namespace DotNetEcuador.API.Exceptions;

public class DuplicateEmailException : Exception
{
    public string Email { get; }
    
    public DuplicateEmailException(string email) 
        : base($"Ya existe una aplicación de voluntario registrada con el email: {email}")
    {
        Email = email;
    }
    
    public DuplicateEmailException(string email, string message) 
        : base(message)
    {
        Email = email;
    }
    
    public DuplicateEmailException(string email, string message, Exception innerException) 
        : base(message, innerException)
    {
        Email = email;
    }
}