using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProjetFinEtude.ViewModel
{
    public class CheckboxViewModel
    {
        public int Id { get; set; }
        public string NationalId { get; set; }

        public string DisplayValue { get; set; }
        public bool IsSelected { get; set; }
    }
}
