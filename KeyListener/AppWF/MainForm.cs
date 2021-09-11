using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;

namespace AppWF
{
    public partial class MainForm : Form
    {
        private const string PATH = "data.xml";
        private int secondsUpdateKeys = 10; // обновление информации про нажатые кнопки
        private int secondsSaveFile = 60; // сохранение файла
        private Dictionary<string, int> keys = new Dictionary<string, int>(); // нажатые клавиши
        public MainForm(Dictionary<string, int> keys)
        {
            InitializeComponent();
            this.keys = keys;
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            // загрузка сохраненных данных
            XDocument xmlDocument;
            if (File.Exists(PATH))
            {
                xmlDocument = XDocument.Load(PATH);
                var xmlRootElement = xmlDocument.Root;
                SetConfig(xmlRootElement);
                SetKeys(xmlRootElement);
            }
            else SaveFile(true);
            // регистрация таймеров
            Timer timerUpdateButtons = new Timer();
            timerUpdateButtons.Interval = (secondsUpdateKeys * 1000); // def. 10 secs
            timerUpdateButtons.Tick += (timerSender, args) => SetLines();
            timerUpdateButtons.Start();

            Timer timerSaveFile = new Timer();
            timerSaveFile.Interval = (secondsSaveFile * 1000); // def. 60 secs
            timerSaveFile.Tick += (timerSender, args) => SaveFile();
            timerSaveFile.Start();

            SetLines();
            SecUpdateKeys.Value = secondsUpdateKeys;
            SecSaveFile.Value = secondsSaveFile;
        }
        // пятать в трей
        private void MainForm_Resize(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Minimized)
            {
                Hide();
                NotifyIcon.Visible = true;
            }
        }
        private void NotifyIcon_MouseClick(object sender, MouseEventArgs e)
        {
            Show();
            WindowState = FormWindowState.Normal;
            NotifyIcon.Visible = false;
        }
        // при закрытии: сохранение
        private void MainForm_FormClosing(object sender, FormClosingEventArgs e) => SaveFile();
        void SetConfig(XElement root)
        {
            var strSecondsUpdateButtons = root.Element("config").Attribute("secondsUpdateButtons").Value;
            var strSecondsSaveFile = root.Element("config").Attribute("secondsSaveFile").Value;
            int.TryParse(strSecondsUpdateButtons, out secondsUpdateKeys);
            int.TryParse(strSecondsSaveFile, out secondsSaveFile);
        }
        void SetKeys(XElement root)
        {
            foreach (var item in root.Element("keys").Elements())
            {
                int count = 0;
                string name = item.Attribute("name").Value;
                string strCount = item.Attribute("count").Value;
                int.TryParse(strCount, out count);

                if (keys.ContainsKey(name)) keys[name] += count;
                else keys.Add(name, count);
            }
        }
        void SaveFile(bool newFile = false)
        {
            XElement xmlRootElement = new XElement("data");
            xmlRootElement.Add(new XElement("config", new XAttribute("secondsUpdateButtons", secondsUpdateKeys), new XAttribute("secondsSaveFile", secondsSaveFile)));
            XElement xmlKeysElement = new XElement("keys");
            if (!newFile)
            {
                foreach (var key in keys)
                    xmlKeysElement.Add(new XElement("key", new XAttribute("name", key.Key), new XAttribute("count", key.Value)));
            }
            xmlRootElement.Add(xmlKeysElement);
            XDocument xmlDocument = new XDocument(xmlRootElement);
            xmlDocument.Save(PATH);
        }
        /// <summary> Вывод клавиш на UI </summary>
        void SetLines()
        {
            if (keys is null) return;
            string[] lines = new string[keys.Count];
            int i = 0;
            foreach (var item in keys.OrderBy(key => key.Value).Reverse())
            {
                lines[i] = $"{item.Key}: {item.Value}";
                i++;
            }
            textBox1.Lines = lines;
        }
        private void SecUpdateKeys_ValueChanged(object sender, EventArgs e) => secondsUpdateKeys = (int)SecUpdateKeys.Value;
        private void SecSaveFile_ValueChanged(object sender, EventArgs e) => secondsSaveFile = (int)SecSaveFile.Value;

        private void ButtonSave_Click(object sender, EventArgs e) => SaveFile();        
    }
}
