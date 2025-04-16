using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GYMISFAMILY.Models;
using System.Security.Claims;
using GYMISFAMILY.Data;
using GYMISFAMILY.Models.BaseDeDatos;


namespace GYMISFAMILY.Controllers
{
    [Authorize(Roles = "client")]
    public class ClientController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public ClientController(UserManager<ApplicationUser> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public async Task<IActionResult> Index(int page = 1)
        {
            int pageSize = 10;

            // Obtener el ID del cliente autenticado
            var clientId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var clientUser = await _userManager.FindByIdAsync(clientId);

            if (clientUser == null)
            {
                return NotFound();
            }

            // Contar las órdenes totales del cliente autenticado
          
            // Crear el modelo para la vista del cliente
            var model = new ClientDashboardViewModel
            {
                FullName = $"{clientUser.Nombre} {clientUser.Apellido_P}",
                Email = clientUser.Email,
               
                CurrentPage = page,
            };

            return View(model);
        }


        //<!-- =========== Modelos =========  -->
        public class OrderDetailsViewModel
        {
            public int OrderId { get; set; }
            public string DeliveryAddress { get; set; } = "";
            public string PaymentMethod { get; set; } = "";
            public string PaymentStatus { get; set; } = "";
            public string OrderStatus { get; set; } = "";
            public DateTime CreatedAt { get; set; }
            public List<ProductDetailsViewModel> Products { get; set; } = new List<ProductDetailsViewModel>();
            public decimal Subtotal { get; set; }
            public decimal ShippingFee { get; set; }
            public decimal Total => Subtotal + ShippingFee;
        }
        public class ProductDetailsViewModel
        {
            public string Name { get; set; } = "";
            public string ImageFileName { get; set; } = "";
            public int Quantity { get; set; }
            public decimal UnitPrice { get; set; }
            public decimal Total => Quantity * UnitPrice;
        }

        public class ClientDashboardViewModel
        {
            public string FullName { get; set; } = "";
            public string Email { get; set; } = "";
            public List<OrderViewModel> Orders { get; set; } = new List<OrderViewModel>();
            public int CurrentPage { get; set; }
            public int TotalPages { get; set; }
        }

        public class OrderViewModel
        {
            public int Id { get; set; }
            public string ClientName { get; set; } = "";
            public int Units { get; set; }
            public decimal Total { get; set; }
            public string PaymentMethod { get; set; } = "";
            public string PaymentStatus { get; set; } = "";
            public string OrderStatus { get; set; } = "";
            public DateTime Date { get; set; }
        }
    }
}
