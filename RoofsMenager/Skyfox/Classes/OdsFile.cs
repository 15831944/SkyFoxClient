using ICSharpCode.SharpZipLib.Zip;
using System;
using System.IO;
using System.Diagnostics;
using WebSupergoo.ABCpdf10;
using System.Drawing.Imaging;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using Skyfox.Classes;
using System.Linq;
namespace SKYfox
{
    class OdsFile
    {
        public string odsType;
        public string ProjectDir;
        public string ProjectName;
        public string cashDir;
        public string data;
        public string doneDir;
        private string dwgFile;
        public string DwgDataInTxt;
        public string[] PlohaTxtForFind = { "Text_plochy", "Vikyre_Text"};
        public string[] DelkaTxtForFind = { "Text_delky", "Level 60" };
        public static string StatcashDir = @"data\res\current\";
        public static string StatResDir = @"data\res\";
        public static string DiffTxtM, DiffTxtO;
        public List<DelkaList> DlList;
        public List<string> OdsList;
        public void GenerateDelkaList()
        {
            DlList = new List<DelkaList>();
            for (int i = 0; i < DelkaTxtForFind.Length; i++)
            {
                Regex regex = new Regex(@"Layer = ("+ DelkaTxtForFind[i] +@")\s+Color = (\w+)\s+Start point: X = ([\d-]+),[\d]+ Y = ([\d-]+),\d+ Z = ([\d-]+),\d+\s+Text: ([0-9.]+) m\s+");
                var r = regex.Match(DwgDataInTxt);
                while (r.Success)
                {
                    DelkaList Dlo = new DelkaList(r.Groups[0].Value, r.Groups[2].Value, r.Groups[3].Value, r.Groups[4].Value, r.Groups[5].Value, r.Groups[6].Value);
                    if (!DlList.Contains(Dlo, Dlo))
                        DlList.Add(Dlo);
                    r = r.NextMatch();
                }
            }
        }
        public string GetValuesList()
        {
            string endLis="";
            int n = DlList.Count;
            n -= n % 3;
                for (int i=0;i<n;i+=3)
            {
                endLis+=DlList[i].value+"\t"+DlList[i+1].value+"\t"+DlList[i + 2].value + "\n";
            }
            for(int i=0;i< DlList.Count-n;i++)
            {
                endLis += DlList[n + i].value + "\t";
            }
            return endLis;
        }
        public int findNumOf(string[] data)
        {
            int n = 0;
            for (int i = 0; i < data.Length; i++)
            {
                Regex regex = new Regex(data[i]);
                var r = regex.Match(DwgDataInTxt);
                while (r.Success)
                {
                    r = r.NextMatch();
                    n++;
                }
            }
            return n;
        }
        public OdsFile(string project_dir,string project_name)
        {
            ProjectName = project_name;
            ProjectDir = project_dir;
            odsType = @"data\res\simple.ods";
            cashDir = @"data\res\current\";
            DwgDataInTxt = null;
        }
        public OdsFile(string project_dir, string project_name,string type,string done_dir)
        {
            ProjectName = project_name;
            ProjectDir = project_dir;
            if(type=="simple")
            odsType = @"data\res\simple.ods";
            else
                if(type== "flat")
                odsType = @"data\res\flat.ods";
            else
                odsType = @"data\res\gybrid.ods";
            cashDir = @"data\res\current\";
            doneDir = done_dir;
            DwgDataInTxt = null;
        }
        public bool openProjDir()
        {
            try
            {
                Process.Start(ProjectDir);
                return true;
            }
            catch
            {
                return false;
            }
        }
        public bool openOdsFile()
        {
            try
            {
                Process.Start(ProjectDir + "//Vystupni_formular_" + ProjectName + ".ods");
                return true;
            }
            catch
            {
                return false;
            }
        }
        public void SaveDWGtxt()
        {
            FileStream fs = File.OpenWrite(cashDir+ ProjectName+"\\DWGtxt.txt");
            TextWriter fswrite = new StreamWriter(fs);
            fswrite.Write(DwgDataInTxt);
            fswrite.Close();
            fs.Close();
        }
        public string GetTxtFormat()
        {
            if (DwgDataInTxt==null)
            {
                string toDir = cashDir + ProjectName;
                try
                {
                    string[] dirs = Directory.GetFiles(toDir, "*.dwg");
                    dwgFile = dirs[0];
                }
                catch
                {

                }
                if (dwgFile != null)
                {
                    DwgDataInTxt = new SkySql(dwgFile).getDvgTxt();
                    return DwgDataInTxt;
                }
            }
            return DwgDataInTxt;
        }
        public bool OpenDWG()
        {
            string toDir = cashDir + ProjectName;
            try
            {
                string[] dirs = Directory.GetFiles(toDir, "*.dwg");
                Process.Start(dirs[0]);
                return true;
            }
            catch 
            {
                return false;
            }

        }
        public void openZipCashDir()
        {
            Process.Start(cashDir + "\\" + ProjectName);
        }
        void Log_Error(string message)
        {
            TextWriter fswrite = new StreamWriter(File.OpenWrite("errorLog.txt"));
            fswrite.WriteLine(message);
            fswrite.Close();
        }
        public void openZip()
        {
            try
            {
                string toDir = cashDir + "\\" + ProjectName;
                Directory.CreateDirectory(toDir);
                //Log_Error(ProjectDir + ".zip");
                FastZip zip = new FastZip();
                zip.ExtractZip(ProjectDir + ".zip", toDir, null);
            }
            catch { }
            
        }
        public void delZipCesh()
        {
            try
            {
                string toDir = cashDir + "\\" + ProjectName;
                Directory.Delete(toDir, true);
            }
            catch { }
        }
        public void safeEndZip()
        {
            FastZip zip = new FastZip();
            Directory.CreateDirectory(doneDir);
            Directory.CreateDirectory(doneDir+"\\"+ProjectName);
            Directory.Move(ProjectDir, doneDir + "\\" + ProjectName+"\\"+ProjectName+"\\");
            zip.CreateZip(doneDir+"\\"+ProjectName+".zip", doneDir + "\\" + ProjectName, true, null);
            
        }
        public static void DleteCash()
        {
            Directory.Delete(StatcashDir, true);
            Directory.CreateDirectory(StatcashDir);
        }
        public bool OpenEndDir()
        {
            try
            {
                string path = Path.GetFullPath( doneDir + "\\" + ProjectName + ".zip");

                if (File.Exists(path))
                {
                    Process PrFolder = new Process();
                    ProcessStartInfo psi = new ProcessStartInfo();
                    psi.CreateNoWindow = true;
                    psi.WindowStyle = ProcessWindowStyle.Normal;
                    psi.FileName = "explorer";
                    psi.Arguments = @"/n, /select, " + path;
                    PrFolder.StartInfo = psi;
                    PrFolder.Start();
                }
                return true;
            }
            catch
            {
                return false;
            }
        }
        public void openZip(bool open)
        {
            string toDir = cashDir + ProjectName;
            Directory.CreateDirectory(toDir);
            //Log_Error(ProjectDir);
            FastZip zip = new FastZip();
            zip.ExtractZip(ProjectDir + '\\' + ProjectName + ".zip", toDir, null);
            Process.Start(toDir);
        }
        public static void ConvertPDFToImage(string pdfInputPath, string imageOutputPath, string imageName, ImageFormat imageFormat)
        {

            FileStream fs = new FileStream(pdfInputPath, FileMode.Open);
            Doc document = new Doc();
            document.Read(fs);

            document.Rendering.DotsPerInch = 72;
            document.Rendering.DrawAnnotations = true;
            document.Rendering.AntiAliasImages = true;
            document.Rect.String = document.CropBox.String;
            document.Rendering.Save(Path.ChangeExtension(Path.Combine(imageOutputPath, imageName), imageFormat.ToString()));
            fs.Close();
        }
        void copy3dImage()
        {
            string typeDir = ProjectDir;
            string scrinImage = cashDir + ProjectName;
            string toDir = cashDir + ProjectName;
            try
            {
                string[] dirs = Directory.GetFiles(typeDir, "*3d_model*.pdf");
                ConvertPDFToImage(dirs[0], scrinImage, "image3.png", ImageFormat.Png);
                File.Copy(scrinImage+"\\image3.png", cashDir + ProjectName + @"ods\media\image3.png", true);
            }
            catch (Exception e)
            {
                Log_Error(e.ToString());
            }
        }
        void copy2dImage()
        {
            string typeDir = ProjectDir;
            string scrinImage = cashDir + ProjectName;
            string toDir = cashDir + ProjectName;
            try
            {
                string[] dirs = Directory.GetFiles(typeDir, "*Zaměření_plochy*.pdf");
                if (dirs.Length==0)
                    dirs = Directory.GetFiles(typeDir, "*Zamereni_plochy*.pdf");
                ConvertPDFToImage(dirs[0], scrinImage, "image4.png", ImageFormat.Png);
                File.Copy(scrinImage + "\\image4.png", cashDir + ProjectName + @"ods\media\image4.png", true);
            }
            catch (Exception e)
            {
                Log_Error(e.ToString());
            }
        }
        public string getPathScrin()
        {
            string toDir = cashDir + ProjectName;
            try
            {
                string[] dirs = Directory.GetFiles(toDir, "*.png");
                if(dirs.Length<1)
                {
                    dirs = Directory.GetFiles(toDir, "*.tiff");
                }
                return dirs[0];
            }
            catch (Exception e)
            {
                Log_Error(e.ToString());
            }
            return null;
        }
        public void copyImage()
        {
            string typeDir = cashDir + ProjectName + "ods";
            string scrinImage = typeDir + @"\media\image2.png";
            string toDir = cashDir + ProjectName;
            try
            {
                string[] dirs = Directory.GetFiles(toDir, "*.png");
                if(dirs.Length < 1)
                {
                    dirs = Directory.GetFiles(toDir, "*.tiff");
                }
                //Log_Error(toDir + "\\" + dirs[0]);
                //Log_Error(dirs[0]+ "     " +scrinImage);
                //File.Delete(scrinImage);
                File.Copy(dirs[0],scrinImage,true);                
            }
            catch(Exception e)
            {
                Log_Error(e.ToString()); 
            }

        }        
        string[] GetImgParam(string scrinImage,bool bg)
        {
            string[] Params = new String[4];
            string typeDir = cashDir + ProjectName + "ods";
            try
            {
                float wmx=1,hmx=1;
                if(bg)
                {
                    hmx = (float)4.60764;
                    wmx = (float)6.72136;
                }
                else
                {
                    hmx = (float)3.3125;
                    wmx = (float)6.80208;
                }
                //Log_Error(scrinImage);
                System.Drawing.Image img = System.Drawing.Image.FromFile(scrinImage);                
                float w = img.Width, h = img.Height;
                float ws = (float)((w * hmx) / h), hs = (float)hmx, x = (float)(((wmx -ws)/2)*(1/wmx)), y = (float)0.07292;
                Params[0] = ws.ToString();
                Params[1] = hs.ToString();
                Params[2] = x.ToString();
                Params[3] = y.ToString();
                for (int i = 0; i < 4; i++)
                    Params[i]=Params[i].Replace(",",".");
                return Params;
            }
            catch(Exception e)
            {
                //Log_Error(e.ToString());
            }
            return null;
        }
        public void CopyZip()
        {
            if(!File.Exists(ProjectDir + "\\" + ProjectName + ".zip"))
            File.Copy(ProjectDir + ".zip", ProjectDir + "\\" + ProjectName + ".zip");
        }
        public void checkOds(string odsDD)
        {
            try
            {
                string dataCh;
                FastZip zip = new FastZip();
                string typeDir = cashDir + ProjectName + "ods";
                Directory.CreateDirectory(typeDir);
                zip.ExtractZip(odsDD, typeDir, null);
                Directory.CreateDirectory(ProjectDir);
                string contentFile = typeDir + @"\content.xml";
                //string scrinImage = typeDir + @"\media\image2.png";
                FileStream fs = File.OpenRead(contentFile);
                TextReader fsread = new StreamReader(fs);
                dataCh = fsread.ReadToEnd();
                fs.Close();
                fsread.Close();
                //Directory.Delete(typeDir, true);
                OdsList = new List<string>();
                Regex regex = new Regex("office:value=\"([\\d.]+)\" table:styl");
                var r = regex.Match(dataCh);
                //OdsList.Add(dataCh);
                while (r.Success)
                {
                    OdsList.Add(r.Groups[1].Value);
                    r = r.NextMatch();
                }
                
            }
            catch (Exception ex)
            {

            }
        }
        public string RoundString(string str)
        {
            Double a=0;
            str = str.Replace(@".", @",");
            if (Double.TryParse(str, out a))
                return a.ToString();
            else
                return str;
        }
        public string CheckValues()
        {
            bool exist = false;
            string diff="";
            for(int i=0;i<DlList.Count;i++)
            {
                exist = false;
                DlList[i].value = RoundString(DlList[i].value);
                for (int j=1;j<OdsList.Count-3;j++)
                {
                    OdsList[j] = RoundString(OdsList[j]);
                    if (OdsList[j] == DlList[i].value /*a==b*/)
                    {
                        exist = true;
                        OdsList[j] = "Check";
                        break;
                    }
                }
                if(!exist)
                {
                    diff += DiffTxtM+" \t" + DlList[i].value + "\n";
                }
            }
            for (int j = 1; j < OdsList.Count - 2; j++)
            {
                if(OdsList[j]!="Check")
                {
                    diff += DiffTxtO+ "Ексель: \t" + OdsList[j] + "\n";
                }
            }
            return diff;
            }
        public void openForWrite()
        {
            try
            {
                FastZip zip = new FastZip();
                string typeDir = cashDir + ProjectName + "ods";
                zip.ExtractZip(odsType, typeDir, null);

                Directory.CreateDirectory(ProjectDir);
                string contentFile = typeDir + @"\content.xml";
                string scrinImage = typeDir + @"\media\image2.png";

                FileStream fs = File.OpenRead(contentFile);
                TextReader fsread = new StreamReader(fs);
                data = fsread.ReadToEnd();
                fs.Close();
                fsread.Close();
            }
            catch (Exception ex)
            {

            }
        }
        public void SaveOdsFile()
        {
            try
            {
                string typeDir = cashDir + ProjectName + "ods";
                string contentFile = typeDir + @"\content.xml";
                string scrinImage = typeDir + @"\media\image2.png";
                FastZip zip = new FastZip();
                FileStream fs = File.OpenWrite(contentFile);
                TextWriter fswrite = new StreamWriter(fs);
                fswrite.Write(data);
                fswrite.Close();
                fs.Close();
                string odsFile = ProjectDir + "//Vystupni_formular_" + ProjectName + ".ods";
                zip.CreateZip(odsFile, typeDir, true, null);                
            }
            catch (Exception ex)
            {

            }
        }
        public void setAllParameters(string zpracovatel, string zakazka, string adresa, string gps)
        {
            try
            {
                /*
                [xscrin] - 0.04167
                [yscrin] - 0.07292
                [widthscrin] - 6.80208
                [heightscrin] - 3.3125
                [x3d]in" svg:y="[y3d]in" svg:width="[width3d]in" svg:height="[height3d]

                */
                string typeDir = cashDir + ProjectName + "ods";
                copyImage();
                data = data.Replace("[zpracovatel]", zpracovatel);
                data += '\n';
                data = data.Replace("[zakazka]", zakazka);
                data += '\n';
                data = data.Replace("[adresa]", adresa);
                data += '\n';
                data = data.Replace("[gps]", gps);
                data += '\n';
                string[] Params = GetImgParam(typeDir + @"\media\image2.png",false);
                data = data.Replace("[xscrin]", Params[2]);
                data += '\n';
                data = data.Replace("[yscrin]", Params[3]);
                data += '\n';
                data = data.Replace("[widthscrin]", Params[0]);
                data += '\n';
                data = data.Replace("[heightscrin]", Params[1]);
                data += '\n';
                copy3dImage();
                Params = GetImgParam(typeDir + @"\media\image3.png",false);
                data = data.Replace("[x3d]", Params[2]);
                data += '\n';
                data = data.Replace("[y3d]", Params[3]);
                data += '\n';
                data = data.Replace("[width3d]", Params[0]);
                data += '\n';
                data = data.Replace("[height3d]", Params[1]);
                data += '\n';
                copy2dImage();
                Params = GetImgParam(typeDir + @"\media\image4.png",true);
                data = data.Replace("[x2d]", Params[2]);
                data += '\n';
                data = data.Replace("[y2d]", Params[3]);
                data += '\n';
                data = data.Replace("[width2d]", Params[0]);
                data += '\n';
                data = data.Replace("[height2d]", Params[1]);
                data += '\n';
                //Directory.Delete(typeDir,true);

            }
            catch (Exception exn)
            {
                Log_Error(exn.ToString());
            }
}
    }
}
