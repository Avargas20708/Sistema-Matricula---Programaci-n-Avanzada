using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ProyectoFinal_VargasAnaCarolina.Data;
using ProyectoFinal_VargasAnaCarolina.Models;

namespace ProyectoFinal_VargasAnaCarolina.Controllers;
[Authorize(Roles = "Administrador")]
public class CursosController(CampusContext db) : Controller
{
    private async Task Choices() => ViewBag.Carreras = new SelectList(await db.Carreras.AsNoTracking().Where(x => x.Activa).OrderBy(x => x.Nombre).ToListAsync(), "Id", "Nombre");
    public async Task<IActionResult> Index(string? q)
    {
        ViewBag.Q = q;
        var query = db.Cursos.AsNoTracking().Include(x => x.Carrera).AsQueryable();
        if (!string.IsNullOrWhiteSpace(q))
            query = query.Where(x => x.Nombre.Contains(q));
        return View(await query.OrderBy(x => x.Nombre).ToListAsync());
    }

    public async Task<IActionResult> Details(int id)
    {
        var e = await db.Cursos.Include(x => x.Carrera).SingleOrDefaultAsync(x => x.Id == id);
        return e == null ? NotFound() : View(e);
    }

    public async Task<IActionResult> Create()
    {
        await Choices();
        return View("Formulario", new Curso());
    }

    [HttpPost]
    public async Task<IActionResult> Create([Bind("Codigo,Nombre,Creditos,CarreraId")] Curso vm)
    {
        return await Guardar(0, vm);
    }

    public async Task<IActionResult> Edit(int id)
    {
        var e = await db.Cursos.FindAsync(id);
        if (e == null)
            return NotFound();
        await Choices();
        return View("Formulario", e);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(int id, [Bind("Codigo,Nombre,Creditos,CarreraId")] Curso vm)
    {
        return await Guardar(id, vm);
    }

    private async Task<IActionResult> Guardar(int id, Curso vm)
    {
        vm.Id = id;
        vm.Nombre = (vm.Nombre ?? "").Trim();
        vm.Codigo = (vm.Codigo ?? "").Trim().ToUpperInvariant();
        if (await db.Cursos.AnyAsync(x => x.Id != id && x.Codigo == vm.Codigo))
            ModelState.AddModelError("Codigo", "El código ya está registrado.");
        if (!await db.Carreras.AnyAsync(x => x.Id == vm.CarreraId && x.Activa))
            ModelState.AddModelError("CarreraId", "Seleccione una carrera activa.");
        if (ModelState.IsValid)
        {
            try
            {
                if (id == 0)
                    db.Cursos.Add(vm);
                else
                {
                    var e = await db.Cursos.FindAsync(id);
                    if (e == null)
                        return NotFound();
                    e.Codigo = vm.Codigo;
                    e.Nombre = vm.Nombre;
                    e.Creditos = vm.Creditos;
                    e.CarreraId = vm.CarreraId;
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
        var e = await db.Cursos.FindAsync(id);
        return e == null ? NotFound() : View(e);
    }

    [HttpPost, ActionName("Estado")]
    public async Task<IActionResult> ConfirmarEstado(int id)
    {
        var e = await db.Cursos.FindAsync(id);
        if (e == null)
            return NotFound();
        e.Activo = !e.Activo;
        await db.SaveChangesAsync();
        TempData["Ok"] = "Estado actualizado. Se conservó el historial.";
        return RedirectToAction(nameof(Index));
    }
}
