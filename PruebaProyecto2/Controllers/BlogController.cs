using Microsoft.AspNetCore.Mvc;

namespace GYMISFAMILY.Controllers
{
    //----------------------------------------------------------------------------------------Blog
    //Controlador para las vistas del Blog
    public class BlogController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Rutina()
        {
            return View();
        }
        public IActionResult RutinaAdaptacion()
        {
            return View();
        }

        public IActionResult RutinaBasica()
        {
            return View();
        }

        public IActionResult RutinaIntermedia()
        {
            return View();
        }

        public IActionResult RutinaAvanzada()
        {
            return View();
        }

        public IActionResult Nutricion()
        {
            return View();
        }

        public IActionResult Calculator()
        {
            return View();
        }

        public IActionResult Tip()
        {
            return View();
        }
    }
}
