using Microsoft.AspNetCore.Mvc;
using Npgsql;
using StorekeeperAssistant.Controllers.Db;
using StorekeeperAssistant.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StorekeeperAssistant.Controllers
{
    public class StockBalanceController : Controller
    {
        private List<StockBalance> p_stock_balance;
        Repository p_rep;

        public IActionResult Index()
        {
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
            p_stock_balance = p_rep.getAllWarehousesBalance();

            return View("StockBalance", p_stock_balance);
        }

        
    }
}
