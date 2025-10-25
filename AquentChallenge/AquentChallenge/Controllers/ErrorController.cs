using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace AquentChallenge.Controllers
{
    public class ErrorController : Controller
    {
        private readonly ILogger<ErrorController> _logger;

        public ErrorController(ILogger<ErrorController> logger)
        {
            _logger = logger;
        }

        [Route("Error/Error")]
        public IActionResult Error()
        {
            var feature = HttpContext.Features.Get<IExceptionHandlerFeature>();
            if (feature != null)
            {
                _logger.LogError(feature.Error,
                    "Unhandled exception occurred. Request ID: {RequestId}",
                    HttpContext.TraceIdentifier);
            }

            return View();
        }

        [Route("Error/StatusCode")]
        public IActionResult StatusCodePage(int code)
        {
            if (code >= 400)
            {
                _logger.LogWarning("HTTP {StatusCode} returned for path {Path}. Request ID: {RequestId}",
                    code,
                    HttpContext.Request.Path,
                    HttpContext.TraceIdentifier);
            }

            return View("StatusCode");
        }
    }
}
