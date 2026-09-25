using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using TP07PoniachikDanaFalk.Models;

namespace TP07PoniachikDanaFalk.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index()
    {
        if (HttpContext.Session.GetString("NombreUsuario") != null)
        {
            return RedirectToAction("Publicaciones");
        }

        return View();
    }

    public IActionResult Registrarse()
    {
        if (HttpContext.Session.GetString("NombreUsuario") != null)
        {
            return RedirectToAction("Publicaciones");
        }

        return View();
    }

    public IActionResult IniciarSesion()
    {
        if (HttpContext.Session.GetString("NombreUsuario") != null)
        {
            return RedirectToAction("Publicaciones");
        }

        return View();
    }

    public IActionResult Bienvenida()
    {
        if (HttpContext.Session.GetString("NombreUsuario") == null)
        {
            return RedirectToAction("IniciarSesion");
        }

        return RedirectToAction("Publicaciones");
    }

    public IActionResult Publicaciones()
    {
        if (HttpContext.Session.GetString("NombreUsuario") == null)
        {
            return RedirectToAction("IniciarSesion");
        }

        return View();
    }

    [HttpPost]
    public IActionResult GuardarPublicacion(string titulo, string descripcion, string? imagen)
    {
        if (HttpContext.Session.GetString("NombreUsuario") == null)
        {
            return RedirectToAction("IniciarSesion");
        }

        if (string.IsNullOrWhiteSpace(titulo) || string.IsNullOrWhiteSpace(descripcion))
        {
            TempData["Mensaje"] = "Completa el título y la descripción.";
            return RedirectToAction("Publicaciones");
        }

        var publicacion = new Publicacion
        {
            IdUsuario = HttpContext.Session.GetInt32("ID") ?? 0,
            Titulo = titulo.Trim(),
            Descripcion = descripcion.Trim(),
            Imagen = string.IsNullOrWhiteSpace(imagen) ? null : imagen.Trim(),
            FechaPublicacion = DateTime.Now
        };

        var bd = new BD();
        bd.GuardarPublicacion(publicacion);

        TempData["Mensaje"] = "Publicación creada correctamente.";
        return RedirectToAction("Publicaciones");
    }

    [HttpGet]
    public IActionResult ObtenerPublicaciones(int desde = 0, int cantidad = 10)
    {
        if (HttpContext.Session.GetString("NombreUsuario") == null)
        {
            return Unauthorized();
        }

        var bd = new BD();
        var usuarioActualId = HttpContext.Session.GetInt32("ID") ?? 0;
        var publicaciones = bd.ObtenerPublicaciones(desde, cantidad, usuarioActualId);
        var total = bd.ContarPublicaciones();

        return Json(new
        {
            publicaciones,
            hayMas = (desde + publicaciones.Count) < total
        });
    }

    [HttpPost]
    public IActionResult ToggleLike([FromBody] LikeRequest request)
    {
        if (HttpContext.Session.GetString("NombreUsuario") == null)
        {
            return Unauthorized();
        }

        var bd = new BD();
        var usuarioId = HttpContext.Session.GetInt32("ID") ?? 0;

        if (request == null || request.IdPublicacion <= 0)
        {
            return BadRequest(new { message = "Identificador de publicación inválido." });
        }

        if (!bd.PublicacionExiste(request.IdPublicacion))
        {
            return NotFound(new { message = "La publicación no existe." });
        }

        var yaLeDioLike = bd.TieneLike(request.IdPublicacion, usuarioId);

        if (yaLeDioLike)
        {
            bd.QuitarLike(request.IdPublicacion, usuarioId);
        }
        else
        {
            bd.GuardarLike(request.IdPublicacion, usuarioId);
        }

        var cantidadLikes = bd.ObtenerCantidadLikes(request.IdPublicacion);
        var liked = !yaLeDioLike;

        return Json(new
        {
            ok = true,
            liked,
            cantidadLikes,
            message = liked ? "Se agregó el Me Gusta." : "Se quitó el Me Gusta."
        });
    }

    [HttpPost]
    public IActionResult AgregarComentario([FromBody] ComentarioRequest request)
    {
        if (HttpContext.Session.GetString("NombreUsuario") == null)
        {
            return Unauthorized();
        }

        var bd = new BD();
        var usuarioId = HttpContext.Session.GetInt32("ID") ?? 0;

        if (request == null || request.IdPublicacion <= 0)
        {
            return BadRequest(new { message = "Identificador de publicación inválido." });
        }

        if (!bd.PublicacionExiste(request.IdPublicacion))
        {
            return NotFound(new { message = "La publicación no existe." });
        }

        if (string.IsNullOrWhiteSpace(request.Texto))
        {
            return BadRequest(new { message = "El comentario no puede estar vacío." });
        }

        var comentario = bd.GuardarComentario(request.IdPublicacion, usuarioId, request.Texto.Trim());

        return Json(new
        {
            ok = true,
            comentario = new
            {
                id = comentario.Id,
                idPublicacion = comentario.IdPublicacion,
                texto = comentario.Texto,
                fechaComentario = comentario.FechaComentario,
                nombreUsuario = comentario.NombreUsuario
            }
        });
    }

    public IActionResult Registrado(string nombreUsuario, string contraseña, string nombre, string apellido)
    {
        BD bd = new BD();

        if (string.IsNullOrWhiteSpace(nombreUsuario) || string.IsNullOrWhiteSpace(contraseña) || string.IsNullOrWhiteSpace(nombre) || string.IsNullOrWhiteSpace(apellido))
        {
            ViewBag.mensaje = "Complete todos los campos.";
            return View("Registrarse");
        }

        if (bd.UsuarioExiste(nombreUsuario))
        {
            ViewBag.mensaje = "El usuario ya existe";
            return View("Registrarse");
        }

        Usuario nuevoUsuario = new Usuario(nombreUsuario, contraseña, nombre, apellido);
        bd.GuardarUsuario(nuevoUsuario);

        Usuario usuarioRegistrado = bd.ObtenerUsuarios().FirstOrDefault(u => u.NombreUsuario == nombreUsuario);

        if (usuarioRegistrado == null)
        {
            ViewBag.mensaje = "No se pudo registrar el usuario.";
            return View("Registrarse");
        }

        HttpContext.Session.SetString("NombreUsuario", usuarioRegistrado.NombreUsuario);
        HttpContext.Session.SetString("Nombre", usuarioRegistrado.Nombre);
        HttpContext.Session.SetString("Apellido", usuarioRegistrado.Apellido);
        HttpContext.Session.SetInt32("ID", usuarioRegistrado.Id);
        HttpContext.Session.SetString("TipoUsuario", "Usuario");

        return RedirectToAction("Publicaciones");
    }

    public IActionResult Logueado(string nombreUsuario, string contraseña)
    {
        BD bd = new BD();
        List<Usuario> usuarios = bd.ObtenerUsuarios();
        ViewBag.mensaje = string.Empty;

        if (Usuario.Logueo(nombreUsuario, contraseña, usuarios))
        {
            Usuario usuarioLogueado = usuarios.FirstOrDefault(u => u.NombreUsuario == nombreUsuario);
            if (usuarioLogueado == null)
            {
                ViewBag.mensaje = "El usuario o la contraseña son incorrectos";
                return View("IniciarSesion");
            }

            HttpContext.Session.SetString("NombreUsuario", usuarioLogueado.NombreUsuario);
            HttpContext.Session.SetString("Nombre", usuarioLogueado.Nombre);
            HttpContext.Session.SetString("Apellido", usuarioLogueado.Apellido);
            HttpContext.Session.SetInt32("ID", usuarioLogueado.Id);
            HttpContext.Session.SetString("TipoUsuario", "Usuario");

            return RedirectToAction("Publicaciones");
        }

        ViewBag.mensaje = "El usuario o la contraseña son incorrectos";
        return View("IniciarSesion");
    }

    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction("Index");
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}

public class LikeRequest
{
    public int IdPublicacion { get; set; }
}

public class ComentarioRequest
{
    public int IdPublicacion { get; set; }
    public string Texto { get; set; } = string.Empty;
}
