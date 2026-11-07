using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ProyectoFinal_VargasAnaCarolina.Data;
/// <summary>Las migraciones se generan para SQL Server sin ejecutar el inicializador de la aplicación.</summary>
public class DesignFactory : IDesignTimeDbContextFactory<CampusContext>
{
    public CampusContext CreateDbContext(string[] args)
    {
        var config = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json", optional: false).AddUserSecrets<DesignFactory>(optional: true).AddEnvironmentVariables().Build();
        var options = new DbContextOptionsBuilder<CampusContext>().UseSqlServer(config.GetConnectionString("CampusConnection")).Options;
        return new CampusContext(options);
    }
}
