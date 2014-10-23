using System;
using System.Windows.Forms;
using LogApp.Logic;

namespace LogApp.Forms
{
    public partial class MainForm : Form
    {
        #region Constants
        public const int CheckClientInterval = 10000;
        public const int SendInfoInterval = 5000;
        #endregion

        #region Data Members
        private ConnectionInfoForm connectionInfoForm = new ConnectionInfoForm();
        private SettingsForm settingsForm = new SettingsForm();
        private LogAppController controller;
        private Timer checkConnectionTimer = new Timer();
        private Timer sendInfoTimer = new Timer();
        #endregion

        #region Properties
        bool IsVisibilityChangeAllowed { get; set; }
        #endregion

        #region Constructors
        public MainForm()
        {
            this.InitializeComponent();
            this.SetupForm();
            this.checkConnectionTimer.Interval = MainForm.CheckClientInterval;
            this.checkConnectionTimer.Tick += CheckConnectionTimerOnTick;
            this.sendInfoTimer.Interval = MainForm.SendInfoInterval;
            this.sendInfoTimer.Tick += SendInfoTimerOnTick;

            this.settingsForm.SettingsChanged += SettingsFormOnSettingsChanged;

            this.controller = new LogAppController(this.settingsForm.Settings);
            this.controller.ConnectToServer();
            this.controller.StartLogging();

            this.checkConnectionTimer.Start();
            this.sendInfoTimer.Start();
        }
        #endregion

        #region Overridden Members
        protected override void SetVisibleCore(bool value)
        {
            if (this.IsVisibilityChangeAllowed)
            {
                base.SetVisibleCore(value);
            }
        }
        #endregion

        #region Event Handlers
        private void Main_Load(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
            this.Hide();
        }
        private void выходToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.settingsForm.Close();
            this.connectionInfoForm.Close();
            this.Close();
            Application.Exit();
        }
        private void сведенияОСоединенииToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.connectionInfoForm.ConnectionInfo = this.controller.ConnectionInfo;
            this.connectionInfoForm.Show();
        }
        private void настройкаToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.settingsForm.Show();
        }
        private void CheckConnectionTimerOnTick(object sender, EventArgs eventArgs)
        {
            if (!this.controller.IsConnectedToServer)
            {
                this.controller.ConnectToServer();
            }
        }
        private void SendInfoTimerOnTick(object sender, EventArgs eventArgs)
        {
            this.controller.SendInfoToServer();
        }
        private void SettingsFormOnSettingsChanged(object sender, EventArgs eventArgs)
        {
            this.sendInfoTimer.Stop();
            this.checkConnectionTimer.Stop();

            this.controller.ResetConnection();
            this.controller.ResetLogging();

            this.checkConnectionTimer.Start();
            this.sendInfoTimer.Start();
        }
        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.controller.Dispose();
        }
        #endregion

        #region Assistants
        private void SetupForm()
        {
            this.Visible = false;
            this.IsVisibilityChangeAllowed = false;
            this.notifyIcon.Visible = true;
        }
        #endregion
    }
}
