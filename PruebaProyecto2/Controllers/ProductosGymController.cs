using GYMISFAMILY.Data;
using GYMISFAMILY.Models.BaseDeDatos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GYMISFAMILY.Models;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using GYMISFAMILY.Models.DTOs;

namespace GYMISFAMILY.Controllers
{
    //----------------------------------------------------------------------------------------ProductosGym
    //Controlador para todas las funciones de productosGYM
    [Authorize(Roles = "admin")]
    public class ProductosGymController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProductosGymController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Crear Producto
        public IActionResult Crear()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Crear(Productos producto)
        {
            if (ModelState.IsValid)
            {
                _context.Productos.Add(producto);
                _context.SaveChanges();
                return RedirectToAction(nameof(Index));
            }
            return View(producto);
        }

        // Ver lista de Productos
        public IActionResult Index()
        {
            var productos = _context.Productos.ToList();
            return View(productos);
        }

        // Eliminar Producto
        public IActionResult Eliminar(int id)
        {
            var producto = _context.Productos.Find(id);
            if (producto == null)
            {
                return NotFound();
            }
            _context.Productos.Remove(producto);
            _context.SaveChanges();
            return RedirectToAction(nameof(Index));
        }
    }
}





