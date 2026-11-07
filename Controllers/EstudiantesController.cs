using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ProyectoFinal_VargasAnaCarolina.Data;
using ProyectoFinal_VargasAnaCarolina.Models;
using ProyectoFinal_VargasAnaCarolina.Services;
using ProyectoFinal_VargasAnaCarolina.ViewModels;

namespace ProyectoFinal_VargasAnaCarolina.Controllers;
[Authorize(Roles = "Administrador")]
public class EstudiantesController(CampusContext db, UserManager<CampusUser> users, RegistroService registro) : Controller
{
    private async Task Choices() => ViewBag.Carreras = new SelectList(await db.Carreras.AsNoTracking().Where(x => x.Activa).OrderBy(x => x.Nombre).ToListAsync(), "Id", "Nombre");
    public async Task<IActionResult> Index(string? q)
    {
        ViewBag.Q = q;
        var query = db.Estudiantes.AsNoTracking().Include(x => x.User).Include(x => x.Carrera).AsQueryable();
        if (!string.IsNullOrWhiteSpace(q))
            query = query.Where(x => x.Nombre.Contains(q) || x.Identificacion.Contains(q));
        return View(await query.OrderBy(x => x.Nombre).ToListAsync());
    }

    public async Task<IActionResult> Details(int id)
    {
        var e = await db.Estudiantes.Include(x => x.User).Include(x => x.Carrera).SingleOrDefaultAsync(x => x.Id == id);
        return e == null ? NotFound() : View(e);
    }

    public async Task<IActionResult> Create()
    {
        await Choices();
        return View(new RegistroVm());
    }

    [HttpPost]
    public async Task<IActionResult> Create(RegistroVm vm)
    {
        if (ModelState.IsValid)
        {
            var result = await registro.Crear(vm);
            if (result.User != null)
            {
                TempData["Ok"] = "Estudiante registrado con cuenta de acceso.";
                return RedirectToAction(nameof(Index));
            }

            foreach (var e in result.Errores)
                ModelState.AddModelError("", e);
        }

        await Choices();
        return View(vm);
    }

    public async Task<IActionResult> Edit(int id)
    {
        var e = await db.Estudiantes.Include(x => x.User).SingleOrDefaultAsync(x => x.Id == id);
        if (e == null)
            return NotFound();
        await Choices();
        return View(new EstudianteEditarVm { Id = id, Identificacion = e.Identificacion, Nombre = e.Nombre, Correo = e.User!.Email!, Telefono = e.Telefono, Direccion = e.Direccion, CarreraId = e.CarreraId });
    }

    [HttpPost]
    public async Task<IActionResult> Edit(int id, EstudianteEditarVm vm)
    {
        vm.Id = id;
        var e = await db.Estudiantes.Include(x => x.User).SingleOrDefaultAsync(x => x.Id == id);
        if (e == null)
            return NotFound();
        vm.Identificacion = (vm.Identificacion ?? "").Trim().ToUpperInvariant();
        vm.Correo = (vm.Correo ?? "").Trim().ToLowerInvariant();
        if (await db.Estudiantes.AnyAsync(x => x.Id != id && x.Identificacion == vm.Identificacion))
            ModelState.AddModelError("Identificacion", "La identificación ya está registrada.");
        var otro = await users.FindByEmailAsync(vm.Correo);
        if (otro != null && otro.Id != e.UserId)
            ModelState.AddModelError("Correo", "El correo ya está registrado.");
        if (!await db.Carreras.AnyAsync(x => x.Id == vm.CarreraId && x.Activa))
            ModelState.AddModelError("CarreraId", "Seleccione una carrera activa.");
        if (e.CarreraId != vm.CarreraId && await db.Matriculas.AnyAsync(x => x.EstudianteId == id) && !vm.RevisionCambioCarrera)
            ModelState.AddModelError("RevisionCambioCarrera", "Confirme la revisión administrativa antes de cambiar la carrera. Las matrículas previas conservarán su carrera histórica.");
        if (ModelState.IsValid)
        {
            await using var tx = await db.Database.BeginTransactionAsync();
            try
            {
                e.Identificacion = vm.Identificacion;
                e.Nombre = (vm.Nombre ?? "").Trim();
                e.Telefono = (vm.Telefono ?? "").Trim();
                e.Direccion = (vm.Direccion ?? "").Trim();
                e.CarreraId = vm.CarreraId;
                e.User!.Email = vm.Correo;
                e.User.UserName = vm.Correo;
                var result = await users.UpdateAsync(e.User);
                if (!result.Succeeded)
                {
                    foreach (var error in result.Errors)
                        ModelState.AddModelError("", error.Description);
                    await tx.RollbackAsync();
                }
                else
                {
                    await users.UpdateSecurityStampAsync(e.User);
                    await db.SaveChangesAsync();
                    await tx.CommitAsync();
                    TempData["Ok"] = "Expediente actualizado; se conservó el historial y se renovará el acceso del estudiante.";
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (DbUpdateException)
            {
                await tx.RollbackAsync();
                ModelState.AddModelError("", "No se pudo guardar. Revise los datos duplicados.");
            }
        }

        await Choices();
        return View(vm);
    }

    public async Task<IActionResult> Estado(int id)
    {
        var e = await db.Estudiantes.FindAsync(id);
        return e == null ? NotFound() : View(e);
    }

    [HttpPost, ActionName("Estado")]
    public async Task<IActionResult> CambiarEstado(int id)
    {
        var e = await db.Estudiantes.FindAsync(id);
        if (e == null)
            return NotFound();
        e.Activo = !e.Activo;
        await db.SaveChangesAsync();
        TempData["Ok"] = "Estado del expediente actualizado. El acceso se controla por separado.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Acceso(int id)
    {
        var e = await db.Estudiantes.Include(x => x.User).SingleOrDefaultAsync(x => x.Id == id);
        return e == null ? NotFound() : View(e);
    }

    [HttpPost, ActionName("Acceso")]
    public async Task<IActionResult> CambiarAcceso(int id)
    {
        var e = await db.Estudiantes.Include(x => x.User).SingleOrDefaultAsync(x => x.Id == id);
        if (e == null)
            return NotFound();
        if (!await users.IsInRoleAsync(e.User!, "Estudiante") || await users.IsInRoleAsync(e.User!, "Administrador"))
            return Forbid();
        await using var tx = await db.Database.BeginTransactionAsync();
        e.User!.Habilitado = !e.User.Habilitado;
        var result = await users.UpdateAsync(e.User);
        if (result.Succeeded)
            result = await users.UpdateSecurityStampAsync(e.User);
        if (!result.Succeeded)
        {
            await tx.RollbackAsync();
            ModelState.AddModelError("", "No se pudo actualizar el acceso.");
            return View("Acceso", e);
        }

        await tx.CommitAsync();
        TempData["Ok"] = "Acceso actualizado; las sesiones previas ya no son válidas.";
        return RedirectToAction(nameof(Index));
    }
}
