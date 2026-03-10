using System;
using MySql.Data;
using MySql.Data.MySqlClient;
namespace Stadium_company
{
    public class DBConnection
    {
        private DBConnection() { }

        public string Server { get; set; }
        public string DatabaseName { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }

        public MySqlConnection Connection { get; set; }

        private static DBConnection _instance = null;
        public static DBConnection Instance()
        {
            if (_instance == null)
                _instance = new DBConnection();
            return _instance;
        }

        public bool IsConnect()
        {
            if (Connection == null || Connection.State != System.Data.ConnectionState.Open)
            {
                if (String.IsNullOrEmpty(DatabaseName))
                    return false;

                try
                {
                    string connstring = string.Format("Server={0}; database={1}; UID={2}; password={3}",
                                        Server, DatabaseName, UserName, Password);
                    Connection = new MySqlConnection(connstring);
                    Connection.Open();
                }
                catch (Exception)
                {
                    return false;
                }
            }
            return true;
        }

        public void Close()
        {
            if (Connection != null && Connection.State == System.Data.ConnectionState.Open)
            {
                Connection.Close();
            }
        }
    }
}
