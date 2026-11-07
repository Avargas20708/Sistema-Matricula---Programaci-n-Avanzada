using Microsoft.AspNetCore.Identity;

namespace ProyectoFinal_VargasAnaCarolina.Services;
/// <summary>Mensajes de Identity presentados en español, sin exponer detalles de inicio de sesión.</summary>
public class ErroresIdentity : IdentityErrorDescriber
{
    private static IdentityError Error(string code, string message) => new()
    {
        Code = code,
        Description = message
    };
    public override IdentityError DuplicateEmail(string email) => Error(nameof(DuplicateEmail), "El correo ya está registrado.");
    public override IdentityError DuplicateUserName(string userName) => Error(nameof(DuplicateUserName), "El correo ya está registrado.");
    public override IdentityError InvalidEmail(string? email) => Error(nameof(InvalidEmail), "Ingrese un correo válido.");
    public override IdentityError InvalidUserName(string? userName) => Error(nameof(InvalidUserName), "El correo contiene caracteres no admitidos.");
    public override IdentityError PasswordTooShort(int length) => Error(nameof(PasswordTooShort), $"Use al menos {length} caracteres.");
    public override IdentityError PasswordRequiresDigit() => Error(nameof(PasswordRequiresDigit), "La contraseña debe incluir un número.");
    public override IdentityError PasswordRequiresLower() => Error(nameof(PasswordRequiresLower), "La contraseña debe incluir una minúscula.");
    public override IdentityError PasswordRequiresUpper() => Error(nameof(PasswordRequiresUpper), "La contraseña debe incluir una mayúscula.");
    public override IdentityError PasswordRequiresNonAlphanumeric() => Error(nameof(PasswordRequiresNonAlphanumeric), "La contraseña debe incluir un símbolo.");
    public override IdentityError PasswordRequiresUniqueChars(int uniqueChars) => Error(nameof(PasswordRequiresUniqueChars), $"Use al menos {uniqueChars} caracteres diferentes.");
    public override IdentityError ConcurrencyFailure() => Error(nameof(ConcurrencyFailure), "Los datos cambiaron. Actualice la página e intente nuevamente.");
}
