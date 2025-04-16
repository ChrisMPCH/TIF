using GYMISFAMILY.Models.BaseDeDatos;
using System.ComponentModel.DataAnnotations;

namespace GYMISFAMILY.Models.DTOs
{
    public class RegistroTDTO
    {
        [Required(ErrorMessage = "El campo Nombre es obligatorio"), MaxLength(50)]
        public string Nombre { get; set; }

        [Required(ErrorMessage = "El campo Apellido Paterno es obligatorio"), MaxLength(50)]
        public string Apellido_P { get; set; }

        [Required(ErrorMessage = "El campo Apellido Materno es obligatorio"), MaxLength(50)]
        public string Apellido_M { get; set; }

        [Phone(ErrorMessage = "El formato del número de teléfono no es válido"), MaxLength(20)]
        public string Telefono { get; set; } = "";

        [Required(ErrorMessage = "El campo Email es obligatorio"), EmailAddress(ErrorMessage = "El formato del Email no es válido"), MaxLength(255)]
        public string Email { get; set; } = "";

        [Required(ErrorMessage = "El campo Fecha de Creación es obligatorio")]
        public DateTime FechaCreación { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "El campo Contraseña es obligatorio")]
        public string Password { get; set; } = "";

        [Required(ErrorMessage = "El campo Confirmar Contraseña es obligatorio")]
        [Compare("Password", ErrorMessage = "La Confirmación de la Contraseña no coincide con la Contraseña")]
        public string ConfirmPassword { get; set; } = "";
    }
}
