using System.ComponentModel.DataAnnotations;

namespace ProyectoFinal_VargasAnaCarolina.ViewModels;
public class RegistroVm
{
    [Required(ErrorMessage = "Ingrese la identificación."), StringLength(30)]
    public string Identificacion { get; set; } = "";

    [Required(ErrorMessage = "Ingrese el nombre completo."), StringLength(120)]
    public string Nombre { get; set; } = "";

    [Required(ErrorMessage = "Ingrese el correo."), EmailAddress(ErrorMessage = "Ingrese un correo válido."), StringLength(256)]
    public string Correo { get; set; } = "";

    [Required(ErrorMessage = "Ingrese el teléfono."), RegularExpression(@"[0-9+() -]{8,20}", ErrorMessage = "Use un teléfono de 8 a 20 caracteres.")]
    public string Telefono { get; set; } = "";

    [Required(ErrorMessage = "Ingrese la dirección."), StringLength(250)]
    public string Direccion { get; set; } = "";

    [Range(1, int.MaxValue, ErrorMessage = "Seleccione una carrera activa.")]
    public int CarreraId { get; set; }

    [Required(ErrorMessage = "Ingrese la contraseña."), StringLength(100, MinimumLength = 8, ErrorMessage = "Use entre 8 y 100 caracteres."), DataType(DataType.Password)]
    public string Password { get; set; } = "";

    [Required(ErrorMessage = "Confirme la contraseña."), Compare(nameof(Password), ErrorMessage = "Las contraseñas no coinciden."), DataType(DataType.Password)]
    public string ConfirmPassword { get; set; } = "";
}

public class LoginVm
{
    [Required, EmailAddress]
    public string Correo { get; set; } = "";

    [Required, DataType(DataType.Password)]
    public string Password { get; set; } = "";
    public string? ReturnUrl { get; set; }
}

public class PerfilVm
{
    public string Nombre { get; set; } = "";
    public string Identificacion { get; set; } = "";
    public string Carrera { get; set; } = "";
    public string Correo { get; set; } = "";

    [Required(ErrorMessage = "Ingrese el teléfono."), RegularExpression(@"[0-9+() -]{8,20}", ErrorMessage = "Use un teléfono de 8 a 20 caracteres.")]
    public string Telefono { get; set; } = "";

    [Required(ErrorMessage = "Ingrese la dirección."), StringLength(250)]
    public string Direccion { get; set; } = "";
}

public class EstudianteEditarVm
{
    public int Id { get; set; }

    [Required, StringLength(30)]
    public string Identificacion { get; set; } = "";

    [Required, StringLength(120)]
    public string Nombre { get; set; } = "";

    [Required, EmailAddress, StringLength(256)]
    public string Correo { get; set; } = "";

    [Required, RegularExpression(@"[0-9+() -]{8,20}")]
    public string Telefono { get; set; } = "";

    [Required, StringLength(250)]
    public string Direccion { get; set; } = "";

    [Range(1, int.MaxValue)]
    public int CarreraId { get; set; }
    public bool RevisionCambioCarrera { get; set; }
}
