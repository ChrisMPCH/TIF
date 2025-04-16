using System.ComponentModel.DataAnnotations;

namespace GYMISFAMILY.Models.BaseDeDatos
{
    public class Acceso
    {
        [Key]
        public int ID_Acceso { get; set; } // Clave primaria
        public required ApplicationUser Usuario { get; set; }
        public DateTime FechaHoraEntrada { get; set; } = DateTime.Now;
        public DateTime? FechaHoraSalida { get; set; }
    }
}