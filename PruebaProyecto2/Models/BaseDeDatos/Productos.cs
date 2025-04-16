using System.ComponentModel.DataAnnotations;

namespace GYMISFAMILY.Models.BaseDeDatos
{
    public class Productos
    {
        [Key]
        public int ID_Producto { get; set; }

        [Required, MaxLength(100)]
        public string Nombre { get; set; }

        [MaxLength(100)]
        public string SubCategory { get; set; } = "";

        [MaxLength(100)]
        public string Category { get; set; } = "";

        [MaxLength(500)]
        public string Descripcion { get; set; }

        [Range(0, 100000)]
        public decimal Precio { get; set; }
        public string Especificacion { get; set; } = "";

        [MaxLength(100)]
        public string ImageFileName { get; set; } = "";

        public DateTime CreatedAt { get; set; }

        public ICollection<DetallesVenta> DetallesVentas { get; set; } = new List<DetallesVenta>();
    }

}
