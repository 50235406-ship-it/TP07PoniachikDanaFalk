using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using TP07PoniachikDanaFalk.Models;
using Microsoft.AspNetCore.Http;

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
        if (HttpContext.Session.GetString("NombreUsuario") != null) {
            return RedirectToAction("Bienvenida");
        }
        return View();
    }

    public IActionResult Registrarse() {
        if (HttpContext.Session.GetString("NombreUsuario") != null) {
            return RedirectToAction("Bienvenida");
        }
        return View();
    }

    public IActionResult IniciarSesion() {
        if (HttpContext.Session.GetString("NombreUsuario") != null) {
            return RedirectToAction("Bienvenida");
        }
        return View();
    }

    public IActionResult Bienvenida() {
        if (HttpContext.Session.GetString("NombreUsuario") == null) {
            return RedirectToAction("IniciarSesion");
        }
        return View();
    }

    public IActionResult Registrado(string nombreUsuario, string contraseña, string nombre, string apellido, string tipoUsuario) {

        BD bd = new BD();
        if (bd.UsuarioExiste(nombreUsuario)) {

            ViewBag.mensaje="El usuario ya existe";
            return View("Registrarse");

        }

        Usuario nuevoUsuario = new Usuario(nombreUsuario, contraseña, nombre, apellido, tipoUsuario);
        bd.GuardarUsuario(nuevoUsuario);

        List<Usuario> usuarios = bd.ObtenerUsuarios();
        Usuario usuarioRegistrado = usuarios.FirstOrDefault(u => u.NombreUsuario == nombreUsuario);

        HttpContext.Session.SetString("NombreUsuario", usuarioRegistrado.NombreUsuario);
        HttpContext.Session.SetString("Nombre", usuarioRegistrado.Nombre);
        HttpContext.Session.SetString("Apellido", usuarioRegistrado.Apellido);
        HttpContext.Session.SetString("TipoUsuario", usuarioRegistrado.TipoUsuario);
        HttpContext.Session.SetInt32("ID", usuarioRegistrado.ID);

        return View("Bienvenida");

    }

    public IActionResult Logueado(string nombreUsuario, string contraseña) {

        BD bd = new BD();
        List<Usuario> usuarios = bd.ObtenerUsuarios();
        ViewBag.mensaje="";

        if (Usuario.Logueo(nombreUsuario, contraseña, usuarios)) {

            Usuario usuarioLogueado = usuarios.FirstOrDefault(u => u.NombreUsuario == nombreUsuario);
            HttpContext.Session.SetString("NombreUsuario", usuarioLogueado.NombreUsuario);
            HttpContext.Session.SetString("Nombre", usuarioLogueado.Nombre);
            HttpContext.Session.SetString("Apellido", usuarioLogueado.Apellido);
            HttpContext.Session.SetString("TipoUsuario", usuarioLogueado.TipoUsuario);
            HttpContext.Session.SetInt32("ID", usuarioLogueado.ID);

            return View("Bienvenida");

        } else {

            ViewBag.mensaje="El usuario o la contraseña son incorrectos";
            return View("IniciarSesion");

        }

    }

    public IActionResult Logout() {
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
