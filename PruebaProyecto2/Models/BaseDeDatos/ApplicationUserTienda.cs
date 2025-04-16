using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace GYMISFAMILY.Models.BaseDeDatos
{
    public class ApplicationUserTienda : IdentityUser
    {
        [Required, MaxLength(50)]
        public string Nombre { get; set; }

        [Required, MaxLength(50)]
        public string Apellido_P { get; set; }

        [Required, MaxLength(50)]
        public string Apellido_M { get; set; }

        [Phone, MaxLength(12)]
        public string Telefono { get; set; }

        [Required, EmailAddress, MaxLength(255)]
        public new string Email { get; set; } 

        public DateTime FechaCreación { get; set; } = DateTime.Now;
        public RolUsuario Rol { get; set; } // ENUM
        public ICollection<Venta> Ventas { get; set; } = new List<Venta>();
    }


}
