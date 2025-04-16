using GYMISFAMILY.Models.BaseDeDatos;

namespace GYMISFAMILY.Models.DTOs
{
    public class ProductoVentaDTO
    {
        public int ID_Producto { get; set; }
        public Productos Producto { get; set; } // Opcional, si deseas incluir los detalles completos del producto
        public decimal PrecioUnitario { get; set; }
        public int Cantidad { get; set; }
    }

}