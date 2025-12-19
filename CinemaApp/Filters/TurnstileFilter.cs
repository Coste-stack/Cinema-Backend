using CinemaApp.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace CinemaApp.Filters;

public class TurnstileFilter : IAsyncActionFilter
{
    private readonly ITurnstileService _turnstile;
    private readonly ILogger<TurnstileFilter> _logger;

    public TurnstileFilter(ITurnstileService turnstile, ILogger<TurnstileFilter> logger)
    {
        _turnstile = turnstile;
        _logger = logger;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        _logger.LogInformation("Turnstile filter starting");
        var turnstileToken = context.HttpContext.Request.Headers["CF-Turnstile-Token"].FirstOrDefault() ?? 
                    context.HttpContext.Request.Headers["Turnstile-Token"].FirstOrDefault();

        if (string.IsNullOrWhiteSpace(turnstileToken))
        {
            _logger.LogInformation("Turnstile token not provided");
            context.Result = new BadRequestObjectResult(new { message = "Turnstile token not provided" });
            return;
        }

        var remoteip = context.HttpContext.Request.Headers["CF-Connecting-IP"].FirstOrDefault() ?? 
                       context.HttpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault() ?? 
                       context.HttpContext.Connection.RemoteIpAddress?.ToString();

        var result = await _turnstile.VerifyAsync(turnstileToken, remoteip);
        if (!result.Success)
        {
            _logger.LogInformation("Turnstile verification failed");
            context.Result = new BadRequestObjectResult(new { message = "Turnstile verification failed" });
            return;
        }

        _logger.LogInformation("Turnstile filter successfull");
        await next();
    }
}