using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using Stadium_company.Models;

namespace Stadium_company.Controleurs
{
    public class UtilisateurControleur
    {
        private DBConnection _db = DBConnection.Instance();
        private Hashage _hasheur = new Hashage();

        public void Inscrire(string pseudo, string email, string passwordClair)
        {
            // Ton SHA256 est parfait car mdp est en VARCHAR(64)
            string passwordHache = _hasheur.GenerateSHA256String(passwordClair);

            try
            {
                if (_db.IsConnect())
                {
                    // On utilise les noms EXACTS de ton fichier SQL
                    string sql = "INSERT INTO utilisateur (pseudo, email, mdp) VALUES (@pseudo, @email, @mdp)";
                    MySqlCommand cmd = new MySqlCommand(sql, _db.Connection);

                    cmd.Parameters.AddWithValue("@pseudo", pseudo);
                    cmd.Parameters.AddWithValue("@email", email);
                    cmd.Parameters.AddWithValue("@mdp", passwordHache);

                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex) { throw new Exception("Erreur Inscription : " + ex.Message); }
        }
    }
}