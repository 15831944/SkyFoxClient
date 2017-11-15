using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SqlClient;
using ss = System.Windows.Forms;
using CADImport;
using CADImport.CADImportForms;
using CADImport.HPGL2;
using CADImport.RasterImage;
using System.IO;
using CADImportNetDemos.CADImportDemo.TXTImport;
using CADImportNetDemos.CADImportDemo.XMLImport;

namespace Skyfox.Classes
{
    public partial class SkySql
    {
        private CADImport.CADImage cadImage;
        private CADImportNetDemos.CADImportDemo.XMLImport.XMLImporter xmlImporter;
        private CADImportNetDemos.CADImportDemo.TXTImport.TXTImporter txtImporter;
        private int entitiesLimit;
        public string fileName;
        public SkySql(string FileName)
        {
            fileName = FileName;
        }
        public void LoadFile()
        {
            if (fileName != null)
            {
                if (cadImage != null)
                {
                    cadImage.Dispose();
                    cadImage = null;
                }
                this.cadImage = CADImage.CreateImageByExtension(fileName);
            }
            if (this.cadImage != null)
            {
                if (CADConst.IsWebPath(fileName))
                    this.cadImage.LoadFromWeb(fileName);
                else
                    cadImage.LoadFromFile(fileName);
            }
            CADImage.LastLoadedFilePath = Path.GetDirectoryName(fileName);
            SetCADImageOptions();
        }
        public void SetCADImageOptions()
        {
            //cadImage.UseWinEllipse = false;
            cadImage.IsShowLineWeight = false;
            //this.cadImage.UseDoubleBuffering = true;
            ObjEntity.cadImage = cadImage;
            xmlImporter.Image = this.cadImage;
            txtImporter.Image = this.cadImage;
        }
        private void InitParams()
        {
            xmlImporter = new XMLImporter();
            txtImporter = new TXTImporter();
            entitiesLimit = 100000;
            #region protect
#if protect
			regFrm = new RegForm();
#endif
            #endregion protect
        }
        public static string ChooseFile()
        {
            ss.OpenFileDialog folderBrowserDialog1 = new ss.OpenFileDialog();
            if (folderBrowserDialog1.ShowDialog() == ss.DialogResult.OK)
            {
                return folderBrowserDialog1.FileName;
            }
            return null;
        }
        public string getDvgTxt()
        {
            InitParams();
            LoadFile();
            return "ENTITIES STRUCTURE (limit = " + entitiesLimit + ")\r\n\r\n" +
                                    this.txtImporter.EntStructure(entitiesLimit);
        }
    }
    internal abstract class ApplicationConstants
    {
        public static readonly string[] hatchStyle;
        public static readonly string sepstr;
        public static readonly string loadfilestr;
        public static readonly string msgBoxDebugCaption;
        public static readonly string notsupportedstr;
        public static readonly string notsupportedextstr;
        public static readonly string appnamestr;
        public static readonly string sepstr2;
        public static readonly string xmlextstr;
        public static readonly string xmlextstr2;
        public static readonly string sepstr3;
        public static readonly string txtextstr;

        static ApplicationConstants()
        {
            hatchStyle = new string[8]{"BDiagonal", "Cross", "DiagCross", "FDiagonal",
                                       "Horizontal", "PatternData", "Solid", "Vertical"};
            sepstr = " - ";
            sepstr2 = " : ";
            loadfilestr = "Loading file...";
            msgBoxDebugCaption = "Debug application message";
            notsupportedstr = "not supported";
            notsupportedextstr = "Not supported in current version!";
            appnamestr = "CADImportNet Demo";
            xmlextstr = ".XML";
            xmlextstr2 = ".xml";
            sepstr3 = string.Empty + (char)13 + (char)10;
            txtextstr = ".txt";
        }

        public static string GetColor(Color val)
        {
            if (val.Equals(CADConst.clNone))
                return "[black/white]";
            if (val.Equals(CADConst.clByBlock))
                return "[ByBlock]";
            if (val.Equals(CADConst.clByLayer))
                return "[ByLayer]";
            return val.ToString();
        }
    }
}
