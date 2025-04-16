namespace GYMISFAMILY.Models.BaseDeDatos
{
    public class Gasto
    {
        public int ID { get; set; }
        public string Concepto { get; set; } // Ejemplo: "Compra de material"
        public decimal Monto { get; set; } // Ejemplo: 250.00
        public DateTime Fecha { get; set; } // Fecha del gasto
    }

}
