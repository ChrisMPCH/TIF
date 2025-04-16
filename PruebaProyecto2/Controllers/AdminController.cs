using GYMISFAMILY.Data;
using GYMISFAMILY.Models;
using GYMISFAMILY.Models.BaseDeDatos;
using GYMISFAMILY.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Collections.Generic;
using static Org.BouncyCastle.Math.EC.ECCurve;

namespace GYMISFAMILY.Controllers
{
    //----------------------------------------------------------------------------------------Admin
    //Controlador para todas las funciones de Administrador
    [Authorize(Roles = "admin")]
    public class AdminController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly SignInManager<ApplicationUser> _signInManager;
        IWebHostEnvironment _environment;
        private readonly IConfiguration _config;

        public AdminController(UserManager<ApplicationUser> userManager, ApplicationDbContext context, IWebHostEnvironment environment, 
            SignInManager<ApplicationUser> signInManager, IConfiguration config)
        {
            _userManager = userManager;
            _context = context;
            _environment = environment;
            _signInManager = signInManager;
            _config = config;
        }
        public IActionResult Registro()
        {
            return View();
        }

        //Registro de clientes GYM (opcion en Admin)
        [HttpPost]
        public async Task<IActionResult> Registro(RegistroDTO model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Verificar que la imagen no esté vacía
            if (string.IsNullOrEmpty(model.ImageBase64))
            {
                ModelState.AddModelError("ImageBase64", "La imagen es obligatoria.");
                return View(model);
            }

            // Convertir la cadena base64 a un archivo
            var base64String = model.ImageBase64.Split(',')[1];  // Eliminar el prefijo "data:image/jpeg;base64,"
            var imageBytes = Convert.FromBase64String(base64String);

            // Guardar la imagen en el servidor
            string imageFolderPath = Path.Combine(_environment.WebRootPath, "clienteImagen");
            if (!Directory.Exists(imageFolderPath))
            {
                Directory.CreateDirectory(imageFolderPath);
            }

            string fileName = DateTime.Now.ToString("yyyyMMddHHmmssfff") + ".jpg";
            string imagePath = Path.Combine(imageFolderPath, fileName);

            await System.IO.File.WriteAllBytesAsync(imagePath, imageBytes);
            ApplicationUser usuario;
            // Reemplazar la declaración de la variable implícita 'var result;' con una inicialización explícita.
            IdentityResult result;
            if (model.EsEmpleado)
            {
                usuario = new ApplicationUser
                {
                    UserName = model.RFID,
                    Email = model.RFID + "@correo.com",
                    Nombre = model.Nombre,
                    Apellido_P = model.Apellido_P,
                    Apellido_M = model.Apellido_M,
                    PhoneNumber = model.PhoneNumber,
                    Telefono = model.PhoneNumber,
                    RFID = model.RFID,
                    FechaDeNacimiento = model.FechaDeNacimiento,
                    ImageFileName = $"/clienteImagen/{fileName}",
                    Rol = RolUsuario.admin,
                    EsEmpleado = true,
                    FechaCreación = DateTime.Now
                };
            }
            else
            {
                // Crear el usuario
                usuario = new ApplicationUser
                {
                    UserName = model.RFID,
                    Email = model.RFID + "@correo.com",
                    Nombre = model.Nombre,
                    Apellido_P = model.Apellido_P,
                    Apellido_M = model.Apellido_M,
                    PhoneNumber = model.PhoneNumber,
                    Telefono = model.PhoneNumber,
                    RFID = model.RFID,
                    FechaDeNacimiento = model.FechaDeNacimiento,
                    ImageFileName = $"/clienteImagen/{fileName}",
                    Rol = RolUsuario.client,
                    EsEmpleado = false,
                    FechaCreación = DateTime.Now
                };
            }
            result = await _userManager.CreateAsync(usuario);

            if (result.Succeeded)
            {
                return RedirectToAction("Clientes", "Admin");
            }
            else
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return RedirectToAction("Clientes", "Admin");
        }

        //Print de todos los clientes con indice (vista Admin/Cliente/)
        public IActionResult Clientes(int page = 1)
        {
            int pageSize = 5; // Número de registros por página
            var clientes = _context.Users.Where(c => c.EsEmpleado == false).ToList(); // Obtiene todos los usuarios
            var clientesActivos = _context.Users.Where(c => c.Activo).ToList(); // Filtra los activos
            var emplados = _context.Users.Where(c => c.EsEmpleado == true).ToList(); // Filtra los empleados

            var viewModel = new ClientesViewModel
            {
                Clientes = clientes.Skip((page - 1) * pageSize).Take(pageSize).ToList(),
                ClientesActivos = clientesActivos,
                Empleados = emplados,
                PaginaActual = page,
                TotalPaginas = (int)Math.Ceiling(clientes.Count / (double)pageSize)
            };

            return View(viewModel);
        }

        //Cambiar el estado de los clientes Activo (usada en Admin/Cliente/)
        public IActionResult CambiarEstado(string clienteId, bool estado)
        {
            var cliente = _context.Users.FirstOrDefault(c => c.Id == clienteId);

            if (cliente == null)
            {
                TempData["Error"] = "Cliente no encontrado.";
                return RedirectToAction("Clientes");
            }

            try
            {
                cliente.Activo = estado;
                var accesoActivo = _context.Accesos.FirstOrDefault(a => a.Usuario.Id == cliente.Id && a.FechaHoraSalida == null);
                // Si está adentro
                accesoActivo.FechaHoraSalida = DateTime.Now;
                _context.SaveChanges();

                TempData["Success"] = "Estado actualizado correctamente.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Ocurrió un error al actualizar el estado.";
            }

            return RedirectToAction("Clientes");
        }

        //Eliminar a un cliente (usada en Admin/Cliente/)
        [HttpPost]
        public IActionResult Delete(string id)
        {
            var usuario = _context.Users.FirstOrDefault(u => u.Id == id);
            if (usuario == null)
            {
                return Json(new { success = false, message = "Cliente no encontrado" });
            }

            _context.Users.Remove(usuario);
            _context.SaveChanges();

            return Json(new { success = true, message = "Cliente eliminado correctamente" });
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            var usuario = await _userManager.FindByIdAsync(id);
            if (usuario == null) return NotFound();

            var dto = new ClienteEditDTO
            {
                Id = usuario.Id,
                Nombre = usuario.Nombre,
                Apellido_P = usuario.Apellido_P,
                Apellido_M = usuario.Apellido_M,
                Email = usuario.Email,
                PhoneNumber = usuario.PhoneNumber,
                FechaDeNacimiento = (DateTime)usuario.FechaDeNacimiento,
                RFID = usuario.RFID,
                EsEmpleado = (bool)usuario.EsEmpleado
            };

            return View(dto);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(ClienteEditDTO model)
        {
            // Validación personalizada para contraseña de empleados
            if (model.EsEmpleado && string.IsNullOrEmpty(model.Password))
            {
                ModelState.AddModelError("Password", "La contraseña es obligatoria para empleados");
            }

            if (!ModelState.IsValid) return View(model);

            var usuario = await _userManager.FindByIdAsync(model.Id);
            if (usuario == null) return NotFound();

            // Actualizar propiedades básicas
            usuario.Nombre = model.Nombre;
            usuario.Apellido_P = model.Apellido_P;
            usuario.Apellido_M = model.Apellido_M;
            usuario.Email = model.Email;
            usuario.UserName = model.Email;
            usuario.PhoneNumber = model.PhoneNumber;
            usuario.FechaDeNacimiento = model.FechaDeNacimiento;
            usuario.RFID = model.RFID;

            // Manejar cambio de rol PRIMERO (antes de la contraseña)
            if (model.EsEmpleado)
            {
                usuario.Rol = RolUsuario.admin;
                usuario.EsEmpleado = true;
            }
            else if (!model.EsEmpleado)
            {
                usuario.Rol = RolUsuario.client;
                usuario.EsEmpleado = false;
            }

            // Actualizar contraseña SOLO si es empleado y se proporcionó contraseña
            if (model.EsEmpleado && !string.IsNullOrEmpty(model.Password))
            {
                if (model.Password != model.ConfirmPassword)
                {
                    ModelState.AddModelError("ConfirmPassword", "Las contraseñas no coinciden");
                    return View(model);
                }
                usuario.PasswordHash = model.Password;
            }

            // Guardar cambios del usuario
            _context.SaveChanges();

            return RedirectToAction("Clientes", "Admin");
        }

        //Vista Ventana Registro RFID
        public IActionResult RegistroRFID()
        {
            return View();
        }

        //Vista Ventana Membresias
        public IActionResult PagoMembresia()
        {
            return View();
        }

        // Controlador para obtener los accesos con paginación
        [HttpGet]
        public IActionResult ObtenerAccesos(int page = 1)
        {
            int pageSize = 20;  // Cantidad de registros
            var accesos = _context.Accesos
                .Include(a=> a.Usuario)
                .OrderByDescending(a => a.FechaHoraEntrada) // Ordenar por fecha de entrada
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var totalAccesos = _context.Accesos.Count();
            var totalPages = (int)Math.Ceiling(totalAccesos / (double)pageSize);

            // Crear un modelo con los accesos y la información
            var model = new AccesosViewModel
            {
                Accesos = accesos,
                Page = page,
                TotalPages = totalPages
            };
            return View(model);
        }

        [HttpGet]
        [Route("api/getAdminPassword")]
        public IActionResult GetAdminPassword()
        {
            var password = _config["AppSettings:AdminPassword"];
            return Ok(new { password });
        }


    }



    //<!-- =========== Modelos =========  -->

    public class ClientesViewModel
    {
        public List<ApplicationUser> Clientes { get; set; }
        public List<ApplicationUser> ClientesActivos { get; set; }
        public List<ApplicationUser> Empleados { get; set; }

        public int PaginaActual { get; set; }
        public int TotalPaginas { get; set; }
    }
    public class AccesosViewModel
    {
        public List<Acceso> Accesos { get; set; }
        public int Page { get; set; }
        public int TotalPages { get; set; }
    }




}
