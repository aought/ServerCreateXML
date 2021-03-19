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

            DirectoryInfo dirInfo = new DirectoryInfo(@ConfigurationManager.AppSettings["updatePath"]);
            BuildXML(xmlDoc, rootElement, dirInfo);
            xmlDoc.Save(@ConfigurationManager.AppSettings["config"]);

            MessageBox.Show("已生成配置文件");


        }

        /// <summary>
        /// 组装XML
        /// </summary>
        public void BuildXML(XmlDocument xmlDoc, XmlElement rootElement, DirectoryInfo dirInfo)
        {
            foreach (var file in dirInfo.GetFiles())
            {
                XmlElement fileElement = xmlDoc.CreateElement(String.Empty, "file", string.Empty);
                fileElement.SetAttribute("name", file.Name);
                fileElement.SetAttribute("src", ConfigurationManager.AppSettings["serverURL"]);
                fileElement.SetAttribute("version", Guid.NewGuid().ToString());
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


    }
}
