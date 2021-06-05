using Npgsql;
using StorekeeperAssistant.Models;
using System;
using System.Collections.Generic;
using System.Data;

namespace StorekeeperAssistant.Controllers.Db
{
    public class Repository
    {
        NpgsqlConnection p_connection;
        List<Movement> p_movements;
        Dictionary<int, Warehouse> p_warehouses;
        Dictionary<int, Nomenclature> p_nomenclature;

        public Repository(NpgsqlConnection connection)
        {
            p_movements = new List<Movement>();
            p_warehouses = new Dictionary<int, Warehouse>();
            p_nomenclature = new Dictionary<int, Nomenclature>();
            p_connection = connection;
        }

        public List<StockBalance> getAllWarehousesBalance()
        {
            List<StockBalance> balance_list = new List<StockBalance>();
            StockBalance stock_balance = new StockBalance();

            if (p_connection.State != ConnectionState.Open)
                p_connection.Open(); 
            string sql = "select t.warehouse_name, t.nomenclature_name, sum(count) from ( " +
                "select wh.name as warehouse_name, n.name as nomenclature_name, -sum(mc.count) as count " +
                "from movement_content mc " +
                "join movement m on m.id = mc.movement_id " +
                "join nomenclature n on n.id = mc.nomenclature_id " +
                "join warehouse wh on (m.from_warehouse_id = wh.id) " +
                "where m.date_time >= '2021-06-03' group by warehouse_name, nomenclature_name " +
                "UNION " +
                "select wh.name as warehouse_name, n.name as nomenclature_name, sum(mc.count) as count " +
                "from movement_content mc " +
                "join movement m on m.id = mc.movement_id " +
                "join nomenclature n on n.id = mc.nomenclature_id " +
                "join warehouse wh on (m.to_warehouse_id = wh.id)" +
                "where m.date_time >= '2021-06-03'" +
                "group by warehouse_name, nomenclature_name ) t " +
                "where warehouse_name <> 'ExternalWherehouse'" +
                " group by t.warehouse_name, t.nomenclature_name " +
                "order by warehouse_name, nomenclature_name";

            NpgsqlCommand cmd = new NpgsqlCommand(sql, p_connection);
            using (cmd)
            {
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        stock_balance = new StockBalance();
                        stock_balance.warehouse_name = (string)reader[0];
                        stock_balance.nomenclature_name = (string)reader[1];
                        stock_balance.count = (decimal)reader[2];

                        balance_list.Add(stock_balance);
                    }
                }
            }
            p_connection.Close();
            return balance_list;
        }
        public Int64 getMovementCount()
        {
            p_connection.Open();
            string sql = "SELECT count(id) FROM movement;";

            NpgsqlCommand cmd = new NpgsqlCommand(sql, p_connection);
            Int64 count = 0;
            using (cmd)
            {
                count = (Int64)cmd.ExecuteScalar();
            }
            p_connection.Close();
            return count;
        }

        public List<MovementContent> getMovementContent(int id)
        {
            List<MovementContent> movement_content_list = new List<MovementContent>();
            p_connection.Open();
            string sql = "SELECT mc.nomenclature_id, n.name, mc.count " +
                "FROM movement_content mc " +
                "JOIN nomenclature n on mc.nomenclature_id = n.id " +
                "WHERE mc.movement_id = @id;";

            NpgsqlCommand cmd = new NpgsqlCommand(sql, p_connection);
            using (cmd)
            {
                cmd.Parameters.AddWithValue("id", id);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        MovementContent movement_content = new MovementContent();
                        Nomenclature nomenclature = new Nomenclature();
                        nomenclature.id = (int)reader[0];
                        nomenclature.name = (string)reader[1];

                        movement_content.movement = getMovementById(id);
                        movement_content.nomenclature = nomenclature;
                        movement_content.count = (int)reader[2];

                        movement_content_list.Add(movement_content);
                    }
                }
            }
            p_connection.Close();
            return movement_content_list;
        }

        public Dictionary<int, Warehouse> getWarehouses()
        {
            return p_warehouses;
        }

        public Dictionary<int, Nomenclature> getNomenclature()
        {
            return p_nomenclature;
        }
        public List<Movement> getMovements()
        {
            return p_movements;
        }

        public Movement getMovementById(int id)
        {
            Movement movement = new Movement();
            foreach (var m in p_movements)
            {
                if(m.id == id)
                {
                    movement = m;
                    break;
                }
            }
            return movement;
        }
        public bool UpdateMovement(Movement movement)
        {
            p_connection.Open();
            String sql = "UPDATE movement " +
                "SET date_time = @date_time, " +
                "from_warehouse_id = @from_warehouse_id, " +
                "to_warehouse_id = @to_warehouse_id" +
                "WHERE id = @id)";

            NpgsqlCommand cmd = new NpgsqlCommand(sql, p_connection);
            using (cmd)
            {
                cmd.Parameters.AddWithValue("date_time", movement.date_time);
                cmd.Parameters.AddWithValue("from_warehouse_id", movement.from_warehouse.id);
                cmd.Parameters.AddWithValue("to_warehouse_id", movement.to_warehouse.id);
                cmd.Parameters.AddWithValue("id", movement.id);
                cmd.ExecuteNonQuery();
            }
            p_connection.Close();
            return true;
        }

        public bool AddMovement(Movement movement)
        {
            p_connection.Open();
            String sql = "BEGIN; " +
                "INSERT INTO movement(date_time, from_warehouse_id, to_warehouse_id)" +
                "VALUES (@date_time, @from_warehouse_id, @to_warehouse_id); " +
                "COMMIT;";

            NpgsqlCommand cmd = new NpgsqlCommand(sql, p_connection);
            using (cmd)
            {
                cmd.Parameters.AddWithValue("date_time", movement.date_time);
                cmd.Parameters.AddWithValue("from_warehouse_id", movement.from_warehouse.id);
                cmd.Parameters.AddWithValue("to_warehouse_id", movement.to_warehouse.id);
                cmd.ExecuteNonQuery();
            }

            sql = "SELECT id FROM movement " +
                "WHERE date_time = @date_time AND " +
                      "from_warehouse_id = @from_warehouse_id " +
                      "AND to_warehouse_id = @to_warehouse_id;";

            cmd = new NpgsqlCommand(sql, p_connection);

            int id = 0;
            using (cmd)
            {
                cmd.Parameters.AddWithValue("date_time", movement.date_time);
                cmd.Parameters.AddWithValue("from_warehouse_id", movement.from_warehouse.id);
                cmd.Parameters.AddWithValue("to_warehouse_id", movement.to_warehouse.id);

                id = (int)cmd.ExecuteScalar();
            }

            if (id == 0)
                return false;

            p_connection.Close();
            movement.id = id;
            p_movements.Add(movement);


            return true;
        }

        public bool AddWarehouse(string name)
        {
            p_connection.Open();
            String sql = "BEGIN; " +
                "INSERT INTO warehoune(name)" +
                "VALUES (@name); " +
                "COMMIT;";

            NpgsqlCommand cmd = new NpgsqlCommand(sql, p_connection);
            using (cmd)
            {
                cmd.Parameters.AddWithValue("name", name);
                cmd.ExecuteNonQuery();
            }

            sql = "SELECT id FROM warehoune WHERE name = @name;";

            cmd = new NpgsqlCommand(sql, p_connection);

            int id = 0;
            using (cmd)
            {
                cmd.Parameters.AddWithValue("name", name);
                id = (int)cmd.ExecuteScalar();
            }

            if (id == 0)
                return false;

            p_connection.Close();
            Warehouse warehouse = new Warehouse();
            warehouse.id = id;
            warehouse.name = name;
            p_warehouses.Add(id,warehouse);
            return true;
        }

        public bool RemoveMovement(Movement movement)
        {
            p_connection.Open();
            String sql = "DELETE FROM movement WHERE id = @id;";

            NpgsqlCommand cmd = new NpgsqlCommand(sql, p_connection);
            using (cmd)
            {
                cmd.Parameters.AddWithValue("id", movement.id);
                cmd.ExecuteNonQuery();
            }
            p_connection.Close();
            p_movements.Remove(movement);
            return true;
        }

        public bool refresh()
        {
            if(p_connection.State != ConnectionState.Open) 
                p_connection.Open();
            if (p_movements != null) p_movements.Clear();
            if (p_warehouses != null) p_warehouses.Clear();
            if (p_nomenclature != null) p_nomenclature.Clear();

            //Склады
            String sql = "SELECT id, name FROM warehouse";
            NpgsqlCommand cmd = new NpgsqlCommand(sql, p_connection);
            using (cmd)
            {
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int id = (int)reader[0];
                        String name = (String)reader[1];
                        if ((id == 0) || (name == null))
                        {
                            return false;
                        }
                        Warehouse w = new Warehouse(id, name);
                        p_warehouses.Add(w.id, w);
                    }
                }
            }

            //Номенклатура
            sql = "SELECT id, name FROM nomenclature";
            cmd = new NpgsqlCommand(sql, p_connection);
            using (cmd)
            {
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int id = 0;
                        id = (int)reader[0];
                        String name = (String)reader[1];
                        if ((id == 0) || (name == null)) { return false; }
                        Nomenclature n = new Nomenclature(id, name);
                        p_nomenclature.Add(n.id, n);
                    }
                }
            }

            //Перемещения
            sql = "SELECT id, date_time, from_warehouse_id, to_warehouse_id FROM movement";
            cmd = new NpgsqlCommand(sql, p_connection);
            using (cmd)
            {
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Movement movement = new Movement();
                        movement.id = (int)reader[0];
                        movement.date_time = (DateTime)reader[1];

                        Warehouse w;
                        bool ok = p_warehouses.TryGetValue((int)reader[2], out w);
                        if (ok) movement.from_warehouse = w;
                        ok = p_warehouses.TryGetValue((int)reader[3], out w);
                        if (ok) movement.to_warehouse = w;

                        p_movements.Add(movement);
                    }
                }
            }
            p_connection.Close();

            return true;
        }
    }
}
