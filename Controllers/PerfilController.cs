using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoFinal_VargasAnaCarolina.Data;
using ProyectoFinal_VargasAnaCarolina.Models;
using ProyectoFinal_VargasAnaCarolina.ViewModels;

namespace ProyectoFinal_VargasAnaCarolina.Controllers;
[Authorize(Roles = "Estudiante")]
public class PerfilController(CampusContext db) : Controller
{
    private Task<Estudiante?> Propio() => db.Estudiantes.Include(x => x.User).Include(x => x.Carrera).SingleOrDefaultAsync(x => x.UserId == User.FindFirstValue(ClaimTypes.NameIdentifier));
    private static void Completar(PerfilVm vm, Estudiante e)
    {
        vm.Nombre = e.Nombre;
        vm.Identificacion = e.Identificacion;
        vm.Carrera = e.Carrera!.Nombre;
        vm.Correo = e.User!.Email!;
    }

    public async Task<IActionResult> Index()
    {
        var e = await Propio();
        if (e == null)
            return NotFound();
        var vm = new PerfilVm
        {
            Telefono = e.Telefono,
            Direccion = e.Direccion
        };
        Completar(vm, e);
        return View(vm);
    }

    [HttpPost]
    public async Task<IActionResult> Index([Bind("Telefono,Direccion")] PerfilVm vm)
    {
        var e = await Propio();
        if (e == null)
            return NotFound();
        if (Request.Form.Keys.Any(k => new[] { "Id", "UserId", "Identificacion", "CarreraId", "Rol", "Role", "Nombre", "Correo" }.Contains(k, StringComparer.OrdinalIgnoreCase)))
            ModelState.AddModelError("", "Solo puede modificar el teléfono y la dirección de su propio expediente.");
        if (ModelState.IsValid)
        {
            e.Telefono = (vm.Telefono ?? "").Trim();
            e.Direccion = (vm.Direccion ?? "").Trim();
            await db.SaveChangesAsync();
            TempData["Ok"] = "Sus datos de contacto se actualizaron.";
            return RedirectToAction(nameof(Index));
        }

        Completar(vm, e);
        return View(vm);
    }
}
