using GYMISFAMILY.Data;
using GYMISFAMILY.Models.BaseDeDatos;
using GYMISFAMILY.Services;
using Microsoft.EntityFrameworkCore;

public class MembresiaRepository : IMembresiaRepository
{
    private readonly ApplicationDbContext _context;

    public MembresiaRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<MembresiaUsuario>> ObtenerMembresiasVencidas(DateTime fechaHoy)
    {
        return await _context.MembresiasUsuarios
            .Where(m => m.Estatus == 0 && m.FechaFin < fechaHoy)
            .ToListAsync();
    }

    public async Task ActualizarMembresia(MembresiaUsuario membresia)
    {
        _context.MembresiasUsuarios.Update(membresia);
        await _context.SaveChangesAsync();
    }
}
