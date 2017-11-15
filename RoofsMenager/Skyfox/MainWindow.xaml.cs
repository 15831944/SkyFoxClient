using SKYfox;
using System;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using ss = System.Windows.Forms;
using ICSharpCode.SharpZipLib.Zip;
using System.Diagnostics;
using WebSupergoo.ABCpdf10;
using System.Drawing.Imaging;
using si = System.IO;

namespace Skyfox
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        string ProcessDir;
        OdsFile odsRes,ViueOds;
        string DWGNotFoundMessage="DWG файл не знайдено. Потрібно розпочати проект або розпакувати архів вручну.",ODSNotFoundMessage="ODS файл ще не згенеровано.",DirNotFoundMessage="Папку ще не згенеровано.";
        string GPSOrAdresError = "Не вказано Aдресу або GPS";        
        public MainWindow()
        {
            InitializeComponent();
            OdsFile.DleteCash();
            LoadSettings();
            LoadLocal("data\\res\\localEN.lc");
            TakeProcessingProject();
            //ChooseFolder();
            //TakeProcessingProject();
        }
        public void LoadSettings()
        {
            try
            {
                FileStream fs = File.OpenRead(OdsFile.StatResDir + "\\setting.inf");
                BinaryReader fsread = new BinaryReader(fs);
                ZpracovatelTXT.Text = fsread.ReadString();
                ProcessDir = ProcDirTxt.Text = fsread.ReadString();
                DWGCheckBox.IsChecked = fsread.ReadBoolean();
                ProjDirCheckBox.IsChecked = fsread.ReadBoolean();
                OdsCheckBox.IsChecked = fsread.ReadBoolean();
                fsread.Close();
                fs.Close();
            }
            catch { }
        }
        public void SaveSettings()
        {
            try
            {
                FileStream fs = File.OpenWrite(OdsFile.StatResDir + "\\setting.inf");
                BinaryWriter fswrite = new BinaryWriter(fs);
                fswrite.Write(ZpracovatelTXT.Text);
                fswrite.Write(ProcDirTxt.Text);
                fswrite.Write(DWGCheckBox.IsChecked.Value);
                fswrite.Write(ProjDirCheckBox.IsChecked.Value);
                fswrite.Write(OdsCheckBox.IsChecked.Value);
                fswrite.Close();
                fs.Close();
            }
            catch { }
        }
        public void ChooseFolder()
        {
            ss.FolderBrowserDialog folderBrowserDialog1=new ss.FolderBrowserDialog();
            if (folderBrowserDialog1.ShowDialog() == ss.DialogResult.OK)
            {
                ProcessDir = folderBrowserDialog1.SelectedPath+"\\";
                ProcDirTxt.Text = folderBrowserDialog1.SelectedPath+"\\";
            }
        }
        public string ChooseFile()
        {
            ss.OpenFileDialog folderBrowserDialog1 = new ss.OpenFileDialog();
            if (folderBrowserDialog1.ShowDialog() == ss.DialogResult.OK)
            {
                return folderBrowserDialog1.FileName;
            }
            return null;
        }
        public void TakeProcessingProject()
        {
            try
            {
                ProjectListBox.Items.Clear();
                string[] DirList = Directory.GetFiles(ProcessDir, "*.zip");
                for (int i = 0; i < DirList.Length; i++)
                    ProjectListBox.Items.Add(System.IO.Path.GetFileName(DirList[i]));
                /*FileStream fs = File.OpenRead();
                TextReader fsread = new StreamReader(fs);
                fs.Close();*/
            }
            catch (Exception exn)
            {
                //MessageBox.Show(exn.ToString());
            }
        }
        private void SetProcesDirButton_Click(object sender, RoutedEventArgs e)
        {
            ChooseFolder();
            TakeProcessingProject();
        }        
        private void Progect1runButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string typtOfProject;
                if (SimpleradioButton.IsChecked.Value)
                    typtOfProject = "simple";
                else
                    if (FlatradioButton.IsChecked.Value)
                    typtOfProject = "flat";
                else
                    typtOfProject = "gybrid";
                odsRes = new OdsFile(ProcessDir + System.IO.Path.GetFileNameWithoutExtension(ProjectListBox.SelectedItem.ToString()), System.IO.Path.GetFileNameWithoutExtension(ProjectListBox.SelectedItem.ToString()),typtOfProject,ProcessDir+"\\Done");
                odsRes.openZip();               
                // odsRes.delZipCesh();                
                odsRes.openForWrite();
                if(ProjDirCheckBox.IsChecked.Value)                
                odsRes.openProjDir();
                odsRes.CopyZip();
                if (DWGCheckBox.IsChecked.Value)
                odsRes.OpenDWG();
            }
            catch (Exception exn)
            {
                MessageBox.Show(exn.ToString());
            }
        }

        private void ProjectDir_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!odsRes.openProjDir())
                    MessageBox.Show(DirNotFoundMessage);
            }
            catch
            {
                MessageBox.Show(DirNotFoundMessage);
            }

        }

        private void OdsButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!odsRes.openOdsFile())
                    MessageBox.Show(ODSNotFoundMessage);
            }
            catch
            {
                MessageBox.Show(ODSNotFoundMessage);
            }
        }

        private void _ZamereniPrip_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {

        }

        private void _ZamerenyCopyButton_Click(object sender, RoutedEventArgs e)
        {
            string textData = _ZamereniPrip.Text;
            Clipboard.SetData(DataFormats.Text, (Object)textData);
        }

        private void _3dModelCopyButton_Click(object sender, RoutedEventArgs e)
        {
            string textData = _3dModelPrip.Text;
            Clipboard.SetData(DataFormats.Text, (Object)textData);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            SaveSettings();
            /*
            si.FileStream fs = si.File.OpenWrite(@"data\res\setting");
            BinaryWriter br = new BinaryWriter(fs);
            byte[] mas=new b
            br.Write(;*/
        }

        private void EndProjButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                odsRes.safeEndZip();
                odsRes.OpenEndDir();
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void CheckButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ViueOds.checkOds(Classes.SkySql.ChooseFile());
                Values_DWG.Content = ViueOds.CheckValues();
               
            }catch(Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void SaveODSFileButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (Adres_proj.Text == null || GPSOfProj.Text == null)
                    MessageBox.Show(GPSOrAdresError);
                else
                {
                    odsRes.setAllParameters(ZpracovatelTXT.Text, System.IO.Path.GetFileNameWithoutExtension(ProjectListBox.SelectedItem.ToString()), Adres_proj.Text, GPSOfProj.Text);
                    odsRes.SaveOdsFile();
                    if (OdsCheckBox.IsChecked.Value)
                        if (!odsRes.openOdsFile())
                            MessageBox.Show(ODSNotFoundMessage);
                }
            }
            catch (Exception exn)
            {
                MessageBox.Show(exn.ToString());
            }
        }

        private void ChangeLangUA_Click(object sender, RoutedEventArgs e)
        {
            LoadLocal("data\\res\\localUA.lc");
            System.Windows.Media.Brush tt = ChangeLangUA.Background;
            ChangeLangUA.Background = ChangeLangEN.Background;
            ChangeLangEN.Background = tt;
        }

        private void ChangeLangEN_Click(object sender, RoutedEventArgs e)
        {
            LoadLocal("data\\res\\localEN.lc");
            System.Windows.Media.Brush tt = ChangeLangUA.Background;
            ChangeLangUA.Background = ChangeLangEN.Background;
            ChangeLangEN.Background = tt;
        }

        public void LoadLocal(string Path)
        {
            try
            {
                FileStream fs = File.OpenRead(Path);
                TextReader fsread = new StreamReader(fs);
                SetProcesDirButton.Content = fsread.ReadLine();
                AdressLabel.Content = fsread.ReadLine();
                GpsLabel.Content = fsread.ReadLine();
                DwgButton.Content = fsread.ReadLine();
                ProjectDir.Content = fsread.ReadLine();
                OdsButton.Content = fsread.ReadLine();
                DWGCheckBox.Content = fsread.ReadLine();
                ProjDirCheckBox.Content = fsread.ReadLine();
                OdsCheckBox.Content = fsread.ReadLine();
                _3dModelCopyButton.Content = fsread.ReadLine();
                _ZamerenyCopyButton.Content = fsread.ReadLine();
                label.Content = fsread.ReadLine();
                SimpleradioButton.Content = fsread.ReadLine();
                FlatradioButton.Content = fsread.ReadLine();
                GybridradioButton.Content = fsread.ReadLine();
                CheckButton.Content = fsread.ReadLine();
                Progect1runButton.Content = fsread.ReadLine();
                SaveODSFileButton.Content = fsread.ReadLine();
                EndProjButton.Content = fsread.ReadLine();
                label1.Content = fsread.ReadLine();
                label1_Copy.Content = fsread.ReadLine();
                OdsFile.DiffTxtM = fsread.ReadLine();
                OdsFile.DiffTxtO = fsread.ReadLine();
                DWGNotFoundMessage = fsread.ReadLine();
                ODSNotFoundMessage = fsread.ReadLine();
                DirNotFoundMessage = fsread.ReadLine();
                GPSOrAdresError = fsread.ReadLine();
                fs.Close();
                fsread.Close();
            }catch(Exception mss)
            {
                MessageBox.Show(mss.ToString());
            }
        }
        private void DwgButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!ViueOds.OpenDWG())
                    MessageBox.Show(DWGNotFoundMessage);
            }
            catch
            {
                MessageBox.Show(DWGNotFoundMessage);
            }
        }        
        private void ProjectListBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            ////fgfgfgfg
            try
            {
                ViueOds = new OdsFile(ProcessDir + System.IO.Path.GetFileNameWithoutExtension(ProjectListBox.SelectedItem.ToString()), System.IO.Path.GetFileNameWithoutExtension(ProjectListBox.SelectedItem.ToString()));
                ViueOds.openZip();
                string Pathh = ViueOds.getPathScrin();
                //MessageBox.Show(Pathh);
                imageScrin.Source = new BitmapImage(new Uri(@"file:///" + Path.GetFullPath(Pathh)));
                ViueOds.GetTxtFormat();
                //ViueOds.delZipCesh();
                ViueOds.SaveDWGtxt();
                ViueOds.GenerateDelkaList();
                NumOfPl.Text = (ViueOds.findNumOf(ViueOds.PlohaTxtForFind) - 1).ToString();
                NumOfVl.Text = ViueOds.DlList.Count.ToString();
                Values_DWG.Content = ViueOds.GetValuesList();                
            }
            catch (Exception ex) { MessageBox.Show(ex.ToString()); }
        }
    }
}
