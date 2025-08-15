using CitizenFX.Core;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CitizenFX.Core.Native.API;

namespace nEUP_Menu_Server.EUP
{
    class Database : BaseScript
    {
        public static MySqlConnection GetConnection()
        {
            try
            {
                string connectionString = GetConvar("mysql_connection_string", "");
                var conn = new MySqlConnection(connectionString);
                conn.Open();
                return conn;
            }
            catch (Exception e)
            {
                Debug.WriteLine($"{EUPController.ColorRed}[Error-GetConnection] {e}");
            }
            return null;
        }
    }
}
