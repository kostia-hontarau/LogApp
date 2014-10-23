using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Windows.Forms;
using LogAppServer.Logic;

namespace LogAppServer.Forms
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
            this.LoadSettings();
        }
        #endregion

        #region Event Handlers
        private void SettingsForm_Shown(object sender, EventArgs e)
        {
            this.SetupIpComboBox();
            this.portTextBox.Text = this.settings[LogAppServerSettings.ServerPort];
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
        #endregion

        #region Assistants
        private void OnSettingsChanged()
        {
            EventHandler handler = SettingsChanged;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        private void LoadSettings()
        {
            this.settings.Add(LogAppServerSettings.ServerIp, Properties.Settings.Default.ServerIp);
            this.settings.Add(LogAppServerSettings.ServerPort, Properties.Settings.Default.ServerPort);
            this.settings.Add(LogAppServerSettings.ConnectionCheckingTime, Properties.Settings.Default.ConnectionCheckingTime);
            this.settings.Add(LogAppServerSettings.ClientsCheckingTime, Properties.Settings.Default.ClientsCheckingTime);

        }
        private void SaveSettings()
        {
            this.WriteIpChanges();
            this.WritePortChanges();

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
            IPAddress address = (IPAddress)this.IpComboBox.SelectedItem;
            Properties.Settings.Default.ServerIp = address.ToString();
        }
        private void WritePortChanges()
        {
            int port = 0;
            bool succeeded = int.TryParse(this.portTextBox.Text, out port);
            if (succeeded && port >= IPEndPoint.MinPort && port <= IPEndPoint.MaxPort)
            {
                Properties.Settings.Default.ServerPort = port.ToString(CultureInfo.InvariantCulture);
            }
            else
            {
                MessageBox.Show("Ошибка!", "Введён некорректный порт!");
            }
        }

        private void SetupIpComboBox()
        {
            IPAddress currentIp;
            IPAddress.TryParse(this.settings[LogAppServerSettings.ServerIp], out currentIp);
            if (currentIp == null)
            {
                currentIp = IPAddress.Any;
            }
             
            IPHostEntry localMachine = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress[] addresses = localMachine.AddressList
                .Where(item => item.AddressFamily == AddressFamily.InterNetwork)
                .ToArray();

            this.IpComboBox.Items.Clear();
            this.IpComboBox.Items.AddRange(addresses);
            this.IpComboBox.SelectedItem = addresses
                .FirstOrDefault(item => item.ToString() == currentIp.ToString());
        }
        #endregion
    }
}
