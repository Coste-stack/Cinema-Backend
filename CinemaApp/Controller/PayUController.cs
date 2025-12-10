using CinemaApp.DTO;
using CinemaApp.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Text.Json;

namespace CinemaApp.Controller
{
    [ApiController]
    [Route("[controller]")]
    public class PayUController(
        IPayUService payUService,
        IBookingService bookingService,
        IConfiguration configuration,
        ILogger<PayUController> logger) : ControllerBase
    {
        private readonly IPayUService _payUService = payUService;
        private readonly IBookingService _bookingService = bookingService;
        private readonly IConfiguration _configuration = configuration;
        private readonly ILogger<PayUController> _logger = logger;

        [HttpPost("order")]
        [AllowAnonymous]
        public async Task<IActionResult> CreatePayment([FromBody] CreatePaymentRequest request)
        {
            try
            {
                var booking = _bookingService.GetById(request.BookingId);
                
                // Calculate total using lowest currency unit
                var totalAmount = booking.Tickets.Sum(t => t.TotalPrice);
                var amountInGrosze = ((int)(totalAmount * 100)).ToString();

                var customerIp = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1";

                var payURequest = new PayUOrderRequest
                {
                    NotifyUrl = _configuration["PayU:NotifyUrl"]!,
                    CustomerIp = customerIp,
                    MerchantPosId = _configuration["PayU:MerchantPosId"]!,
                    Description = $"Cinema Booking #{booking.Id}",
                    CurrencyCode = "PLN",
                    TotalAmount = amountInGrosze,
                    ContinueUrl = $"{_configuration["PayU:ContinueUrl"]}?bookingId={booking.Id}",
                    ExtOrderId = booking.Id.ToString(),
                    Buyer = new PayUBuyer
                    {
                        Email = booking.User?.Email ?? "guest@cinema.app",
                        FirstName = booking.User?.FirstName,
                        LastName = booking.User?.LastName
                    },
                    Products = new List<PayUProduct>
                    {
                        new()
                        {
                            Name = $"Cinema Tickets - {booking.Screening?.Movie?.Title ?? "Movie"}",
                            UnitPrice = amountInGrosze,
                            Quantity = "1"
                        }
                    }
                };

                var response = await _payUService.CreateOrderAsync(payURequest);

                // Store PayU order ID in booking
                _bookingService.UpdatePaymentInfo(booking.Id, response.OrderId!, totalAmount);

                _logger.LogInformation("Payment order created for booking {BookingId}, PayU OrderId: {OrderId}", 
                    booking.Id, response.OrderId);

                return Ok(new
                {
                    redirectUri = response.RedirectUri,
                    orderId = response.OrderId,
                    bookingId = booking.Id
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Payment creation failed for booking {BookingId}", request.BookingId);
                return StatusCode(500, new { error = "Payment creation failed", details = ex.Message });
            }
        }

        [HttpPost("notify")]
        [AllowAnonymous]
        public async Task<IActionResult> PayUNotification()
        {
            try
            {
                var body = await new StreamReader(Request.Body).ReadToEndAsync();
                var signature = Request.Headers["OpenPayu-Signature"].ToString();

                _logger.LogInformation("PayU notification received. Signature: {Signature}", signature);
                _logger.LogDebug("PayU notification body: {Body}", body);

                if (!_payUService.ValidateNotification(body, signature))
                {
                    _logger.LogWarning("Invalid PayU signature");
                    return Unauthorized(new { error = "Invalid signature" });
                }

                var notification = JsonSerializer.Deserialize<PayUNotification>(body,
                    new JsonSerializerOptions 
                    { 
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase 
                    });

                if (notification?.Order == null)
                {
                    _logger.LogWarning("Invalid notification structure");
                    return BadRequest(new { error = "Invalid notification" });
                }

                var order = notification.Order;
                _logger.LogInformation("Payment notification: OrderId={OrderId}, Status={Status}, ExtOrderId={ExtOrderId}", 
                    order.OrderId, order.Status, order.ExtOrderId);

                if (order.Status == "COMPLETED")
                {
                    try
                    {
                        _bookingService.ConfirmPayment(order.OrderId!);
                        _logger.LogInformation("Booking confirmed for PayU OrderId: {OrderId}", order.OrderId);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to confirm booking for PayU OrderId: {OrderId}", order.OrderId);
                    }
                }
                else if (order.Status == "CANCELED")
                {
                    _logger.LogInformation("Payment canceled for OrderId: {OrderId}", order.OrderId);
                    // Optionally cancel the booking
                    if (int.TryParse(order.ExtOrderId, out var bookingId))
                    {
                        try
                        {
                            _bookingService.CancelBooking(bookingId);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Failed to cancel booking {BookingId}", bookingId);
                        }
                    }
                }

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "PayU notification processing failed");
                return StatusCode(500, new { error = "Notification processing failed" });
            }
        }

        [HttpGet("status/{bookingId}")]
        [AllowAnonymous]
        public IActionResult GetPaymentStatus(int bookingId)
        {
            try
            {
                var booking = _bookingService.GetById(bookingId);
                
                return Ok(new
                {
                    bookingId = booking.Id,
                    status = booking.BookingStatus.ToString(),
                    payUOrderId = booking.PayUOrderId,
                    paymentAmount = booking.PaymentAmount,
                    paymentDate = booking.PaymentDate,
                    isPaid = booking.BookingStatus == Model.BookingStatus.Confirmed && booking.PaymentDate != null
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting payment status for booking {BookingId}", bookingId);
                return NotFound(new { error = "Booking not found" });
            }
        }
    }
}