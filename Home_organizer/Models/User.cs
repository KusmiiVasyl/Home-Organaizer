using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using To_Do_List.Models;

namespace Home_organizer.Models
{
    public class User 
    {
        public string login { get; set; }
        public string password { get; set; }
        public BindingList<ToDoModel> ToDoTasksList { get; set; }
        public List<Communal> listCommunal { get; set; }
        public string address { get; set; }
    }
}
