using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ProyectoFinal_VargasAnaCarolina.Models;
using ProyectoFinal_VargasAnaCarolina.Services;
using ProyectoFinal_VargasAnaCarolina.ViewModels;

namespace ProyectoFinal_VargasAnaCarolina.Data;
public static class Inicializador
{
    public static async Task Ejecutar(IServiceProvider services, IConfiguration cfg)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CampusContext>();
        if (cfg.GetValue<bool>("Demo:Sqlite"))
            await db.Database.EnsureCreatedAsync();
        else
            await db.Database.MigrateAsync();
        var roles = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        foreach (var role in new[]
        {
            "Estudiante",
            "Administrador"
        }

        )
            if (!await roles.RoleExistsAsync(role))
            {
                var r = await roles.CreateAsync(new(role));
                if (!r.Succeeded)
                    throw new InvalidOperationException("No se pudo crear el rol.");
            }

        if (cfg.GetValue<bool>("Demo:Seed"))
        {
            if (!await db.Carreras.AnyAsync())
            {
                db.Carreras.AddRange(new Carrera { Codigo = "IS", Nombre = "Ingeniería en Sistemas", Descripcion = "Desarrollo de software, datos y soluciones tecnológicas." }, new Carrera { Codigo = "AE", Nombre = "Administración de Empresas", Descripcion = "Gestión de organizaciones, proyectos y recursos." });
                await db.SaveChangesAsync();
            }

            if (!await db.Cursos.AnyAsync())
            {
                db.Cursos.Add(new() { Codigo = "IS-101", Nombre = "Programación I", Creditos = 4, CarreraId = (await db.Carreras.FirstAsync()).Id });
                await db.SaveChangesAsync();
            }

            if (!await db.Docentes.AnyAsync())
            {
                db.Docentes.Add(new() { Identificacion = "DOC-DEMO", Nombre = "Docente de demostración", Correo = "docente@campus.local", Telefono = "8888-7777" });
                await db.SaveChangesAsync();
            }
        }

        var users = scope.ServiceProvider.GetRequiredService<UserManager<CampusUser>>();
        var email = cfg["Bootstrap:AdminEmail"];
        var password = cfg["Bootstrap:AdminPassword"];
        if (!string.IsNullOrWhiteSpace(email) && !string.IsNullOrWhiteSpace(password) && await users.FindByEmailAsync(email) == null)
        {
            var admin = new CampusUser
            {
                UserName = email,
                Email = email
            };
            var r = await users.CreateAsync(admin, password);
            if (!r.Succeeded)
                throw new InvalidOperationException("La contraseña de administración no cumple la política.");
            r = await users.AddToRoleAsync(admin, "Administrador");
            if (!r.Succeeded)
                throw new InvalidOperationException("No se pudo asignar administración.");
        }

        if (cfg.GetValue<bool>("Demo:Seed") && await users.FindByEmailAsync("estudiante@campus.local") == null)
        {
            var carrera = await db.Carreras.FirstAsync(x => x.Activa);
            var reg = scope.ServiceProvider.GetRequiredService<RegistroService>();
            var result = await reg.Crear(new() { Identificacion = "DEMO-001", Nombre = "Estudiante de demostración", Correo = "estudiante@campus.local", Telefono = "8888-9999", Direccion = "San José", CarreraId = carrera.Id, Password = "Estudiante!2026", ConfirmPassword = "Estudiante!2026" });
            if (result.User is null)
                throw new InvalidOperationException(string.Join("; ", result.Errores));
        }
    }
}
