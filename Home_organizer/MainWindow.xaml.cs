using Home_organizer.Models;
using System;
using System.Collections.Generic;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Home_organizer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly string path = $"{Environment.CurrentDirectory}\\users.json";
        private FileReadWrite file;
        public List<User> listUsers;
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Reg_Click(object sender, RoutedEventArgs e) //Натискання кнопки РЄСТРАЦІЯ
        {
            this.Opacity = 0.25;
            Registr registrWindow = new Registr();
            registrWindow.ShowDialog();
            this.Opacity = 1;
        }

        private void Button_EnterToProgram_Click(object sender, RoutedEventArgs e)//Натискання кнопки УВІЙТИ
        {
            int i = 0;
            foreach (var item in listUsers)
            {
                if (textBoxLogin.Text==item.login && passBox.Password==item.password)
                {
                    ProgramWindow programWindow = new ProgramWindow(i);
                    Close();
                    programWindow.Show();
                    return;
                }
                i++;
            }
            MessageBox.Show("Користувача не знайдено!\nПройдіть реєстрацію.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            file = new FileReadWrite(path);
            try
            {
                listUsers = file.LoadData();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

    }
}
