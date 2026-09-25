namespace TP07PoniachikDanaFalk.Models;

public class Comentarios
{
    public int Id { get; set; }
    public int IdPublicacion { get; set; }
    public int IdUsuarioComenta { get; set; }
    public string Texto { get; set; } = string.Empty;
    public DateTime FechaComentario { get; set; }
}
