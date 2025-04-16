using GYMISFAMILY.Data;
using GYMISFAMILY.Models.BaseDeDatos;
using Microsoft.Extensions.DependencyInjection;

public class ActualizarEstadosMembresiasService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;

    public ActualizarEstadosMembresiasService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var _dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                // Obtener membresías vencidas
                var membresiasVencidas = _dbContext.MembresiasUsuarios
                    .Where(m => m.FechaFin <= DateTime.Now && m.Estatus == 0) // 0 = Pagada
                    .ToList();

                foreach (var membresia in membresiasVencidas)
                {
                    membresia.Estatus = (EstatusMembresia)1; // 1 = Vencida
                }

                await _dbContext.SaveChangesAsync();
            }

            // Esperar 1 día antes de volver a ejecutar
            await Task.Delay(TimeSpan.FromDays(1), stoppingToken);
        }
    }
}
