namespace TP07PoniachikDanaFalk.Models;

public class Publicacion
{
    public int Id { get; set; }
    public int IdUsuario { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public string Imagen { get; set; } = string.Empty;
    public DateTime FechaPublicacion { get; set; }
    public string NombreUsuario { get; set; } = string.Empty;
    public int CantidadLikes { get; set; }
    public bool MeGustaUsuario { get; set; }
    public List<Comentario> Comentarios { get; set; } = new();

    public Publicacion()
    {
    }

    public Publicacion(int idUsuario, string titulo, string descripcion, string imagen, DateTime fechaPublicacion, string nombreUsuario)
    {
        IdUsuario = idUsuario;
        Titulo = titulo;
        Descripcion = descripcion;
        Imagen = imagen;
        FechaPublicacion = fechaPublicacion;
        NombreUsuario = nombreUsuario;
        CantidadLikes = 0;
        MeGustaUsuario = false;
    }

}
