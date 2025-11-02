using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

public class NotFoundException : Exception
{
    public NotFoundException(string message) 
        : base(message)
    {
    }
}