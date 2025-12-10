using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using CinemaApp.DTO;
using Microsoft.Extensions.Options;

namespace CinemaApp.Service;

public interface IPayUService
{
    Task<PayUOrderResponse> CreateOrderAsync(PayUOrderRequest request);
    bool ValidateNotification(string body, string signature);
}

public class PayUService : IPayUService
{
    private readonly HttpClient _httpClient;
    private readonly PayUSettingsDTO _settings;
    private readonly ILogger<PayUService> _logger;
    private string? _cachedToken;
    private DateTime _tokenExpiry = DateTime.MinValue;

    public PayUService(HttpClient httpClient, IOptions<PayUSettingsDTO> settings, ILogger<PayUService> logger)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
        _logger = logger;
    }

    private async Task<string> GetAccessTokenAsync()
    {
        // Return cached token if still valid
        if (_cachedToken != null && DateTime.UtcNow < _tokenExpiry)
            return _cachedToken;

        try
        {
            var request = new HttpRequestMessage(HttpMethod.Post, "/pl/standard/user/oauth/authorize");
            request.Content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("grant_type", "client_credentials"),
                new KeyValuePair<string, string>("client_id", _settings.ClientId),
                new KeyValuePair<string, string>("client_secret", _settings.ClientSecret)
            });

            var response = await _httpClient.SendAsync(request);
            var responseBody = await response.Content.ReadAsStringAsync();
            
            _logger.LogDebug("PayU token response: Status={Status}", response.StatusCode);
            
            response.EnsureSuccessStatusCode();

            var tokenResponse = JsonSerializer.Deserialize<PayUTokenResponse>(responseBody);

            if (tokenResponse?.AccessToken == null)
                throw new Exception("Failed to get access token from PayU");

            _cachedToken = tokenResponse.AccessToken;
            _tokenExpiry = DateTime.UtcNow.AddSeconds(tokenResponse.ExpiresIn - 60); // 60s buffer

            _logger.LogDebug("PayU access token obtained successfully");
            return _cachedToken;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to obtain PayU access token");
            throw;
        }
    }

    public async Task<PayUOrderResponse> CreateOrderAsync(PayUOrderRequest orderRequest)
    {
        try
        {
            var token = await GetAccessTokenAsync();
            
            var request = new HttpRequestMessage(HttpMethod.Post, "/api/v2_1/orders");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            
            // Add X-OpenPayU-Signature header for REST API
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
            request.Headers.Add("X-OpenPayU-Signature", $"sender=checkout;timestamp={timestamp}");
            
            var jsonOptions = new JsonSerializerOptions 
            { 
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
            };
            
            var jsonContent = JsonSerializer.Serialize(orderRequest, jsonOptions);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
            
            // Add PayU headers
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json") 
            { 
                CharSet = "utf-8" 
            };
            
            request.Content = content;

            _logger.LogDebug("Creating PayU order for extOrderId: {ExtOrderId}, amount: {Amount}", 
                orderRequest.ExtOrderId, orderRequest.TotalAmount);

            var response = await _httpClient.SendAsync(request);
            var responseBody = await response.Content.ReadAsStringAsync();

            _logger.LogDebug("PayU order response: Status={Status}, ContentType={ContentType}", 
                response.StatusCode,
                response.Content.Headers.ContentType?.MediaType ?? "unknown");

            // PayU 302 Found JSON body with order details
            if (response.StatusCode == System.Net.HttpStatusCode.Found)
            {
                PayUOrderResponse? orderResponse;
                try
                {
                    orderResponse = JsonSerializer.Deserialize<PayUOrderResponse>(responseBody, jsonOptions);
                }
                catch (JsonException e)
                {
                    _logger.LogError(e, "Failed to deserialize PayU response JSON. Body: {Body}", 
                        responseBody.Length > 500 ? responseBody.Substring(0, 500) : responseBody);
                    throw new Exception($"Failed to parse PayU response: {e.Message}");
                }

                if (orderResponse == null)
                {
                    _logger.LogError("PayU response deserialized to null");
                    throw new Exception("Invalid PayU response: deserialized object was null");
                }

                _logger.LogInformation("PayU order created successfully: OrderId={OrderId}, ExtOrderId={ExtOrderId}, Amount={Amount} PLN", 
                    orderResponse.OrderId, orderRequest.ExtOrderId, decimal.Parse(orderRequest.TotalAmount) / 100);

                return orderResponse;
            }

            // Error responses
            _logger.LogError("PayU order creation failed: {StatusCode} - {Body}", 
                response.StatusCode, responseBody);
            throw new Exception($"PayU API error: {response.StatusCode} - {responseBody}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating PayU order");
            throw;
        }
    }

    public bool ValidateNotification(string body, string signature)
    {
        try
        {   
            if (string.IsNullOrEmpty(signature))
            {
                _logger.LogWarning("Empty signature received");
                return false;
            }

            // For sandbox/testing, you can use SecondKey MD5 validation
            var expectedSignature = ComputeMD5Signature(body + _settings.SecondKey);
            
            // Extract the hash part from signature header
            var signatureParts = signature.Split(';');
            var hashPart = signatureParts.FirstOrDefault(p => p.StartsWith("signature="))?.Replace("signature=", "");
            
            if (hashPart == null)
            {
                _logger.LogWarning("No signature hash found in header");
                return false;
            }

            var isValid = expectedSignature.Equals(hashPart, StringComparison.OrdinalIgnoreCase);
            
            if (!isValid)
            {
                _logger.LogWarning("Signature validation failed. Expected: {Expected}, Received: {Received}", 
                    expectedSignature, hashPart);
            }

            return isValid;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating notification signature");
            return false;
        }
    }

    private static string ComputeMD5Signature(string input)
    {
        var bytes = Encoding.UTF8.GetBytes(input);
        var hash = MD5.HashData(bytes);
        return BitConverter.ToString(hash).Replace("-", "").ToLower();
    }

    private static string? ExtractOrderIdFromUrl(string url)
    {
        try
        {
            var uri = new Uri(url);
            
            // Try query string first (e.g., ?token=XXX)
            var query = System.Web.HttpUtility.ParseQueryString(uri.Query);
            var token = query["token"];
            if (!string.IsNullOrEmpty(token))
                return token;

            // Try path segments (e.g., /pay/XXX or /summary/XXX)
            var segments = uri.Segments;
            if (segments.Length > 0)
            {
                var lastSegment = segments[^1].TrimEnd('/');
                // PayU uses token as orderId in URLs
                if (lastSegment.Length > 10) // Reasonable length for a token
                    return lastSegment;
            }

            return null;
        }
        catch
        {
            return null;
        }
    }
}