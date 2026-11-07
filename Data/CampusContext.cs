using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ProyectoFinal_VargasAnaCarolina.Models;

namespace ProyectoFinal_VargasAnaCarolina.Data;
public class CampusContext(DbContextOptions<CampusContext> options) : IdentityDbContext<CampusUser>(options)
{
    public DbSet<Carrera> Carreras => Set<Carrera>();
    public DbSet<Curso> Cursos => Set<Curso>();
    public DbSet<Docente> Docentes => Set<Docente>();
    public DbSet<Estudiante> Estudiantes => Set<Estudiante>();
    public DbSet<Periodo> Periodos => Set<Periodo>();
    public DbSet<Grupo> Grupos => Set<Grupo>();
    public DbSet<Matricula> Matriculas => Set<Matricula>();
    public DbSet<DetalleMatricula> DetallesMatricula => Set<DetalleMatricula>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        base.OnModelCreating(b);
        b.Entity<Carrera>().HasIndex(x => x.Codigo).IsUnique();
        b.Entity<Curso>().HasIndex(x => x.Codigo).IsUnique();
        b.Entity<Docente>().HasIndex(x => x.Identificacion).IsUnique();
        b.Entity<Docente>().HasIndex(x => x.Correo).IsUnique();
        b.Entity<Estudiante>().HasIndex(x => x.Identificacion).IsUnique();
        b.Entity<Estudiante>().HasIndex(x => x.UserId).IsUnique();
        b.Entity<Estudiante>().HasOne(x => x.User).WithOne().HasForeignKey<Estudiante>(x => x.UserId);
        b.Entity<Periodo>().Property(x => x.Codigo).HasMaxLength(20);
        b.Entity<Periodo>().HasIndex(x => x.Codigo).IsUnique();
        b.Entity<Grupo>().Property(x => x.Codigo).HasMaxLength(20);
        b.Entity<Grupo>().HasIndex(x => new { x.Codigo, x.PeriodoId }).IsUnique();
        b.Entity<Matricula>().Property(x => x.Numero).HasMaxLength(40);
        b.Entity<Matricula>().HasIndex(x => x.Numero).IsUnique();
        b.Entity<Matricula>().HasIndex(x => new { x.EstudianteId, x.PeriodoId }).IsUnique();
        b.Entity<Matricula>().Property(x => x.Estado).HasMaxLength(20);
        b.Entity<DetalleMatricula>().Property(x => x.Estado).HasMaxLength(20);
        b.Entity<DetalleMatricula>().HasIndex(x => new { x.MatriculaId, x.GrupoId }).IsUnique();
        foreach (var fk in b.Model.GetEntityTypes().Where(t => !t.ClrType.Namespace!.StartsWith("Microsoft.AspNetCore.Identity")).SelectMany(t => t.GetForeignKeys()))
            fk.DeleteBehavior = DeleteBehavior.Restrict;
    }
}
