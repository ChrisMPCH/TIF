using System.ComponentModel.DataAnnotations;

namespace GYMISFAMILY.Models.BaseDeDatos
{
    public class DetallesVenta
    {
        [Key]
        public int ID_DetallesVenta { get; set; }
        public int ID_Venta { get; set; }
        public Venta Venta { get; set; }
        public int ID_Producto { get; set; }
        public Productos Producto { get; set; }

        public decimal UnitPrice { get; set; }
        public int Cantidad { get; set; }
    }

}
