using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;

public class CheckoutModel : PageModel
{
    private readonly IConfiguration _configuration;
    public string PublishableKey { get; private set; }

    public CheckoutModel(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public void OnGet()
    {
        PublishableKey = _configuration["Stripe:PublishableKey"] ?? string.Empty;
    }
}
