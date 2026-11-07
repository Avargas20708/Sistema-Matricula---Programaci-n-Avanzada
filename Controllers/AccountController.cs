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
public class AccountController(SignInManager<CampusUser> signin, UserManager<CampusUser> users, CampusContext db, RegistroService registro) : Controller
{
    private async Task Carreras(int selected = 0) => ViewBag.Carreras = new SelectList(await db.Carreras.AsNoTracking().Where(x => x.Activa).OrderBy(x => x.Nombre).ToListAsync(), "Id", "Nombre", selected);
    [HttpGet]
    public async Task<IActionResult> Register(int carreraId = 0)
    {
        await Carreras(carreraId);
        return View(new RegistroVm { CarreraId = carreraId });
    }

    [HttpPost]
    public async Task<IActionResult> Register(RegistroVm vm)
    {
        if (ModelState.IsValid)
        {
            var result = await registro.Crear(vm);
            if (result.User != null)
            {
                await signin.SignInAsync(result.User, false);
                TempData["Ok"] = "Su cuenta y expediente se crearon correctamente.";
                return RedirectToAction("Index", "Home");
            }

            foreach (var e in result.Errores)
                ModelState.AddModelError("", e);
        }

        await Carreras(vm.CarreraId);
        return View(vm);
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl = null) => View(new LoginVm { ReturnUrl = returnUrl });
    [HttpPost]
    public async Task<IActionResult> Login(LoginVm vm)
    {
        if (ModelState.IsValid)
        {
            var user = await users.FindByEmailAsync((vm.Correo ?? "").Trim().ToLowerInvariant());
            if (user != null && user.Habilitado)
            {
                var r = await signin.PasswordSignInAsync(user, vm.Password, false, true);
                if (r.Succeeded)
                    return Url.IsLocalUrl(vm.ReturnUrl) ? LocalRedirect(vm.ReturnUrl!) : RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError("", "No fue posible ingresar. Revise sus credenciales o consulte a administración si su cuenta está bloqueada.");
        }

        return View(vm);
    }

    [Authorize, HttpPost]
    public async Task<IActionResult> Logout()
    {
        await signin.SignOutAsync();
        return RedirectToAction(nameof(Login));
    }

    public IActionResult Denied()
    {
        Response.StatusCode = 403;
        return View();
    }
}
