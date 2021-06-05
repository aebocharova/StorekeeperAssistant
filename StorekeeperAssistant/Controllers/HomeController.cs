using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Npgsql;
using StorekeeperAssistant.Controllers.Db;
using StorekeeperAssistant.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace StorekeeperAssistant.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private HomeModel p_home_model;
        Repository p_rep;

        private void refresh() 
        {
            bool ok = p_rep.refresh();

            if (!ok)
            {
                ViewBag.Message = "Проблема обновления модели";
            }

            p_home_model.movements = p_rep.getMovements();
            p_home_model.warehouses = p_rep.getWarehouses();

            if (p_home_model.movements.Count == 0)
            {
                ViewBag.Message = "Пустая модель";
            }
        }

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
            DbInit db_init = new DbInit();
            bool ok = db_init.connect();
            if (!ok)
            {
                db_init.init();
                ok = db_init.connect();
                if (!ok)
                {
                    ViewBag.Message = "Проблема подключения к БД";
                }
            }

            NpgsqlConnection con = db_init.getConnection();
            if (con == null)
            {
                ViewBag.Message = "Проблема получения подключения";
            }

            p_rep = new Repository(con);
            p_home_model = new HomeModel();
            refresh();
            p_home_model.movements = p_rep.getMovements();
            p_home_model.warehouses = p_rep.getWarehouses();
            db_init.disconnect();
        }

        public IActionResult Index()
        {
            return View("Index", p_home_model);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public IActionResult getMovementContent(int id)
        {
            List<StockBalance> stock_balance_list = new List<StockBalance>();
            List<MovementContent> movement_content_list = p_rep.getMovementContent(id);

            return View("MovementContent", movement_content_list); ;
        }

        [HttpPost]
        public IActionResult AddMovement(Movement movement)
        {
            Warehouse wh;
            bool ok = p_home_model.warehouses.TryGetValue(movement.from_warehouse.id, out wh);
            
            if (!ok)
            {
                ViewBag.Message = "Проблема добавления перемещения";
                return View("Index", p_home_model);
            }
            movement.from_warehouse.name = wh.name;

            p_home_model.warehouses.TryGetValue(movement.to_warehouse.id, out wh);
            movement.to_warehouse.name = wh.name;

            Int64 count = p_rep.getMovementCount();
            ok = p_rep.AddMovement(movement);
            count = p_rep.getMovementCount();

            if (!ok)
            {
                ViewBag.Message = "Проблема добавления перемещения";
                return View("Index", p_home_model);
            }

            p_home_model.movements = p_rep.getMovements();
            return View("Index", p_home_model);
        }

        [HttpPost]
        public IActionResult AddWarehouse(string name)
        {
            bool ok = p_rep.AddWarehouse(name);
            if (!ok)
            {
                ViewBag.Message = "Не удалось добавить новый склад " + name;
                return View("Index", p_home_model);
            }

            p_home_model.warehouses = p_rep.getWarehouses();
            return View("Index", p_home_model);
        }

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

            bool ok = p_rep.RemoveMovement(movement);
            p_home_model.movements.Remove(movement);
            return View("Index", p_home_model);
        }

        public IActionResult EditMovement(int id)
        {
            Movement movement = p_rep.getMovementById(id);
            if (movement != null)
                return View("EditMovement", movement);
            else
            {
                ViewBag.Message = "Ошибка операции редактирования";
                return View("Index", p_home_model);
            }
        }

        [HttpPost]
        public IActionResult EditMovement(Movement movement)
        {
            bool ok = p_rep.UpdateMovement(movement);
            if (!ok)
            {
                ViewBag.Message = "Проблема добавления перемещения";
            }
            p_home_model.movements = p_rep.getMovements();
            return View("Index", p_home_model);
        }
    }
}
