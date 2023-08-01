using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProjetFinEtude.ViewModel
{
    public class QuizResultListViewModel
    {
        public List<QuizResultViewModel> quizResultViewModels { get; set; }
        public int QuizId { get; set; }
        public int SubjectId { get; set; }
    }
}
