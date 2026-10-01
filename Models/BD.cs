using Dapper;
using Microsoft.Data.SqlClient;

namespace TP07PoniachikDanaFalk.Models;

public class BD
{
    private string _connectionString = @"Server=localhost;Database=DBRedSocial;Integrated Security=True;TrustServerCertificate=True;";

    public BD()
    {
    }

    public List<Usuario> ObtenerUsuarios()
    {
        string query = "SELECT * FROM dbo.Usuarios ORDER BY Id";
        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            return connection.Query<Usuario>(query).ToList();
        }
    }

    public bool UsuarioExiste(string nombreUsuario)
    {
        return ObtenerUsuarios().Any(u => u.NombreUsuario == nombreUsuario);
    }

    public void GuardarUsuario(Usuario usuario)
    {
        string query = @"INSERT INTO dbo.Usuarios (NombreUsuario, Contraseña, Nombre, Apellido) VALUES (@NombreUsuario, @Contraseña, @Nombre, @Apellido)";
        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            connection.Execute(query, new { usuario.NombreUsuario, usuario.Contraseña, usuario.Nombre, usuario.Apellido });
        }
    }

    public int ContarPublicaciones()
    {
        string query = "SELECT COUNT(*) FROM dbo.Publicaciones";
        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            return connection.ExecuteScalar<int>(query);
        }
    }

    public List<Publicacion> ObtenerPublicaciones(int desde = 0, int cantidad = 10, int usuarioActualId = 0)
    {
        string query = @"SELECT p.Id, p.IdUsuario, p.Titulo, p.Descripcion, p.Imagen, p.FechaPublicacion, u.NombreUsuario
                         FROM dbo.Publicaciones p
                         INNER JOIN dbo.Usuarios u ON u.Id = p.IdUsuario
                         ORDER BY p.FechaPublicacion DESC
                         OFFSET @Desde ROWS FETCH NEXT @Cantidad ROWS ONLY";

        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            var publicaciones = connection.Query<Publicacion>(query, new { Desde = desde, Cantidad = cantidad }).ToList();

            foreach (var publicacion in publicaciones)
            {
                publicacion.Comentarios = ObtenerComentarios(publicacion.Id);
                publicacion.CantidadLikes = ObtenerCantidadLikes(publicacion.Id);
                publicacion.MeGustaUsuario = TieneLike(publicacion.Id, usuarioActualId);
            }

            return publicaciones;
        }
    }

    public void GuardarPublicacion(Publicacion publicacion)
    {
        string query = @"INSERT INTO dbo.Publicaciones (IdUsuario, Titulo, Descripcion, Imagen, FechaPublicacion) VALUES (@IdUsuario, @Titulo, @Descripcion, @Imagen, @FechaPublicacion)";
        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            connection.Execute(query, new { publicacion.IdUsuario, publicacion.Titulo, publicacion.Descripcion, publicacion.Imagen, publicacion.FechaPublicacion });
        }
    }

    public bool PublicacionExiste(int idPublicacion)
    {
        string query = "SELECT COUNT(*) FROM dbo.Publicaciones WHERE Id = @IdPublicacion";
        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            return connection.ExecuteScalar<int>(query, new { IdPublicacion = idPublicacion }) > 0;
        }
    }

    public bool TieneLike(int idPublicacion, int idUsuario)
    {
        string query = @"SELECT COUNT(*) FROM dbo.PublicacionesMeGusta WHERE IdPublicacion = @IdPublicacion AND IdUsuario = @IdUsuario";
        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            return connection.ExecuteScalar<int>(query, new { IdPublicacion = idPublicacion, IdUsuario = idUsuario }) > 0;
        }
    }

    public int ObtenerCantidadLikes(int idPublicacion)
    {
        string query = @"SELECT COUNT(*) FROM dbo.PublicacionesMeGusta WHERE IdPublicacion = @IdPublicacion";
        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            return connection.ExecuteScalar<int>(query, new { IdPublicacion = idPublicacion });
        }
    }

    public void GuardarLike(int idPublicacion, int idUsuario)
    {
        string query = @"INSERT INTO dbo.PublicacionesMeGusta (IdPublicacion, IdUsuario) VALUES (@IdPublicacion, @IdUsuario)";
        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            connection.Execute(query, new { IdPublicacion = idPublicacion, IdUsuario = idUsuario });
        }
    }

    public void QuitarLike(int idPublicacion, int idUsuario)
    {
        string query = @"DELETE FROM dbo.PublicacionesMeGusta WHERE IdPublicacion = @IdPublicacion AND IdUsuario = @IdUsuario";
        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            connection.Execute(query, new { IdPublicacion = idPublicacion, IdUsuario = idUsuario });
        }
    }

    public List<Comentario> ObtenerComentarios(int idPublicacion)
    {
        string query = @"SELECT c.Id, c.IdPublicacion, c.IdUsuarioComenta, c.Texto, c.FechaComentario, u.NombreUsuario
                         FROM dbo.Comentarios c
                         INNER JOIN dbo.Usuarios u ON u.Id = c.IdUsuarioComenta
                         WHERE c.IdPublicacion = @IdPublicacion
                         ORDER BY c.FechaComentario ASC";
        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            return connection.Query<Comentario>(query, new { IdPublicacion = idPublicacion }).ToList();
        }
    }

    public Comentario GuardarComentario(int idPublicacion, int idUsuarioComenta, string texto)
    {
        string query = @"INSERT INTO dbo.Comentarios (IdPublicacion, IdUsuarioComenta, Texto, FechaComentario) OUTPUT INSERTED.* VALUES (@IdPublicacion, @IdUsuarioComenta, @Texto, @FechaComentario)";
        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            var comentario = connection.QuerySingle<Comentario>(query, new { IdPublicacion = idPublicacion, IdUsuarioComenta = idUsuarioComenta, Texto = texto, FechaComentario = DateTime.Now });
            comentario.NombreUsuario = ObtenerUsuarioPorId(idUsuarioComenta)?.NombreUsuario ?? string.Empty;
            return comentario;
        }
    }

    public Usuario? ObtenerUsuarioPorId(int idUsuario)
    {
        string query = "SELECT * FROM dbo.Usuarios WHERE Id = @IdUsuario";
        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            return connection.QuerySingleOrDefault<Usuario>(query, new { IdUsuario = idUsuario });
        }
    }
}