using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using GYMISFAMILY.Extensions;
using System.Diagnostics;

namespace GYMISFAMILY.Controllers
{
    public class ErrorController : Controller
    {
        [Route("Error")]
        public IActionResult Error()
        {
            var exceptionFeature = HttpContext.Features.Get<IExceptionHandlerPathFeature>();

            var errorModel = new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier,
                ErrorMessage = exceptionFeature?.Error.Message,
                SolutionHint = "Intenta nuevamente o contacta a soporte.",
                StackTrace = exceptionFeature?.Error.StackTrace
            };

            return View(errorModel); // Manda a la vista Error.cshtml
        }
    }
}
