using MySql.Data.MySqlClient;
using Stadium_company.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Stadium_company.Controleurs
{
    public class ConnexionControleur
    {
        private DBConnection _db = DBConnection.Instance();

        private Hashage _hasheur = new Hashage();

        public UtilisateurModel Authentifier(string pseudo, string passwordSaisi)
        {
            UtilisateurModel user = null;
            string hash = _hasheur.GenerateSHA256String(passwordSaisi);

            try
            {
                if (_db.IsConnect())
                {
                    string sql = "SELECT id, pseudo, email FROM utilisateur WHERE pseudo = @pseudo AND mdp = @mdp";
                    MySqlCommand cmd = new MySqlCommand(sql, _db.Connection);
                    cmd.Parameters.AddWithValue("@pseudo", pseudo);
                    cmd.Parameters.AddWithValue("@mdp", hash);

                    using (MySqlDataReader r = cmd.ExecuteReader())
                    {
                        if (r.Read())
                        {
                            user = new UtilisateurModel
                            {
                                Id = r.GetInt32("id"),
                                Pseudo = r.GetString("pseudo"),
                                Email = r.GetString("email")
                            };
                        }
                    }
                }
            }
            catch (Exception ex) { throw new Exception("Erreur Connexion : " + ex.Message); }
            return user;
        }
    }
}


