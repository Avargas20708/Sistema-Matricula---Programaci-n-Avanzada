using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ProyectoFinal_VargasAnaCarolina.Data;
using ProyectoFinal_VargasAnaCarolina.Models;
using ProyectoFinal_VargasAnaCarolina.ViewModels;

namespace ProyectoFinal_VargasAnaCarolina.Services;
public class RegistroService(CampusContext db, UserManager<CampusUser> users)
{
    public async Task<(CampusUser? User, string[] Errores)> Crear(RegistroVm vm)
    {
        vm.Identificacion = (vm.Identificacion ?? "").Trim().ToUpperInvariant();
        vm.Correo = (vm.Correo ?? "").Trim().ToLowerInvariant();
        var errores = new List<string>();
        if (!await db.Carreras.AnyAsync(x => x.Id == vm.CarreraId && x.Activa))
            errores.Add("Seleccione una carrera activa.");
        if (await db.Estudiantes.AnyAsync(x => x.Identificacion == vm.Identificacion))
            errores.Add("La identificación ya está registrada.");
        if (await users.FindByEmailAsync(vm.Correo) != null)
            errores.Add("El correo ya está registrado.");
        if (errores.Count > 0)
            return (null, errores.ToArray());
        await using var tx = await db.Database.BeginTransactionAsync();
        var user = new CampusUser
        {
            UserName = vm.Correo,
            Email = vm.Correo
        };
        try
        {
            var result = await users.CreateAsync(user, vm.Password);
            if (!result.Succeeded)
                return (null, result.Errors.Select(x => "Contraseña o cuenta inválida: " + x.Description).ToArray());
            result = await users.AddToRoleAsync(user, "Estudiante");
            if (!result.Succeeded)
                return (null, new[] { "No se pudo asignar el rol Estudiante." });
            db.Estudiantes.Add(new() { UserId = user.Id, Identificacion = vm.Identificacion, Nombre = (vm.Nombre ?? "").Trim(), Telefono = (vm.Telefono ?? "").Trim(), Direccion = (vm.Direccion ?? "").Trim(), CarreraId = vm.CarreraId });
            await db.SaveChangesAsync();
            await tx.CommitAsync();
            return (user, Array.Empty<string>());
        }
        catch (DbUpdateException)
        {
            await tx.RollbackAsync();
            return (null, new[] { "Los datos cambiaron o están duplicados. Revise identificación, correo y carrera." });
        }
    }
}
