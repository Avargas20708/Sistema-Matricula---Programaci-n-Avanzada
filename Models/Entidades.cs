using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace ProyectoFinal_VargasAnaCarolina.Models;
public class CampusUser : IdentityUser
{
    public bool Habilitado { get; set; } = true;
}

public class Carrera
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Ingrese el código."), StringLength(20)]
    public string Codigo { get; set; } = "";

    [Required(ErrorMessage = "Ingrese el nombre."), StringLength(120)]
    public string Nombre { get; set; } = "";

    [Required(ErrorMessage = "Ingrese la descripción."), StringLength(1000)]
    public string Descripcion { get; set; } = "";
    public bool Activa { get; set; } = true;
}

public class Curso
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Ingrese el código."), StringLength(20)]
    public string Codigo { get; set; } = "";

    [Required(ErrorMessage = "Ingrese el nombre."), StringLength(120)]
    public string Nombre { get; set; } = "";

    [Range(1, 12, ErrorMessage = "Los créditos deben ser enteros entre 1 y 12.")]
    public int Creditos { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Seleccione una carrera.")]
    public int CarreraId { get; set; }
    public Carrera? Carrera { get; set; }
    public bool Activo { get; set; } = true;
}

public class Docente
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Ingrese la identificación."), StringLength(30)]
    public string Identificacion { get; set; } = "";

    [Required(ErrorMessage = "Ingrese el nombre."), StringLength(120)]
    public string Nombre { get; set; } = "";

    [Required(ErrorMessage = "Ingrese el correo."), EmailAddress(ErrorMessage = "Ingrese un correo válido."), StringLength(256)]
    public string Correo { get; set; } = "";

    [Required(ErrorMessage = "Ingrese el teléfono."), RegularExpression(@"[0-9+() -]{8,20}", ErrorMessage = "Use un teléfono de 8 a 20 caracteres.")]
    public string Telefono { get; set; } = "";
    public bool Activo { get; set; } = true;
}

public class Estudiante
{
    public int Id { get; set; }
    public string UserId { get; set; } = "";
    public CampusUser? User { get; set; }

    [Required, StringLength(30)]
    public string Identificacion { get; set; } = "";

    [Required, StringLength(120)]
    public string Nombre { get; set; } = "";

    [Required, StringLength(20)]
    public string Telefono { get; set; } = "";

    [Required, StringLength(250)]
    public string Direccion { get; set; } = "";
    public int CarreraId { get; set; }
    public Carrera? Carrera { get; set; }
    public bool Activo { get; set; } = true;
}

// Estructura prevista para avances posteriores. No se expone matrícula operativa en Avance 2.
public class Periodo
{
    public int Id { get; set; }
    public string Codigo { get; set; } = "";
    public DateTime InicioMatricula { get; set; }
    public DateTime FinMatricula { get; set; }
    public DateTime LimiteRetiro { get; set; }
}

public class Grupo
{
    public int Id { get; set; }
    public string Codigo { get; set; } = "";
    public int CursoId { get; set; }
    public Curso? Curso { get; set; }
    public int DocenteId { get; set; }
    public Docente? Docente { get; set; }
    public int PeriodoId { get; set; }
    public Periodo? Periodo { get; set; }
    public int Cupo { get; set; }
    public bool Activo { get; set; } = true;
}

public class Matricula
{
    public int Id { get; set; }
    public string Numero { get; set; } = "";
    public int EstudianteId { get; set; }
    public Estudiante? Estudiante { get; set; }
    public int PeriodoId { get; set; }
    public Periodo? Periodo { get; set; }
    public int CarreraHistoricaId { get; set; }
    public Carrera? CarreraHistorica { get; set; }
    public DateTime Fecha { get; set; }
    public string Estado { get; set; } = "Confirmada";
}

public class DetalleMatricula
{
    public int Id { get; set; }
    public int MatriculaId { get; set; }
    public Matricula? Matricula { get; set; }
    public int GrupoId { get; set; }
    public Grupo? Grupo { get; set; }
    public string Estado { get; set; } = "Activo";
}
