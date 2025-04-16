using System.ComponentModel.DataAnnotations;

namespace GYMISFAMILY.Models.BaseDeDatos
{
    public class TipoMembresia
    {
        [Key]
        public int ID_TipoMembresia { get; set; }

        [Required, MaxLength(100)]
        public string Nombre { get; set; }

        [Required]
        public TipoMembresiaCategoria Tipo { get; set; } // ENUM

        [Required]
        public int Tiempo { get; set; } // En días

        [Range(0, 100000)]
        public decimal Precio { get; set; }

        public ICollection<MembresiaUsuario> MembresiasUsuarios { get; set; } = new List<MembresiaUsuario>();
    }

    public enum TipoMembresiaCategoria
    {
        Dias,
        Semanal,
        Mensual
    }
}
