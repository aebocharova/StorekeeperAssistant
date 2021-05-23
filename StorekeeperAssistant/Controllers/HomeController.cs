using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Npgsql;
using StorekeeperAssistant.Controllers.Db;
using StorekeeperAssistant.Models;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace StorekeeperAssistant.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private HomeModel p_home_model;
        Repository p_rep;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
            DbInit db_init = new DbInit();
            bool ok = db_init.connect();
            if (!ok)
            {
                ViewBag.Message = "Проблема подключения к БД";
            }

            db_init.init();

            NpgsqlConnection con = db_init.getConnection();
            if (con == null)
            {
                ViewBag.Message = "Проблема получения подключения";
            }

            p_rep = new Repository(con);
        }

        public IActionResult Index()
        {
            bool ok = p_rep.refresh();
            
            if (!ok)
            {
                ViewBag.Message = "Проблема обновления модели";
                return View("index");
            }
            p_home_model = p_rep.getHomeModel();

            if (p_home_model.movements.Count == 0)
            {
                ViewBag.Message = "Пустая модель";
                return View("index");
            }

            return View("index",p_home_model);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpPost]
        public IActionResult AddMovement(Movement movement)
        {
            bool ok = p_rep.AddMovement(movement);
            if (!ok)
            {
                ViewBag.Message = "Проблема добавления перемещения";
            }
            p_home_model = p_rep.getHomeModel();
            return View("index", p_home_model);
        }

        [HttpPost]
        public IActionResult DeleteMovement(Movement movement)
        {
            p_home_model.movements.Remove(movement);
            return View(p_home_model);
        }

        public IActionResult EditMovement()
        {
            return View("EditMovement",p_home_model.movements[0]);
        }

        [HttpPost]
        public IActionResult EditMovement(Movement movement)
        {
            bool ok = p_rep.AddMovement(movement);
            if (!ok)
            {
                ViewBag.Message = "Проблема добавления перемещения";
            }
            p_home_model = p_rep.getHomeModel();
            return View("index", p_home_model);
        }
    }
}
