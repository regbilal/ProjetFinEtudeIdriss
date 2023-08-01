using ProjetFinEtude.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProjetFinEtude.ViewModel
{
    public class EnrollmentViewModel
    {
        public int ClassId { get; set; }
        public int SubjectCount { get; set; }
        public List<Subject> Subjects { get; set; }
        public string ClassName { get; set; }
    }
}
