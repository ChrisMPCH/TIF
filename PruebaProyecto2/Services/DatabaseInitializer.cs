using GYMISFAMILY.Models.BaseDeDatos;
using Microsoft.AspNetCore.Identity;

public class DatabaseInitializer
{
    public static async Task SeedDataAsync(UserManager<ApplicationUser>? userManager, RoleManager<IdentityRole>? roleManager)
    {
        // Validación si userManager o roleManager son nulos
        if (userManager == null || roleManager == null)
        {
            Console.WriteLine("userManager or roleManager is null => exit");
            return;
        }

        // Crear los roles si no existen
        var roles = new[] { "admin", "client"};
        foreach (var role in roles)
        {
            var roleExists = await roleManager.RoleExistsAsync(role);
            if (!roleExists)
            {
                Console.WriteLine($"{role} role is not defined, creating it.");
                var createRoleResult = await roleManager.CreateAsync(new IdentityRole(role));
                if (!createRoleResult.Succeeded)
                {
                    Console.WriteLine($"Failed to create the {role} role. Errors: {string.Join(", ", createRoleResult.Errors.Select(e => e.Description))}");
                    return;
                }
            }
        }

        // Verificar si ya existe un usuario administrador
        var adminUsers = await userManager.GetUsersInRoleAsync("admin");
        if (adminUsers.Any())
        {
            // Ya existe un usuario administrador
            Console.WriteLine("Admin user already exists => exit");
            return;
        }

        // Crear un nuevo usuario administrador
        var user = new ApplicationUser()
        {
            Nombre = "Admin",
            Apellido_P = "GYM",
            Apellido_M = "TIF",
            UserName = "admin@admin.com",  // UserName usado para autenticación
            Email = "admin@admin.com",
            Telefono = "1234567890",  // Asignar un valor a Telefono
            RFID = "0003979457",  // Asignar un valor a RFID
            Rol = RolUsuario.admin,  // Asignar el rol de Admin
            FechaDeNacimiento = new DateTime(1980, 1, 1),  // Fecha de nacimiento
            FechaCreación = DateTime.Now,  // Fecha de creación
            EsEmpleado = false
        };


        string initialPassword = "Admintifempleados2025$+";

        // Crear el usuario administrador
        var result = await userManager.CreateAsync(user, initialPassword);
        if (result.Succeeded)
        {
            // Asignar el rol de administrador al usuario creado y tienda
            await userManager.AddToRoleAsync(user, "admin");
            Console.WriteLine("Admin and store user created successfully! Please update the initial password.");
            Console.WriteLine($"Admin Email: {user.Email}");
            Console.WriteLine($"Initial password: {initialPassword}");
        }
        else
        {
            // En caso de error, mostrar los errores
            Console.WriteLine("Failed to create the admin user.");
            foreach (var error in result.Errors)
            {
                Console.WriteLine($"Error: {error.Description}");
            }
        }
    }
}