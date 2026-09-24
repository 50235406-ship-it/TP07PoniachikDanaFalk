using Microsoft.Data.SqlClient;
using Dapper;

namespace TP07PoniachikDanaFalk.Models;

public class BD
{
    private readonly string[] _connectionNames = ["DBRedSocial", "BDTP07"];

    private string GetConnectionString()
    {
        foreach (var databaseName in _connectionNames)
        {
            var connectionString = $@"Server=localhost;DataBase={databaseName};Integrated Security=True;TrustServerCertificate=True;";
            try
            {
                using var connection = new SqlConnection(connectionString);
                connection.Open();

                var hasUsuarios = connection.ExecuteScalar<int>("SELECT CASE WHEN OBJECT_ID(@TableName, 'U') IS NOT NULL THEN 1 ELSE 0 END", new { TableName = "dbo.Usuarios" }) == 1;
                var hasPublicaciones = connection.ExecuteScalar<int>("SELECT CASE WHEN OBJECT_ID(@TableName, 'U') IS NOT NULL THEN 1 ELSE 0 END", new { TableName = "dbo.Publicaciones" }) == 1;

                if (hasUsuarios && hasPublicaciones)
                {
                    return connectionString;
                }
            }
            catch
            {
                // Intenta con la siguiente base de datos disponible.
            }
        }

        return @"Server=localhost;DataBase=DBRedSocial;Integrated Security=True;TrustServerCertificate=True;";
    }

    public List<Usuario> ObtenerUsuarios()
    {
        using (SqlConnection connection = new SqlConnection(GetConnectionString()))
        {
            return connection.Query<Usuario>("SELECT * FROM dbo.Usuarios").ToList();
        }
    }

    public bool UsuarioExiste(string nombreUsuario)
    {
        return ObtenerUsuarios().Any(u => u.NombreUsuario == nombreUsuario);
    }

    public void GuardarUsuario(Usuario usuario)
    {
        using (SqlConnection connection = new SqlConnection(GetConnectionString()))
        {
            connection.Open();
            string query = "INSERT INTO dbo.Usuarios (NombreUsuario, Contraseña, Nombre, Apellido) VALUES (@NombreUsuario, @Contraseña, @Nombre, @Apellido)";
            connection.Execute(query, new { usuario.NombreUsuario, usuario.Contraseña, usuario.Nombre, usuario.Apellido });
        }
    }

    public List<Publicacion> ObtenerPublicaciones()
    {
        using (SqlConnection connection = new SqlConnection(GetConnectionString()))
        {
            return connection.Query<Publicacion>(@"
                SELECT p.Id, p.IdUsuario, p.Titulo, p.Descripcion, p.Imagen, p.FechaPublicacion, u.NombreUsuario
                FROM dbo.Publicaciones p
                INNER JOIN dbo.Usuarios u ON u.Id = p.IdUsuario
                ORDER BY p.FechaPublicacion DESC").ToList();
        }
    }

    public void GuardarPublicacion(Publicacion publicacion)
    {
        using (SqlConnection connection = new SqlConnection(GetConnectionString()))
        {
            connection.Open();
            string query = @"
                INSERT INTO dbo.Publicaciones (IdUsuario, Titulo, Descripcion, Imagen, FechaPublicacion)
                VALUES (@IdUsuario, @Titulo, @Descripcion, @Imagen, @FechaPublicacion)";

            connection.Execute(query, new
            {
                publicacion.IdUsuario,
                publicacion.Titulo,
                publicacion.Descripcion,
                publicacion.Imagen,
                publicacion.FechaPublicacion
            });
        }
    }
}