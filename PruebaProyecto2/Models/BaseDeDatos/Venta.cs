using System.ComponentModel.DataAnnotations;

namespace GYMISFAMILY.Models.BaseDeDatos
{
    public class Venta
    {
        [Required, Key]
        public int ID_Venta { get; set; }

        [MaxLength(255)]
        public ApplicationUser Usuario { get; set; }

        [Required]
        public DateTime Fecha { get; set; } = DateTime.Now;

        [Required, MaxLength(255)]
        public decimal Total { get; set; }

        [Required, MaxLength(255)]
        public ICollection<DetallesVenta> DetallesVentas { get; set; } = new List<DetallesVenta>();
    }
}
