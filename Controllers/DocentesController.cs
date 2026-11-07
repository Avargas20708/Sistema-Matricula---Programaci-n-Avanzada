using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ProyectoFinal_VargasAnaCarolina.Data;
using ProyectoFinal_VargasAnaCarolina.Models;

namespace ProyectoFinal_VargasAnaCarolina.Controllers;
[Authorize(Roles = "Administrador")]
public class DocentesController(CampusContext db) : Controller
{
    private Task Choices() => Task.CompletedTask;
    public async Task<IActionResult> Index(string? q)
    {
        ViewBag.Q = q;
        var query = db.Docentes.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(q))
            query = query.Where(x => x.Nombre.Contains(q));
        return View(await query.OrderBy(x => x.Nombre).ToListAsync());
    }

    public async Task<IActionResult> Details(int id)
    {
        var e = await db.Docentes.SingleOrDefaultAsync(x => x.Id == id);
        return e == null ? NotFound() : View(e);
    }

    public async Task<IActionResult> Create()
    {
        await Choices();
        return View("Formulario", new Docente());
    }

    [HttpPost]
    public async Task<IActionResult> Create([Bind("Identificacion,Nombre,Correo,Telefono")] Docente vm)
    {
        return await Guardar(0, vm);
    }

    public async Task<IActionResult> Edit(int id)
    {
        var e = await db.Docentes.FindAsync(id);
        if (e == null)
            return NotFound();
        await Choices();
        return View("Formulario", e);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(int id, [Bind("Identificacion,Nombre,Correo,Telefono")] Docente vm)
    {
        return await Guardar(id, vm);
    }

    private async Task<IActionResult> Guardar(int id, Docente vm)
    {
        vm.Id = id;
        vm.Nombre = (vm.Nombre ?? "").Trim();
        vm.Identificacion = (vm.Identificacion ?? "").Trim().ToUpperInvariant();
        vm.Correo = (vm.Correo ?? "").Trim().ToLowerInvariant();
        vm.Telefono = (vm.Telefono ?? "").Trim();
        if (await db.Docentes.AnyAsync(x => x.Id != id && x.Identificacion == vm.Identificacion))
            ModelState.AddModelError("Identificacion", "La identificación ya está registrada.");
        if (await db.Docentes.AnyAsync(x => x.Id != id && x.Correo == vm.Correo))
            ModelState.AddModelError("Correo", "El correo ya está registrado.");
        if (ModelState.IsValid)
        {
            try
            {
                if (id == 0)
                    db.Docentes.Add(vm);
                else
                {
                    var e = await db.Docentes.FindAsync(id);
                    if (e == null)
                        return NotFound();
                    e.Identificacion = vm.Identificacion;
                    e.Nombre = vm.Nombre;
                    e.Correo = vm.Correo;
                    e.Telefono = vm.Telefono;
                }

                await db.SaveChangesAsync();
                TempData["Ok"] = "Registro guardado correctamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError("", "No se pudo guardar. Revise los datos duplicados y las relaciones.");
            }
        }

        await Choices();
        return View("Formulario", vm);
    }

    public async Task<IActionResult> Estado(int id)
    {
        var e = await db.Docentes.FindAsync(id);
        return e == null ? NotFound() : View(e);
    }

    [HttpPost, ActionName("Estado")]
    public async Task<IActionResult> ConfirmarEstado(int id)
    {
        var e = await db.Docentes.FindAsync(id);
        if (e == null)
            return NotFound();
        var ahora = DateTime.UtcNow.AddHours(-6);
        if (e.Activo && await db.Grupos.AnyAsync(x => x.DocenteId == id && x.Activo && x.Periodo!.InicioMatricula <= ahora && x.Periodo.FinMatricula >= ahora))
        {
            ModelState.AddModelError("", "Este docente tiene grupos activos en un período abierto. Reasígnelos antes de desactivarlo.");
            return View("Estado", e);
        }

        e.Activo = !e.Activo;
        await db.SaveChangesAsync();
        TempData["Ok"] = "Estado actualizado. Se conservó el historial.";
        return RedirectToAction(nameof(Index));
    }
}
