using MySql.Data.MySqlClient;
using Stadium_company.Models;
using System.Collections.Generic;

namespace Stadium_company.Controleurs
{
    public class ThemeControleur
    {
        private DBConnection _db = DBConnection.Instance();

        public List<ThemeModel> GetAll()
        {
            List<ThemeModel> liste = new List<ThemeModel>();
            if (!_db.IsConnect()) return liste;

            // On récupère id et nom de la table theme
            string sql = "SELECT id, nom FROM theme";
            MySqlCommand cmd = new MySqlCommand(sql, _db.Connection);

            using (MySqlDataReader r = cmd.ExecuteReader())
            {
                while (r.Read())
                {
                    liste.Add(new ThemeModel
                    {
                        Id = r.GetInt32("id"),
                        Nom = r.GetString("nom")
                    });
                }
            }
            return liste;
        }
    }
}