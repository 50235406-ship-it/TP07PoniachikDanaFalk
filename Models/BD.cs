using Dapper;
using Microsoft.Data.SqlClient;

namespace TP07PoniachikDanaFalk.Models;

public class BD
{
    private readonly string[] _connectionNames = ["DBRedSocial", "BDTP07"];

    private static string BuildConnectionString(string databaseName)
    {
        var builder = new SqlConnectionStringBuilder
        {
            DataSource = "localhost",
            InitialCatalog = databaseName,
            IntegratedSecurity = true,
            TrustServerCertificate = true
        };

        return builder.ConnectionString;
    }

    private static string BuildMasterConnectionString()
    {
        var builder = new SqlConnectionStringBuilder
        {
            DataSource = "localhost",
            InitialCatalog = "master",
            IntegratedSecurity = true,
            TrustServerCertificate = true
        };

        return builder.ConnectionString;
    }

    private bool DatabaseHasRequiredTables(string databaseName)
    {
        try
        {
            using var connection = new SqlConnection(BuildConnectionString(databaseName));
            connection.Open();

            var hasUsuarios = connection.ExecuteScalar<int>("SELECT CASE WHEN OBJECT_ID('dbo.Usuarios', 'U') IS NOT NULL THEN 1 ELSE 0 END") == 1;
            var hasPublicaciones = connection.ExecuteScalar<int>("SELECT CASE WHEN OBJECT_ID('dbo.Publicaciones', 'U') IS NOT NULL THEN 1 ELSE 0 END") == 1;
            var hasMeGusta = connection.ExecuteScalar<int>("SELECT CASE WHEN OBJECT_ID('dbo.PublicacionesMeGusta', 'U') IS NOT NULL THEN 1 ELSE 0 END") == 1;
            var hasComentarios = connection.ExecuteScalar<int>("SELECT CASE WHEN OBJECT_ID('dbo.Comentarios', 'U') IS NOT NULL THEN 1 ELSE 0 END") == 1;

            return hasUsuarios && hasPublicaciones && hasMeGusta && hasComentarios;
        }
        catch
        {
            return false;
        }
    }

    private void EnsureDatabaseAndSchema()
    {
        try
        {
            using var masterConnection = new SqlConnection(BuildMasterConnectionString());
            masterConnection.Open();

            masterConnection.Execute("IF DB_ID(N'DBRedSocial') IS NULL CREATE DATABASE [DBRedSocial]");
        }
        catch
        {
            // Si no hay SQL Server disponible, se intenta con la base de datos configurada por el entorno.
        }

        try
        {
            using var connection = new SqlConnection(BuildConnectionString("DBRedSocial"));
            connection.Open();

            connection.Execute(@"IF OBJECT_ID('dbo.Usuarios', 'U') IS NULL
                BEGIN
                    CREATE TABLE dbo.Usuarios (
                        Id INT IDENTITY(1,1) PRIMARY KEY,
                        NombreUsuario VARCHAR(50) NOT NULL,
                        Contraseña VARCHAR(50) NOT NULL,
                        Nombre VARCHAR(50) NOT NULL,
                        Apellido VARCHAR(50) NOT NULL
                    );
                END");

            connection.Execute(@"IF OBJECT_ID('dbo.Publicaciones', 'U') IS NULL
                BEGIN
                    CREATE TABLE dbo.Publicaciones (
                        Id INT IDENTITY(1,1) PRIMARY KEY,
                        IdUsuario INT NOT NULL,
                        Titulo VARCHAR(200) NOT NULL,
                        Descripcion TEXT NOT NULL,
                        Imagen VARCHAR(255) NULL,
                        FechaPublicacion DATETIME NOT NULL,
                        CONSTRAINT FK_Publicaciones_Usuarios FOREIGN KEY (IdUsuario) REFERENCES dbo.Usuarios(Id)
                    );
                END");

            connection.Execute(@"IF OBJECT_ID('dbo.PublicacionesMeGusta', 'U') IS NULL
                BEGIN
                    CREATE TABLE dbo.PublicacionesMeGusta (
                        Id INT IDENTITY(1,1) PRIMARY KEY,
                        IdPublicacion INT NOT NULL,
                        IdUsuario INT NOT NULL,
                        CONSTRAINT FK_PublicacionesMeGusta_Publicaciones FOREIGN KEY (IdPublicacion) REFERENCES dbo.Publicaciones(Id),
                        CONSTRAINT FK_PublicacionesMeGusta_Usuarios FOREIGN KEY (IdUsuario) REFERENCES dbo.Usuarios(Id)
                    );
                END");

            connection.Execute(@"IF OBJECT_ID('dbo.Comentarios', 'U') IS NULL
                BEGIN
                    CREATE TABLE dbo.Comentarios (
                        Id INT IDENTITY(1,1) PRIMARY KEY,
                        IdPublicacion INT NOT NULL,
                        IdUsuarioComenta INT NOT NULL,
                        Texto TEXT NOT NULL,
                        FechaComentario DATETIME NOT NULL,
                        CONSTRAINT FK_Comentarios_Publicaciones FOREIGN KEY (IdPublicacion) REFERENCES dbo.Publicaciones(Id),
                        CONSTRAINT FK_Comentarios_Usuarios FOREIGN KEY (IdUsuarioComenta) REFERENCES dbo.Usuarios(Id)
                    );
                END");
        }
        catch
        {
            // Si no se puede crear la base de datos, el error real se verá al intentar usar la aplicación.
        }
    }

    private string GetConnectionString()
    {
        foreach (var databaseName in _connectionNames)
        {
            if (DatabaseHasRequiredTables(databaseName))
            {
                return BuildConnectionString(databaseName);
            }
        }

        EnsureDatabaseAndSchema();
        return BuildConnectionString("DBRedSocial");
    }

    public List<Usuario> ObtenerUsuarios()
    {
        using var connection = new SqlConnection(GetConnectionString());
        return connection.Query<Usuario>("SELECT * FROM dbo.Usuarios ORDER BY Id").ToList();
    }

    public bool UsuarioExiste(string nombreUsuario)
    {
        return ObtenerUsuarios().Any(u => u.NombreUsuario == nombreUsuario);
    }

    public void GuardarUsuario(Usuario usuario)
    {
        using var connection = new SqlConnection(GetConnectionString());
        connection.Open();

        const string query = @"INSERT INTO dbo.Usuarios (NombreUsuario, Contraseña, Nombre, Apellido)
                               VALUES (@NombreUsuario, @Contraseña, @Nombre, @Apellido)";

        connection.Execute(query, new
        {
            usuario.NombreUsuario,
            usuario.Contraseña,
            usuario.Nombre,
            usuario.Apellido
        });
    }

    public int ContarPublicaciones()
    {
        using var connection = new SqlConnection(GetConnectionString());
        return connection.ExecuteScalar<int>("SELECT COUNT(*) FROM dbo.Publicaciones");
    }

    public List<Publicacion> ObtenerPublicaciones(int desde = 0, int cantidad = 10, int usuarioActualId = 0)
    {
        using var connection = new SqlConnection(GetConnectionString());

        var query = @"
            SELECT p.Id,
                   p.IdUsuario,
                   p.Titulo,
                   p.Descripcion,
                   p.Imagen,
                   p.FechaPublicacion,
                   u.NombreUsuario,
                   COUNT(DISTINCT mg.Id) AS CantidadLikes,
                   CASE WHEN EXISTS (
                        SELECT 1
                        FROM dbo.PublicacionesMeGusta mg2
                        WHERE mg2.IdPublicacion = p.Id
                          AND mg2.IdUsuario = @UsuarioActualId
                   ) THEN 1 ELSE 0 END AS MeGustaUsuario
            FROM dbo.Publicaciones p
            INNER JOIN dbo.Usuarios u ON u.Id = p.IdUsuario
            LEFT JOIN dbo.PublicacionesMeGusta mg ON mg.IdPublicacion = p.Id
            GROUP BY p.Id, p.IdUsuario, p.Titulo, p.Descripcion, p.Imagen, p.FechaPublicacion, u.NombreUsuario
            ORDER BY p.FechaPublicacion DESC
            OFFSET @Desde ROWS FETCH NEXT @Cantidad ROWS ONLY;";

        var publicaciones = connection.Query<Publicacion>(query, new
        {
            UsuarioActualId = usuarioActualId,
            Desde = desde,
            Cantidad = cantidad
        }).ToList();

        foreach (var publicacion in publicaciones)
        {
            publicacion.Comentarios = ObtenerComentarios(publicacion.Id);
            publicacion.CantidadLikes = ObtenerCantidadLikes(publicacion.Id);
            publicacion.MeGustaUsuario = TieneLike(publicacion.Id, usuarioActualId);
        }

        return publicaciones;
    }

    public void GuardarPublicacion(Publicacion publicacion)
    {
        using var connection = new SqlConnection(GetConnectionString());
        connection.Open();

        const string query = @"INSERT INTO dbo.Publicaciones (IdUsuario, Titulo, Descripcion, Imagen, FechaPublicacion)
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

    public bool PublicacionExiste(int idPublicacion)
    {
        using var connection = new SqlConnection(GetConnectionString());
        return connection.ExecuteScalar<int>("SELECT COUNT(*) FROM dbo.Publicaciones WHERE Id = @IdPublicacion", new { IdPublicacion = idPublicacion }) > 0;
    }

    public bool TieneLike(int idPublicacion, int idUsuario)
    {
        using var connection = new SqlConnection(GetConnectionString());
        return connection.ExecuteScalar<int>(@"SELECT COUNT(*) FROM dbo.PublicacionesMeGusta WHERE IdPublicacion = @IdPublicacion AND IdUsuario = @IdUsuario",
            new { IdPublicacion = idPublicacion, IdUsuario = idUsuario }) > 0;
    }

    public int ObtenerCantidadLikes(int idPublicacion)
    {
        using var connection = new SqlConnection(GetConnectionString());
        return connection.ExecuteScalar<int>(@"SELECT COUNT(*) FROM dbo.PublicacionesMeGusta WHERE IdPublicacion = @IdPublicacion",
            new { IdPublicacion = idPublicacion });
    }

    public void GuardarLike(int idPublicacion, int idUsuario)
    {
        using var connection = new SqlConnection(GetConnectionString());
        connection.Open();

        const string query = @"INSERT INTO dbo.PublicacionesMeGusta (IdPublicacion, IdUsuario)
                               VALUES (@IdPublicacion, @IdUsuario)";

        connection.Execute(query, new { IdPublicacion = idPublicacion, IdUsuario = idUsuario });
    }

    public void QuitarLike(int idPublicacion, int idUsuario)
    {
        using var connection = new SqlConnection(GetConnectionString());
        connection.Open();

        const string query = @"DELETE FROM dbo.PublicacionesMeGusta
                               WHERE IdPublicacion = @IdPublicacion AND IdUsuario = @IdUsuario";

        connection.Execute(query, new { IdPublicacion = idPublicacion, IdUsuario = idUsuario });
    }

    public List<Comentario> ObtenerComentarios(int idPublicacion)
    {
        using var connection = new SqlConnection(GetConnectionString());

        return connection.Query<Comentario>(@"
            SELECT c.Id,
                   c.IdPublicacion,
                   c.IdUsuarioComenta,
                   c.Texto,
                   c.FechaComentario,
                   u.NombreUsuario
            FROM dbo.Comentarios c
            INNER JOIN dbo.Usuarios u ON u.Id = c.IdUsuarioComenta
            WHERE c.IdPublicacion = @IdPublicacion
            ORDER BY c.FechaComentario ASC",
            new { IdPublicacion = idPublicacion }).ToList();
    }

    public Comentario GuardarComentario(int idPublicacion, int idUsuarioComenta, string texto)
    {
        using var connection = new SqlConnection(GetConnectionString());
        connection.Open();

        const string query = @"INSERT INTO dbo.Comentarios (IdPublicacion, IdUsuarioComenta, Texto, FechaComentario)
                               OUTPUT INSERTED.*
                               VALUES (@IdPublicacion, @IdUsuarioComenta, @Texto, @FechaComentario)";

        var comentario = connection.QuerySingle<Comentario>(query, new
        {
            IdPublicacion = idPublicacion,
            IdUsuarioComenta = idUsuarioComenta,
            Texto = texto,
            FechaComentario = DateTime.Now
        });

        comentario.NombreUsuario = ObtenerUsuarioPorId(idUsuarioComenta)?.NombreUsuario ?? string.Empty;
        return comentario;
    }

    public Usuario? ObtenerUsuarioPorId(int idUsuario)
    {
        using var connection = new SqlConnection(GetConnectionString());
        return connection.QuerySingleOrDefault<Usuario>("SELECT * FROM dbo.Usuarios WHERE Id = @IdUsuario", new { IdUsuario = idUsuario });
    }
}