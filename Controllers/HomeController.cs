using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using TP07PoniachikDanaFalk.Models;

namespace TP07PoniachikDanaFalk.Controllers;

public class HomeController : Controller
{
	private readonly ILogger<HomeController> _logger;

	public HomeController(ILogger<HomeController> logger)
	{
		_logger = logger;
	}

	private bool HaySesionActiva()
	{
		return !string.IsNullOrWhiteSpace(HttpContext.Session.GetString("NombreUsuario"));
	}

	private void GuardarMensajeSesion(string mensaje)
	{
		HttpContext.Session.SetString("Mensaje", mensaje ?? string.Empty);
	}

	private string LeerYBorrarMensajeSesion()
	{
		var m = HttpContext.Session.GetString("Mensaje") ?? string.Empty;
		HttpContext.Session.Remove("Mensaje");
		return m;
	}

	private List<Publicacion> CargarPublicaciones(int desde, int cantidad)
	{
		var bd = new BD();
		var usuarioActualId = HttpContext.Session.GetInt32("ID") ?? 0;
		return bd.ObtenerPublicaciones(desde, cantidad, usuarioActualId);
	}

	public IActionResult Index()
	{
		if (HaySesionActiva()) return RedirectToAction("Publicaciones");
		return View();
	}

	public IActionResult Registrarse()
	{
		if (HaySesionActiva()) return RedirectToAction("Publicaciones");
		return View();
	}

	public IActionResult IniciarSesion()
	{
		if (HaySesionActiva()) return RedirectToAction("Publicaciones");
		return View();
	}

	public IActionResult Bienvenida()
	{
		if (!HaySesionActiva()) return RedirectToAction("IniciarSesion");
		return RedirectToAction("Publicaciones");
	}

	public IActionResult Publicaciones(int desde = 0, int cantidad = 10)
	{
		if (!HaySesionActiva()) return RedirectToAction("IniciarSesion");

		var publicaciones = CargarPublicaciones(desde, cantidad);
		var bd = new BD();

		ViewBag.Publicaciones = publicaciones;
		ViewData["Publicaciones"] = publicaciones;
		ViewBag.TotalPublicaciones = bd.ContarPublicaciones();
		ViewBag.Mensaje = LeerYBorrarMensajeSesion();
		ViewBag.UsuarioActual = HttpContext.Session.GetString("NombreUsuario") ?? string.Empty;

		return View("Publicaciones", publicaciones);
	}

	[HttpPost]
	public IActionResult GuardarPublicacion(string titulo, string descripcion, string img)
	{
		if (!HaySesionActiva()) return RedirectToAction("IniciarSesion");

		if (string.IsNullOrWhiteSpace(titulo) || string.IsNullOrWhiteSpace(descripcion))
		{
			GuardarMensajeSesion("Completá el título y la descripción.");
			return RedirectToAction("Publicaciones");
		}

		var publicacion = new Publicacion(
			HttpContext.Session.GetInt32("ID") ?? 0,
			titulo.Trim(),
			descripcion.Trim(),
			img?.Trim() ?? string.Empty,
			DateTime.Now,
			HttpContext.Session.GetString("NombreUsuario") ?? string.Empty
		);

		var bd = new BD();
		bd.GuardarPublicacion(publicacion);

		GuardarMensajeSesion("Publicación creada con éxito.");
		return RedirectToAction("Publicaciones");
	}

	[HttpPost]
	public IActionResult ToggleLike(int idPublicacion)
	{
		if (!HaySesionActiva()) return RedirectToAction("IniciarSesion");

		if (idPublicacion <= 0)
		{
			GuardarMensajeSesion("Identificador de publicación inválido.");
			return RedirectToAction("Publicaciones");
		}

		var bd = new BD();
		var usuarioId = HttpContext.Session.GetInt32("ID") ?? 0;

		if (!bd.PublicacionExiste(idPublicacion))
		{
			GuardarMensajeSesion("La publicación no existe.");
			return RedirectToAction("Publicaciones");
		}

		if (bd.TieneLike(idPublicacion, usuarioId))
		{
			bd.QuitarLike(idPublicacion, usuarioId);
			GuardarMensajeSesion("Quitaste tu Me Gusta.");
		}
		else
		{
			bd.GuardarLike(idPublicacion, usuarioId);
			GuardarMensajeSesion("Agregaste un Me Gusta.");
		}

		return RedirectToAction("Publicaciones");
	}

	[HttpPost]
	public IActionResult AgregarComentario(int idPublicacion, string texto)
	{
		if (!HaySesionActiva()) return RedirectToAction("IniciarSesion");

		if (idPublicacion <= 0)
		{
			GuardarMensajeSesion("Identificador de publicación inválido.");
			return RedirectToAction("Publicaciones");
		}

		if (string.IsNullOrWhiteSpace(texto))
		{
			GuardarMensajeSesion("El comentario no puede estar vacío.");
			return RedirectToAction("Publicaciones");
		}

		var bd = new BD();
		var usuarioId = HttpContext.Session.GetInt32("ID") ?? 0;

		if (!bd.PublicacionExiste(idPublicacion))
		{
			GuardarMensajeSesion("La publicación no existe.");
			return RedirectToAction("Publicaciones");
		}

		bd.GuardarComentario(idPublicacion, usuarioId, texto.Trim());
		GuardarMensajeSesion("Comentario agregado correctamente.");

		return RedirectToAction("Publicaciones");
	}

	public IActionResult Registrado(string nombreUsuario, string contraseña, string nombre, string apellido)
	{
		var bd = new BD();

		if (string.IsNullOrWhiteSpace(nombreUsuario) || string.IsNullOrWhiteSpace(contraseña) || string.IsNullOrWhiteSpace(nombre) || string.IsNullOrWhiteSpace(apellido))
		{
			ViewBag.mensaje = "Completá todos los campos.";
			return View("Registrarse");
		}

		nombreUsuario = nombreUsuario.Trim();
		nombre = nombre.Trim();
		apellido = apellido.Trim();

		if (bd.UsuarioExiste(nombreUsuario))
		{
			ViewBag.mensaje = "Ese nombre de usuario ya está en uso.";
			return View("Registrarse");
		}

		var nuevoUsuario = new Usuario(nombreUsuario, contraseña, nombre, apellido);
		bd.GuardarUsuario(nuevoUsuario);

		var usuarioRegistrado = bd.ObtenerUsuarios().FirstOrDefault(u => u.NombreUsuario == nombreUsuario);

		if (usuarioRegistrado == null)
		{
			ViewBag.mensaje = "No pudimos registrar el usuario.";
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
		var bd = new BD();
		var usuarios = bd.ObtenerUsuarios();
		ViewBag.mensaje = string.Empty;

		nombreUsuario = nombreUsuario?.Trim() ?? string.Empty;

		if (Usuario.Logueo(nombreUsuario, contraseña, usuarios))
		{
			var usuarioLogueado = usuarios.FirstOrDefault(u => u.NombreUsuario == nombreUsuario);

			if (usuarioLogueado == null)
			{
				ViewBag.mensaje = "No encontramos tus datos. Probá de nuevo.";
				return View("IniciarSesion");
			}

			HttpContext.Session.SetString("NombreUsuario", usuarioLogueado.NombreUsuario);
			HttpContext.Session.SetString("Nombre", usuarioLogueado.Nombre);
			HttpContext.Session.SetString("Apellido", usuarioLogueado.Apellido);
			HttpContext.Session.SetInt32("ID", usuarioLogueado.Id);
			HttpContext.Session.SetString("TipoUsuario", "Usuario");

			return RedirectToAction("Publicaciones");
		}

		ViewBag.mensaje = "Usuario o contraseña incorrectos.";
		return View("IniciarSesion");
	}

	public IActionResult Logout()
	{
		HttpContext.Session.Clear();
		return RedirectToAction("Index");
	}

	public IActionResult Privacy() => View();

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
