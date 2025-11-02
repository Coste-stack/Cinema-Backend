using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

public class BadRequestException : Exception
{
    public BadRequestException(string message) 
        : base(message)
    {
    }
}