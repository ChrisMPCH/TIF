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
    [Authorize(Roles = "admin")]
    public class RFIDController : Controller
    {
        private readonly ApplicationDbContext _context;

        public RFIDController(ApplicationDbContext context)
        {
            _context = context;
        }

        //Controlador para obtener los datos de un usuario y actualizar estado de Activo (usado en Admin/RegistroRFID)
        [HttpGet]
        public IActionResult ObtenerDatosUsuario(string claveSocio)
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

                if (respuesta.estadoMembresia.Equals("Pagada"))
                {
                    if (usuario.Activo)
                    {
                        CambiarEstadoActivo(usuario.Id, false);

                        var accesoActivo = _context.Accesos.FirstOrDefault(a => a.Usuario.Id == usuario.Id && a.FechaHoraSalida == null);
                        // Si está adentro
                        accesoActivo.FechaHoraSalida = DateTime.Now;
                        _context.SaveChanges();
                    }
                    else
                    {
                        CambiarEstadoActivo(usuario.Id, true);
                        // Si no está adentro, registramos su entrada
                        var nuevoAcceso = new Acceso
                        {
                            Usuario = usuario,
                            FechaHoraEntrada = DateTime.Now
                        };
                        _context.Accesos.Add(nuevoAcceso);
                        _context.SaveChanges();
                    }
                }
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

            if (usuario.Activo)
            {
                CambiarEstadoActivo(usuario.Id, false);

                var accesoActivo = _context.Accesos.FirstOrDefault(a => a.Usuario.Id == usuario.Id && a.FechaHoraSalida == null);
                // Si está adentro
                accesoActivo.FechaHoraSalida = DateTime.Now;
                _context.SaveChanges();
            }
            else
            {
                CambiarEstadoActivo(usuario.Id, true);
                // Si no está adentro, registramos su entrada
                var nuevoAcceso = new Acceso
                {
                    Usuario = usuario,
                    FechaHoraEntrada = DateTime.Now
                };
                _context.Accesos.Add(nuevoAcceso);
                _context.SaveChanges();
            }

            return Ok(respuesta1);
        }


        //Controlador para obtener los datos de un usuario y actualizar estado de Activo (usado en Admin/RegistroRFID) Socio
        [Authorize] // Cambiado para que todos los usuarios autenticados puedan acceder
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



        //Controlador para actualizar estado de Activo (usado en Admin/RegistroRFID)
        public IActionResult CambiarEstadoActivo(String clienteId, bool estado)
        {
            var cliente = _context.Users.FirstOrDefault(c => c.Id == clienteId);
            if (cliente == null)
            {
                return Json(new { success = false, message = "Cliente no encontrado." });
            }

            cliente.Activo = estado; //false
            _context.SaveChanges();

            return RedirectToAction("ClientesActivos");
        }
    }
}

