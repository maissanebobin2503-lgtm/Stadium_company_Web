
using Microsoft.AspNetCore.Mvc;
using Stadium_company.Controleurs;

namespace Stadium_company_Web.Controllers
{
    public class QuestionnaireController : Controller
    {
        private QuestionnaireControleur _dataCtrl = new QuestionnaireControleur();

        public IActionResult Index()
        {
            var data = _dataCtrl.GetAll();
            return View("Index", data);
        }


        public IActionResult Details(int id)
        {
            // On récupère le questionnaire complet avec ses questions et réponses
            var questionnaire = _dataCtrl.GetById(id);

            if (questionnaire == null)
            {
                return NotFound();
            }

            return View(questionnaire);
        }
        [HttpPost]
        public IActionResult CalculerScore(int idQuestionnaire, Dictionary<int, int> reponses)
        {
            int scoreTotal = 0;
            int scoreMax = 0;

            var questionnaire = _dataCtrl.GetById(idQuestionnaire);

            // Calcul du score MAX
            foreach (var q in questionnaire.Questions)
            {
                scoreMax += q.Choix.Where(r => r.EstBonne).Sum(r => r.Poids);
            }

            // Calcul du score de l'utilisateur
            foreach (var entry in reponses)
            {
                int idQuestion = entry.Key;
                int idReponseChoisie = entry.Value;

                var question = questionnaire.Questions.FirstOrDefault(q => q.Id == idQuestion);
                // On cherche la réponse choisie dans la liste
                var rep = question?.Choix.FirstOrDefault(r => r.Id == idReponseChoisie);

                if (rep != null && rep.EstBonne)
                {
                    scoreTotal += rep.Poids;
                }
            }

            // On passe les deux infos à la vue
            ViewBag.Score = scoreTotal;
            ViewBag.Max = scoreMax;

            return View("Résultats"); 
        }
    }
}