using MySql.Data.MySqlClient;
using Stadium_company.Models;
using System;
using System.Collections.Generic;

namespace Stadium_company.Controleurs
{
    public class ReponseControleur
    {
        private DBConnection _db = DBConnection.Instance();

        public List<ReponseModel> GetByQuestion(int idQuestion)
        {
            List<ReponseModel> liste = new List<ReponseModel>();
            if (!_db.IsConnect()) return liste;

            string sql = @"SELECT r.id, r.contenu, rep.bonne_reponse, rep.poids, rep.num_reponse 
                           FROM reponse r
                           INNER JOIN repondre rep ON r.id = rep.id_reponse
                           WHERE rep.id_question = @idQ
                           ORDER BY rep.num_reponse ASC";

            MySqlCommand cmd = new MySqlCommand(sql, _db.Connection);
            cmd.Parameters.AddWithValue("@idQ", idQuestion);

            using (MySqlDataReader r = cmd.ExecuteReader())
            {
                while (r.Read())
                {
                    liste.Add(new ReponseModel
                    {
                        Id = r.GetInt32("id"),
                        Contenu = r.GetString("contenu"),
                        EstBonne = r.GetBoolean("bonne_reponse"),
                        Poids = r.GetInt32("poids"),
                        Ordre = r.GetInt32("num_reponse")
                    });
                }
            }
            return liste;
        }

        public void AjouterReponse(int idQuestion, ReponseModel model)
        {
            if (!_db.IsConnect()) return;

            try
            {
                // 1. Si c'est une bonne réponse, on reset les autres
                if (model.EstBonne)
                {
                    string sqlReset = "UPDATE repondre SET bonne_reponse = 0 WHERE id_question = @idQ";
                    MySqlCommand cmdReset = new MySqlCommand(sqlReset, _db.Connection);
                    cmdReset.Parameters.AddWithValue("@idQ", idQuestion);
                    cmdReset.ExecuteNonQuery();
                }

                // 2. Création du texte de réponse
                string sqlR = "INSERT INTO reponse (contenu, id_type) VALUES (@cont, 1)";
                MySqlCommand cmdR = new MySqlCommand(sqlR, _db.Connection);
                cmdR.Parameters.AddWithValue("@cont", model.Contenu.Length > 50 ? model.Contenu.Substring(0, 50) : model.Contenu);
                cmdR.ExecuteNonQuery();
                int idRep = (int)cmdR.LastInsertedId;

                string sqlNum = "SELECT IFNULL(MAX(num_reponse), 0) + 1 FROM repondre WHERE id_question = @idQ";
                MySqlCommand cmdNum = new MySqlCommand(sqlNum, _db.Connection);
                cmdNum.Parameters.AddWithValue("@idQ", idQuestion);
                int nextNum = Convert.ToInt32(cmdNum.ExecuteScalar());

                string sqlL = @"INSERT INTO repondre (id_question, id_reponse, bonne_reponse, num_reponse, poids) 
                        VALUES (@idQ, @idR, @bonne, @num, @poids)";

                MySqlCommand cmdL = new MySqlCommand(sqlL, _db.Connection);
                cmdL.Parameters.AddWithValue("@idQ", idQuestion);
                cmdL.Parameters.AddWithValue("@idR", idRep);
                cmdL.Parameters.AddWithValue("@bonne", model.EstBonne);
                cmdL.Parameters.AddWithValue("@num", nextNum);
                cmdL.Parameters.AddWithValue("@poids", model.Poids);
                cmdL.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                throw new Exception("Erreur lors de l'ajout de la réponse : " + ex.Message);
            }
        }
        public void SupprimerReponse(int idRep)
        {
            if (!_db.IsConnect()) return;

            try
            {
                // 1. On supprime d'abord le lien dans la table 'repondre'
                string sqlLink = "DELETE FROM repondre WHERE id_reponse = @id";
                MySqlCommand cmdLink = new MySqlCommand(sqlLink, _db.Connection);
                cmdLink.Parameters.AddWithValue("@id", idRep);
                cmdLink.ExecuteNonQuery();

                // 2. On supprime ensuite la ligne dans la table 'reponse'
                string sqlRep = "DELETE FROM reponse WHERE id = @id";
                MySqlCommand cmdRep = new MySqlCommand(sqlRep, _db.Connection);
                cmdRep.Parameters.AddWithValue("@id", idRep);

                int rowsAffected = cmdRep.ExecuteNonQuery();

                if (rowsAffected == 0)
                {
                    throw new Exception("La réponse n'a pas été trouvée dans la base.");
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Erreur de suppression SQL : " + ex.Message);
            }
        
    }
    }
}