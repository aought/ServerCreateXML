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

            // 拼接下载路径
            // 获取当前目录名称
            string currentDirName = new DirectoryInfo(".").Name;
            string serverDownloadURL = String.Format("{0}/{1}/{2}", ConfigurationManager.AppSettings["serverURL"], currentDirName, ConfigurationManager.AppSettings["updatePathName"]);
            // 此段可略去
            Configuration configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            configuration.AppSettings.Settings["serverDownloadURL"].Value = serverDownloadURL;
            configuration.Save(ConfigurationSaveMode.Full);
            ConfigurationManager.RefreshSection("appSettings");


            // 获取当前路径
            // path=>"C:\\Users\\Empty\\Documents\\GitHub\\ServerCreateXML\\ServerCreateXML\\bin\\Debug"
            string path = Directory.GetCurrentDirectory();
            // 获取更新文件存放路径
            // updatePath=>"C:\\Users\\Empty\\Documents\\GitHub\\ServerCreateXML\\ServerCreateXML\\bin\\Debug\\VersionFolder"
            string updatePath = Path.Combine(path, ConfigurationManager.AppSettings["updatePathName"]);
            DirectoryInfo dirInfo = new DirectoryInfo(updatePath);
            string configPath = Path.Combine(path, ConfigurationManager.AppSettings["config"]);
            string tempConfigPath = Path.Combine(path, "temp_config.xml");

            // 获取父路径
            string parentFolder = new DirectoryInfo("..").FullName;


            if (!File.Exists(configPath))
            {
                XmlDocument tempXmlDocument = new XmlDocument();

                XmlDeclaration tempXmlDeclaration = tempXmlDocument.CreateXmlDeclaration("1.0", "UTF-8", null);
                XmlElement tempRoot = tempXmlDocument.DocumentElement;
                tempXmlDocument.InsertBefore(tempXmlDeclaration, tempRoot);

                XmlElement tempRootElement = tempXmlDocument.CreateElement(String.Empty, "updateFiles", string.Empty);
                tempXmlDocument.AppendChild(tempRootElement);
                tempXmlDocument.Save(configPath);
                MessageBox.Show("初始化");
                
            }
            
            File.Copy(configPath, "temp_config.xml");
            

            BuildXML(xmlDoc, rootElement, dirInfo, tempConfigPath, parentFolder);
            xmlDoc.Save(configPath);

            MessageBox.Show("已生成配置文件");

            File.Delete(tempConfigPath);
        }

        /// <summary>
        /// 组装XML
        /// </summary>
        public void BuildXML(XmlDocument xmlDoc, XmlElement rootElement, DirectoryInfo dirInfo, string tempConfigPath, string parentFolder)
        {
            // 获取业务名称
            // serverDownloadNameURL=>"Debug"
            // string serverDownloadNameURL = new DirectoryInfo(".").Name;
            // 拼接服务器更新文件下载路径：serverDownloadURL = "ftp://192.168.2.113/Debug/EXE"
            // string serverDownloadURL = String.Format("{0}/{1}/{2}", ConfigurationManager.AppSettings["serverURL"], serverDownloadNameURL, dirInfo.Name);

            // 此时传入的dirInfo是new DirectoryInfo(updatePath)即updatePath=>"C:\\Users\\Empty\\Documents\\GitHub\\ServerCreateXML\\ServerCreateXML\\bin\\Debug\\VersionFolder"

            // 判断文件夹是否存在，不存在则创建
            if (!Directory.Exists(dirInfo.FullName))
            {
                Directory.CreateDirectory(dirInfo.FullName);
            }

            foreach (var file in dirInfo.GetFiles())
            {
                // string hash = GetHash(file.Name, dirInfo);
                string hash = GetSHA256(file.FullName);

                XmlElement fileElement = xmlDoc.CreateElement(String.Empty, "file", string.Empty);
                fileElement.SetAttribute("name", file.Name);
                fileElement.SetAttribute("src", ConfigurationManager.AppSettings["serverDownloadURL"]);

                // 此时文件名称为：file.Name，这个值也是生成的XML节点中name属性的值；
                // 通过这个值来获取该节点的其他属性，比如哈希值和版本号；
                string localName = file.Name;
                string localHash = null;
                int localVersion = 0;
                XmlDocument xmlDocument = new XmlDocument();
                xmlDocument.Load(tempConfigPath);
                XmlNode xmlNode = xmlDocument.SelectSingleNode("updateFiles");
                XmlNodeList xmlNodeList = xmlNode.ChildNodes;
                foreach (XmlNode singleXmlNode in xmlNodeList)
                {
                    string temp = singleXmlNode.Attributes["name"].Value;
                    int tempVersion = Convert.ToInt32(singleXmlNode.Attributes["version"].Value);
                    if (localName == temp)
                    {
                        localHash = singleXmlNode.Attributes["hash"].Value;
                        localVersion = tempVersion;
                    }
                }


                if (localHash != hash)
                {
                    localVersion += 1;
                }
                fileElement.SetAttribute("version", localVersion.ToString());
                fileElement.SetAttribute("hash", hash);
                fileElement.SetAttribute("size", file.Length.ToString());
                fileElement.SetAttribute("option", "add");
                rootElement.AppendChild(fileElement);


            }
            // 递归子文件夹
            foreach (var dir in dirInfo.GetDirectories())
            {
                Configuration configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                // temp指的是FTP搭建的物理根目录
                // TODO:
                // "C:\\Users\\Empty\\Documents\\GitHub\\ServerCreateXML\\ServerCreateXML\\bin" => "C:\\Users\\Empty\\Documents\\GitHub\\ServerCreateXML\\ServerCreateXML\\bin\\Debug"
                // 多次递归之后变化
                // string temp = dirInfo.Parent.Parent.FullName;
                string temp = parentFolder;
                // MessageBox.Show(temp);
                configuration.AppSettings.Settings["serverDownloadURL"].Value = dir.FullName.Replace(temp, configuration.AppSettings.Settings["serverURL"].Value).Replace("\\", "/");
                configuration.Save(ConfigurationSaveMode.Full);
                ConfigurationManager.RefreshSection("appSettings");

                BuildXML(xmlDoc, rootElement, dir, tempConfigPath, parentFolder);
            }

        }

        public string GetSHA256(string test)
        {
            string hash = null;
            using (SHA256 sha256 = SHA256.Create())
            {
                using (FileStream fileStream = File.OpenRead(test))
                {
                    byte[] bytes = sha256.ComputeHash(fileStream);

                    StringBuilder builder = new StringBuilder();
                    for (int i = 0; i < bytes.Length; i++)
                    {
                        builder.Append(bytes[i].ToString("x2"));
                    }
                    hash = builder.ToString().ToUpper();
                }
            }
            return hash;
        }

    }
}
