using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace CinemaApp.DTO;

// Frontend request DTO
public class CreatePaymentRequest
{
    [Required]
    public int BookingId { get; set; }
}

// PayU api request
public class PayUOrderRequest
{
    [JsonPropertyName("notifyUrl")]
    public string NotifyUrl { get; set; } = string.Empty;
    
    [JsonPropertyName("customerIp")]
    public string CustomerIp { get; set; } = string.Empty;
    
    [JsonPropertyName("merchantPosId")]
    public string MerchantPosId { get; set; } = string.Empty;
    
    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;
    
    [JsonPropertyName("currencyCode")]
    public string CurrencyCode { get; set; } = "PLN";
    
    [JsonPropertyName("totalAmount")]
    public string TotalAmount { get; set; } = string.Empty;
    
    [JsonPropertyName("continueUrl")]
    public string ContinueUrl { get; set; } = string.Empty;
    
    [JsonPropertyName("buyer")]
    public PayUBuyer? Buyer { get; set; }
    
    [JsonPropertyName("products")]
    public List<PayUProduct> Products { get; set; } = new();
    
    [JsonPropertyName("extOrderId")]
    public string? ExtOrderId { get; set; } // Internal booking ID
}

public class PayUBuyer
{
    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;
    
    [JsonPropertyName("phone")]
    public string? Phone { get; set; }
    
    [JsonPropertyName("firstName")]
    public string? FirstName { get; set; }
    
    [JsonPropertyName("lastName")]
    public string? LastName { get; set; }
}

public class PayUProduct
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
    
    [JsonPropertyName("unitPrice")]
    public string UnitPrice { get; set; } = string.Empty;
    
    [JsonPropertyName("quantity")]
    public string Quantity { get; set; } = string.Empty;
}

// PayU order response
public class PayUOrderResponse
{
    [JsonPropertyName("status")]
    public PayUStatus? Status { get; set; }
    
    [JsonPropertyName("redirectUri")]
    public string? RedirectUri { get; set; }
    
    [JsonPropertyName("orderId")]
    public string? OrderId { get; set; }
    
    [JsonPropertyName("extOrderId")]
    public string? ExtOrderId { get; set; }
    
    [JsonPropertyName("iframeAllowed")]
    public bool? IframeAllowed { get; set; }
}

public class PayUStatus
{
    [JsonPropertyName("statusCode")]
    public string? StatusCode { get; set; }
    
    [JsonPropertyName("statusDesc")]
    public string? StatusDesc { get; set; }
}

// PayU token response
public class PayUTokenResponse
{
    [JsonPropertyName("access_token")]
    public string? AccessToken { get; set; }
    
    [JsonPropertyName("token_type")]
    public string? TokenType { get; set; }
    
    [JsonPropertyName("expires_in")]
    public int ExpiresIn { get; set; }
    
    [JsonPropertyName("grant_type")]
    public string? GrantType { get; set; }
}


public class PayUNotification
{
    [JsonPropertyName("order")]
    public PayUOrder? Order { get; set; }
}

public class PayUOrder
{
    [JsonPropertyName("orderId")]
    public string? OrderId { get; set; }
    
    [JsonPropertyName("extOrderId")]
    public string? ExtOrderId { get; set; }
    
    [JsonPropertyName("orderCreateDate")]
    public DateTime? OrderCreateDate { get; set; }
    
    [JsonPropertyName("notifyUrl")]
    public string? NotifyUrl { get; set; }
    
    [JsonPropertyName("customerIp")]
    public string? CustomerIp { get; set; }
    
    [JsonPropertyName("merchantPosId")]
    public string? MerchantPosId { get; set; }
    
    [JsonPropertyName("description")]
    public string? Description { get; set; }
    
    [JsonPropertyName("currencyCode")]
    public string? CurrencyCode { get; set; }
    
    [JsonPropertyName("totalAmount")]
    public string? TotalAmount { get; set; }

    [JsonPropertyName("buyer")]
    public PayUBuyer? Buyer { get; set; }
    
    [JsonPropertyName("status")]
    public string? Status { get; set; } // PENDING, WAITING_FOR_CONFIRMATION, COMPLETED, CANCELED
}
