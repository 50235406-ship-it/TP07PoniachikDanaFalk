namespace TP07PoniachikDanaFalk.Models;

public class Usuario
{
    public string NombreUsuario { get; set; } = string.Empty;
    public string Contraseña { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Apellido { get; set; } = string.Empty;
    public int Id { get; set; }

    public Usuario()
    {
    }

    public Usuario(string NombreUsuario2, string Contraseña2, string Nombre2, string Apellido2)
    {
        NombreUsuario = NombreUsuario2;
        Contraseña = Contraseña2;
        Nombre = Nombre2;
        Apellido = Apellido2;
    }

    public static bool Logueo(string nombreUsuario, string contraseña, List<Usuario> usuarios)
    {
        return usuarios.Any(usuario => usuario.NombreUsuario == nombreUsuario && usuario.Contraseña == contraseña);
    }
}