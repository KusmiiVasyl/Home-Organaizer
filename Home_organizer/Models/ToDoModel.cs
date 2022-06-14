using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace To_Do_List.Models
{
    public class ToDoModel : INotifyPropertyChanged
    {  
        public DateTime TimeSet { get; set; } = DateTime.Now;
        private DateTime dateToDo = DateTime.Now;
        private bool isDone;
        private string text;

        public string StrTimeSet
        {
            get { return TimeSet.ToString("d"); }
        }

        public DateTime DateToDo
        {
            get { return dateToDo; }
            set
            {     
                dateToDo = value; 
                OnPropertyChange("DateToDo");
            }
        }

        public bool IsDone
        {
            get { return isDone; }
            set
            {
                if (isDone == value) return;
                isDone = value;
                OnPropertyChange("IsDone");
            }
        }

        public string Text
        {
            get { return text; }
            set
            {
                if (text == value) return;
                text = value;
                OnPropertyChange("Text");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChange(string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
