using Npgsql;
using StorekeeperAssistant.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StorekeeperAssistant.Controllers.Db
{
    public class Repository
    {
        NpgsqlConnection p_connection;
        List<Movement> p_movements;
        Dictionary<int, Warehouse> p_warehouses;
        Dictionary<int, Nomenclature> p_nomenclature;
        HomeModel p_homeModel;

        public Repository(NpgsqlConnection connection)
        {
            p_movements = new List<Movement>();
            p_warehouses = new Dictionary<int, Warehouse>();
            p_nomenclature = new Dictionary<int, Nomenclature>();
            p_homeModel = new HomeModel();
            p_homeModel.movements = p_movements;
            p_homeModel.warehouses = p_warehouses.Values.ToList();
            p_connection = connection;
        }

        public HomeModel getHomeModel()
        {
            return p_homeModel;
        }

        public Dictionary<int,Warehouse> getWarehouses()
        {
            refresh();
            return p_warehouses;
        }

        public Dictionary<int, Nomenclature> getNomenclature()
        {
            refresh();
            return p_nomenclature;
        }
        public List<Movement> getMovements()
        {
            refresh();
            return p_movements;
        }

        public bool AddMovement(Movement movement )
        {
            int error_flag = 0;

            String sql = "INSERT INTO movement(date_time, from_warehouse_id, to_warehouse_id)" +
                "VALUES (@date_time, @from_warehouse_id, @to_warehouse_id)";

            NpgsqlCommand cmd = new NpgsqlCommand(sql, p_connection);
            using (cmd)
            {
                cmd.Parameters.AddWithValue("date_time", movement.date_time);
                cmd.Parameters.AddWithValue("from_warehouse_id", movement.from_warehouse.id);
                cmd.Parameters.AddWithValue("to_warehouse_id", movement.to_warehouse.id);
                error_flag = cmd.ExecuteNonQuery();                
            }

            if (error_flag == 0)
                return false;

            sql = "SELECT id FROM movement WHERE date_time = AND from_warehouse_id = AND to_warehouse_id = ;";

            cmd = new NpgsqlCommand(sql, p_connection);
            using (cmd)
            {
                cmd.Parameters.AddWithValue("date_time", movement.date_time);
                cmd.Parameters.AddWithValue("from_warehouse_id", movement.from_warehouse.id);
                cmd.Parameters.AddWithValue("to_warehouse_id", movement.to_warehouse.id);
                error_flag = cmd.ExecuteNonQuery();
            }

            if (error_flag != 0)
                return false;

            int id = 0;
            NpgsqlDataReader dr = cmd.ExecuteReader();
            id = (int)dr[0];

            if (id == 0)
                return false;

            movement.id = id;
            p_movements.Add(movement);
            p_homeModel.movements = p_movements;

            return true;
        }

        public bool RemoveMovement(Movement movement)
        {
            int error_flag = 0;

            String sql = "DELETE FROM movement WHERE id = @id;";

            NpgsqlCommand cmd = new NpgsqlCommand(sql, p_connection);
            using (cmd)
            {
                cmd.Parameters.AddWithValue("id", movement.id);
                error_flag = cmd.ExecuteNonQuery();
            }

            if (error_flag != 0)
                return false;

            p_movements.Remove(movement);
            p_homeModel.movements = p_movements;

            return true;
        }

        public bool refresh()
        {
            if(p_movements != null) p_movements.Clear();
            if(p_warehouses  != null) p_warehouses.Clear();
            if(p_nomenclature != null) p_nomenclature.Clear();
            
            //Склады
            String sql = "SELECT id, name FROM warehouse";
            NpgsqlCommand cmd = new NpgsqlCommand(sql, p_connection);
            NpgsqlDataReader dr = cmd.ExecuteReader();
            while (dr.Read())
            {
                int id = (int)dr[0];
                String name = (String)dr[1];
                if ((id == 0) || (name == null)) 
                { 
                    return false; 
                }
                Warehouse w = new Warehouse(id, name);
                p_warehouses.Add(w.id, w);
            }
            dr.Close();
            cmd.Cancel();

            //Номенклатура
            sql = "SELECT id, name FROM nomenclature";
            cmd = new NpgsqlCommand(sql, p_connection);
            dr = cmd.ExecuteReader();
            while (dr.Read())
            {
                int id = 0;
                id = (int)dr[0];
                String name = (String)dr[1];
                if ((id == 0) || (name == null)) { return false; }
                Nomenclature n = new Nomenclature(id, name);
                p_nomenclature.Add(n.id, n);
            }

            dr.Close();
            cmd.Cancel();

            //Перемещения
            sql = "SELECT id, date_time, from_warehouse_id, to_warehouse_id FROM movement";
            cmd = new NpgsqlCommand(sql, p_connection);
            dr = cmd.ExecuteReader();
            while (dr.Read())
            {
                Movement movement = new Movement();
                movement.id = (int)dr[0];
                movement.date_time = (DateTime)dr[1];

                Warehouse w;
                bool ok = p_warehouses.TryGetValue((int)dr[2], out w);
                if (ok) movement.from_warehouse = w;
                ok = p_warehouses.TryGetValue((int)dr[3], out w);
                if (ok) movement.to_warehouse = w;

                p_movements.Add(movement);
            }

            p_homeModel.movements = p_movements;
            p_homeModel.warehouses = p_warehouses.Values.ToList();
            return true;
        }
    }
}
