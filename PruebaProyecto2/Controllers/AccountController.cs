using GYMISFAMILY.Models.BaseDeDatos;
using GYMISFAMILY.Models.DTOs;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace PruebaProyecto2.Controllers
{
    //----------------------------------------------------------------------------------------Tienda
    //Controlador para los registros de los clientes tienda
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManagerT;
        private readonly SignInManager<ApplicationUser> _signInManagerT;

        public AccountController(UserManager<ApplicationUser> userManagerT,
        SignInManager<ApplicationUser> signInManagerT)
        {
            this._userManagerT = userManagerT;
            this._signInManagerT = signInManagerT;

        }
        public IActionResult RegistroT()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> RegistroT(RegistroTDTO registroDTO)
        {
            if (!ModelState.IsValid)
            {
                return View(registroDTO);
            }

            // Crear una nueva cuenta y autenticar al usuario
            var user = new ApplicationUser
            {
                Email = registroDTO.Email,
                UserName = registroDTO.Email, // Necesario para Identity
                Nombre = registroDTO.Nombre,
                Apellido_P = registroDTO.Apellido_P,
                Apellido_M = registroDTO.Apellido_M,
                Telefono = registroDTO.Telefono,
                Rol = RolUsuario.clientT,
                FechaCreación = DateTime.Now
            };

            var result = await _userManagerT.CreateAsync(user, registroDTO.Password);

            if (result.Succeeded)
            {
                return RedirectToAction("Index", "Home");
            }

            // Registro fallido => mostrar errores de registro
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            return View(registroDTO);
        }


    }
}

