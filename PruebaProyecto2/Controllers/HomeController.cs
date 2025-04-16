using GYMISFAMILY.Data;
using GYMISFAMILY.Models.BaseDeDatos;
using GYMISFAMILY.Models.DTOs;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace GYMISFAMILY.Controllers
{
    //----------------------------------------------------------------------------------------Home
    //Controlador para todas las funciones de Login, logout y register de clientesT y clientesGYM E inicio de la pagina
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<HomeController> _logger;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public HomeController(ApplicationDbContext context, ILogger<HomeController> logger, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
        {
            _context = context;
            _logger = logger;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public IActionResult AccessDenied()
        {
            return View();
        }
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult InicioSesion()
        {
            return View(new LoginDTO());
        }

        [HttpPost]
        public async Task<IActionResult> InicioSesion(LoginDTO loginDTO)
        {
            if (!ModelState.IsValid)
            {
                return View(loginDTO);
            }

            // Verificamos si el usuario existe
            var result = await _signInManager.PasswordSignInAsync(loginDTO.Email,
                                                                  loginDTO.Password,
                                                                  loginDTO.RememberMe,
                                                                  lockoutOnFailure: false);
            
            var user = await _userManager.FindByEmailAsync(loginDTO.Email);

            if (result.Succeeded)
            {
                return RedirectToAction("Index", "Home");
            }
            else if (result.IsLockedOut)
            {
                ModelState.AddModelError("", "La cuenta está bloqueada. Intenta más tarde.");
            }
            else if (result.IsNotAllowed)
            {
                ModelState.AddModelError("", "El inicio de sesión no está permitido para este usuario.");
            }
            else
            {
                ModelState.AddModelError("", "Correo electrónico o contraseña inválidos.");
            }


            return View(loginDTO);
        }

        public async Task<IActionResult> Logout()
        {
            if (_signInManager.IsSignedIn(User))
            {
                await _signInManager.SignOutAsync();
            }
            return RedirectToAction("Index", "Home");
        }

        public IActionResult AvisodePrivacidad()
        {
            return View();
        }

        public IActionResult TerminosCondiciones()
        {
            return View();
        }

        public IActionResult NuestraMision()
        {
            return View();
        }

    }
}
