using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using Stadium_company.Models;

namespace Stadium_company.Controleurs
{
    public class QuestionControleur
    {
        private DBConnection _db = DBConnection.Instance();

        /// <summary>
        /// Ajoute une question et crée le lien dans la table 'comporte'.
        /// </summary>
        public int AjouterQuestion(int idQuestionnaire, QuestionModel q)
        {
            if (!_db.IsConnect()) return 0;

            // Utilisation d'une transaction pour garantir l'intégrité
            using (MySqlTransaction tra = _db.Connection.BeginTransaction())
            {
                try
                {
                    string sqlQ = "INSERT INTO question (libelle, id_type) VALUES (@lib, @type)";
                    MySqlCommand cmdQ = new MySqlCommand(sqlQ, _db.Connection, tra);
                    cmdQ.Parameters.AddWithValue("@lib", q.Libelle);
                    cmdQ.Parameters.AddWithValue("@type", q.IdType);
                    cmdQ.ExecuteNonQuery();

                    // On récupère l'ID généré par MySQL
                    int idNewQuest = (int)cmdQ.LastInsertedId;

                    // 2. Création du lien avec le questionnaire dans 'comporte'
                    string sqlC = @"INSERT INTO comporte (id_questionnaire, id_question, num_question) 
                           VALUES (@idQ, @idQuest, 
                           (SELECT IFNULL(MAX(c2.num_question), 0) + 1 FROM comporte c2 WHERE c2.id_questionnaire = @idQ))";

                    MySqlCommand cmdC = new MySqlCommand(sqlC, _db.Connection, tra);
                    cmdC.Parameters.AddWithValue("@idQ", idQuestionnaire);
                    cmdC.Parameters.AddWithValue("@idQuest", idNewQuest);
                    cmdC.ExecuteNonQuery();

                    tra.Commit(); // On valide les deux opérations
                    return idNewQuest;
                }
                catch (Exception ex)
                {
                    tra.Rollback(); // En cas d'erreur, MySQL annule tout
                    throw new Exception("Erreur création question : " + ex.Message);
                }
            }
        }

        /// <summary>
        /// Supprime une question et nettoie les tables de liaison et de réponses.
        /// </summary>
        public void SupprimerQuestion(int idQuestion)
        {
            if (!_db.IsConnect()) return;

            using (MySqlTransaction tra = _db.Connection.BeginTransaction())
            {
                try
                {
                    // A. Suppression des réponses dans 'repondre'
                    string sql1 = "DELETE FROM repondre WHERE id_question = @id";
                    MySqlCommand cmd1 = new MySqlCommand(sql1, _db.Connection, tra);
                    cmd1.Parameters.AddWithValue("@id", idQuestion);
                    cmd1.ExecuteNonQuery();

                    // B. Suppression du lien dans 'comporte'
                    string sql2 = "DELETE FROM comporte WHERE id_question = @id";
                    MySqlCommand cmd2 = new MySqlCommand(sql2, _db.Connection, tra);
                    cmd2.Parameters.AddWithValue("@id", idQuestion);
                    cmd2.ExecuteNonQuery();

                    // C. Suppression finale dans 'question'
                    string sql3 = "DELETE FROM question WHERE id = @id";
                    MySqlCommand cmd3 = new MySqlCommand(sql3, _db.Connection, tra);
                    cmd3.Parameters.AddWithValue("@id", idQuestion);
                    cmd3.ExecuteNonQuery();

                    tra.Commit();
                }
                catch (Exception ex)
                {
                    tra.Rollback();
                    throw new Exception("Erreur de suppression (Controleur) : " + ex.Message);
                }
            }
        }

        /// <summary>
        /// Récupère les types de questions disponibles.
        /// </summary>
        public List<TypeQuestionModel> GetTypes()
        {
            List<TypeQuestionModel> liste = new List<TypeQuestionModel>();
            try
            {
                if (_db.IsConnect())
                {
                    string query = "SELECT id, libelle FROM type_question";
                    MySqlCommand cmd = new MySqlCommand(query, _db.Connection);
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
                }
            }
            catch (Exception ex) { throw new Exception("Erreur GetTypes : " + ex.Message); }
            return liste;
        }
        public void ModifierLibelle(int idQuestion, string nouveauLibelle)
        {
            if (!_db.IsConnect()) return;
            try
            {
                string sql = "UPDATE question SET libelle = @lib WHERE id = @id";
                MySqlCommand cmd = new MySqlCommand(sql, _db.Connection);
                cmd.Parameters.AddWithValue("@lib", nouveauLibelle);
                cmd.Parameters.AddWithValue("@id", idQuestion);

                cmd.ExecuteNonQuery();
            }
            catch (Exception ex) { throw new Exception("Erreur libellé : " + ex.Message); }
        }
    }
}