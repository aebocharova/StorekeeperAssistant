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
