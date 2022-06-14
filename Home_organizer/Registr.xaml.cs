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
using System.Windows.Shapes;
using System.Windows.Media.Animation;
using Home_organizer.Models;

namespace Home_organizer
{
    /// <summary>
    /// Interaction logic for Registr.xaml
    /// </summary>
    public partial class Registr : Window
    {
        private readonly string path = $"{Environment.CurrentDirectory}\\users.json";
        private FileReadWrite file;
        public Registr()
        {
            InitializeComponent();
            //анімація кнопки ЗАРЕЄСТРУВАТИСЬ
            DoubleAnimation animeBtn = new DoubleAnimation();
            animeBtn.From = 0;
            animeBtn.To = 400;
            animeBtn.Duration = TimeSpan.FromSeconds(3);
            regButton.BeginAnimation(Button.WidthProperty, animeBtn);
            /////////
        }


        private void Button_EnterReg_Click(object sender, RoutedEventArgs e) //натиск на кнопку ЗАРЕЄСТРУВАТИСЬ
        {
            int cnt = 0;
            string login = textBoxLogin.Text;
            string pass = passBox_1.Password;
            string pass_2 = passBox_2.Password;

            if (login.Length < 5)
            {
                textBoxLogin.ToolTip = "Не коректний логін!!! \nЛогін повинен містити не менше 5 символів!";
                textBoxLogin.Background = Brushes.DarkOrange;
            }
            else { textBoxLogin.ToolTip = ""; textBoxLogin.Background = Brushes.Transparent; cnt++; }
            if (pass.Length < 5)
            {
                passBox_1.ToolTip = "Не коректний пароль!!! \nПароль повинен містити не менше 5 символів!";
                passBox_1.Background = Brushes.DarkOrange;
            }
            else { passBox_1.ToolTip = ""; passBox_1.Background = Brushes.Transparent; cnt++; }
            if (pass != pass_2 || pass_2.Length == 0)
            {
                passBox_2.ToolTip = "Не коректний пароль!!!";
                passBox_2.Background = Brushes.DarkOrange;
            }
            else { passBox_2.ToolTip = ""; passBox_2.Background = Brushes.Transparent; cnt++; }
            if (cnt == 3)
            {
                User user = new User();
                user.login = login;
                user.password = pass;
                MainWindow mainWindow = new MainWindow();
                file = new FileReadWrite(path);
                mainWindow.listUsers = file.LoadData();
                mainWindow.listUsers.Add(user);
                file.SaveData(mainWindow.listUsers);
                MessageBox.Show("Користувача додано!");
                mainWindow.Show();
                Close();
            }
        }
    }
}
