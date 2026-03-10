using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stadium_company.Models
{
    public class QuestionnaireModel
    {
        public int Id { get; set; }
        public string Nom { get; set; }
        public int IdTheme { get; set; }
        public string NomTheme { get; set; }
        public string Nom_type { get; set; }
        public int NbQuestion { get; set; }
        public bool Publier { get; set; }

        public List<QuestionModel> Questions { get; set; } = new List<QuestionModel>();
    }
}
