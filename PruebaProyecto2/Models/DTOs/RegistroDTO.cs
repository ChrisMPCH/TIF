using GYMISFAMILY.Models.BaseDeDatos;
using System.ComponentModel.DataAnnotations;

namespace GYMISFAMILY.Models.DTOs
{
    public class RegistroDTO
    {
        [Required(ErrorMessage = "El campo Nombre es obligatorio"), MaxLength(50)]
        public required string Nombre { get; set; }

        [Required(ErrorMessage = "El campo Apellido Paterno es obligatorio"), MaxLength(50)]
        public required string Apellido_P { get; set; }

        [Required(ErrorMessage = "El campo Apellido Materno es obligatorio"), MaxLength(50)]
        public required string Apellido_M { get; set; }

        [Required, Phone(ErrorMessage = "El formato del número de teléfono no es válido"), MaxLength(20)]
        public string PhoneNumber { get; set; } = "";

        [Required(ErrorMessage = "Se requiere un archivo de imagen.")]
        public string ImageBase64 { get; set; } = " ";

        [Required(ErrorMessage = "El campo RFID es obligatorio"), MaxLength(10)]
        public string RFID { get; set; } = " ";

        [Required(ErrorMessage = "El campo Fecha de Nacimiento es obligatorio")]
        public DateTime FechaDeNacimiento { get; set; } = DateTime.Now;
        public bool EsEmpleado { get; set; }
    }

}
