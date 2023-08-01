using ProjetFinEtude.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProjetFinEtude.ViewModel
{
    public class AdminHomeViewModel
    {
        public List<Notice> Notice { get; set; }
        public List<Chat> Chats { get; set; }
    }
}
