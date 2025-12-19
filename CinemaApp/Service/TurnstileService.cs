using System.Text.Json;
using Microsoft.Extensions.Options;
using CinemaApp.DTO.Turnstile;

namespace CinemaApp.Service;

public interface ITurnstileService
{
    Task<TurnstileResponseDTO> VerifyAsync(string token, string? ip);
}

public class TurnstileService : ITurnstileService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<TurnstileService> _logger;
    private readonly string _secretKey;
    private const string SiteverifyUrl = "https://challenges.cloudflare.com/turnstile/v0/siteverify";

    public TurnstileService(HttpClient httpClient, ILogger<TurnstileService> logger, IOptions<TurnstileSettingsDTO> settings)
    {
        _httpClient = httpClient;
        _logger = logger;
        _secretKey = settings?.Value?.SecretKey ?? throw new ArgumentException("Turnstile secret not configured");
    }

    public async Task<TurnstileResponseDTO> VerifyAsync(string token, string? ip)
    {
        if (string.IsNullOrWhiteSpace(token))
            return new TurnstileResponseDTO
            {
                Success = false,
                ErrorCodes = ["internal-error", "No token provided"]
            };

        var parameters = new Dictionary<string, string>
        {
            { "secret", _secretKey },
            { "response", token }
        };

        if (!string.IsNullOrWhiteSpace(ip))
            parameters["remoteip"] = ip;

        try
        {
            using var postContent = new FormUrlEncodedContent(parameters);
            var response = await _httpClient.PostAsync(SiteverifyUrl, postContent);
            var stringContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Turnstile verification returned non-success status {Status}: {Body}", response.StatusCode, stringContent);
                throw new Exception("Turnstile verification returned non-success status");
            }

            var opts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var result = JsonSerializer.Deserialize<TurnstileResponseDTO>(stringContent, opts);

            if (result == null)
            {
                _logger.LogWarning("Turnstile verification returned empty response body");
                throw new Exception("Turnstile verification returned empty response body");
            }

            if (!result.Success)
            {
                _logger.LogInformation("Turnstile verification failed: {Errors}", result.ErrorCodes != null ? string.Join(',', result.ErrorCodes) : "(none)");
                throw new Exception("Turnstile verification failed");
            }

            _logger.LogInformation("Turnstile verification succeded!");
            return new TurnstileResponseDTO
            {
                Success = true,
                ErrorCodes = Array.Empty<string>()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while verifying Turnstile token");
            return new TurnstileResponseDTO
            {
                Success = false,
                ErrorCodes = ["internal-error", ex.Message]
            };
        }
    }
}

