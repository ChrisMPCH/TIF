using System;
using System.ComponentModel.DataAnnotations;

namespace GYMISFAMILY.Models.DTOs
{
    public class ClienteEditDTO
    {
        public string Id { get; set; }

        [Required(ErrorMessage = "El campo Nombre es obligatorio")]
        [MaxLength(50)]
        public string Nombre { get; set; }

        [Required(ErrorMessage = "El campo Apellido Paterno es obligatorio")]
        [MaxLength(50)]
        public string Apellido_P { get; set; }

        [Required(ErrorMessage = "El campo Apellido Materno es obligatorio")]
        [MaxLength(50)]
        public string Apellido_M { get; set; }

        [Required(ErrorMessage = "El correo electrónico es obligatorio")]
        [EmailAddress(ErrorMessage = "El formato del correo no es válido")]
        [MaxLength(255)]
        public string Email { get; set; }

        [Required(ErrorMessage = "El teléfono es obligatorio")]
        [Phone(ErrorMessage = "Formato de teléfono inválido")]
        [MaxLength(20)]
        public string PhoneNumber { get; set; }

        [Required(ErrorMessage = "La fecha de nacimiento es obligatoria")]
        public DateTime FechaDeNacimiento { get; set; }

        [Required(ErrorMessage = "El RFID es obligatorio")]
        [MaxLength(50)]
        public string RFID { get; set; }

        public bool EsEmpleado { get; set; } // Determina si es empleado

        // Campos específicos de empleados (opcionales)
        [Required(ErrorMessage = "La contraseña es obligatoria para empleados")]
        [DataType(DataType.Password)]
        [ValidatePasswordIfEmployee]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Las contraseñas no coinciden")]
        public string ConfirmPassword { get; set; }

    }

    // Atributo de validación personalizado
    public class ValidatePasswordIfEmployeeAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var model = (ClienteEditDTO)validationContext.ObjectInstance;

            if (model.EsEmpleado && string.IsNullOrEmpty(value?.ToString()))
            {
                return new ValidationResult(ErrorMessage);
            }

            return ValidationResult.Success;
        }
    }
}