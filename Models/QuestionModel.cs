using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stadium_company.Models
{
    public class QuestionModel
    {
        public int Id { get; set; }
        public string Libelle { get; set; }
        public int IdType { get; set; }
        public string NomType { get; set; }
        public int Poids { get; set; }

        // Liste des choix possibles pour cette question
        public List<ReponseModel> Choix { get; set; } = new List<ReponseModel>();
        public List<QuestionModel>TypeQuestion { get; set; } = new List<QuestionModel>();
    }
}
