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
using Skyfox.Classes;

namespace Manager
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public System.Windows.Threading.DispatcherTimer dispatcherTimer;
        List<string[]> listOfProj;
        public void SetTimer()
        {
            dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
            dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);
            dispatcherTimer.Interval = new TimeSpan(0, 0, 1);
            dispatcherTimer.Start();
        }
        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            try
            {
                listOfProj = SkySql.getProjListOnZpracovatel(SkySql.userId);                
                listOfProj.Remove(listOfProj[0]);
                foreach (string[] vl in listOfProj)
                {
                    try
                    {
                        ProjListForDoing.Items.Add(vl[1]);
                    }
                    catch { }
                }
                MessageBox.Show(SkySql.userId);
                dispatcherTimer.Stop();
            }
            catch { }
        }
        public MainWindow()
        {
            InitializeComponent();
            SetTimer();
        }

        private void LogInButton_Click(object sender, RoutedEventArgs e)
        {
            string[] respData = SkySql.getResponse(@"http://slavik.xyz/roofsql/login.php?login="+LoginText.Text+"&password="+PassvordText.Text).Split(';');
            SkySql.Key = respData[0];
            SkySql.userId = respData[1];
            //MessageBox.Show(SkySql.Key);
            if (SkySql.Key.Length>6)
            {
                AutorizationGird.Visibility = Visibility.Hidden;
            }

            SetTimer();
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            SkySql.addProject(NameText.Text, AdresText.Text, GpsText.Text, PlohyText.Text, TypeText.Text,SkySql.ChooseFile());
        }

        private void button_Click_1(object sender, RoutedEventArgs e)
        {
            try
            {
                List<string[]> lst = SkySql.getUserList();
                lst.Remove(lst[0]);
                foreach (string[] vl in lst)
                {
                    UserListBox.Items.Add(vl[1]);
                }
            }
            catch(Exception exx)
            {
                //MessageBox.Show(exx.ToString());
            }
            try
            {

                List<string[]> lst = SkySql.getProjList();
                lst.Remove(lst[0]);
                foreach (string[] vl in lst)
                {
                    ProjListBox.Items.Add(vl[1]);
                }
            }
            catch (Exception exx)
            {
                //MessageBox.Show(exx.ToString());
            }
        }

        private void ProjListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string idOfProj = "";
                MessageBox.Show(ProjListForDoing.SelectedItem.ToString());
                foreach (string[] vl in listOfProj)
                {
                    if (vl[1] == ProjListForDoing.SelectedItem.ToString())
                    { idOfProj = vl[0]; break; }
                }
                SkySql.DoneProject("2", SkySql.ChooseFile(), idOfProj, "1");
            }catch(Exception exxx) { MessageBox.Show(exxx.ToString()); }
        }

        private void ProjListForDoing_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void Window_Closed(object sender, EventArgs e)
        {

        }
    }
}
