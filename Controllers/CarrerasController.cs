using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ProyectoFinal_VargasAnaCarolina.Data;
using ProyectoFinal_VargasAnaCarolina.Models;

namespace ProyectoFinal_VargasAnaCarolina.Controllers;
[Authorize(Roles = "Administrador")]
public class CarrerasController(CampusContext db) : Controller
{
    private Task Choices() => Task.CompletedTask;
    public async Task<IActionResult> Index(string? q)
    {
        ViewBag.Q = q;
        var query = db.Carreras.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(q))
            query = query.Where(x => x.Nombre.Contains(q));
        return View(await query.OrderBy(x => x.Nombre).ToListAsync());
    }

    public async Task<IActionResult> Details(int id)
    {
        var e = await db.Carreras.SingleOrDefaultAsync(x => x.Id == id);
        return e == null ? NotFound() : View(e);
    }

    public async Task<IActionResult> Create()
    {
        await Choices();
        return View("Formulario", new Carrera());
    }

    [HttpPost]
    public async Task<IActionResult> Create([Bind("Codigo,Nombre,Descripcion")] Carrera vm)
    {
        return await Guardar(0, vm);
    }

    public async Task<IActionResult> Edit(int id)
    {
        var e = await db.Carreras.FindAsync(id);
        if (e == null)
            return NotFound();
        await Choices();
        return View("Formulario", e);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(int id, [Bind("Codigo,Nombre,Descripcion")] Carrera vm)
    {
        return await Guardar(id, vm);
    }

    private async Task<IActionResult> Guardar(int id, Carrera vm)
    {
        vm.Id = id;
        vm.Nombre = (vm.Nombre ?? "").Trim();
        vm.Codigo = (vm.Codigo ?? "").Trim().ToUpperInvariant();
        if (await db.Carreras.AnyAsync(x => x.Id != id && x.Codigo == vm.Codigo))
            ModelState.AddModelError("Codigo", "El código ya está registrado.");
        if (ModelState.IsValid)
        {
            try
            {
                if (id == 0)
                    db.Carreras.Add(vm);
                else
                {
                    var e = await db.Carreras.FindAsync(id);
                    if (e == null)
                        return NotFound();
                    e.Codigo = vm.Codigo;
                    e.Nombre = vm.Nombre;
                    e.Descripcion = vm.Descripcion;
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
        var e = await db.Carreras.FindAsync(id);
        return e == null ? NotFound() : View(e);
    }

    [HttpPost, ActionName("Estado")]
    public async Task<IActionResult> ConfirmarEstado(int id)
    {
        var e = await db.Carreras.FindAsync(id);
        if (e == null)
            return NotFound();
        e.Activa = !e.Activa;
        await db.SaveChangesAsync();
        TempData["Ok"] = "Estado actualizado. Se conservó el historial.";
        return RedirectToAction(nameof(Index));
    }
}
