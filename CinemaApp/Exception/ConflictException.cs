using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

public class ConflictException : Exception
{
    public ConflictException(string message, Exception? innerException = null) 
        : base(message, innerException)
    {
    }
}