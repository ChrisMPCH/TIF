using GYMISFAMILY.Data;
using GYMISFAMILY.Models.BaseDeDatos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GYMISFAMILY.Models;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using GYMISFAMILY.Extensions;
using System.Diagnostics;


namespace GYMISFAMILY.Controllers
{
    //----------------------------------------------------------------------------------------Membresias
    //Controlador para todas las funciones de Membresias y RFID
    [Authorize(Roles = "admin")]
    public class MembresiasController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MembresiasController(ApplicationDbContext context)
        {
            _context = context;
        }

        //Ver todas las membresias registradas
        public IActionResult Index()
        {
            var tiposMembresias = _context.TiposMembresia.ToList();
                return View(tiposMembresias);
        }

        //Ver ventana de create
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        //Controlador para poder crear un tipo de membresia (usada en Membresias/create)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TipoMembresia model)
        {
            if (ModelState.IsValid)
            {
                model.Tiempo = model.Tipo switch
                {
                    TipoMembresiaCategoria.Dias => model.Tiempo,
                    TipoMembresiaCategoria.Semanal => model.Tiempo * 7, 
                    TipoMembresiaCategoria.Mensual => model.Tiempo * 30,
                    _ => model.Tiempo
                };

                _context.Add(model);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        //Controlador para poder editar un tipo de membresia (usada en Membresias/edit)
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var tipoMembresia = _context.TiposMembresia.Find(id);
            if (tipoMembresia == null)
            {
                return NotFound();
            }

            var tiempoAjustado = tipoMembresia.Tipo switch
            {
                TipoMembresiaCategoria.Mensual => tipoMembresia.Tiempo / 30,
                TipoMembresiaCategoria.Semanal => tipoMembresia.Tiempo / 7,
                _ => tipoMembresia.Tiempo // Días por defecto
            };

            var model = new TipoMembresia
            {
                ID_TipoMembresia = tipoMembresia.ID_TipoMembresia,
                Nombre = tipoMembresia.Nombre,
                Tipo = tipoMembresia.Tipo,
                Tiempo = tiempoAjustado, // Tiempo ajustado para la edición
                Precio = tipoMembresia.Precio
            };

            return View(model);
        }

        //Controlador para poder editar un tipo de membresia pero solo para estandarizar (usada en Membresias/edit)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, TipoMembresia model)
        {
            if (id != model.ID_TipoMembresia)
            {
                return NotFound();
            }
            if (ModelState.IsValid)
            {
                    model.Tiempo = model.Tipo switch
                    {
                        TipoMembresiaCategoria.Dias => model.Tiempo,
                        TipoMembresiaCategoria.Semanal => model.Tiempo * 7,
                        TipoMembresiaCategoria.Mensual => model.Tiempo * 30,
                        _ => model.Tiempo
                    };
                    _context.Update(model);
                    await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        //Controlador para poder eliminar un tipo de membresia pero solo para estandarizar (usada en Membresias/edit)
        [HttpPost]
        public IActionResult Delete(int id)
        {
            var tipoMembresia = _context.TiposMembresia.Find(id);
            if (tipoMembresia == null)
            {
                return NotFound();
            }

            _context.TiposMembresia.Remove(tipoMembresia);
            _context.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        //----------------------------------------------------MembresiasUsuario---

        // Buscar un miembro por RFID o ID (Usado en Admin/PagoMembresia)
        [HttpGet]
        public IActionResult BuscarSocio(string rfid)
        {
            if (string.IsNullOrWhiteSpace(rfid))
            {
                return Json(new { success = false, message = "La clave es requerida." });
            }

            var user = _context.Users.FirstOrDefault(u => u.RFID == rfid || u.Id.ToString() == rfid);

            if (user == null)
            {
                return Json(new { success = false, message = "No se encontró un socio con la clave proporcionada." });
            }

            var fotoPath = string.IsNullOrWhiteSpace(user.ImageFileName)
            ? "default-user.webp" // Imagen predeterminada
            : $"{user.ImageFileName}";

            return Json(new
            {
                success = true,
                data = new
                {
                    nombreCompleto = $"{user.Nombre} {user.Apellido_P} {user.Apellido_M}",
                    telefono = user.Telefono,
                    foto = fotoPath
                }
            });
        }

        // Buscar y obtener membreasias registradas (Usado en Admin/PagoMembresia)
        [HttpGet]
        public IActionResult ObtenerMembresias()
        {
            var membresias = _context.TiposMembresia
                .Select(m => new
                {
                    m.ID_TipoMembresia,
                    m.Nombre,
                    m.Precio,
                    m.Tipo,
                    m.Tiempo
                })
                .ToList();

            return Json(membresias);
        }

        // Agregar una membreasia a un usuario (Usado en Admin/PagoMembresia)
        [HttpPost]
        public IActionResult AgregarMembresia([FromBody] MembresiaUsuario nuevaMembresia)
        {
            try
            {
                // Validar datos
                if (nuevaMembresia == null || nuevaMembresia.ID_TipoMembresia <= 0)
                    return Json(new { success = false, message = "Datos inválidos." });

                // Buscar usuario por su ID
                var usuario = _context.Users.FirstOrDefault(u => u.RFID == nuevaMembresia.Usuario.Id);
                if (usuario == null)
                    return Json(new { success = false, message = "Usuario no encontrado." });

                // Buscar el tipo de membresía
                var tipoMembresia = _context.TiposMembresia.FirstOrDefault(tm => tm.ID_TipoMembresia == nuevaMembresia.ID_TipoMembresia);
                if (tipoMembresia == null)
                    return Json(new { success = false, message = "Tipo de membresía no válido." });

                // Calcular FechaFin basada en el tiempo de la membresía
                var fechaInicio = nuevaMembresia.FechaInicio;
                DateTime fechaFin = fechaInicio;

                switch (tipoMembresia.Tipo)
                {
                    case 0: // Día
                        fechaFin = fechaInicio.AddDays(tipoMembresia.Tiempo);
                        break;
                    case (TipoMembresiaCategoria)1: // Semana
                        fechaFin = fechaInicio.AddDays(tipoMembresia.Tiempo);
                        break;
                    case (TipoMembresiaCategoria)2: // Mes
                        fechaFin = fechaInicio.AddDays(tipoMembresia.Tiempo);
                        break;
                    default:
                        return Json(new { success = false, message = "Tipo de membresía no reconocido." });
                }

                // Crear la nueva membresía del usuario
                var nuevaMembresiaUsuario = new MembresiaUsuario
                {
                    Usuario = usuario,
                    ID_TipoMembresia = tipoMembresia.ID_TipoMembresia,
                    TipoMembresia = tipoMembresia,
                    Estatus = EstatusMembresia.Sin_pagar, // Estado inicial
                    FechaInicio = fechaInicio,
                    FechaFin = fechaFin
                };

                // Guardar en la base de datos
                _context.MembresiasUsuarios.Add(nuevaMembresiaUsuario);
                _context.SaveChanges();

                return Json(new { success = true, message = "Membresía agregada correctamente." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error al agregar la membresía: " + ex.Message });
            }
        }

        // Obtener las membreasias de un usuario (Usado en Admin/PagoMembresia)
        [HttpGet]
        public IActionResult ObtenerMembresiasPorUsuario(string userKey)
        {
            if (string.IsNullOrEmpty(userKey))
            {
                return Json(new { success = false, message = "Clave de usuario no proporcionada." });
            }

            var usuario = _context.Users.FirstOrDefault(u => u.RFID == userKey);

            if (usuario == null)
            {
                return Json(new { success = false, message = "Usuario no encontrado." });
            }

            var membresias = _context.MembresiasUsuarios
                .Where(m => m.Usuario.RFID == userKey)
                .Select(m => new
                {
                    m.FechaInicio,
                    m.FechaFin,
                    TipoMembresia = new
                    {
                        m.TipoMembresia.Nombre,
                        m.TipoMembresia.Precio
                    },
                    m.Estatus,
                    m.ID_MembresiaUsuario
                }).OrderByDescending(m => m.FechaFin)
                .ToList();

            return Json(new { success = true, membresias });
        }

        // Actualizar membreasia seleccionada pagada o cancelada (Usado en Admin/PagoMembresia)
        [HttpPost]
        public async Task<IActionResult> ActualizarEstadoAsync([FromBody] ActualizarEstadoRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.NuevoEstado))
            {
                return Json(new { success = false, message = "Datos inválidos." });
            }

            var membresia = _context.MembresiasUsuarios.Include(m=> m.Usuario).Include(m => m.TipoMembresia).FirstOrDefault(m => m.ID_MembresiaUsuario == request.Id);

            if (membresia == null)
            {
                return Json(new { success = false, message = "Membresía no encontrada." });
            }

            if (!Enum.TryParse<EstatusMembresia>(request.NuevoEstado, out var nuevoEstado))
            {
                return Json(new { success = false, message = "Estado no válido." });
            }

            if (membresia.Estatus == EstatusMembresia.Pagada && nuevoEstado == EstatusMembresia.Pagada)
            {
                return Json(new { success = false, message = "No puedes pagar una membresía ya pagada." });
            }

            if (membresia.Estatus == EstatusMembresia.Cancelada && nuevoEstado == EstatusMembresia.Cancelada)
            {
                return Json(new { success = false, message = "No puedes cancelar una membresía ya cancelada." });
            }

            if (membresia.Estatus == EstatusMembresia.Cancelada && nuevoEstado == EstatusMembresia.Pagada)
            {
                return Json(new { success = false, message = "No puedes pagar una membresía cancelada." });
            }

            if (membresia.Estatus == EstatusMembresia.Vencida && nuevoEstado == EstatusMembresia.Pagada)
            {
                return Json(new { success = false, message = "No puedes pagar una membresía vencida." });
            }

            //if (membresia.Estatus == EstatusMembresia.Pagada && nuevoEstado == EstatusMembresia.Cancelada)
            //{
            //    return Json(new { success = false, message = "No puedes cancelar una membresía pagada." });
            //}

            if (membresia.Estatus == EstatusMembresia.Vencida && nuevoEstado == EstatusMembresia.Cancelada)
            {
                return Json(new { success = false, message = "No puedes cancelar una membresía vencida." });
            }

            if (nuevoEstado == EstatusMembresia.Pagada)
            {
                // Generar e imprimir ticket
                string ticketText = GenerarTicket(membresia);
                string imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Images", "LogoTIFsinfondo.png");

                TicketPrinter printer = new TicketPrinter();
                printer.Print(ticketText, imagePath);
                Thread.Sleep(5000);
                printer.Print(ticketText, imagePath);

                try
                {
                    var venta = new Venta
                    {
                        Usuario = membresia.Usuario,
                        Fecha = DateTime.Now,
                        Total = membresia.TipoMembresia.Precio,
                        DetallesVentas = new List<DetallesVenta>()
                    };
                    _context.Ventas.Add(venta);

                }
                catch (Exception ex)
                {
                    return Json(new
                    {
                        success = false,
                        message = "Ocurrió un error al guardar la venta.",
                        error = ex.Message
                    });
                }
            }
            membresia.Estatus = nuevoEstado;
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = $"La membresía fue marcada como {nuevoEstado} correctamente." });

        }

        // Actualizar membreasia a vencidas Servidor
        public void ActualizarMembresiasVencidas()
        {
            var hoy = DateTime.Now.Date;

            // Seleccionar membresías activas cuya fecha de finalización haya pasado
            var membresiasVencidas = _context.MembresiasUsuarios
                .Where(m => m.Estatus == 0 && m.FechaFin < hoy)
                .ToList();

            foreach (var membresia in membresiasVencidas)
            {
                membresia.Estatus = (EstatusMembresia)1;
            }

            // Guardar cambios en la base de datos
            _context.SaveChanges();
        }

        private string GenerarTicket(MembresiaUsuario membresia)
        {
            // Línea separadora
            string separator = new string('*', 64);

            // Verifica si el usuario está presente
            if (membresia.Usuario == null)
            {
                throw new Exception("El usuario asociado a esta membresía no existe o no está cargado.");
            }

            // Formato inicial
            var ticketText = "  \n\n\n\n\n\n            TEAM IS FAMILY\n";
            ticketText += "     POPOCATEPETL 23 COL. XINANTECATL\n";
            ticketText += "           Tel: 7292290000\n\n";
            ticketText += $"{separator}\n";

            // Información del socio
            ticketText += $"DATOS DEL SOCIO\n";
            ticketText += $"Clave #: {membresia.Usuario.RFID?.PadLeft(2) ?? "N/A"}\n";
            ticketText += $"Nombre #: {((membresia.Usuario.Nombre + " " + membresia.Usuario.Apellido_P + " " + membresia.Usuario.Apellido_M)?.PadLeft(2) ?? "N/A")}\n";
            ticketText += $"Membresía #: {membresia.TipoMembresia?.Nombre?.PadLeft(2) ?? "N/A"}\n";
            ticketText += $"Estado #: Pagada\n";
            ticketText += $"Fecha Inicio: {membresia.FechaInicio.ToShortDateString()}\n";
            ticketText += $"Fecha Final: {membresia.FechaFin.ToShortDateString()}\n";
            ticketText += $"Fecha: {DateTime.Now:dd/MM/yyyy hh:mm:ss tt}\n";
            ticketText += $"{separator}\n";

            ticketText += $"Pago de membresia #:{membresia.TipoMembresia.Precio}\n\n\n";
            ticketText += $"TOTAL #:{membresia.TipoMembresia.Precio}\n\n";
            ticketText += $"\n\n";

            // Términos y condiciones
            ticketText += "   *** TERMINOS Y CONDICIONES ***\n";
            ticketText += "1.- El usuario es responsable de \n cualquier lesión que pudiera sufrir dentro \n de las instalaciones.\n";
            ticketText += "2.- El pago de la membresía es \n PERSONAL e INTRANSFERIBLE.\n";
            ticketText += "3.- Por ningún motivo será REEMBOLSABLE \n la cuota de la membresía.\n\n";
            ticketText += "\n\n\n\nNOS RESERVAMOS EL DERECHO DE ADMISION\n\n\n\n\n\n\n\n\n\n\n\n\n\n";

            return ticketText;
        }



        //<!-- =========== Modelos =========  -->
        public class ActualizarEstadoRequest
        {
            public int Id { get; set; }
            public string NuevoEstado { get; set; }
        }

    }
}





