using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Windows.Forms;
using LogApp.Logic;


namespace LogApp.Forms
{
    public partial class SettingsForm : Form
    {
        #region Data Members
        private readonly Dictionary<string, string> settings = new Dictionary<string, string>();
        #endregion

        #region Properties
        public IReadOnlyDictionary<String, String> Settings
        {
            get { return this.settings; }    
        } 
        #endregion

        #region Events
        public event EventHandler SettingsChanged;
        #endregion

        #region Constructors
        public SettingsForm()
        {
            this.InitializeComponent();

            this.SetupSaveFileDialog();
            this.LoadSettings();
        }
        #endregion

        #region Event Handlers
        private void SettingsForm_Shown(object sender, EventArgs e)
        {
            this.serverIpTextBox.Text = this.settings[LogAppSettings.ServerIp];
            this.serverPortTextBox.Text = this.settings[LogAppSettings.ServerPort];
            this.fileLabel.Text = this.settings[LogAppSettings.StorageFilePath];
        }
        private void buttonSave_Click(object sender, EventArgs e)
        {
            this.SaveSettings();
            this.Hide();
        }
        private void buttonCancel_Click(object sender, EventArgs e)
        {
            this.Hide();
        }
        private void buttonExplore_Click(object sender, EventArgs e)
        {
            if (this.saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                if (this.saveFileDialog.CheckPathExists)
                {
                    this.fileLabel.Text = this.saveFileDialog.FileName;
                }
                else
                {
                    MessageBox.Show("Ошибка!", "Такой директории не существует!");
                }
            }
        }
        #endregion

        #region Assistants
        private void OnSettingsChanged()
        {
            EventHandler handler = SettingsChanged;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        private void SetupSaveFileDialog()
        {
            this.saveFileDialog.Title = @"Выберите файл для хранения данных";
            this.saveFileDialog.InitialDirectory = @"C:\";
            this.saveFileDialog.Filter = @"Json files (*.json)|*.json|All files|*.*";
        }
        private void LoadSettings()
        {
            this.settings.Add(LogAppSettings.ServerIp, Properties.Settings.Default.ServerIP);
            this.settings.Add(LogAppSettings.ServerPort, Properties.Settings.Default.ServerPort);
            this.settings.Add(LogAppSettings.StorageFilePath, Properties.Settings.Default.StorageFilePath);
            this.settings.Add(LogAppSettings.FileUpdateTime, Properties.Settings.Default.FileUpdateTime);
            this.settings.Add(LogAppSettings.ProcessStateUpdateTime, Properties.Settings.Default.ProcessStateUpdateTime);
        }
        private void SaveSettings()
        {
            this.WriteIpChanges();
            this.WritePortChanges();
            this.WriteFileChanges();
            Properties.Settings.Default.Save();
            Properties.Settings.Default.Upgrade();
            this.ReloadSettings();
            this.OnSettingsChanged();
        }
        private void ReloadSettings()
        {
            this.settings.Clear();
            this.LoadSettings();
        }
        private void WriteIpChanges()
        {
            IPAddress address;
            bool succeeded = IPAddress.TryParse(this.serverIpTextBox.Text, out address);
            if (succeeded)
            {
                Properties.Settings.Default.ServerIP = address.ToString();
            }
            else
            {
                MessageBox.Show("Ошибка!", "Введён некорректный IP-адрес!");
            }
        }
        private void WritePortChanges()
        {
            int port;
            bool succeeded = int.TryParse(this.serverPortTextBox.Text, out port);
            if (succeeded && port >= IPEndPoint.MinPort && port <= IPEndPoint.MaxPort)
            {
                Properties.Settings.Default.ServerPort = port.ToString(CultureInfo.InvariantCulture);
            }
            else
            {
                MessageBox.Show("Ошибка!", "Введён некорректный порт!");
            }
        }
        private void WriteFileChanges()
        {
            Properties.Settings.Default.StorageFilePath = this.fileLabel.Text;
        }
        #endregion
    }
}
