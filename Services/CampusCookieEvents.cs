using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using ProyectoFinal_VargasAnaCarolina.Models;

namespace ProyectoFinal_VargasAnaCarolina.Services;
public class CampusCookieEvents(UserManager<CampusUser> users) : CookieAuthenticationEvents
{
    public override async Task ValidatePrincipal(CookieValidatePrincipalContext context)
    {
        await SecurityStampValidator.ValidatePrincipalAsync(context);
        if (context.Principal is null)
            return;
        var user = await users.GetUserAsync(context.Principal);
        if (user is null || !user.Habilitado)
        {
            context.RejectPrincipal();
            await context.HttpContext.SignOutAsync(IdentityConstants.ApplicationScheme);
        }
    }
}
