using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoFinal_VargasAnaCarolina.Data;

namespace ProyectoFinal_VargasAnaCarolina.Controllers;
public class CatalogoController(CampusContext db) : Controller
{
    public async Task<IActionResult> Index() => View(await db.Carreras.AsNoTracking().Where(x => x.Activa).OrderBy(x => x.Nombre).ToListAsync());
}
