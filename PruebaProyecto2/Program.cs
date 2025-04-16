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

// Esto ya deber�a estar configurado por defecto
builder.Services.AddSingleton<IConfiguration>(builder.Configuration);

// Configuraci�n de la base de datos
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    var connectionsString = builder.Configuration.GetConnectionString("DefaultConnection");
    options.UseMySql(
        connectionsString,
        ServerVersion.AutoDetect(connectionsString)
    );
});


// Configuraci�n de la identidad
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(
    options =>
    {
        options.Password.RequiredLength = 6;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequireUppercase = false;
        options.Password.RequireLowercase = false;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>();

// Configuraci�n de cookies para acceso denegado e inicio de sesi�n
builder.Services.ConfigureApplicationCookie(options =>
{
    options.AccessDeniedPath = "/Home/AccessDenied";  // P�gina de acceso denegado
    options.LoginPath = "/Account/Login";  // P�gina de inicio de sesi�n
});

// Habilitaci�n de almacenamiento en memoria para la sesi�n
builder.Services.AddDistributedMemoryCache(); // Necesario para almacenar en la sesi�n
builder.Services.AddSession(); // Habilita la sesi�n en la aplicaci�n
builder.Services.AddHostedService<ActualizarEstadosMembresiasService>();

var app = builder.Build();

// Configuraci�n del pipeline de solicitudes HTTP
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

// Habilitar sesi�n en la aplicaci�n
app.UseSession(); // Habilita la sesi�n

app.UseRouting();

app.UseAuthorization();

// Configuraci�n de rutas de los controladores
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
    app.UseExceptionHandler("/Error"); // Redirige a la acci�n Error
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage(); // En desarrollo muestra el error normal
}


app.Run();

