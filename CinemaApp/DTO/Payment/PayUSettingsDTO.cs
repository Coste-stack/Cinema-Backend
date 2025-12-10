namespace CinemaApp.DTO;

public class PayUSettingsDTO
{
    public string MerchantPosId { get; set; } = string.Empty;
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public string SecondKey { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = string.Empty;
    public string NotifyUrl { get; set; } = string.Empty;
    public string ContinueUrl { get; set; } = string.Empty;
}
