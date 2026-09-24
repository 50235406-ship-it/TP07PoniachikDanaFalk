namespace TP07PoniachikDanaFalk.Models;

public class Usuario
{

    public string NombreUsuario {get; set;}
    public string Contraseña {get; set;}
    public string Nombre {get; set;}
    public string Apellido {get; set;}
    public string TipoUsuario {get; set;}
    public int Id {get; set;}

    public Usuario() { }

    public Usuario (string NombreUsuario2, string Contraseña2, string Nombre2, string Apellido2, string TipoUsuario2) {

        NombreUsuario = NombreUsuario2;
        Contraseña = Contraseña2;
        Nombre = Nombre2;
        Apellido = Apellido2;
        TipoUsuario = TipoUsuario2;

    }

    public static bool Logueo(string nombreUsuario, string contraseña, List<Usuario> usuarios)
    {
        foreach (var usuario in usuarios)
        {
            if (usuario.NombreUsuario == nombreUsuario && usuario.Contraseña == contraseña)
            {
                return true;
            }
        }
        return false;
    }
}