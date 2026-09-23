using Microsoft.Data.SqlClient;
using Dapper;

namespace TP07PoniachikDanaFalk.Models;

public class BD
{

    private string _connectionString = @"Server=localhost;DataBase=BDTP07;Integrated Security=True;TrustServerCertificate=True;";
    public List<Usuario> ObtenerUsuarios()
    {

        List<Usuario> usuarios = new List<Usuario>();
        using (SqlConnection connection = new SqlConnection(_connectionString))
        {

            usuarios = connection.Query<Usuario>("SELECT * FROM Usuarios").ToList();

        }
        return usuarios;

    }

    public bool UsuarioExiste(string nombreUsuario)
    {

        bool existe = false;
        for (int i = 0; i < ObtenerUsuarios().Count; i++)
        {

            if (ObtenerUsuarios()[i].NombreUsuario == nombreUsuario)
            {

                existe = true;

            }

        }
        return existe;
        
    }

    public void GuardarUsuario(Usuario usuario)
    {
        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            connection.Open();
            string query = "INSERT INTO Usuarios (NombreUsuario, Contraseña, Nombre, Apellido) VALUES (@NombreUsuario, @Contraseña, @Nombre, @Apellido)";
            connection.Execute(query, new { usuario.NombreUsuario, usuario.Contraseña, usuario.Nombre, usuario.Apellido});
        }
    }

}