using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using Stadium_company.Controleurs;
using Stadium_company.Models;

namespace Stadium_company.Controleurs
{

    public class QuestionnaireControleur
    {
        private DBConnection _db = DBConnection.Instance();

        /// <summary>
        /// Récupère la liste simplifiée de tous les questionnaires pour l'accueil.
        /// </summary>
        public List<QuestionnaireModel> GetAll()

        {
            if (!_db.IsConnect()) throw new Exception("La connexion à MySQL a échoué !");

            List<QuestionnaireModel> liste = new List<QuestionnaireModel>();
            try
            {
                if (_db.IsConnect())
                {
                    // On utilise une sous-requête pour compter les questions dans la table 'comporte'
                    string query = @"SELECT id, nom, id_theme, publier, 
                            (SELECT COUNT(*) FROM comporte WHERE id_questionnaire = questionnaire.id) as nb_question 
                            FROM questionnaire";

                    MySqlCommand cmd = new MySqlCommand(query, _db.Connection);

                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            liste.Add(new QuestionnaireModel
                            {
                                Id = reader.GetInt32("id"),
                                Nom = reader.GetString("nom"),
                                IdTheme = reader.GetInt32("id_theme"),
                                NbQuestion = reader.GetInt32("nb_question"),
                                Publier = reader.GetBoolean("publier")
                            });
                        }
                    }
                }
            }
            catch (Exception ex) { throw new Exception("Erreur GetAll : " + ex.Message); }
            return liste;

        }

        /// <summary>
        /// Charge un questionnaire complet (avec ses questions) par son ID.
        /// </summary>
        public QuestionnaireModel GetById(int id)
        {
            QuestionnaireModel q = null;
            if (!_db.IsConnect()) return null;

            // 1. Récupérer le Questionnaire + Nom du Thème (JOIN)
            string sqlQ = @"SELECT q.*, t.nom as nom_theme 
                    FROM questionnaire q 
                    INNER JOIN theme t ON q.id_theme = t.id 
                    WHERE q.id = @id";

            MySqlCommand cmd = new MySqlCommand(sqlQ, _db.Connection);
            cmd.Parameters.AddWithValue("@id", id);

            using (MySqlDataReader dr = cmd.ExecuteReader())
            {
                if (dr.Read())
                {
                    q = new QuestionnaireModel
                    {
                        Id = Convert.ToInt32(dr["id"]),
                        Nom = dr["nom"].ToString(),
                        NomTheme = dr["nom_theme"].ToString(), // On stocke le texte ici
                        Questions = new List<QuestionModel>()
                    };
                }
            }

            if (q != null)
            {
                // 2. Récupérer les Questions + Nom du Type (JOIN)
                string sqlQuest = @"SELECT q.*, tq.libelle as nom_type 
                            FROM question q
                            INNER JOIN comporte c ON q.id = c.id_question 
                            INNER JOIN type_question tq ON q.id_type = tq.id
                            WHERE c.id_questionnaire = @idQ
                            ORDER BY c.num_question";

                MySqlCommand cmd2 = new MySqlCommand(sqlQuest, _db.Connection);
                cmd2.Parameters.AddWithValue("@idQ", id);

                using (MySqlDataReader dr2 = cmd2.ExecuteReader())
                {
                    while (dr2.Read())
                    {
                        q.Questions.Add(new QuestionModel
                        {
                            Id = Convert.ToInt32(dr2["id"]),
                            Libelle = dr2["libelle"].ToString(),
                            NomType = dr2["nom_type"].ToString(), // On stocke "QCM", "Texte", etc.
                            Choix = new List<ReponseModel>()
                        });
                    }
                }

                // 3. Récupérer les Réponses pour chaque question
                // 3. Récupérer les Réponses pour chaque question (Jointure entre 'reponse' et 'répondre')
                foreach (var quest in q.Questions)
                {
                    // On lie 'reponse' (r) et 'répondre' (link) via l'ID de la réponse
                    string sqlRep = @"SELECT r.id, r.contenu, link.bonne_reponse, link.poids 
                      FROM reponse r 
                      INNER JOIN repondre link ON r.id = link.id_reponse 
                      WHERE link.id_question = @idQuest";

                    MySqlCommand cmd3 = new MySqlCommand(sqlRep, _db.Connection);
                    cmd3.Parameters.AddWithValue("@idQuest", quest.Id);

                    using (MySqlDataReader dr3 = cmd3.ExecuteReader())
                    {
                        while (dr3.Read())
                        {
                            quest.Choix.Add(new ReponseModel
                            {
                                Id = Convert.ToInt32(dr3["id"]),
                                Contenu = dr3["contenu"].ToString(),
                                // On utilise les noms exacts de ton CSV 'répondre'
                                EstBonne = Convert.ToInt32(dr3["bonne_reponse"]) == 1,
                                Poids = Convert.ToInt32(dr3["poids"])
                            });
                        }
                    }
                }
            }
            
            return q;
        }
        /// <summary>
        /// Enregistre ou met à jour un questionnaire.
        /// </summary>
        public int Save(QuestionnaireModel model)
        {
            try
            {
                if (_db.IsConnect())
                {
                    string sql = model.Id == 0
                        ? "INSERT INTO questionnaire (nom, id_theme, nb_question, publier) VALUES (@nom, @theme, 0, @pub)"
                        : "UPDATE questionnaire SET nom = @nom, id_theme = @theme, publier = @pub WHERE id = @id";

                    MySqlCommand cmd = new MySqlCommand(sql, _db.Connection);
                    cmd.Parameters.AddWithValue("@nom", model.Nom);
                    cmd.Parameters.AddWithValue("@theme", model.IdTheme);
                    cmd.Parameters.AddWithValue("@pub", model.Publier ? 1 : 0);
                    if (model.Id > 0) cmd.Parameters.AddWithValue("@id", model.Id);

                    cmd.ExecuteNonQuery();

                    // Si c'est un nouvel enregistrement, on récupère l'ID généré
                    if (model.Id == 0)
                    {
                        return (int)cmd.LastInsertedId;
                    }
                    return model.Id;
                }
                return 0;
            }
            catch (Exception ex) { throw new Exception("Erreur Save : " + ex.Message); }
        }

        public void Supprimer(int id)
        {
            // 1. Vérification de la connexion
            if (!_db.IsConnect()) return;

            try
            {
                // ÉTAPE A : On nettoie la table de liaison 'comporte'
                // On retire toutes les questions associées à ce questionnaire
                string sqlComporte = "DELETE FROM comporte WHERE id_questionnaire = @id";
                MySqlCommand cmdComporte = new MySqlCommand(sqlComporte, _db.Connection);
                cmdComporte.Parameters.AddWithValue("@id", id);
                cmdComporte.ExecuteNonQuery();

                // ÉTAPE B : On supprime le questionnaire lui-même
                string sqlQuest = "DELETE FROM questionnaire WHERE id = @id";
                MySqlCommand cmdQuest = new MySqlCommand(sqlQuest, _db.Connection);
                cmdQuest.Parameters.AddWithValue("@id", id);

                int rowsAffected = cmdQuest.ExecuteNonQuery();

                if (rowsAffected == 0)
                {
                    throw new Exception("Le questionnaire n'a pas été trouvé ou a déjà été supprimé.");
                }
            }
            catch (Exception ex)
            {
                // On remonte l'erreur vers la Vue pour l'afficher à l'utilisateur
                throw new Exception("Erreur lors de la suppression du questionnaire : " + ex.Message);
            }
        }
    }
}

