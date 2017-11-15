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
using ss = System.Windows.Forms;
using System.Threading.Tasks;
using System.Collections.Specialized;
using System.IO;
using System.Net;
namespace Skyfox.Classes
{
    public partial class SkySql
    {
        static public string server= @"http://slavik.xyz/roofsql/";
        static public string Key = "";
        static public string userId = "";
        static public List<string[]> getProjList()
        {
            string data = getResponse(server + @"getlistproject.php?key=" + Key);
            return GetPL(data);
        }
        static public List<string[]> getProjListOnZpracovatel(string zpracovatel)
        {
            string data = getResponse(server + @"getprojonzpracovatel.php?key=" + Key + @"&zpracovatel="+ zpracovatel);
            return GetPL(data);
        }
        static public List<string[]> getUserList()
        {
            string data = getResponse(server + @"getzpracovatels.php?key=" + Key);
            return GetPL(data);
        }
        static public List<string[]> GetPL(string Indata)
        {
            string[] data = Indata.Split('~');
            List<string[]> list = new List<string[]>();
            list.Add(data);
            for (int i = 0; i < data.Length; i++)
            {
                list.Add(data[i].Split(';'));
            }
            return list;
        }
        static public bool SetZpracovatel(string projId,string zpracovatelId)
        {
            getResponse(server + @"setzpracovatel.php?key=" + Key + @"&projid=" + projId + @"&zpracovatelid=" + zpracovatelId);
            return false;
        }
        static public bool addProject(string name,string adres,string gps,string plohy, string type, string file)
        {
            if (File.Exists(file))
            {
                file = Path.GetFullPath(file);
                //HttpUploadFile();
                NameValueCollection nvc = new NameValueCollection();
                //nvc.Add("user", "user");
                //nvc.Add("passwd", "passwd");
                HttpUploadFile(server+ "filedownload.php", file, "uploadfile", "user/zip", nvc);
                getResponse(server+ @"addemptyproject.php?key=" + Key+@"&name="+name+@"&adres="+adres+@"&gps="+gps + @"&plohy="+plohy + @"&type="+type);
                return true;
            //return server+ @"addemptyproject.php?key=" + Key+@"&name="+name+@"&adres="+adres+@"&gps="+gps + @"&plohy="+plohy + @"&type="+type;

             }
             return false;
        }
        static public bool DoneProject(string zpracovatel,string file,string progectid,string zpracovatelid)
        {
            if (File.Exists(file))
            {
                file = Path.GetFullPath(file);
                //HttpUploadFile();
                NameValueCollection nvc = new NameValueCollection();
                //nvc.Add("user", "user");
                //nvc.Add("passwd", "passwd");
                HttpUploadFile(server + "loaddonefile.php", file, "uploadfile", "user/zip", nvc);
                getResponse(server + @"projectisdone.php?key=" + Key + @"&projectid=" + progectid + @"&zpracovatelid=" + zpracovatelid);
                return true;
                //return server+ @"addemptyproject.php?key=" + Key+@"&name="+name+@"&adres="+adres+@"&gps="+gps + @"&plohy="+plohy + @"&type="+type;

            }
            return false;
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
        static public string getResponse(string uri)
        {
            HttpWebRequest req;
            HttpWebResponse resp;
            StreamReader sr;
            string content;

            req = (HttpWebRequest)WebRequest.Create(uri);
            resp = (HttpWebResponse)req.GetResponse();
            sr = new StreamReader(resp.GetResponseStream(), Encoding.UTF8);
            content = sr.ReadToEnd();
            sr.Close();
            return content;
        }
        public static void HttpUploadFile(string url, string file, string paramName, string contentType, NameValueCollection nvc)
        {
            Console.WriteLine(string.Format("Uploading {0} to {1}", file, url));
            string boundary = "---------------------------" + DateTime.Now.Ticks.ToString("x");
            byte[] boundarybytes = System.Text.Encoding.ASCII.GetBytes("\r\n--" + boundary + "\r\n");

            HttpWebRequest wr = (HttpWebRequest)WebRequest.Create(url);
            wr.ContentType = "multipart/form-data; boundary=" + boundary;
            wr.Method = "POST";
            wr.KeepAlive = true;
            wr.Credentials = System.Net.CredentialCache.DefaultCredentials;

            Stream rs = wr.GetRequestStream();

            string formdataTemplate = "Content-Disposition: form-data; name=\"{0}\"\r\n\r\n{1}";
            foreach (string key in nvc.Keys)
            {
                rs.Write(boundarybytes, 0, boundarybytes.Length);
                string formitem = string.Format(formdataTemplate, key, nvc[key]);
                byte[] formitembytes = System.Text.Encoding.UTF8.GetBytes(formitem);
                rs.Write(formitembytes, 0, formitembytes.Length);
            }
            rs.Write(boundarybytes, 0, boundarybytes.Length);

            string headerTemplate = "Content-Disposition: form-data; name=\"{0}\"; filename=\"{1}\"\r\nContent-Type: {2}\r\n\r\n";
            string header = string.Format(headerTemplate, paramName, file, contentType);
            byte[] headerbytes = System.Text.Encoding.UTF8.GetBytes(header);
            rs.Write(headerbytes, 0, headerbytes.Length);

            FileStream fileStream = new FileStream(file, FileMode.Open, FileAccess.Read);
            byte[] buffer = new byte[4096];
            int bytesRead = 0;
            while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) != 0)
            {
                rs.Write(buffer, 0, bytesRead);
            }
            fileStream.Close();

            byte[] trailer = System.Text.Encoding.ASCII.GetBytes("\r\n--" + boundary + "--\r\n");
            rs.Write(trailer, 0, trailer.Length);
            rs.Close();

            WebResponse wresp = null;
            try
            {
                wresp = wr.GetResponse();
                Stream stream2 = wresp.GetResponseStream();
                StreamReader reader2 = new StreamReader(stream2);
                Console.WriteLine(string.Format("File uploaded, server response is: {0}", reader2.ReadToEnd()));
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error uploading file", ex);
                if (wresp != null)
                {
                    wresp.Close();
                    wresp = null;
                }
            }
            finally
            {
                wr = null;
            }
        }
    }
}
