using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stadium_company.Models
{
    public class ReponseModel
    {
        public int Id { get; set; } // ID de la table reponse
        public string Contenu { get; set; }
        public bool EstBonne { get; set; } // Vient de la table repondre
        public int Poids { get; set; }     // Vient de la table repondre
        public int Ordre { get; set; }     // num_reponse
    
    
    }
         
    }
