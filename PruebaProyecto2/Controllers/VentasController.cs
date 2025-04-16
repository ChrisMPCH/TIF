using GYMISFAMILY.Data;
using GYMISFAMILY.Models.BaseDeDatos;
using GYMISFAMILY.Models.DTOs;
using GYMISFAMILY.Services;
using iTextSharp.text.pdf;
using iTextSharp.text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Drawing;
using System.Drawing.Printing;
using System.Linq;
using Microsoft.AspNetCore.Hosting.Server;
using GYMISFAMILY.Extensions;
using System.Diagnostics;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;


namespace GYMISFAMILY.Controllers
{
    public class VentasController : Controller
    {
        private readonly ApplicationDbContext _context;

        public VentasController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Acción para listar todas las ventas
        public async Task<IActionResult> Index(int page = 1, int pageSize = 10)
        {
            try
            {
                var totalVentas = await _context.Ventas
                                                 .Include(v => v.DetallesVentas)
                                                 .ThenInclude(d => d.Producto)
                                                 .Include(v => v.Usuario)
                                                 .OrderByDescending(v => v.Fecha)
                                                 .ToListAsync();

                var ventasPaginadas = totalVentas
                                      .Skip((page - 1) * pageSize)
                                      .Take(pageSize)
                                      .ToList();

                var totalPages = (int)Math.Ceiling(totalVentas.Count / (double)pageSize);

                var model = new VentaIndexViewModel
                {
                    Ventas = ventasPaginadas,
                    PaginaActual = page,
                    TotalPaginas = totalPages
                };

                return View(model);
            }
            catch (Exception ex)
            {
                var errorModel = new ErrorViewModel
                {
                    RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier,
                    ErrorMessage = ex.Message,
                    SolutionHint = "Verifica la conexión con la base de datos o la consulta realizada.",
                    StackTrace = ex.StackTrace
                };

                return View("Error", errorModel);
            }
        }


        // Acción para ver los detalles de una venta específica
        public async Task<IActionResult> Detalle(int id)
        {
            // Obtener los detalles de la venta especificada
            var venta = await _context.Ventas
                                      .Include(v => v.DetallesVentas)
                                      .ThenInclude(d => d.Producto)
                                      .FirstOrDefaultAsync(v => v.ID_Venta == id);

            if (venta == null)
            {
                return NotFound();
            }

            return View(venta);
        }

        public IActionResult Crear()
        {
            var productos = _context.Productos.ToList();
            var model = new CrearVentaViewModel
            {
                ListaProductos = productos,
                Productos = new List<int>(),  // Inicializar las listas vacías
                Cantidades = new List<int>()
            };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Registrar(Dictionary<int, int> cantidades)
        {
            try
            {
                var admin = _context.Users.FirstOrDefault(c => c.RFID == "0003979457");

                var venta = new Venta
                {
                    Usuario = admin,
                    Fecha = DateTime.Now,
                    Total = 0,
                    DetallesVentas = new List<DetallesVenta>()
                };

                foreach (var cantidad in cantidades)
                {
                    if (cantidad.Value > 0)
                    {
                        var producto = await _context.Productos.FindAsync(cantidad.Key);
                        if (producto != null)
                        {
                            var detalleVenta = new DetallesVenta
                            {
                                ID_Producto = producto.ID_Producto,
                                Producto = producto,
                                UnitPrice = producto.Precio,
                                Cantidad = cantidad.Value
                            };
                            venta.Total += detalleVenta.UnitPrice * detalleVenta.Cantidad;
                            venta.DetallesVentas.Add(detalleVenta);
                        }
                    }
                }

                _context.Ventas.Add(venta);
                await _context.SaveChangesAsync();

                string ticketText = GenerarTicket(venta);
                TicketPrinter printer = new TicketPrinter();
                string imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Images", "LogoTIFsinfondo.png");

                printer.Print(ticketText, imagePath);
                Thread.Sleep(5000);
                printer.Print(ticketText, imagePath);

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                var errorModel = new ErrorViewModel
                {
                    RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier,
                    ErrorMessage = ex.Message,
                    SolutionHint = "Verifica los datos del producto y la cantidad.",
                    StackTrace = ex.StackTrace
                };

                return View("Error", errorModel);
            }
        }


        public async Task<IActionResult> VentasDeHoy()
        {
            var hoy = DateTime.Today;

            var ventasHoy = await _context.Ventas
                .Include(v => v.Usuario)
                .Where(v => v.Fecha.Date == hoy)
                .ToListAsync();

            var gastosHoy = await _context.Gastos
                .Where(g => g.Fecha.Date == hoy)
                .ToListAsync();

            var cajaHoy = await _context.Caja.FirstOrDefaultAsync(c => c.Fecha.Date == hoy);
            var inicioCaja = cajaHoy?.InicioCaja ?? 0;

            var totalVentas = ventasHoy.Sum(v => v.Total);
            var totalGastos = gastosHoy.Sum(g => g.Monto);
            var totalDia = inicioCaja + totalVentas - totalGastos;

            var empleados = await _context.Users
                .Where(u => u.EsEmpleado == true)
                .ToListAsync();

            var viewModel = new VentaIndexViewModel2
            {
                Ventas = ventasHoy,
                Gastos = gastosHoy,
                InicioCaja = inicioCaja,
                TotalVentas = totalVentas,
                TotalGastos = totalGastos,
                TotalDia = totalDia,
                EmpleadosDisponibles = empleados
            };

            return View(viewModel);
        }



        [HttpPost]
        public async Task<IActionResult> RegistrarGasto(string concepto, decimal monto)
        {
            // Crear un nuevo gasto
            var gasto = new Gasto
            {
                Concepto = concepto,
                Monto = monto,
                Fecha = DateTime.Now
            };

            // Guardar el gasto en la base de datos
            _context.Gastos.Add(gasto);
            await _context.SaveChangesAsync();

            // Redirigir a la vista de ventas de hoy
            return RedirectToAction("VentasDeHoy");
        }

        [HttpPost]
        public async Task<IActionResult> EstablecerInicioCaja(decimal inicioCaja)
        {
            var hoy = DateTime.Today;

            // Verificar si ya existe un registro de la caja de hoy
            var cajaHoy = await _context.Caja.FirstOrDefaultAsync(c => c.Fecha.Date == hoy);
            if (cajaHoy == null)
            {
                // Crear un nuevo registro si no existe
                cajaHoy = new Caja
                {
                    Fecha = hoy,
                    InicioCaja = inicioCaja
                };
                _context.Caja.Add(cajaHoy);
            }
            else
            {
                // Actualizar el inicio de caja si ya existe
                cajaHoy.InicioCaja = inicioCaja;
                _context.Caja.Update(cajaHoy);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("VentasDeHoy");
        }

        private string GenerarTicket(Venta venta)
        {
            // Crear una línea separadora
            string separator = new string('*', 64);

            // Formato inicial
            var ticketText = "  \n\n\n\n\n\n            TEAM IS FAMILY\n";
            ticketText += "     POPOCATEPETL 23 COL. XINANTECATL\n";
            ticketText += "           Tel: 7292290000\n\n";
            ticketText += $"{separator}\n";

            // Información de la venta
            ticketText += $"Venta #: {venta.ID_Venta.ToString().PadLeft(2)}\n";
            ticketText += $"Fecha: {DateTime.Now:dd/MM/yyyy hh:mm:ss tt}\n";
            ticketText += $"{separator}\n";

            // Encabezado de productos
            ticketText += "Producto         Cant  Precio   Total\n";
            ticketText += $"{separator}\n";

            // Productos vendidos
            foreach (var detalle in venta.DetallesVentas)
            {
                string nombreProducto = detalle.Producto.Nombre.PadRight(12).Substring(0, 12); // Limitar a 10
                string cantidad = detalle.Cantidad.ToString().PadLeft(7);
                string precio = $"${detalle.UnitPrice}".PadLeft(8);
                string total = $"${detalle.UnitPrice * detalle.Cantidad}".PadLeft(7);
                ticketText += $"{nombreProducto}{cantidad}{precio}{total}\n";
            }

            ticketText += $"{separator}\n";

            // Total de la venta
            ticketText += $"Total: {venta.Total.ToString("C").PadLeft(24)}\n";
            ticketText += $"\n\n";

            // Términos y condiciones
            ticketText += "   *** TERMINOS Y CONDICIONES ***\n";
            ticketText += "1.- El usuario es responsable de \n cualquier lesión que pudiera sufrir dentro \n de las instalaciones.\n";
            ticketText += "2.- El pago de la membresía es \n PERSONAL e INTRANSFERIBLE.\n";
            ticketText += "3.- Por ningún motivo será REEMBOLSABLE \n la cuota de la membresía.\n\n";
            ticketText += "\n\n\n\nNOS RESERVAMOS EL DERECHO DE ADMISION\n\n\n\n\n\n\n\n\n\n\n\n\n\n";

            return ticketText;
        }

        [HttpPost]
        public IActionResult GenerarTicketResumen()
        {
            var hoy = DateTime.Today;

            // Obtener datos del día
            var ventasHoy = _context.Ventas.Where(v => v.Fecha.Date == hoy).ToList();
            var gastosHoy = _context.Gastos.Where(g => g.Fecha.Date == hoy).ToList();
            var cajaHoy = _context.Caja.FirstOrDefault(c => c.Fecha.Date == hoy);
            var inicioCaja = cajaHoy?.InicioCaja ?? 0;

            var totalVentas = ventasHoy.Sum(v => v.Total);
            var totalGastos = gastosHoy.Sum(g => g.Monto);
            var totalDia = inicioCaja + totalVentas - totalGastos;

            // Generar el ticket
            var ticket = "  \n\n\n\n\n\n            TEAM IS FAMILY\n";
            ticket += "      POPOCATEPETL 23 COL. XINANTECATL\n";
            ticket += "            Tel: 7292290000\n\n";
            ticket += new string('*', 64) + "\n";
            ticket += $"Fecha: {DateTime.Now:dd/MM/yyyy hh:mm:ss tt}\n";
            ticket += new string('*', 64) + "\n";

            ticket += "Resumen del Día\n";
            ticket += $"Inicio de Caja: {inicioCaja:C2}\n";
            ticket += $"Total de Ventas: {totalVentas:C2}\n";
            ticket += $"Total de Gastos: {totalGastos:C2}\n";
            ticket += $"Total del Día: {totalDia:C2}\n";
            ticket += new string('*', 64) + "\n";

            ticket += "   *** GRACIAS POR SU PREFERENCIA ***\n\n\n\n\n\n";

            // Imprimir el ticket
            TicketPrinter printer = new TicketPrinter();
            string imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Images", "LogoTIFsinfondo.png");
            printer.Print(ticket, imagePath);  // Llama al método de impresión que ya configuraste

            return RedirectToAction("VentasDeHoy");
        }

        public IActionResult CorteDelDia()
        {
            var hoy = DateTime.Today;

            var model = new VentaIndexViewModel2
            {
                Ventas = _context.Ventas
                    .Include(v => v.Usuario)
                    .Include(v => v.DetallesVentas)
                    .Where(v => v.Fecha.Date == hoy)
                    .ToList(),

                Gastos = _context.Gastos
                    .Where(g => g.Fecha.Date == hoy)
                    .ToList(),

                EmpleadosDisponibles = _context.Users
                    .Where(u => u.EsEmpleado == true)
                    .ToList()
            };

            model.InicioCaja = _context.Caja.FirstOrDefault(c => c.Fecha == hoy)?.InicioCaja ?? 0;
            model.TotalVentas = model.Ventas.Sum(v => v.Total);
            model.TotalGastos = model.Gastos.Sum(g => g.Monto);
            model.TotalDia = model.InicioCaja + model.TotalVentas - model.TotalGastos;

            return View(model);
        }

        [HttpPost]
        public IActionResult GenerarPDFCorteCaja([FromForm] List<string> empleadosSeleccionados)
        {
            QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;

            var hoy = DateTime.Today;

            var ventas = _context.Ventas
                .Include(v => v.Usuario)
                .Include(v => v.DetallesVentas)
                .ThenInclude(d => d.Producto)
                .Where(v => v.Fecha.Date == hoy)
                .ToList();

            var gastos = _context.Gastos
                .Where(g => g.Fecha.Date == hoy)
                .ToList();

            var usuariosEnRecepcion = _context.Users
                .Where(u => empleadosSeleccionados.Contains(u.Id))
                .ToList();

            var inicioCaja = _context.Caja.FirstOrDefault(c => c.Fecha == hoy)?.InicioCaja ?? 0;
            var totalVentas = ventas.Sum(v => v.Total);
            var totalGastos = gastos.Sum(g => g.Monto);
            var totalDia = inicioCaja + totalVentas - totalGastos;

            var document = QuestPDF.Fluent.Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(40);
                    page.Size(PageSizes.A4);
                    page.DefaultTextStyle(x => x.FontSize(11).FontFamily("Helvetica"));

                    page.Header().Row(row =>
                    {
                        row.RelativeItem().Text("TEAM IS FAMILY").FontSize(20).Bold();
                        row.ConstantItem(200).AlignRight().Text(DateTime.Now.ToString("dd/MM/yyyy")).FontSize(12);
                    });

                    page.Content().PaddingVertical(10).Column(col =>
                    {
                        col.Spacing(25);

                        // ───── Recepcionistas ─────
                        col.Item().Border(1).BorderColor("#cccccc").Padding(10).Background("#f9f9f9").Column(section =>
                        {
                            section.Spacing(10);
                            section.Item().Text("Recepcionistas del Día").FontSize(14).Bold().Underline();

                            if (usuariosEnRecepcion.Any())
                            {
                                section.Item().Table(table =>
                                {
                                    table.ColumnsDefinition(c => c.RelativeColumn());
                                    foreach (var user in usuariosEnRecepcion)
                                        table.Cell().Text($"{user.Nombre} {user.Apellido_P} {user.Apellido_M}");
                                });
                            }
                            else
                            {
                                section.Item().Text("No se registraron recepcionistas.");
                            }
                        });

                        // ───── Ventas ─────
                        col.Item().Border(1).BorderColor("#cccccc").Padding(10).Background("#f9f9f9").Column(section =>
                        {
                            section.Spacing(10);
                            section.Item().Text("Ventas").FontSize(14).Bold().Underline();

                            if (ventas.Any())
                            {
                                foreach (var venta in ventas)
                                {
                                    section.Item().Table(table =>
                                    {
                                        table.ColumnsDefinition(columns =>
                                        {
                                            columns.ConstantColumn(60);
                                            columns.RelativeColumn();
                                            columns.ConstantColumn(80);
                                        });

                                        table.Header(header =>
                                        {
                                            header.Cell().Text("ID").Bold();
                                            header.Cell().Text("Hora").Bold();
                                            header.Cell().Text("Total").Bold();
                                        });

                                        table.Cell().Text(venta.ID_Venta.ToString());
                                        table.Cell().Text(venta.Fecha.ToString("HH:mm"));
                                        table.Cell().Text($"${venta.Total:F2}");
                                    });

                                    if (venta.DetallesVentas.Any())
                                    {
                                        section.Item().PaddingLeft(10).Table(table =>
                                        {
                                            table.ColumnsDefinition(columns =>
                                            {
                                                columns.RelativeColumn(); // Producto
                                                columns.ConstantColumn(60); // Cantidad
                                                columns.ConstantColumn(80); // Precio
                                                columns.ConstantColumn(80); // Subtotal
                                            });

                                            table.Header(header =>
                                            {
                                                header.Cell().Text("Producto").Bold();
                                                header.Cell().Text("Cant.").Bold();
                                                header.Cell().Text("Precio").Bold();
                                                header.Cell().Text("Subtotal").Bold();
                                            });

                                            foreach (var detalle in venta.DetallesVentas)
                                            {
                                                var nombreProducto = detalle.Producto?.Nombre ?? "Producto";
                                                var cantidad = detalle.Cantidad;
                                                var precio = detalle.Producto.Precio;
                                                var subtotal = cantidad * precio;

                                                table.Cell().Text(nombreProducto);
                                                table.Cell().Text(cantidad.ToString());
                                                table.Cell().Text($"${precio:F2}");
                                                table.Cell().Text($"${subtotal:F2}");
                                            }
                                        });
                                    }
                                    else
                                    {
                                        section.Item().PaddingLeft(10).Text("Esta venta corresponde a una membresía.").Italic();
                                    }

                                    section.Item().PaddingVertical(5).BorderBottom(1).BorderColor("#dddddd");
                                }
                            }
                            else
                            {
                                section.Item().Text("No hubo ventas hoy.");
                            }
                        });

                        // ───── Gastos ─────
                        col.Item().Border(1).BorderColor("#cccccc").Padding(10).Background("#f9f9f9").Column(section =>
                        {
                            section.Spacing(10);
                            section.Item().Text("Gastos").FontSize(14).Bold().Underline();

                            if (gastos.Any())
                            {
                                section.Item().Table(table =>
                                {
                                    table.ColumnsDefinition(columns =>
                                    {
                                        columns.RelativeColumn();    // Concepto
                                        columns.ConstantColumn(80);  // Monto
                                    });

                                    table.Header(header =>
                                    {
                                        header.Cell().Text("Concepto").Bold();
                                        header.Cell().Text("Monto").Bold();
                                    });

                                    foreach (var gasto in gastos)
                                    {
                                        table.Cell().Text(gasto.Concepto);
                                        table.Cell().Text($"${gasto.Monto:F2}");
                                    }
                                });

                                section.Item().PaddingVertical(5).BorderBottom(1).BorderColor("#dddddd");

                            }
                            else
                            {
                                section.Item().Text("No hubo gastos hoy.");
                            }
                        });

                        // ───── Resumen del Día ─────
                        col.Item().Border(1).BorderColor("#cccccc").Padding(10).Background("#f1f1f1").Column(section =>
                        {
                            section.Spacing(10);
                            section.Item().Text("Resumen del Día").FontSize(14).Bold().Underline();

                            section.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn();
                                    columns.ConstantColumn(120);
                                });

                                table.Cell().Text("Inicio de caja:");
                                table.Cell().AlignRight().Text($"${inicioCaja:F2}");

                                table.Cell().Text("Total de ventas:");
                                table.Cell().AlignRight().Text($"${totalVentas:F2}");

                                table.Cell().Text("Total de gastos:");
                                table.Cell().AlignRight().Text($"${totalGastos:F2}");

                                table.Cell().Text("Total final del día:").Bold();
                                table.Cell().AlignRight().Text($"${totalDia:F2}").Bold();
                            });
                        });
                    });

                    page.Footer().AlignCenter().Text("GYM IS FAMILY - Reporte generado automáticamente").FontSize(9).Italic();
                });
            });

            var pdfBytes = document.GeneratePdf();
            return File(pdfBytes, "application/pdf", $"CorteCaja_{DateTime.Now:yyyyMMdd}.pdf");
        }

    }

    // Clase para imprimir el ticket
    public class TicketPrinter
    {
        public void Print(string ticketText, string imagePath)
        {
            PrintDocument pd = new PrintDocument();

            // Configura el evento de impresión
            pd.PrintPage += (sender, e) =>
            {
                float yPosition = 0; // Posición inicial
                float lineHeight = 10; // Altura de cada línea

                // Cargar la imagen desde la ruta proporcionada
                if (!string.IsNullOrEmpty(imagePath) && System.IO.File.Exists(imagePath))
                {
                    try
                    {
                        System.Drawing.Image logo = System.Drawing.Image.FromFile(imagePath);
                        // Dibuja la imagen en la parte superior del ticket
                        e.Graphics.DrawImage(logo, new RectangleF(0, yPosition, 200, 150)); // Escala imagen
                        yPosition += 60; // Ajusta la posición después de la imagen
                    }
                    catch (Exception ex)
                    {
                        throw new Exception($"Error al cargar la imagen: {ex.Message}");
                    }
                }

                // Usa una fuente de ancho fijo (Courier New) para alinear correctamente el texto
                System.Drawing.Font font = new System.Drawing.Font("Courier New", 6, FontStyle.Regular);

                // Divide el texto en líneas
                string[] lines = ticketText.Split('\n');
                foreach (string line in lines)
                {
                    e.Graphics.DrawString(line, font, Brushes.Black, new PointF(0, yPosition));
                    yPosition += lineHeight;
                }
            };

            // Configura la impresora
            pd.PrinterSettings.PrinterName = "XP-58";

            // Verifica si la impresora es válida
            if (pd.PrinterSettings.IsValid)
            {
                pd.Print();
            }
            else
            {
                throw new Exception("La impresora especificada no está disponible.");
            }

        }

        

    }
    public class VentaIndexViewModel2
    {
        // Ya existentes
        public List<Venta> Ventas { get; set; }
        public List<Gasto> Gastos { get; set; }
        public decimal InicioCaja { get; set; }
        public decimal TotalVentas { get; set; }
        public decimal TotalGastos { get; set; }
        public decimal TotalDia { get; set; }

        // NUEVO: Para selección de empleados
        public List<ApplicationUser> EmpleadosDisponibles { get; set; } = new();
        public List<string> EmpleadosSeleccionados { get; set; } = new(); // IDs de usuarios seleccionados
    }


    public class VentaIndexViewModel
    {
        public List<Venta> Ventas { get; set; }
        public int PaginaActual { get; set; }
        public int TotalPaginas { get; set; }
    }

    public class CrearVentaViewModel
    {
        public List<int> Productos { get; set; }
        public List<int> Cantidades { get; set; }
        public List<Productos> ListaProductos { get; set; } // Lista de productos para el dropdown
    }

}

