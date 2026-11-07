using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using ProyectoFinal_VargasAnaCarolina.Data;
using ProyectoFinal_VargasAnaCarolina.Models;
using ProyectoFinal_VargasAnaCarolina.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<CampusContext>(o =>
{
    if (builder.Configuration.GetValue<bool>("Demo:Sqlite"))
        o.UseSqlite(builder.Configuration.GetConnectionString("DemoConnection"));
    else
        o.UseSqlServer(builder.Configuration.GetConnectionString("CampusConnection"));
});
builder.Services.AddIdentity<CampusUser, IdentityRole>(o =>
{
    o.User.RequireUniqueEmail = true;
    o.Password.RequiredLength = 8;
    o.Password.RequireDigit = true;
    o.Password.RequireUppercase = true;
    o.Password.RequireLowercase = true;
    o.Password.RequireNonAlphanumeric = true;
    o.Lockout.MaxFailedAccessAttempts = 5;
    o.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
}).AddErrorDescriber<ErroresIdentity>().AddEntityFrameworkStores<CampusContext>().AddDefaultTokenProviders();
builder.Services.Configure<SecurityStampValidatorOptions>(o => o.ValidationInterval = TimeSpan.Zero);
builder.Services.AddScoped<CampusCookieEvents>();
builder.Services.AddScoped<RegistroService>();
builder.Services.ConfigureApplicationCookie(o =>
{
    o.LoginPath = "/Account/Login";
    o.AccessDeniedPath = "/Account/Denied";
    o.Cookie.HttpOnly = true;
    o.Cookie.SameSite = SameSiteMode.Lax;
    o.ExpireTimeSpan = TimeSpan.FromMinutes(30);
    o.EventsType = typeof(CampusCookieEvents);
});
builder.Services.AddControllersWithViews(o => o.Filters.Add(new AutoValidateAntiforgeryTokenAttribute())).AddMvcOptions(o => o.ModelBindingMessageProvider.SetValueMustBeANumberAccessor(_ => "Ingrese un número entero válido."));
var app = builder.Build();
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");
await Inicializador.Ejecutar(app.Services, app.Configuration);
app.Run();
public partial class Program
{
}
