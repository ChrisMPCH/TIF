using GYMISFAMILY.Data;
using GYMISFAMILY.Models;
using GYMISFAMILY.Models.BaseDeDatos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace GYMISFAMILY.Controllers
{
    //----------------------------------------------------------------------------------------RFID
    //Controlador para todas las funciones donde se este usando RFID
    public class SociosController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SociosController(ApplicationDbContext context)
        {
            _context = context;
        }

        //Vista Ventana Registro RFID Socio
        public IActionResult RegistroRFIDSocio()
        {
            return View();
        }


        //Controlador para obtener los datos de un usuario y actualizar estado de Activo (usado en Admin/RegistroRFID) Socio
        [HttpGet]
        public IActionResult ObtenerDatosUsuarioSocio(string claveSocio)
        {
            var usuario = _context.Users.FirstOrDefault(u => u.RFID == claveSocio);

            if (usuario == null)
            {
                return NotFound(new { message = "Usuario no encontrado." });
            }

            if (!(bool)usuario.EsEmpleado)
            {
                var membresia = _context.MembresiasUsuarios
                    .Include(m => m.TipoMembresia)
                    .Where(m => m.Usuario.Id == usuario.Id)
                    .OrderByDescending(m => m.FechaInicio)
                    .FirstOrDefault();

                if (membresia == null)
                {
                    return NotFound(new { message = "Usuario no encontrado." });
                }

                membresia = _context.MembresiasUsuarios
                        .Include(m => m.TipoMembresia)
                        .Where(m => m.Usuario.Id == usuario.Id)
                        .OrderByDescending(m => m.FechaInicio)
                        .Include(m => m.TipoMembresia)
                        .Where(m => m.Estatus == EstatusMembresia.Pagada)
                        .FirstOrDefault() ?? _context.MembresiasUsuarios
                        .Include(m => m.TipoMembresia)
                        .Where(m => m.Usuario.Id == usuario.Id)
                        .OrderByDescending(m => m.FechaInicio)
                        .Include(m => m.TipoMembresia)
                        .Where(m => m.Estatus == EstatusMembresia.Cancelada)
                        .FirstOrDefault() ?? _context.MembresiasUsuarios
                        .Include(m => m.TipoMembresia)
                        .Where(m => m.Usuario.Id == usuario.Id)
                        .OrderByDescending(m => m.FechaInicio)
                        .Include(m => m.TipoMembresia)
                        .Where(m => m.Estatus == EstatusMembresia.Vencida)
                        .FirstOrDefault() ?? _context.MembresiasUsuarios
                        .Include(m => m.TipoMembresia)
                        .Where(m => m.Usuario.Id == usuario.Id)
                        .OrderByDescending(m => m.FechaInicio)
                        .Include(m => m.TipoMembresia)
                        .Where(m => m.Estatus == EstatusMembresia.Sin_pagar)
                        .FirstOrDefault();

                var respuesta = new
                {
                    nombre = usuario.Nombre,
                    paterno = usuario.Apellido_P,
                    materno = usuario.Apellido_P,
                    foto = usuario.ImageFileName,
                    vencimiento = membresia?.FechaFin.ToString("yyyy-MM-dd"),
                    adeudo = membresia?.Estatus == EstatusMembresia.Sin_pagar ? membresia.TipoMembresia.Precio : 0,
                    estadoMembresia = membresia?.Estatus.ToString()
                };

                return Ok(respuesta);
            }

            var respuesta1 = new
            {
                nombre = usuario.Nombre,
                paterno = usuario.Apellido_P,
                materno = usuario.Apellido_P,
                foto = usuario.ImageFileName,
                vencimiento = "Es empleado",
                adeudo = 0,
                estadoMembresia = "Pagada"
            };

            return Ok(respuesta1);
        }
    }
}

