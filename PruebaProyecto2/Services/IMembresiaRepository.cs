using GYMISFAMILY.Models.BaseDeDatos;

namespace GYMISFAMILY.Services
{
     public interface IMembresiaRepository
    {
        Task<List<MembresiaUsuario>> ObtenerMembresiasVencidas(DateTime fechaHoy);
        Task ActualizarMembresia(MembresiaUsuario membresia);
     }

}