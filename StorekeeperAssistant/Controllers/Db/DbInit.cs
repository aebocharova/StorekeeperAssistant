using Npgsql;
using System;

namespace StorekeeperAssistant.Controllers.Db
{
    public class DbInit
    {
        private NpgsqlConnection p_connection;

        public DbInit()
        {

        }

        public NpgsqlConnection getConnection()
        {
            return p_connection;
        }

        public bool connect()
        {
            String connString;
            connString = "Host=localhost; Port=5432; User Id=postgres; Password=postgres; Database=StorekeeperAssistant";

            NpgsqlConnection conn = new NpgsqlConnection(connString);
            try
            {
                conn.Open();
                p_connection = conn;
                return true;
            }
            catch (Exception e)
            {
                return false;
            }

        }

        public bool disconnect()
        {
            try
            {
                p_connection.CloseAsync();
                p_connection = null;
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }
        public void clear()
        {
            String sql = "DROP TABLE IF EXISTS movement_content;" +
                "DROP TABLE IF EXISTS movement; " +
                "DROP TABLE IF EXISTS nomenclature; " +
                "DROP TABLE IF EXISTS warehouse;";

            using (var cmd = new NpgsqlCommand(sql, p_connection))
            {
                cmd.ExecuteNonQuery();
            }
        }
        public void init()
        {
            clear();

            String sql = "BEGIN; " +
                "CREATE TABLE warehouse(id SERIAL NOT NULL, name varchar(255) UNIQUE NOT NULL, PRIMARY KEY(id));" +
                "CREATE TABLE nomenclature(id SERIAL NOT NULL, name varchar(255) UNIQUE NOT NULL, PRIMARY KEY(id)); " +
                "CREATE TABLE movement(id SERIAL NOT NULL, date_time timestamp NOT NULL, from_warehouse_id int4 NOT NULL, to_warehouse_id int4 NOT NULL, PRIMARY KEY(id));" +
                "CREATE TABLE movement_content(movement_id int4 NOT NULL, nomenclature_id int4 NOT NULL, count int4 NOT NULL, PRIMARY KEY(movement_id, nomenclature_id)); " +

                "ALTER TABLE movement ADD CONSTRAINT FKmovement649119 FOREIGN KEY(from_warehouse_id) REFERENCES warehouse(id); " +
                "ALTER TABLE movement_content ADD CONSTRAINT FKmovement_c708723 FOREIGN KEY(movement_id) REFERENCES movement(id); " +
                "ALTER TABLE movement ADD CONSTRAINT FKmovement256504 FOREIGN KEY(to_warehouse_id) REFERENCES warehouse(id); " +
                "ALTER TABLE movement_content ADD CONSTRAINT FKmovement_c278973 FOREIGN KEY(nomenclature_id) REFERENCES nomenclature(id);" +

                "INSERT INTO warehouse(name) VALUES('CompanyWherehouse1');" +
                "INSERT INTO warehouse(name) VALUES('CompanyWherehouse2');" +
                "INSERT INTO warehouse(name) VALUES('ExternalWherehouse');" +

                "INSERT INTO nomenclature(name) VALUES('nomenclature1');" +
                "INSERT INTO nomenclature(name) VALUES('nomenclature2');" +
                "INSERT INTO nomenclature(name) VALUES('nomenclature3');" +
                "INSERT INTO nomenclature(name) VALUES('nomenclature4');" +
                "INSERT INTO nomenclature(name) VALUES('nomenclature5');" +
                "INSERT INTO nomenclature(name) VALUES('nomenclature6');" +
                "INSERT INTO nomenclature(name) VALUES('nomenclature7');" +

                "insert into movement(date_time, from_warehouse_id, to_warehouse_id) values(now(), 1, 2); " +
                "insert into movement(date_time, from_warehouse_id, to_warehouse_id) values(now(), 1, 3); " +
                "insert into movement(date_time, from_warehouse_id, to_warehouse_id) values(now(), 3, 2); " +
                "COMMIT;";

            using (var cmd = new NpgsqlCommand(sql, p_connection))
            {
                cmd.ExecuteNonQuery();
            }
        }
    }
}
