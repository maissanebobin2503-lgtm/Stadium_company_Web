using MySql.Data.MySqlClient;
using Stadium_company.Models;
using System.Collections.Generic;

namespace Stadium_company.Controleurs
{
    public class TypeQuestionControleur
    {
        private DBConnection _db = DBConnection.Instance();

        public List<TypeQuestionModel> GetAll()
        {
            List<TypeQuestionModel> liste = new List<TypeQuestionModel>();
            if (!_db.IsConnect()) return liste;

            // Récupération des types de questions
            string sql = "SELECT id, libelle FROM type_question";
            MySqlCommand cmd = new MySqlCommand(sql, _db.Connection);

            using (MySqlDataReader r = cmd.ExecuteReader())
            {
                while (r.Read())
                {
                    liste.Add(new TypeQuestionModel
                    {
                        Id = r.GetInt32("id"),
                        Libelle = r.GetString("libelle")
                    });
                }
            }
            return liste;
        }
    }
}