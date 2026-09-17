using System.Diagnostics;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using TP6_Gorojod_Schwartz_Waserman.Models;

namespace TP6_Gorojod_Schwartz_Waserman.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }
    [HttpPost]
    public IActionResult iniciarPartida(string username)
    {
        Partida partida = new Partida();
        BD bd = new BD();
        if (!string.IsNullOrWhiteSpace(username))
        {
            
            partida = bd.ObtenerPartidaPorNombre(username.Trim());
            if (partida != null)
            {
                HttpContext.Session.SetString("PartidaId", partida.Id.ToString());
                return RedirectToAction("Sala", new { IdSala = partida.IdSala });
            }
        }

        Partida nPartida = new Partida()
        { fechaInicio = DateTime.Today, horaInicio = DateTime.Now, IdSala = 1, estadoActual = "curso", nombreJugador = username.Trim(), Vidas = 3 };
        nPartida.Id = bd.crearPartida(nPartida);
        HttpContext.Session.SetString("PartidaId", nPartida.Id.ToString());
        return RedirectToAction("Sala", new { IdSala = 1 });
    }

    public IActionResult Historia()
    {   
        return View();
    }
    public IActionResult Index()
    {
        return View();
    }
    public IActionResult Integrantes()
    {
        return View();
    }
    public IActionResult pasarSala (int IdSala) // es un método que sirve para pasar de una sala a otra cuando se completa un desafío. Se llama desde la vista de la sala actual y redirige a la vista de la siguiente sala.
    {
        BD bd = new BD();
        string partidaIdSession = HttpContext.Session.GetString("PartidaId");

        if (string.IsNullOrEmpty(partidaIdSession))
            return RedirectToAction("Historia");

        int partidaId = int.Parse(partidaIdSession);
        int nIdSala = IdSala + 1;
        bd.pasarSala(partidaId, nIdSala);
        return RedirectToAction("Sala", new { IdSala = nIdSala });
    }

    /*public IActionResult irAForja()
    {
        string partidaIdSession = HttpContext.Session.GetString("PartidaId");
        if (string.IsNullOrEmpty(partidaIdSession))
            return RedirectToAction("Historia");

        int partidaId = int.Parse(partidaIdSession);
        BD bd = new BD();
        // Actualizar la partida a Sala 3
        bd.pasarSala(partidaId, 3);
        
        HttpContext.Session.SetString("EnForja", "true");
        return RedirectToAction("Sala", new { IdSala = 3 });
    }*/    //fue una respuesta para debuggear mas rapido

    public IActionResult Sala(int IdSala)
    {
        BD bd = new BD();
        string partidaIdSession = HttpContext.Session.GetString("PartidaId");
        if (string.IsNullOrEmpty(partidaIdSession)) return RedirectToAction("Historia");

        int partidaId = int.Parse(partidaIdSession);
        bool puedeEntrar = bd.TieneAcceso(partidaId, IdSala);
        if (!puedeEntrar) return RedirectToAction("AccesoDenegado");

        Partida partida = bd.ObtenerPartidaPorId(partidaId);
        ViewBag.Vidas = partida?.Vidas ?? 0;

        string viewName;
        object model = null;
        
        if (IdSala == 3)
        {
            viewName = "Sala3";
        }
        else
        {
            viewName = IdSala == 2 ? "Sala2" : "Sala" + IdSala;
        }

        if (IdSala == 1)
        {
            SalaTriviaViewModel trivia = bd.ObtenerTriviaSala1();
            Random random = new Random();

            trivia.Preguntas = trivia.Preguntas
                .OrderBy(_ => random.Next())
                .Take(3)
                .Select(pregunta =>
                {
                    pregunta.Respuestas = pregunta.Respuestas
                        .OrderBy(_ => random.Next())
                        .Take(3)
                        .ToList();
                    return pregunta;
                })
                .ToList();

            model = trivia;
        }
        
        return model is null ? View(viewName) : View(viewName, model); //intentamos sacar el is null pero no entendimos bien como era asi que lo volvimos a poner porque no compilaba.   
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

    [HttpPost]
    public IActionResult ValidarSala1(int q1, int q2, int q3)
    {
        List<int> respuestas = new List<int> { q1, q2, q3 };
        BD bd = new BD();
        int aciertos = 0;

        foreach (int idRespuesta in respuestas)
        {
            if (idRespuesta <= 0) continue;

            Respuestas respuesta = bd.ObtenerRespuestaPorId(idRespuesta);
            if (respuesta != null && respuesta.EsCorrecta)
            {
                aciertos++;
            }
        }

        string partidaIdSession = HttpContext.Session.GetString("PartidaId");
        if (string.IsNullOrEmpty(partidaIdSession))
            return RedirectToAction("Historia");

        int partidaId = int.Parse(partidaIdSession);

        if (aciertos >= 2)
        {
            bd.pasarSala(partidaId, 2);

            ViewBag.sala1Mensaje = "Has ganado la confianza de Oro, Sheo y Mato. El Aguijón roto te ha sido entregado.";
            return RedirectToAction("Sala", new { IdSala = 2 });
        }

        int vidas = bd.PerderVida(partidaId);
        return vidas == 0
            ? RedirectToAction("Historia")
            : RedirectToAction("Sala", new { IdSala = 1 });
    }

    [HttpPost]
    public IActionResult PerderVida(int IdSala)
    {
        string partidaIdSession = HttpContext.Session.GetString("PartidaId");
        if (string.IsNullOrEmpty(partidaIdSession))
            return RedirectToAction("Historia");

        int partidaId = int.Parse(partidaIdSession);
        BD bd = new BD();
        int vidas = bd.PerderVida(partidaId);

        if (vidas == 0)
            return RedirectToAction("Historia");

        bd.pasarSala(partidaId, IdSala);
        return RedirectToAction("Sala", new { IdSala });
    }
}
