using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;
using System.Xml;

namespace ServerCreateXML
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            CreateXML();
        }

        public void CreateXML()
        {
            XmlDocument xmlDoc = new XmlDocument();

            XmlDeclaration xmlDeclaration = xmlDoc.CreateXmlDeclaration("1.0", "UTF-8", null);
            XmlElement root = xmlDoc.DocumentElement;
            xmlDoc.InsertBefore(xmlDeclaration, root);

            XmlElement rootElement = xmlDoc.CreateElement(String.Empty, "updateFiles", string.Empty);
            xmlDoc.AppendChild(rootElement);

            // 获取当前路径
            string path = Directory.GetCurrentDirectory();
            string updatePath = Path.Combine(path, ConfigurationManager.AppSettings["updatePathName"]);
            DirectoryInfo dirInfo = new DirectoryInfo(updatePath);
            BuildXML(xmlDoc, rootElement, dirInfo);
            string configPath = Path.Combine(path, ConfigurationManager.AppSettings["config"]);
            xmlDoc.Save(configPath);

            MessageBox.Show("已生成配置文件");


        }

        /// <summary>
        /// 组装XML
        /// </summary>
        public void BuildXML(XmlDocument xmlDoc, XmlElement rootElement, DirectoryInfo dirInfo)
        {
            string serverDownloadNameURL = new DirectoryInfo(".").Name;
            // 拼接服务器更新文件下载路径：serverDownloadURL = "ftp://192.168.2.113/Debug/EXE"
            string serverDownloadURL = String.Format("{0}/{1}/{2}", ConfigurationManager.AppSettings["serverURL"], serverDownloadNameURL, dirInfo.Name);

            foreach (var file in dirInfo.GetFiles())
            {
                string hash = GetHash(file.Name, dirInfo);
                
                XmlElement fileElement = xmlDoc.CreateElement(String.Empty, "file", string.Empty);
                fileElement.SetAttribute("name", file.Name);
                fileElement.SetAttribute("src", serverDownloadURL);   
                
                
                if (ConfigurationManager.AppSettings["hash"] != hash)
                {
                    Configuration configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                    int version = Convert.ToInt32(configuration.AppSettings.Settings["version"].Value);
                    version += 1;
                    configuration.AppSettings.Settings["version"].Value = version.ToString();
                    configuration.AppSettings.Settings["hash"].Value = hash;
                    configuration.Save(ConfigurationSaveMode.Full);
                    ConfigurationManager.RefreshSection("appSettings");
                }
                fileElement.SetAttribute("version", ConfigurationManager.AppSettings["version"]);
                fileElement.SetAttribute("hash", ConfigurationManager.AppSettings["hash"]);
                fileElement.SetAttribute("size", file.Length.ToString());
                fileElement.SetAttribute("option", "add");
                rootElement.AppendChild(fileElement);

            }
            // 递归子文件夹
            foreach (var dir in dirInfo.GetDirectories())
            {
                BuildXML(xmlDoc, rootElement, dir);
            }

        }

        public string GetHash(string fileName, DirectoryInfo dir)
        {
            string hash = null;
            FileInfo[] files = dir.GetFiles();
            using (SHA256 sha256 = SHA256.Create())
            {
                foreach (var fileInfo in files)
                {
                    if (fileInfo.Name == fileName)
                    {
                        try
                        {
                            FileStream fileStream = fileInfo.Open(FileMode.Open);
                            fileStream.Position = 0;
                            byte[] hashValue = sha256.ComputeHash(fileStream);
                            hash = PrintByteArray(hashValue);
                            // MessageBox.Show(PrintByteArray(hashValue));
                            fileStream.Close();

                        }
                        catch (IOException e)
                        {
                            MessageBox.Show($"I/O Exception: {e.Message}");
                        }
                        catch (UnauthorizedAccessException e)
                        {
                            MessageBox.Show($"Access Exception: {e.Message}");
                        }
                    }
                    
                }
            }
            return hash;
        }


        public string PrintByteArray(byte[] byteArray)
        {

            var sb = new StringBuilder();
            for (var i = 0; i < byteArray.Length; i++)
            {
                var b = byteArray[i];
                sb.Append(b);
            }
            return sb.ToString();
        }

    }
}
