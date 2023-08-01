using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ProjetFinEtude.ViewModel
{
    public class RoleFormViewModel
    {
        [Required, StringLength(50)]
        public string Name { get; set; }
    }
}
