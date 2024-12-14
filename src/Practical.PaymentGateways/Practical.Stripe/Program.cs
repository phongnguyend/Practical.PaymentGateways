using Stripe;
using Stripe.Checkout;

var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

app.UseRouting();

StripeConfiguration.ApiKey = builder.Configuration["Stripe:SecretKey"];

app.MapGet("/", () =>
{
    var options = new SessionCreateOptions
    {
        PaymentMethodTypes = new List<string>
        {
            "card"
        },
        LineItems = new List<SessionLineItemOptions>
        {
            new SessionLineItemOptions
            {
                PriceData = new SessionLineItemPriceDataOptions
                {
                    Currency = "USD",
                    UnitAmount = 10 * 100,  // Amount in smallest currency unit (e.g., cents)
                    ProductData = new SessionLineItemPriceDataProductDataOptions
                    {
                        Name = "Product Name",
                        Description = "Product Description"
                    }
                },
                Quantity = 1
            }
        },
        Mode = "payment",
        SuccessUrl = $"{builder.Configuration["Stripe:RedirectUrl"]}/success/?sessionId={{CHECKOUT_SESSION_ID}}",
        CancelUrl = $"{builder.Configuration["Stripe:RedirectUrl"]}/cancel/?sessionId={{CHECKOUT_SESSION_ID}}"
    };

    var service = new SessionService();
    var session = service.Create(options);

    // TODO: store session.Id in database

    return Results.Redirect(session.Url);
});

app.MapGet("/success", async (string sessionId) =>
{
    var service = new SessionService();
    var session = await service.GetAsync(sessionId);
    return session;
});

app.MapGet("/cancel", async (string sessionId) =>
{
    var service = new SessionService();
    var session = await service.GetAsync(sessionId);
    return session;
});

app.Run();

