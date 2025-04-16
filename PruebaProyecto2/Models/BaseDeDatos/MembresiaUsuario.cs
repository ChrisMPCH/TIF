using System.ComponentModel.DataAnnotations;

namespace GYMISFAMILY.Models.BaseDeDatos
{
    public class MembresiaUsuario
    {
        [Key]
        public int ID_MembresiaUsuario { get; set; }

        [Required]
        public ApplicationUser Usuario { get; set; }

        [Required]
        public int ID_TipoMembresia { get; set; }

        [Required]
        public TipoMembresia TipoMembresia { get; set; }

        [Required]
        public EstatusMembresia Estatus { get; set; } // ENUM

        public DateTime FechaInicio { get; set; }

        [Required]
        public DateTime FechaFin { get; set; }

    }

    public enum EstatusMembresia
    {
        Pagada, //0
        Vencida, //1
        Sin_pagar, //2
        Cancelada //3
    }

}
