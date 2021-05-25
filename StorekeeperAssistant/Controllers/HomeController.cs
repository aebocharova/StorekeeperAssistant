using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Npgsql;
using StorekeeperAssistant.Controllers.Db;
using StorekeeperAssistant.Models;
using System.Diagnostics;

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
            p_rep.refresh();
            p_home_model = new HomeModel();
            p_home_model.movements = p_rep.getMovements();
            p_home_model.warehouses = p_rep.getWarehouses();
        }

        public IActionResult Index()
        {
            bool ok = p_rep.refresh();

            if (!ok)
            {
                ViewBag.Message = "Проблема обновления модели";
                return View("index");
            }

            if (p_home_model.movements.Count == 0)
            {
                ViewBag.Message = "Пустая модель";
                return View("index");
            }

            return View("index", p_home_model);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpPost]
        public IActionResult AddMovement(Movement movement)
        {
            Warehouse wh;
            bool ok = p_home_model.warehouses.TryGetValue(movement.from_warehouse.id, out wh);
            movement.from_warehouse.name = wh.name;
            if (!ok)
            {
                ViewBag.Message = "Проблема добавления перемещения";
                return View("index", p_home_model);
            }

            p_home_model.warehouses.TryGetValue(movement.to_warehouse.id, out wh);
            movement.to_warehouse.name = wh.name;

            ok = p_rep.AddMovement(movement);
            if (!ok)
            {
                ViewBag.Message = "Проблема добавления перемещения";
                return View("index", p_home_model);
            }

            p_home_model.movements = p_rep.getMovements();
            return View("index", p_home_model);
        }

        [HttpPost]
        public IActionResult DeleteMovement(Movement movement)
        {
            int id = movement.id;
            foreach (Movement m in p_home_model.movements)
            {
                if (m.id == id)
                {
                    movement = m;
                    break;
                }
            }

            p_home_model.movements.Remove(movement);
            return View("index", p_home_model);
        }

        public IActionResult EditMovement()
        {
            return View("EditMovement", p_home_model.movements[0]);
        }

        [HttpPost]
        public IActionResult EditMovement(Movement movement)
        {
            bool ok = p_rep.AddMovement(movement);
            if (!ok)
            {
                ViewBag.Message = "Проблема добавления перемещения";
            }
            p_home_model.movements = p_rep.getMovements();
            return View("index", p_home_model);
        }
    }
}
