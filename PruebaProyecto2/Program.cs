using GYMISFAMILY.Data;
using GYMISFAMILY.Models;
using GYMISFAMILY.Models.BaseDeDatos;
using GYMISFAMILY.Services;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Net;

var builder = WebApplication.CreateBuilder(args);

// Agregar servicios al contenedor de servicios
builder.Services.AddControllersWithViews();
builder.Services.AddHttpClient();

// Esto ya debería estar configurado por defecto
builder.Services.AddSingleton<IConfiguration>(builder.Configuration);

// Configuración de la base de datos
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    var connectionsString = builder.Configuration.GetConnectionString("DefaultConnection");
    options.UseMySql(
        connectionsString,
        ServerVersion.AutoDetect(connectionsString)
    );
});


// Configuración de la identidad
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(
    options =>
    {
        options.Password.RequiredLength = 6;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequireUppercase = false;
        options.Password.RequireLowercase = false;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>();

// Configuración de cookies para acceso denegado e inicio de sesión
builder.Services.ConfigureApplicationCookie(options =>
{
    options.AccessDeniedPath = "/Home/AccessDenied";  // Página de acceso denegado
    options.LoginPath = "/Account/Login";  // Página de inicio de sesión
});

// Habilitación de almacenamiento en memoria para la sesión
builder.Services.AddDistributedMemoryCache(); // Necesario para almacenar en la sesión
builder.Services.AddSession(); // Habilita la sesión en la aplicación
builder.Services.AddHostedService<ActualizarEstadosMembresiasService>();

var app = builder.Build();

// Configuración del pipeline de solicitudes HTTP
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

// Habilitar sesión en la aplicación
app.UseSession(); // Habilita la sesión

app.UseRouting();

app.UseAuthorization();

// Configuración de rutas de los controladores
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Seed data (datos iniciales) para usuarios y roles
using (var scope = app.Services.CreateScope())
{
    var userManager = scope.ServiceProvider.GetService(typeof(UserManager<ApplicationUser>))
        as UserManager<ApplicationUser>;
    var roleManager = scope.ServiceProvider.GetService(typeof(RoleManager<IdentityRole>))
        as RoleManager<IdentityRole>;
    await DatabaseInitializer.SeedDataAsync(userManager, roleManager);
}

//Toma de excpciones totales
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error"); // Redirige a la acción Error
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage(); // En desarrollo muestra el error normal
}


app.Run();

