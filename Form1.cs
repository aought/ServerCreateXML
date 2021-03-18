using System;
using System.Collections.Generic;
using System.ComponentModel;
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
            XmlDocument xml = new XmlDocument();

            XmlDeclaration xmlDeclaration = xml.CreateXmlDeclaration("1.0", "UTF-8", null);
            XmlElement root = xml.DocumentElement;
            xml.InsertBefore(xmlDeclaration, root);

            XmlElement rootElement = xml.CreateElement(String.Empty, "updateFiles", string.Empty);
            xml.AppendChild(rootElement);
            XmlElement fileElement = xml.CreateElement(String.Empty, "file", string.Empty);
            
            // 配置属性示例
            fileElement.SetAttribute("name", "text.exe");
            fileElement.SetAttribute("src", "ftp://192.168.2.113/");
            fileElement.SetAttribute("version", "999");
            fileElement.SetAttribute("size", "0");
            fileElement.SetAttribute("option", "add");
            rootElement.AppendChild(fileElement);

            xml.Save("E:\\Test\\XML\\test.xml");

            MessageBox.Show("已生成配置文件");

        }

        

        public void GetHash()
        {
            var dir = new DirectoryInfo(@"E:\Test\EXE");
            FileInfo[] files = dir.GetFiles();
            using(SHA256 sha256 = SHA256.Create())
            {
                foreach (var fileInfo in files)
                {
                    try
                    {
                        FileStream fileStream = fileInfo.Open(FileMode.Open);
                        fileStream.Position = 0;
                        byte[] hashValue = sha256.ComputeHash(fileStream);
                        // 目标字段1：文件名
                        MessageBox.Show($"{fileInfo.FullName}");
                        // 目标字段2：哈希值
                        MessageBox.Show(PrintByteArray(hashValue));
                        fileStream.Close();

                    }
                    catch (IOException e)
                    {
                        MessageBox.Show($"I/O Exception: {e.Message}");
                    }
                    catch(UnauthorizedAccessException e)
                    {
                        MessageBox.Show($"Access Exception: {e.Message}");
                    }
                }
            }

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
