using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Home_organizer
{
    /// <summary>
    /// Interaction logic for CalculatorWindow.xaml
    /// </summary>
    public partial class CalculatorWindow : Window
    {
        private string res = "";
        public CalculatorWindow()
        {
            InitializeComponent();
            foreach (UIElement item in MainScreen.Children)
            {
                if (item is Button)
                {
                    ((Button)item).Click += ClickButton;
                }
            }
        }
        private void ClickButton(object sender, RoutedEventArgs e)
        {
            if (res.Length != 0) { res = ""; textLabel2.Text = ""; }
            string str = (string)((Button)e.OriginalSource).Content;
            if (str == "CE") { textLabel2.Text = ""; }
            else if (str == "C") { textLabel2.Text = ""; textLabel1.Text = ""; }
            else if (str == "=")
            {
                res = new DataTable().Compute(textLabel1.Text, null).ToString();
                textLabel2.Text = res;
            }
            else if (str == "<")
            {
                if (textLabel2.Text.Length != 0)
                {
                    textLabel2.Text = textLabel2.Text.Remove(textLabel2.Text.Length - 1);
                    textLabel1.Text = textLabel1.Text.Remove(textLabel1.Text.Length - 1);
                }
            }
            else if (str == "/" | str == "*" | str == "-" | str == "+")
            {
                textLabel2.Text = "";
                if (textLabel1.Text.Length > 0 && textLabel1.Text.Last() != '/' && textLabel1.Text.Last() != '*' && textLabel1.Text.Last() != '-' && textLabel1.Text.Last() != '+')
                    textLabel1.Text += str;
            }
            else if (str == ".")
            {
                if (!textLabel2.Text.Contains('.') && textLabel2.Text.Length > 0) { textLabel2.Text += str; textLabel1.Text += str; }
            }
            else
            {
                for (int i = 0; i <= 9; i++)
                {
                    if (str == i.ToString())
                        textLabel2.Text += str;
                }
                textLabel1.Text += str;
            }
        }
    }
}
