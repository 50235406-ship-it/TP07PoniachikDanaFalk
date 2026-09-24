namespace TP07PoniachikDanaFalk.Models;

public class Publicacion
{
    public int Id { get; set; }
    public int IdUsuario { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public string? Imagen { get; set; }
    public DateTime FechaPublicacion { get; set; }
    public string NombreUsuario { get; set; } = string.Empty;
}
