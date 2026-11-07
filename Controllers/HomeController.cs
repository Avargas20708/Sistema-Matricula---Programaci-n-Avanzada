using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoFinal_VargasAnaCarolina.Data;

namespace ProyectoFinal_VargasAnaCarolina.Controllers;
[Authorize]
public class HomeController(CampusContext db) : Controller
{
    public async Task<IActionResult> Index()
    {
        ViewBag.Carreras = await db.Carreras.CountAsync(x => x.Activa);
        if (User.IsInRole("Administrador"))
        {
            ViewBag.Estudiantes = await db.Estudiantes.CountAsync(x => x.Activo);
            ViewBag.Cursos = await db.Cursos.CountAsync(x => x.Activo);
        }
        return View();
    }

    [AllowAnonymous]
    public IActionResult Error()
    {
        Response.StatusCode = 500;
        return View();
    }
}
