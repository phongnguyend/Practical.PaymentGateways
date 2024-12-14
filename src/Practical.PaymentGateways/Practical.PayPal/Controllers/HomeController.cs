using Microsoft.AspNetCore.Mvc;

namespace Practical.PayPal.Controllers;

public class HomeController : Controller
{
    private readonly IConfiguration _configuration;

    public HomeController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public IActionResult Index()
    {
        return View(_configuration);
    }
}
