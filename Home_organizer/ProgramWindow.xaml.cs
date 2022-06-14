using Home_organizer.Models;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using To_Do_List.Models;

namespace Home_organizer
{
    /// <summary>
    /// Interaction logic for ProgramWindow.xaml
    /// </summary>
    /// 
    public partial class ProgramWindow : Window
    {
        Button btn; //кнопка повернутись назад в списку справ
        private readonly string path = $"{Environment.CurrentDirectory}\\users.json";
        private FileReadWrite file;
        private List<User> listUsers;
        private readonly int num;
        private Communal communalForMonth; // для додавання нових показів, даних для нового м-ця
        private string[] period;//для виведення періоду в Подивитись оплати
        bool searchOfPeriod = false;
        double totalSumForChoosePeriod=0;

        public ProgramWindow(int _num)
        {
            num = _num;
            InitializeComponent();
            
            file = new FileReadWrite(path);
            try
            {
                listUsers = file.LoadData();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            if (listUsers[num].ToDoTasksList == null) listUsers[num].ToDoTasksList = new BindingList<ToDoModel>();
            if (listUsers[num].listCommunal == null) { listUsers[num].listCommunal = new List<Communal>(); listUsers[num].listCommunal.Add(new Communal()); }
            address_TextBox.Text = listUsers[num].address;
            for (int i = 2021; i < 2080; i++) //додаєм роки в comboBoxYear
            {
                var item = new ComboBoxItem { Content = i, Background = Brushes.Beige };
                comboBoxYear.Items.Add(item);
            }
            communalForMonth = new Communal();
            if(listUsers[num].listCommunal.Count!=0) communalForMonth = listUsers[num].listCommunal.Last();
            period = new string[4] { "", "", "", "" };

            ShowLastComunalMonthData();
            ShowLastComunalTarif();

            List<Communal> items = new List<Communal>();
            for (int i = listUsers[num].listCommunal.Count-1; i >= 0; i--)
            {
                items.Add(listUsers[num].listCommunal[i]);
            }
            lvUsers.ItemsSource = items;
            CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(lvUsers.ItemsSource);
            view.Filter = UserFilter;
        }     

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            dataGrid_Tasks.ItemsSource = listUsers[num].ToDoTasksList;
            listUsers[num].ToDoTasksList.ListChanged += List_ListChanged;
            Varification_Tasks();
            comboBoxMonth.Text = "Виберіть місяць";
            comboBoxYear.Text = "Виберіть рік";
        }

        private void List_ListChanged(object sender, ListChangedEventArgs e)
        {  
            if(e.ListChangedType == ListChangedType.ItemAdded || e.ListChangedType == ListChangedType.ItemDeleted || e.ListChangedType== ListChangedType.ItemChanged)
            {
                file.SaveData(listUsers);
            }
        }

        private void Varification_Tasks() //перевірка дати виконання справи на виконання
        {
            MessageBoxResult result;
            BindingList<ToDoModel> tmpToDoTasksList = new BindingList<ToDoModel>();
            bool isDoneTask = false;
            for (int i = 0; i < listUsers[num].ToDoTasksList.Count; i++)
            {
                if (DateTime.Compare(listUsers[num].ToDoTasksList[i].DateToDo, DateTime.Now) < 0 && listUsers[num].ToDoTasksList[i].IsDone == false)
                {
                    isDoneTask = true;
                    tmpToDoTasksList.Add(listUsers[num].ToDoTasksList[i]);
                }
            }
            if (isDoneTask) {
                result = MessageBox.Show("У Вас є невиконані справи!!!\nПереглянути список невиконаних справ?", "Інформація про невиконані справи.", MessageBoxButton.YesNo, MessageBoxImage.Information);
                if(result == MessageBoxResult.Yes)
                {
                    btn = new Button
                    {     
                        Content = "Повернутись до списку всіх справ.",
                        FontSize = 16,
                        VerticalAlignment = VerticalAlignment.Top,
                        HorizontalAlignment = HorizontalAlignment.Left,
                        Margin = new Thickness(10,10,0,0),
                        Name = "btn",
                    };
                    btn.Click += Btn_Click;
                    DockPanel.SetDock(btn, Dock.Top);
                    dockPanel_Task.Children.Insert(0,btn);
                    dataGrid_Tasks.ItemsSource = tmpToDoTasksList;
                    header_Task.Header = "Список невиконаних справ";
                }
            }
        }

        private void Btn_Click(object sender, RoutedEventArgs e)//кнопка повернення до загального списку справ
        {
            header_Task.Header = "Список справ";
            dataGrid_Tasks.ItemsSource = listUsers[num].ToDoTasksList;
            dockPanel_Task.Children.Remove(btn);
        }

        private void comboBoxYear_SelectionChanged(object sender, SelectionChangedEventArgs e) //вибір року періоду розрахунку комунальних
        {
            listUsers = null;
            communalForMonth.Year = comboBoxYear.SelectedItem.ToString().Replace("System.Windows.Controls.ComboBoxItem: ", "");
            listUsers = file.LoadData();
        } 

        private void comboBoxMonth_SelectionChanged(object sender, SelectionChangedEventArgs e) //вибір місяця періоду розрахунку комунальних
        {
            listUsers = null;
            communalForMonth.Month = comboBoxMonth.SelectedItem.ToString().Replace("System.Windows.Controls.ComboBoxItem: ", "");
            listUsers = file.LoadData();
        }

        private void ShowLastComunalMonthData()//витягування останніх даних за м-ць по комунальних і присвоєння в XAML в "Розрахунку"
        {
            waterSupplyTarif.Text = communalForMonth.WaterSupplyTarif.ToString();
            prevWaterSupply.Text = communalForMonth.CurentWaterSupply.ToString();
            drainageTarif.Text = communalForMonth.DrainageTarif.ToString();
            prevDrainage.Text = communalForMonth.CurentDrainage.ToString();
            gasTarif.Text = communalForMonth.GasTarif.ToString();
            prevGas.Text = communalForMonth.CurentGas.ToString();
            deliveryGasTarif.Text = communalForMonth.DeliveryGasTarif.ToString();
            electricTarif.Text = communalForMonth.ElectricTarif.ToString();
            prevElectric.Text = communalForMonth.CurentElectric.ToString();
            rentTarif.Text = communalForMonth.RentTarif.ToString();
            protectionTarif.Text = communalForMonth.ProtectionTarif.ToString();
            garbageTarif.Text = communalForMonth.GarbageTarif.ToString();
            intercomTarif.Text = communalForMonth.IntercomTarif.ToString();
            internetTarif.Text = communalForMonth.InternetTarif.ToString();     
        }

        private void ShowLastComunalTarif()//витягування останніх даних по тарифах по комунальних і присвоєння в XAML в "Змінити тариф"
        {
            //показ останнього тарифу в розрахунку
            waterSupplyTarif.Text = communalForMonth.WaterSupplyTarif.ToString();
            drainageTarif.Text = communalForMonth.DrainageTarif.ToString();
            gasTarif.Text = communalForMonth.GasTarif.ToString();
            deliveryGasTarif.Text = communalForMonth.DeliveryGasTarif.ToString();
            electricTarif.Text = communalForMonth.ElectricTarif.ToString();
            rentTarif.Text = communalForMonth.RentTarif.ToString();
            protectionTarif.Text = communalForMonth.ProtectionTarif.ToString();
            garbageTarif.Text = communalForMonth.GarbageTarif.ToString();
            intercomTarif.Text = communalForMonth.IntercomTarif.ToString();
            internetTarif.Text = communalForMonth.InternetTarif.ToString();

            //показ останнього тарифу в зміні тарифу
            tar_waterSupplyTarif.Text = communalForMonth.WaterSupplyTarif.ToString();
            tar_drainageTarif.Text = communalForMonth.DrainageTarif.ToString();
            tar_gasTarif.Text = communalForMonth.GasTarif.ToString();
            tar_deliveryGasTarif.Text = communalForMonth.DeliveryGasTarif.ToString();
            tar_electricTarif.Text = communalForMonth.ElectricTarif.ToString();
            tar_rentTarif.Text = communalForMonth.RentTarif.ToString();
            tar_protectionTarif.Text = communalForMonth.ProtectionTarif.ToString();
            tar_garbageTarif.Text = communalForMonth.GarbageTarif.ToString();
            tar_intercomTarif.Text = communalForMonth.IntercomTarif.ToString();
            tar_internetTarif.Text = communalForMonth.InternetTarif.ToString();

            //показ теперішнього тарифу в зміні тарифу
            currentTar_waterSupplyTarif.Text = communalForMonth.WaterSupplyTarif.ToString();
            currentTar_drainageTarif.Text = communalForMonth.DrainageTarif.ToString();
            currentTar_gasTarif.Text = communalForMonth.GasTarif.ToString();
            currentTar_deliveryGasTarif.Text = communalForMonth.DeliveryGasTarif.ToString();
            currentTar_electricTarif.Text = communalForMonth.ElectricTarif.ToString();
            currentTar_rentTarif.Text = communalForMonth.RentTarif.ToString();
            currentTar_protectionTarif.Text = communalForMonth.ProtectionTarif.ToString();
            currentTar_garbageTarif.Text = communalForMonth.GarbageTarif.ToString();
            currentTar_intercomTarif.Text = communalForMonth.IntercomTarif.ToString();
            currentTar_internetTarif.Text = communalForMonth.InternetTarif.ToString();

            //показ суми по тарифу
            deliveryGasSum.Text = communalForMonth.DeliveryGasTarif.ToString();
            rentSum.Text = communalForMonth.RentTarif.ToString();
            protectionSum.Text = communalForMonth.ProtectionTarif.ToString();
            garbageSum.Text = communalForMonth.GarbageTarif.ToString();
            intercomSum.Text = communalForMonth.IntercomTarif.ToString();
            internetSum.Text = communalForMonth.InternetTarif.ToString();
        }

        private void ChangeTarif_KeyDown(object sender, KeyEventArgs e)//для введення числа Double
        {
            if (e.Key == Key.NumPad0 || e.Key == Key.NumPad1 || e.Key == Key.NumPad2 || e.Key == Key.NumPad3 || e.Key == Key.NumPad4 || e.Key == Key.NumPad5 || e.Key == Key.NumPad6 || e.Key == Key.NumPad7 || e.Key == Key.NumPad8 || e.Key == Key.NumPad9 || e.Key == Key.Decimal) 
            { e.Handled = false; }
            else { e.Handled = true; }
        }

        private void ChangeIndicator_KeyDown(object sender, KeyEventArgs e)//для введення числа Int
        {
            if (e.Key == Key.NumPad0 || e.Key == Key.NumPad1 || e.Key == Key.NumPad2 || e.Key == Key.NumPad3 || e.Key == Key.NumPad4 || e.Key == Key.NumPad5 || e.Key == Key.NumPad6 || e.Key == Key.NumPad7 || e.Key == Key.NumPad8 || e.Key == Key.NumPad9)
            { e.Handled = false; }
            else { e.Handled = true; }
        }

        private void SaveTarif_MenuItem_Click(object sender, RoutedEventArgs e)//збереження зміненого тарифу
        {
            //перевірка на коректність чисел
            double res;
            bool error = false;
            if (double.TryParse(currentTar_waterSupplyTarif.Text, NumberStyles.Float, CultureInfo.CreateSpecificCulture("uk-UA"), out res)) { communalForMonth.WaterSupplyTarif = res; }
            else { currentTar_waterSupplyTarif.Text = "0"; error = true; }
            if (double.TryParse(currentTar_drainageTarif.Text, NumberStyles.Float, CultureInfo.CreateSpecificCulture("uk-UA"), out res)) { communalForMonth.DrainageTarif = res; }
            else { currentTar_drainageTarif.Text = "0"; error = true; }
            if (double.TryParse(currentTar_gasTarif.Text, NumberStyles.Float, CultureInfo.CreateSpecificCulture("uk-UA"), out res)) { communalForMonth.GasTarif = res; }
            else { currentTar_gasTarif.Text = "0"; error = true; }
            if (double.TryParse(currentTar_deliveryGasTarif.Text, NumberStyles.Float, CultureInfo.CreateSpecificCulture("uk-UA"), out res)) { communalForMonth.DeliveryGasTarif = res; }
            else { currentTar_deliveryGasTarif.Text = "0"; error = true; }
            if (double.TryParse(currentTar_electricTarif.Text, NumberStyles.Float, CultureInfo.CreateSpecificCulture("uk-UA"), out res)) { communalForMonth.ElectricTarif = res; }
            else { currentTar_electricTarif.Text = "0"; error = true; }
            if (double.TryParse(currentTar_rentTarif.Text, NumberStyles.Float, CultureInfo.CreateSpecificCulture("uk-UA"), out res)) { communalForMonth.RentTarif = res; }
            else { currentTar_rentTarif.Text = "0"; error = true; }
            if (double.TryParse(currentTar_protectionTarif.Text, NumberStyles.Float, CultureInfo.CreateSpecificCulture("uk-UA"), out res)) { communalForMonth.ProtectionTarif = res; }
            else { currentTar_protectionTarif.Text = "0"; error = true; }
            if (double.TryParse(currentTar_garbageTarif.Text, NumberStyles.Float, CultureInfo.CreateSpecificCulture("uk-UA"), out res)) { communalForMonth.GarbageTarif = res; }
            else { currentTar_garbageTarif.Text = "0"; error = true; }
            if (double.TryParse(currentTar_intercomTarif.Text, NumberStyles.Float, CultureInfo.CreateSpecificCulture("uk-UA"), out res)) { communalForMonth.IntercomTarif = res; }
            else { currentTar_intercomTarif.Text = "0"; error = true; }
            if (double.TryParse(currentTar_internetTarif.Text, NumberStyles.Float, CultureInfo.CreateSpecificCulture("uk-UA"), out res)) { communalForMonth.InternetTarif = res; }
            else { currentTar_internetTarif.Text = "0"; error = true; }

            if (error) { MessageBox.Show($"Перевірте правильність введення суми тарифу!!!", "Помилка введення числа!", MessageBoxButton.OK, MessageBoxImage.Error); return; }

            //збереження
            ShowLastComunalTarif();
        }

        private void ToCount_Button_Click(object sender, RoutedEventArgs e)//кнопка розрахувати в Розрахунок
        {
            //Водопостачання
            if (Int32.TryParse(prevWaterSupply.Text, out int res1) && Int32.TryParse(curentWaterSupply.Text, out int res2))
            {
                communalForMonth.PrevWaterSupply = res1;
                communalForMonth.CurentWaterSupply = res2;
                if (communalForMonth.CurentWaterSupply < communalForMonth.PrevWaterSupply) { communalForMonth.CurentWaterSupply = communalForMonth.PrevWaterSupply; curentWaterSupply.Text = communalForMonth.CurentWaterSupply.ToString(); }
                communalForMonth.SumWaterSupply = (communalForMonth.CurentWaterSupply - communalForMonth.PrevWaterSupply) * communalForMonth.WaterSupplyTarif;
                different_WaterSupply.Text = (communalForMonth.CurentWaterSupply - communalForMonth.PrevWaterSupply).ToString();
                sum_WaterSupply.Text = communalForMonth.SumWaterSupply.ToString();
            }
            else { communalForMonth.SumWaterSupply = 0; }

            //Водовідведення
            if (Int32.TryParse(prevDrainage.Text, out res1) && Int32.TryParse(curentDrainage.Text, out res2))
            {
                communalForMonth.PrevDrainage = res1;
                communalForMonth.CurentDrainage = res2;
                if (communalForMonth.CurentDrainage < communalForMonth.PrevDrainage) { communalForMonth.CurentDrainage = communalForMonth.PrevDrainage; curentDrainage.Text = communalForMonth.CurentDrainage.ToString(); }
                communalForMonth.SumDrainage = (communalForMonth.CurentDrainage - communalForMonth.PrevDrainage) * communalForMonth.DrainageTarif;
                different_Drainage.Text = (communalForMonth.CurentDrainage - communalForMonth.PrevDrainage).ToString();
                sum_Drainage.Text = communalForMonth.SumDrainage.ToString();
            }
            else { communalForMonth.SumDrainage = 0; }

            //Природний газ
            if (Int32.TryParse(prevGas.Text, out res1) && Int32.TryParse(curentGas.Text, out res2))
            {
                communalForMonth.PrevGas = res1;
                communalForMonth.CurentGas = res2;
                if(communalForMonth.CurentGas< communalForMonth.PrevGas) { communalForMonth.CurentGas = communalForMonth.PrevGas; curentGas.Text = communalForMonth.CurentGas.ToString(); }
                communalForMonth.SumGas = (communalForMonth.CurentGas - communalForMonth.PrevGas) * communalForMonth.GasTarif;
                different_Gas.Text = (communalForMonth.CurentGas - communalForMonth.PrevGas).ToString();
                sum_Gas.Text = communalForMonth.SumGas.ToString();
            }
            else { communalForMonth.SumGas = 0; }

            //Доставка газу
            if (double.TryParse(deliveryGasSum.Text, NumberStyles.Float, CultureInfo.CreateSpecificCulture("uk-UA"), out double res)) { communalForMonth.SumDeliveryGas = res; sum_DeliveryGas.Text = communalForMonth.SumDeliveryGas.ToString(); }
            else { deliveryGasSum.Text = communalForMonth.DeliveryGasTarif.ToString(); }

            //Електроенергія
            if (Int32.TryParse(prevElectric.Text, out res1) && Int32.TryParse(curentElectric.Text, out res2))
            {
                communalForMonth.PrevElectric = res1;
                communalForMonth.CurentElectric = res2;
                if (communalForMonth.CurentElectric < communalForMonth.PrevElectric) { communalForMonth.CurentElectric = communalForMonth.PrevElectric; curentElectric.Text = communalForMonth.CurentElectric.ToString(); }
                communalForMonth.SumElectric = (communalForMonth.CurentElectric - communalForMonth.PrevElectric) * communalForMonth.ElectricTarif;
                different_Electric.Text = (communalForMonth.CurentElectric - communalForMonth.PrevElectric).ToString();
                sum_Electric.Text = communalForMonth.SumElectric.ToString();
            }
            else { communalForMonth.SumElectric = 0; }

            //Квартплата
            if (double.TryParse(rentSum.Text, NumberStyles.Float, CultureInfo.CreateSpecificCulture("uk-UA"), out res)) { communalForMonth.SumRent = res; sum_Rent.Text = communalForMonth.SumRent.ToString(); }
            else { rentSum.Text = communalForMonth.RentTarif.ToString(); }

            //Охорона
            if (double.TryParse(protectionSum.Text, NumberStyles.Float, CultureInfo.CreateSpecificCulture("uk-UA"), out res)) { communalForMonth.SumProtection = res; sum_Protection.Text = communalForMonth.SumProtection.ToString(); }
            else { protectionSum.Text = communalForMonth.ProtectionTarif.ToString(); }

            //Вивіз сміття
            if (double.TryParse(garbageSum.Text, NumberStyles.Float, CultureInfo.CreateSpecificCulture("uk-UA"), out res)) { communalForMonth.SumGarbage = res; sum_Garbage.Text = communalForMonth.SumGarbage.ToString(); }
            else { garbageSum.Text = communalForMonth.GarbageTarif.ToString(); }

            //Домофон
            if (double.TryParse(intercomSum.Text, NumberStyles.Float, CultureInfo.CreateSpecificCulture("uk-UA"), out res)) { communalForMonth.SumIntercom = res; sum_Intercom.Text = communalForMonth.SumIntercom.ToString(); }
            else { intercomSum.Text = communalForMonth.IntercomTarif.ToString(); }

            //Інтернет
            if (double.TryParse(internetSum.Text, NumberStyles.Float, CultureInfo.CreateSpecificCulture("uk-UA"), out res)) { communalForMonth.SumInternet = res; sum_Internet.Text = communalForMonth.SumInternet.ToString(); }
            else { internetSum.Text = communalForMonth.InternetTarif.ToString(); }

            //Загальна сума
            sum_Total.Text = communalForMonth.TotalSum.ToString();

            if (curentWaterSupply.Text.Length!=0 && curentDrainage.Text.Length!=0 && curentGas.Text.Length!=0 && deliveryGasSum.Text.Length!=0 && curentElectric.Text.Length!=0
                && rentSum.Text.Length!=0 && protectionSum.Text.Length!=0 && garbageSum.Text.Length!=0 && intercomSum.Text.Length!=0 && internetSum.Text.Length != 0) 
            {
                saveCount_Button.IsEnabled = true;
            }
        }

        private void SaveCount_Button_Click(object sender, RoutedEventArgs e)//зберегти розрахунок в Розрахунка
        {
            if (curentWaterSupply.Text.Length == 0 || curentDrainage.Text.Length == 0 || curentGas.Text.Length == 0 || deliveryGasSum.Text.Length == 0 || curentElectric.Text.Length == 0
                || rentSum.Text.Length == 0 || protectionSum.Text.Length == 0 || garbageSum.Text.Length == 0 || intercomSum.Text.Length == 0 || internetSum.Text.Length == 0)
            {
                saveCount_Button.IsEnabled = false;
                MessageBox.Show("Дані не збережено!\nЗаповніть всі поля!", "Збереження даних", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            if(comboBoxMonth.Text == "Виберіть місяць" || comboBoxYear.Text== "Виберіть рік")
            {
                MessageBox.Show("Дані не збережено!\nВиберіть період!", "Збереження даних", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            for (int i = 0; i < listUsers[num].listCommunal.Count; i++)
            {
                if (listUsers[num].listCommunal[i].Year == comboBoxYear.Text)
                {
                    if (listUsers[num].listCommunal[i].Month == comboBoxMonth.Text)
                    {
                        MessageBox.Show("Дані не збережено!\nРозрахунок по даному періоду вже існує!!", "Збереження даних", MessageBoxButton.OK, MessageBoxImage.Information);
                        return;
                    }
                }
            }
            MessageBoxResult result = MessageBox.Show("Бажаєте зберегти дані!", "Збереження даних", MessageBoxButton.YesNo,MessageBoxImage.Question);
            if(result == MessageBoxResult.Yes)
            {
                listUsers[num].listCommunal.Add(communalForMonth);
                listUsers[num].address = address_TextBox.Text;
                file.SaveData(listUsers);
                this.Opacity = 0.25;
                MessageBox.Show("Дані збережено!", "Збереження даних", MessageBoxButton.OK, MessageBoxImage.Information);
                MainWindow mainWindow = new MainWindow();
                mainWindow.Show();
                this.Opacity = 1;
                Close();
            }
        }

        private void DeleteCount_ButtonClick(object sender, RoutedEventArgs e)//видалити розрахунок в Розрахунках
        {
            if (listUsers[num].listCommunal.Count == 0)
            {
                this.Opacity = 0.25;
                MessageBox.Show("Дані відсутні!", "Видалення даних", MessageBoxButton.OK, MessageBoxImage.Information);
                this.Opacity = 1;
                return;
            }
            this.Opacity = 0.25;
            MessageBoxResult result = MessageBox.Show("Ви дійсно бажаєте видалити дані по останньому розрахунку?!", "Видалення даних", MessageBoxButton.YesNo, MessageBoxImage.Question);
            this.Opacity = 1;
            if (result == MessageBoxResult.Yes)
            {
                listUsers[num].listCommunal.RemoveAt(listUsers[num].listCommunal.Count-1);
                file.SaveData(listUsers);
                this.Opacity = 0.25;
                MessageBox.Show("Останні дані видалено!", "Видалення даних", MessageBoxButton.OK, MessageBoxImage.Information);
                MainWindow mainWindow = new MainWindow();
                mainWindow.Show();
                this.Opacity = 1;
                Close();
            }
        }

        private ComboBox CreateMonths()//створення ComboBox місяців
        {
            ComboBox comboBox = new ComboBox()
            {
                Width = 200,
                FontSize = 16,
                Text = "Виберіть місяць",
                Background = Brushes.Beige,
                Foreground = Brushes.DarkCyan,
                FontWeight = FontWeights.Bold,
                FontFamily = new FontFamily("Segoe Script"),
                FontStyle = FontStyles.Italic,
                IsEditable = true,
                BorderBrush = Brushes.DarkCyan,
                BorderThickness = new Thickness(2),
                HorizontalAlignment = HorizontalAlignment.Left,
                Margin = new Thickness(5,10,5,10),
            };
            comboBox.Items.Add(new ComboBoxItem() { Content = "січень", Background = Brushes.Beige });
            comboBox.Items.Add(new ComboBoxItem() { Content = "лютий", Background = Brushes.Beige } );
            comboBox.Items.Add(new ComboBoxItem() { Content = "березень", Background = Brushes.Beige });
            comboBox.Items.Add(new ComboBoxItem() { Content = "квітень", Background = Brushes.Beige });
            comboBox.Items.Add(new ComboBoxItem() { Content = "травень", Background = Brushes.Beige });
            comboBox.Items.Add(new ComboBoxItem() { Content = "червень", Background = Brushes.Beige });
            comboBox.Items.Add(new ComboBoxItem() { Content = "липень", Background = Brushes.Beige });
            comboBox.Items.Add(new ComboBoxItem() { Content = "серпень", Background = Brushes.Beige });
            comboBox.Items.Add(new ComboBoxItem() { Content = "вересень", Background = Brushes.Beige });
            comboBox.Items.Add(new ComboBoxItem() { Content = "жовтень", Background = Brushes.Beige });
            comboBox.Items.Add(new ComboBoxItem() { Content = "грудень", Background = Brushes.Beige });
            return comboBox;
        }

        private ComboBox CreateYears()//створення ComboBox років
        {
            ComboBox comboBox = new ComboBox()
            {
                Width = 200,
                FontSize = 16,
                Text = "Виберіть рік",
                Background = Brushes.Beige,
                Foreground = Brushes.DarkCyan,
                FontWeight = FontWeights.Bold,
                FontFamily = new FontFamily("Segoe Script"),
                FontStyle = FontStyles.Italic,
                IsEditable = true,
                BorderBrush = Brushes.DarkCyan,
                BorderThickness = new Thickness(2),
                HorizontalAlignment = HorizontalAlignment.Left,
                Margin = new Thickness(10),
            };
            for (int i = 2021; i < 2080; i++)
            {
                var item = new ComboBoxItem { Content = i, Background = Brushes.Beige };
                comboBox.Items.Add(item);
            }
            return comboBox;
        }

        private void LastCount_RadioButton_Checked(object sender, RoutedEventArgs e)//RadioButton вибір Остання
        {
            //перевірка на коректність даних
            foreach (var item in listUsers[num].listCommunal) { if (item.Month == null || item.Year == null) return; }
            if (listUsers[num].listCommunal.Count == 0) return;
            //
            for (int i = gridCommunal.Children.Count; i > 5; i--) { gridCommunal.Children.RemoveAt(i - 1); }
            showCount_Button.Visibility = Visibility.Hidden;
            if(flowDoc.Document!=null)flowDoc.Document.Blocks.Clear();
            totalSumForChoosePeriod = listUsers[num].listCommunal.Last().TotalSum;
            flowDoc.Document.Blocks.Add(TotalSumFlowDocument());
            flowDoc.Document.Blocks.Add(CreateFlowDocument_ShowMonth(listUsers[num].listCommunal.Count-1));
        }

        private void FirstCount_RadioButton_Checked(object sender, RoutedEventArgs e)//RadioButton вибір Перша
        {
            //перевірка на коректність даних
            foreach (var item in listUsers[num].listCommunal){ if (item.Month == null || item.Year == null) return; }
            if (listUsers[num].listCommunal.Count == 0) return;
            //
            for (int i = gridCommunal.Children.Count; i > 5; i--) { gridCommunal.Children.RemoveAt(i - 1); }
            showCount_Button.Visibility = Visibility.Hidden;
            if (flowDoc.Document != null) flowDoc.Document.Blocks.Clear();
            totalSumForChoosePeriod = listUsers[num].listCommunal[0].TotalSum;
            flowDoc.Document.Blocks.Add(TotalSumFlowDocument());
            flowDoc.Document.Blocks.Add(CreateFlowDocument_ShowMonth(0));
        }

        private void MonthCount_RadioButton_Checked(object sender, RoutedEventArgs e)//RadioButton вибір За місяць
        {
            searchOfPeriod = false;
            for (int i = 0; i < period.Length; i++) {period[i] = ""; }
            //перевірка на коректність даних
            foreach (var item in listUsers[num].listCommunal) { if (item.Month == null || item.Year == null) return; }
            if (listUsers[num].listCommunal.Count == 0) return;
            //
            if (flowDoc.Document != null) flowDoc.Document.Blocks.Clear();
            for (int i = gridCommunal.Children.Count; i > 5; i--){ gridCommunal.Children.RemoveAt(i-1);}
            //створення ComboBox місяців
            tmpComboBoxMonth_1.Visibility = Visibility;
            tmpComboBoxMonth_1 = CreateMonths();
            tmpComboBoxMonth_1.SelectionChanged += new SelectionChangedEventHandler(tmpComboBoxMonth_1_SelectionChanged);
            Grid.SetColumn(tmpComboBoxMonth_1, 3);
            Grid.SetRow(tmpComboBoxMonth_1, 0);
            gridCommunal.Children.Add(tmpComboBoxMonth_1);
            //створення ComboBox років
            tmpComboBoxYear_1.Visibility = Visibility;
            tmpComboBoxYear_1 = CreateYears();
            tmpComboBoxYear_1.SelectionChanged += new SelectionChangedEventHandler(tmpComboBoxYear_1_SelectionChanged);
            Grid.SetColumn(tmpComboBoxYear_1, 4);
            Grid.SetRow(tmpComboBoxYear_1, 0);
            gridCommunal.Children.Add(tmpComboBoxYear_1);

            showCount_Button.Visibility = Visibility;
        }

        private void PeriodCount_RadioButton_Checked(object sender, RoutedEventArgs e)//RadioButton вибір За період
        {
            searchOfPeriod = true;
            for (int i = 0; i < period.Length; i++) { period[i] = ""; }
            //перевірка на коректність даних
            foreach (var item in listUsers[num].listCommunal) { if (item.Month == null || item.Year == null) return; }
            if (listUsers[num].listCommunal.Count == 0) return;
            //
            if (flowDoc.Document != null) flowDoc.Document.Blocks.Clear();
            for (int i = gridCommunal.Children.Count; i > 5; i--) { gridCommunal.Children.RemoveAt(i - 1); }
            //створення ComboBox місяців - з
            Label label1 = new Label() { Content = "з", FontSize = 20, FontWeight = FontWeights.Bold, Margin = new Thickness(0, 10, 0, 10) };
            Grid.SetColumn(label1, 2);
            Grid.SetRow(label1, 0);
            tmpComboBoxMonth_1.Visibility = Visibility;
            tmpComboBoxMonth_1 = CreateMonths();
            tmpComboBoxMonth_1.SelectionChanged += new SelectionChangedEventHandler(tmpComboBoxMonth_1_SelectionChanged);
            Grid.SetColumn(tmpComboBoxMonth_1, 3);
            Grid.SetRow(tmpComboBoxMonth_1, 0);
            gridCommunal.Children.Add(tmpComboBoxMonth_1);

            //створення ComboBox років - з
            tmpComboBoxYear_1.Visibility = Visibility;
            tmpComboBoxYear_1 = CreateYears();
            tmpComboBoxYear_1.SelectionChanged += new SelectionChangedEventHandler(tmpComboBoxYear_1_SelectionChanged);
            Grid.SetColumn(tmpComboBoxYear_1, 4);
            Grid.SetRow(tmpComboBoxYear_1, 0);
            gridCommunal.Children.Add(tmpComboBoxYear_1);

            //створення ComboBox місяців - по
            Label label2 = new Label() { Content = "по", FontSize = 20, FontWeight = FontWeights.Bold, Margin = new Thickness(0, 10, 0, 10) };
            Grid.SetColumn(label2, 2);
            Grid.SetRow(label2, 1);
            tmpComboBoxMonth_2.Visibility = Visibility;
            tmpComboBoxMonth_2 = CreateMonths();
            tmpComboBoxMonth_2.SelectionChanged += new SelectionChangedEventHandler(tmpComboBoxMonth_2_SelectionChanged);
            Grid.SetColumn(tmpComboBoxMonth_2, 3);
            Grid.SetRow(tmpComboBoxMonth_2, 1);
            gridCommunal.Children.Add(tmpComboBoxMonth_2);

            //створення ComboBox років - по
            tmpComboBoxYear_2.Visibility = Visibility;
            tmpComboBoxYear_2 = CreateYears();
            tmpComboBoxYear_2.SelectionChanged += new SelectionChangedEventHandler(tmpComboBoxYear_2_SelectionChanged);
            Grid.SetColumn(tmpComboBoxYear_2, 4);
            Grid.SetRow(tmpComboBoxYear_2, 1);
            gridCommunal.Children.Add(tmpComboBoxYear_2);

            gridCommunal.Children.Add(label1);
            gridCommunal.Children.Add(label2);

            showCount_Button.Visibility = Visibility;
        }
   
        private Table CreateFlowDocument_ShowMonth(int numCom)//створення Table до FlowDocument для показу вибраного періоду в Подивитись Оплати
        {
            var backgroundBrush1 = new LinearGradientBrush
            {
                EndPoint = new Point(x: 0.0, y: 0.0),
                StartPoint = new Point(x: 0.0, y: 1.05)
            };
            backgroundBrush1.GradientStops.Add( new GradientStop(Colors.DarkGray, offset: 1));
            backgroundBrush1.GradientStops.Add(new GradientStop(Colors.Black, offset: 0.85));
            var backgroundBrush2 = new LinearGradientBrush
            {
                EndPoint = new Point(x: 0.0, y: 0.0),
                StartPoint = new Point(x: 0.0, y: 1.05)
            };
            backgroundBrush2.GradientStops.Add(new GradientStop(Colors.DarkGray, offset: 1));
            backgroundBrush2.GradientStops.Add(new GradientStop(Colors.DarkSlateGray, offset: 0.85));

            try
            {
                var cell_1 = new TableCell(new Paragraph(new
               Run($"За {listUsers[num].listCommunal[numCom].Month.Replace("System.Windows.Controls.ComboBoxItem: ", "")} {listUsers[num].listCommunal[numCom].Year.Replace("System.Windows.Controls.ComboBoxItem: ", "")} р.")))
                {
                    FontSize = 22,
                    FontWeight = FontWeights.Bold,
                    ColumnSpan = 2,
                    Foreground = Brushes.LightGreen,
                    TextAlignment = TextAlignment.Left,
                    Background = backgroundBrush1
                };

                var row1 = new TableRow();
                row1.Cells.Add(cell_1);
                var row2 = new TableRow() { Foreground = Brushes.Yellow, Background = backgroundBrush1 };
                row2.Cells.Add(new TableCell(new Paragraph(new Run("Послуга"))));
                row2.Cells.Add(new TableCell(new Paragraph(new Run("Тариф"))));
                row2.Cells.Add(new TableCell(new Paragraph(new Run("Попередній показник"))));
                row2.Cells.Add(new TableCell(new Paragraph(new Run("Теперішній показник"))));
                row2.Cells.Add(new TableCell(new Paragraph(new Run("Різниця"))));
                row2.Cells.Add(new TableCell(new Paragraph(new Run("Сума"))));
                row2.Cells.Add(new TableCell(new Paragraph(new Run())));
                row2.Cells.Add(new TableCell(new Paragraph(new Run())));
                var row3 = new TableRow() { Background = backgroundBrush2, Foreground = Brushes.Bisque };
                row3.Cells.Add(new TableCell(new Paragraph(new Run("Водопостачання"))));
                row3.Cells.Add(new TableCell(new Paragraph(new Run($"{listUsers[num].listCommunal[numCom].WaterSupplyTarif}    грн/м3"))));
                row3.Cells.Add(new TableCell(new Paragraph(new Run($"{listUsers[num].listCommunal[numCom].PrevWaterSupply}"))));
                row3.Cells.Add(new TableCell(new Paragraph(new Run($"{listUsers[num].listCommunal[numCom].CurentWaterSupply}"))));
                row3.Cells.Add(new TableCell(new Paragraph(new Run($"{(listUsers[num].listCommunal[numCom].CurentWaterSupply - listUsers[num].listCommunal[numCom].PrevWaterSupply).ToString()}     м3"))));
                row3.Cells.Add(new TableCell(new Paragraph(new Run($"{listUsers[num].listCommunal[numCom].SumWaterSupply} грн"))));
                var row4 = new TableRow() { Background = backgroundBrush2, Foreground = Brushes.Bisque };
                row4.Cells.Add(new TableCell(new Paragraph(new Run("Водовідведення"))));
                row4.Cells.Add(new TableCell(new Paragraph(new Run($"{listUsers[num].listCommunal[numCom].DrainageTarif}    грн/м3"))));
                row4.Cells.Add(new TableCell(new Paragraph(new Run($"{listUsers[num].listCommunal[numCom].PrevDrainage}"))));
                row4.Cells.Add(new TableCell(new Paragraph(new Run($"{listUsers[num].listCommunal[numCom].CurentDrainage}"))));
                row4.Cells.Add(new TableCell(new Paragraph(new Run($"{(listUsers[num].listCommunal[numCom].CurentDrainage - listUsers[num].listCommunal[numCom].PrevDrainage).ToString()}     м3"))));
                row4.Cells.Add(new TableCell(new Paragraph(new Run($"{listUsers[num].listCommunal[numCom].SumDrainage} грн"))));
                var row5 = new TableRow() { Background = backgroundBrush2, Foreground = Brushes.Bisque };
                row5.Cells.Add(new TableCell(new Paragraph(new Run("Природний газ"))));
                row5.Cells.Add(new TableCell(new Paragraph(new Run($"{listUsers[num].listCommunal[numCom].GasTarif}    грн/м3"))));
                row5.Cells.Add(new TableCell(new Paragraph(new Run($"{listUsers[num].listCommunal[numCom].PrevGas}"))));
                row5.Cells.Add(new TableCell(new Paragraph(new Run($"{listUsers[num].listCommunal[numCom].CurentGas}"))));
                row5.Cells.Add(new TableCell(new Paragraph(new Run($"{(listUsers[num].listCommunal[numCom].CurentGas - listUsers[num].listCommunal[numCom].PrevGas).ToString()}     м3"))));
                row5.Cells.Add(new TableCell(new Paragraph(new Run($"{listUsers[num].listCommunal[numCom].SumGas} грн"))));
                var row6 = new TableRow() { Background = backgroundBrush2, Foreground = Brushes.Bisque };
                row6.Cells.Add(new TableCell(new Paragraph(new Run("Електроенергія"))));
                row6.Cells.Add(new TableCell(new Paragraph(new Run($"{listUsers[num].listCommunal[numCom].ElectricTarif} грн/кВат"))));
                row6.Cells.Add(new TableCell(new Paragraph(new Run($"{listUsers[num].listCommunal[numCom].PrevElectric}"))));
                row6.Cells.Add(new TableCell(new Paragraph(new Run($"{listUsers[num].listCommunal[numCom].CurentElectric}"))));
                row6.Cells.Add(new TableCell(new Paragraph(new Run($"{(listUsers[num].listCommunal[numCom].CurentElectric - listUsers[num].listCommunal[numCom].PrevElectric).ToString()} кВат"))));
                row6.Cells.Add(new TableCell(new Paragraph(new Run($"{listUsers[num].listCommunal[numCom].SumElectric} грн"))));
                var row7 = new TableRow() { Background = backgroundBrush2, Foreground = Brushes.Bisque };
                row7.Cells.Add(new TableCell(new Paragraph(new Run("Доставка газу"))));
                row7.Cells.Add(new TableCell(new Paragraph(new Run($"{listUsers[num].listCommunal[numCom].DeliveryGasTarif} грн/м-ць"))));
                row7.Cells.Add(new TableCell(new Paragraph(new Run("---"))));
                row7.Cells.Add(new TableCell(new Paragraph(new Run("---"))));
                row7.Cells.Add(new TableCell(new Paragraph(new Run("---"))));
                row7.Cells.Add(new TableCell(new Paragraph(new Run($"{listUsers[num].listCommunal[numCom].SumDeliveryGas} грн"))));
                var row8 = new TableRow() { Background = backgroundBrush2, Foreground = Brushes.Bisque };
                row8.Cells.Add(new TableCell(new Paragraph(new Run("Квартплата"))));
                row8.Cells.Add(new TableCell(new Paragraph(new Run($"{listUsers[num].listCommunal[numCom].RentTarif} грн/м-ць"))));
                row8.Cells.Add(new TableCell(new Paragraph(new Run("---"))));
                row8.Cells.Add(new TableCell(new Paragraph(new Run("---"))));
                row8.Cells.Add(new TableCell(new Paragraph(new Run("---"))));
                row8.Cells.Add(new TableCell(new Paragraph(new Run($"{listUsers[num].listCommunal[numCom].SumRent} грн"))));
                var row9 = new TableRow() { Background = backgroundBrush2, Foreground = Brushes.Bisque };
                row9.Cells.Add(new TableCell(new Paragraph(new Run("Охорона"))));
                row9.Cells.Add(new TableCell(new Paragraph(new Run($"{listUsers[num].listCommunal[numCom].ProtectionTarif} грн/м-ць"))));
                row9.Cells.Add(new TableCell(new Paragraph(new Run("---"))));
                row9.Cells.Add(new TableCell(new Paragraph(new Run("---"))));
                row9.Cells.Add(new TableCell(new Paragraph(new Run("---"))));
                row9.Cells.Add(new TableCell(new Paragraph(new Run($"{listUsers[num].listCommunal[numCom].SumProtection} грн"))));
                var row10 = new TableRow() { Background = backgroundBrush2, Foreground = Brushes.Bisque };
                row10.Cells.Add(new TableCell(new Paragraph(new Run("Вивіз сміття"))));
                row10.Cells.Add(new TableCell(new Paragraph(new Run($"{listUsers[num].listCommunal[numCom].GarbageTarif} грн/м-ць"))));
                row10.Cells.Add(new TableCell(new Paragraph(new Run("---"))));
                row10.Cells.Add(new TableCell(new Paragraph(new Run("---"))));
                row10.Cells.Add(new TableCell(new Paragraph(new Run("---"))));
                row10.Cells.Add(new TableCell(new Paragraph(new Run($"{listUsers[num].listCommunal[numCom].SumGarbage} грн"))));
                var row11 = new TableRow() { Background = backgroundBrush2, Foreground = Brushes.Bisque };
                row11.Cells.Add(new TableCell(new Paragraph(new Run("Домофон"))));
                row11.Cells.Add(new TableCell(new Paragraph(new Run($"{listUsers[num].listCommunal[numCom].IntercomTarif} грн/м-ць"))));
                row11.Cells.Add(new TableCell(new Paragraph(new Run("---"))));
                row11.Cells.Add(new TableCell(new Paragraph(new Run("---"))));
                row11.Cells.Add(new TableCell(new Paragraph(new Run("---"))));
                row11.Cells.Add(new TableCell(new Paragraph(new Run($"{listUsers[num].listCommunal[numCom].SumIntercom} грн"))));
                var row12 = new TableRow() { Background = backgroundBrush2, Foreground = Brushes.Bisque };
                row12.Cells.Add(new TableCell(new Paragraph(new Run("Інтернет"))));
                row12.Cells.Add(new TableCell(new Paragraph(new Run($"{listUsers[num].listCommunal[numCom].InternetTarif} грн/м-ць"))));
                row12.Cells.Add(new TableCell(new Paragraph(new Run("---"))));
                row12.Cells.Add(new TableCell(new Paragraph(new Run("---"))));
                row12.Cells.Add(new TableCell(new Paragraph(new Run("---"))));
                row12.Cells.Add(new TableCell(new Paragraph(new Run($"{listUsers[num].listCommunal[numCom].SumInternet} грн"))));
                var row13 = new TableRow() { Background = backgroundBrush1, Foreground = Brushes.Gold };
                row13.Cells.Add(new TableCell(new Paragraph(new Run("Всього:"))));
                row13.Cells.Add(new TableCell(new Paragraph(new Run("   "))));
                row13.Cells.Add(new TableCell(new Paragraph(new Run("   "))));
                row13.Cells.Add(new TableCell(new Paragraph(new Run("   "))));
                row13.Cells.Add(new TableCell(new Paragraph(new Run("   "))));
                row13.Cells.Add(new TableCell(new Paragraph(new Run($"{listUsers[num].listCommunal[numCom].TotalSum} грн"))));

                var rowGroup1 = new TableRowGroup();
                rowGroup1.Rows.Add(row1);
                rowGroup1.Rows.Add(row2);
                rowGroup1.Rows.Add(row3);
                rowGroup1.Rows.Add(row4);
                rowGroup1.Rows.Add(row5);
                rowGroup1.Rows.Add(row6);
                rowGroup1.Rows.Add(row7);
                rowGroup1.Rows.Add(row8);
                rowGroup1.Rows.Add(row9);
                rowGroup1.Rows.Add(row10);
                rowGroup1.Rows.Add(row11);
                rowGroup1.Rows.Add(row12);
                rowGroup1.Rows.Add(row13);

                var table = new Table()
                {
                    TextAlignment = TextAlignment.Right,
                    FontSize = 20
                };
                table.RowGroups.Add(rowGroup1);

                return table;
            }
            catch (Exception)
            {
                if (flowDoc.Document != null) flowDoc.Document.Blocks.Clear();
                MessageBox.Show("Невірний пеіод, або по даному періоду немає даних!", "Інформація", MessageBoxButton.OK, MessageBoxImage.Information);
                return new Table();
            }
           
        }

        private Table TotalSumFlowDocument() // Шапочка "Загальна сума по вибраному періоду" до FlowDocument
        {
            var backgroundBrush1 = new LinearGradientBrush
            {
                EndPoint = new Point(x: 0.0, y: 0.0),
                StartPoint = new Point(x: 0.0, y: 1.05)
            };
            backgroundBrush1.GradientStops.Add(new GradientStop(Colors.DarkGray, offset: 1));
            backgroundBrush1.GradientStops.Add(new GradientStop(Colors.Black, offset: 0.85));
            var cell_0 = new TableCell(new Paragraph(new Run($"Адрес:  {address_TextBox.Text}")))
            {
                FontSize = 20,
                FontWeight = FontWeights.Bold,
                ColumnSpan = 6,
                Foreground = Brushes.DarkCyan,
                TextAlignment = TextAlignment.Left,
                Background = backgroundBrush1,
                FontFamily = new FontFamily("Segoe Script"),
                FontStyle = FontStyles.Italic
            };

            var cell_1 = new TableCell(new Paragraph(new Run($"Загальна сума по вибраному періоду:        {totalSumForChoosePeriod} грн.")))
            {
                FontSize = 30,
                FontWeight = FontWeights.Bold,
                ColumnSpan = 6,
                Foreground = Brushes.DarkOrange,
                TextAlignment = TextAlignment.Left,
                Background = backgroundBrush1,
                FontFamily = new FontFamily("Segoe Script"),
                FontStyle = FontStyles.Italic
            };
            var row0 = new TableRow();
            row0.Cells.Add(cell_0);
            var row1 = new TableRow();
            row1.Cells.Add(cell_1);
            var rowGroup = new TableRowGroup();
            rowGroup.Rows.Add(row0);
            rowGroup.Rows.Add(row1);
            var table = new Table()
            {
                TextAlignment = TextAlignment.Right,
                FontSize = 20
            };
            table.RowGroups.Add(rowGroup);

            return table;
        }

        private void tmpComboBoxMonth_1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            period[0] = tmpComboBoxMonth_1.SelectedItem.ToString().Replace("System.Windows.Controls.ComboBoxItem: ", "");
        }

        private void tmpComboBoxYear_1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            period[1] = tmpComboBoxYear_1.SelectedItem.ToString().Replace("System.Windows.Controls.ComboBoxItem: ", "");
        }

        private void tmpComboBoxMonth_2_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            period[2] = tmpComboBoxMonth_2.SelectedItem.ToString().Replace("System.Windows.Controls.ComboBoxItem: ", "");
        }

        private void tmpComboBoxYear_2_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            period[3] = tmpComboBoxYear_2.SelectedItem.ToString().Replace("System.Windows.Controls.ComboBoxItem: ", "");
        }

        private void showCount_Button_Click(object sender, RoutedEventArgs e)
        {
            totalSumForChoosePeriod = 0;
            //перевірка на коректність даних
            foreach (var item in listUsers[num].listCommunal) { if (item.Month == null || item.Year == null) return; }
            if (listUsers[num].listCommunal.Count == 0) return;
            // очищаєм FlowDocument
            if (flowDoc.Document != null) flowDoc.Document.Blocks.Clear();
            //
            if (searchOfPeriod)
            {
                if (period[0] == "" || period[1] == "" || period[2] == "" || period[3] == "") { MessageBox.Show("Виберіть період", "Інформація", MessageBoxButton.OK, MessageBoxImage.Information); return; }
                else
                {
                    int startPoint=-1, endPoint=-1;
                    for (int i = 0; i < listUsers[num].listCommunal.Count; i++)
                    {
                        if (listUsers[num].listCommunal[i].Month == period[0] && listUsers[num].listCommunal[i].Year == period[1])
                            startPoint = i;
                    }
                    for (int i = 0; i < listUsers[num].listCommunal.Count; i++)
                    {
                        if (listUsers[num].listCommunal[i].Month == period[2] && listUsers[num].listCommunal[i].Year == period[3])
                            endPoint = i;
                    }
                    if (endPoint < startPoint || startPoint == -1 || endPoint == -1) { MessageBox.Show("Невірний діапазон періоду!!!", "Інформація", MessageBoxButton.OK, MessageBoxImage.Information); return; }
                    else
                    {
                        for (int i = startPoint; i <= endPoint; i++) { totalSumForChoosePeriod += listUsers[num].listCommunal[i].TotalSum; }
                        flowDoc.Document.Blocks.Add(TotalSumFlowDocument());
                        for (int i = startPoint; i <= endPoint; i++)
                        {
                            flowDoc.Document.Blocks.Add(CreateFlowDocument_ShowMonth(i));
                        }
                    }
                    return;
                }
            }
            else
            {
                if (period[0] == "" || period[1] == "") { MessageBox.Show("Виберіть період", "Інформація", MessageBoxButton.OK, MessageBoxImage.Information); return; }
                else
                {
                    for (int i = 0; i < listUsers[num].listCommunal.Count; i++)
                    {
                        if (listUsers[num].listCommunal[i].Month == period[0] && listUsers[num].listCommunal[i].Year == period[1])
                        { totalSumForChoosePeriod = listUsers[num].listCommunal[i].TotalSum;}
                    }
                    flowDoc.Document.Blocks.Add(TotalSumFlowDocument());
                    for (int i = 0; i < listUsers[num].listCommunal.Count; i++)
                    {
                        if (listUsers[num].listCommunal[i].Month == period[0] && listUsers[num].listCommunal[i].Year == period[1])
                        { flowDoc.Document.Blocks.Add(CreateFlowDocument_ShowMonth(i)); }
                    }
                    return;
                }
            }
        }

        ///Фільтр пошуку в подивитись споживання
        private bool UserFilter(object item)
        {
            if (String.IsNullOrEmpty(txtFilter.Text))
                return true;
            else
                return ((item as Communal).Period.IndexOf(txtFilter.Text, StringComparison.OrdinalIgnoreCase) >= 0);
        }
        private void txtFilter_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            CollectionViewSource.GetDefaultView(lvUsers.ItemsSource).Refresh();
        }
        ///
        private void Calc_Button_Cklick(object sender, RoutedEventArgs e)
        {
            CalculatorWindow calculatorWindow = new CalculatorWindow();
            calculatorWindow.Show();
        }

        private void save_Button_Click(object sender, RoutedEventArgs e)//Збереження в файл
        {
             TextRange text = new TextRange(flowDoc.Document.ContentStart, flowDoc.Document.ContentEnd);

            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Doc file (*.doc)|*.doc|Text file (*.txt)|*.txt|C# file (*.cs)|*.cs";
            if (saveFileDialog.ShowDialog() == true)
            {
                File.WriteAllText(saveFileDialog.FileName, text.Text);
            }
        }

        
    }
}
