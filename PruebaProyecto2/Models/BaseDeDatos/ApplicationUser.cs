using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace GYMISFAMILY.Models.BaseDeDatos
{
    public class ApplicationUser : IdentityUser
    {
        [Required, MaxLength(50)]
        public string Nombre { get; set; }

        [Required, MaxLength(50)]
        public string Apellido_P { get; set; }

        [Required, MaxLength(50)]
        public string Apellido_M { get; set; }

        [Phone, MaxLength(12)]
        public string Telefono { get; set; }

        // Aquí puedes dejar la propiedad Email.
        [Required, EmailAddress, MaxLength(255)]
        public new string Email { get; set; } 

        public RolUsuario Rol { get; set; } // ENUM

        [MaxLength(100)]
        public string? ImageFileName { get; set; } = "";

        public DateTime? FechaDeNacimiento { get; set; }

        [MaxLength(10)]
        public string? RFID { get; set; } // Unique

        public DateTime FechaCreación { get; set; } = DateTime.Now;

        public bool Activo { get; set; } = false;

        public bool? EsEmpleado { get; set; }

        // Relaciones
        public ICollection<MembresiaUsuario> MembresiasUsuarios { get; set; } = new List<MembresiaUsuario>();
        public ICollection<Acceso> Accesos { get; set; } = new List<Acceso>();
    }

    public enum RolUsuario
    {
        admin,
        client,
        clientT
    }

}
