using Microsoft.AspNetCore.Mvc;
using PaypalServerSdk.Standard;
using PaypalServerSdk.Standard.Authentication;
using PaypalServerSdk.Standard.Http.Response;
using PaypalServerSdk.Standard.Models;

namespace Practical.PayPal.Controllers;

[Route("api/paypal")]
[ApiController]
public class PayPalController : ControllerBase
{
    private readonly PaypalServerSdkClient _payPalClient;

    private readonly Microsoft.Extensions.Configuration.IConfiguration _configuration;

    private string? PaypalClientId
    {
        get { return _configuration["PayPal:ClientId"]; }
    }

    private string? PaypalClientSecret
    {
        get { return _configuration["PayPal:ClientSecret"]; }
    }

    private PaypalServerSdk.Standard.Environment PaypalEnvironment
    {
        get { return _configuration["PayPal:Environment"] switch
        {
            "Sandbox" => PaypalServerSdk.Standard.Environment.Sandbox,
            "Production" => PaypalServerSdk.Standard.Environment.Production,
            _ => throw new ArgumentException("Invalid enum value for Environment", "Environment"),
        }; }
    }

    private readonly ILogger<PayPalController> _logger;

    public PayPalController(Microsoft.Extensions.Configuration.IConfiguration configuration, ILogger<PayPalController> logger)
    {
        _configuration = configuration;
        _logger = logger;

        // Initialize the PayPal SDK client
        _payPalClient = new PaypalServerSdkClient.Builder()
            .Environment(PaypalEnvironment)
            .ClientCredentialsAuth(
                new ClientCredentialsAuthModel.Builder(PaypalClientId, PaypalClientSecret).Build()
            )
            .LoggingConfig(config =>
                config
                    .LogLevel(LogLevel.Information)
                    .RequestConfig(reqConfig => reqConfig.Body(true))
                    .ResponseConfig(respConfig => respConfig.Headers(true))
            )
            .Build();
    }

    [HttpPost("orders")]
    public async Task<IActionResult> CreateOrder([FromBody] dynamic cart)
    {
        try
        {
            var ordersCreateInput = new OrdersCreateInput
            {
                Body = new OrderRequest
                {
                    Intent = CheckoutPaymentIntent.Capture,
                    PurchaseUnits = new List<PurchaseUnitRequest>
                    {
                        new PurchaseUnitRequest
                        {
                            Amount = new AmountWithBreakdown { CurrencyCode = "USD", MValue = "10", },
                        },
                    },
                },
            };

            ApiResponse<Order> result = await _payPalClient.OrdersController.OrdersCreateAsync(ordersCreateInput);
            return StatusCode(result.StatusCode, result.Data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create order:");
            return StatusCode(500, new { error = "Failed to create order." });
        }
    }

    [HttpPost("orders/{orderID}/capture")]
    public async Task<IActionResult> CaptureOrder(string orderID)
    {
        try
        {
            var ordersCaptureInput = new OrdersCaptureInput { Id = orderID, };

            ApiResponse<Order> result = await _payPalClient.OrdersController.OrdersCaptureAsync(ordersCaptureInput);
            return StatusCode(result.StatusCode, result.Data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to capture order:");
            return StatusCode(500, new { error = "Failed to capture order." });
        }
    }
}
