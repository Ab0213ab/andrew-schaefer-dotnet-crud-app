using System.Diagnostics;
using AquentChallenge.Models;
using Microsoft.AspNetCore.Mvc;

namespace AquentChallenge.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }


        public IActionResult Index()
        {
            _logger.LogInformation("GET Home Index called");
            return View();
        }

    }
}
