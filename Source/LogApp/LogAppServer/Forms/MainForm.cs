using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using LogApp.Common.Model;
using LogAppServer.Logic;
using LogAppServer.Logic.Processing;
using LogAppServer.Properties;

namespace LogAppServer.Forms
{
    public partial class MainForm : Form
    {
        #region Constants
        private const string HelpText = 
@"Для начала работы с серверным приложением необходимо зайти в пункт меню ""Управление"", затем выбрать ""Запустить сервер"". Затем можно подключать клиентские приложения.
Для получения сведений от клиентских приложений необходимо выбрать имя копьютера, от которого пришла информация, затем дождаться получения и вывода в графики данных. 
Если необходимо изменить какие-либо настройки - это можно сделать из соответствующего пункта меню. 
Если вы хотите сделать выборку по времени, необходимо вписать цифры в соответствующие поля, которые распологаются ниже графиков,затем нажать кнопку ""Сделать выборку"".
Если вы хотите сделать выборку по группам, необходимо вписать цифры в соответствующие поля, которые распологаются ниже графиков,затем нажать кнопку ""Сделать выборку"".
После выполнения данной процедуры предполагается, что исходные значения будут возвращены в исходное состояние.";
        #endregion

        #region Data Members
        private SettingsForm settingsForm = new SettingsForm();
        private ModuleViewForm moduleViewForm = new ModuleViewForm();
        private GraphicViewForm pieForm = new GraphicViewForm();
        private readonly LogAppServerController controller;

        private Timer clientsChecking = new Timer();
        private object previousDetails = null;
        #endregion

        #region Constructors
        public MainForm()
        {
            this.InitializeComponent();

            this.statusCombobox.SelectedIndex = 0;
            this.controller = new LogAppServerController(this.settingsForm.Settings);
            this.controller.ServerStatusChanged += controller_ServerStatusChanged;
            this.clientsChecking.Tick += clientChecking_Tick;

            int interval;
            int.TryParse(this.settingsForm.Settings[LogAppServerSettings.ClientsCheckingTime], out interval);
            this.clientsChecking.Interval = interval;
            this.clientsChecking.Start();

            this.settingsForm.SettingsChanged += settingsForm_SettingsChanged;
        }
        #endregion

        #region Event Handlers
        private void clientsListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            string currentClient;
            bool clientChanged = this.CheckClientChanging(out currentClient);
            ClientInfo info = this.controller.Clients
                .FirstOrDefault(item => item.Name == currentClient);
            if (info == null)
            {
                this.clientDetailsGridView.DataSource = null;
                return;
            }

            if (clientChanged)
            {
                this.UpdateDataGridInfo(info);
                this.previousDetails = currentClient;
            }
            else
            {
                int row = this.clientDetailsGridView.FirstDisplayedScrollingRowIndex;
                this.UpdateDataGridInfo(info);
                if (row != -1)
                {
                    this.clientDetailsGridView.FirstDisplayedScrollingRowIndex = row;
                }
            }
        }
        private void clientChecking_Tick(object sender, EventArgs eventArgs)
        {
            try
            {
                object selected = this.clientsListBox.SelectedItem;
                string name = selected != null ? selected.ToString() : String.Empty;

                object[] clients = this.controller.Clients
                    .Select(item => item.Name)
                    .ToArray();

                this.clientsListBox.Items.Clear();
                this.clientsListBox.Items.AddRange(clients);

                if (!clients.Any())
                {
                    this.clientDetailsGridView.DataSource = null;
                }
                else
                {
                    if (this.clientsListBox.Items.Contains(name))
                    {
                        this.clientsListBox.SelectedItem = name;
                    }
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Ошибка!", "Не удалось обновить информацию о клиентах.");
            }
        }
        private void controller_ServerStatusChanged(object sender, System.EventArgs e)
        {
            this.statePictureBox.Image = this.controller.IsRunning ? Resources.z1 : Resources.r1;
        }
        private void settingsForm_SettingsChanged(object sender, System.EventArgs e)
        {
            this.controller.ResetServer();
        }
        private void стартToolStripMenuItem_Click(object sender, System.EventArgs e)
        {
            this.controller.StartServer();
        }
        private void стопToolStripMenuItem_Click(object sender, System.EventArgs e)
        {
            this.controller.StopServer();
        }
        private void настройкиToolStripMenuItem_Click(object sender, System.EventArgs e)
        {
            this.settingsForm.ShowDialog();
        }
        private void выходToolStripMenuItem_Click(object sender, System.EventArgs e)
        {
            this.Close();
        }
        private void clientDetailsGridView_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            DataGridViewSelectedRowCollection selected = this.clientDetailsGridView.SelectedRows;
            if (selected.Count > 0)
            {
                DataGridViewRow row = selected[0];
                ApplicationInfo info = (ApplicationInfo)row.DataBoundItem;
                this.moduleViewForm.Modules = info.Modules;
                this.moduleViewForm.ShowDialog();
            }
        }
        private void disconnectButton_Click(object sender, EventArgs e)
        {
            String name = this.clientsListBox.SelectedItem != null ?
                this.clientsListBox.SelectedItem.ToString() :
                String.Empty;
            ClientInfo info = this.controller.Clients
                .FirstOrDefault(client => client.Name == name);
            if (info != null)
            {
                this.controller.DisconnectClient(info);
            }
        }
        private void graphicButton_Click(object sender, EventArgs e)
        {
            object selected = this.clientsListBox.SelectedItem;
            string current = selected != null
                        ? selected.ToString()
                        : String.Empty;
            ClientInfo info = this.controller.Clients
                .FirstOrDefault(item => item.Name == current);
            if (info == null)
            {
                MessageBox.Show("Ошибка!", "Необходимо выбрать клиента!");
                return;
            }

            this.pieForm.State = info.CurrentState;
            this.pieForm.ShowDialog();
        }
        private void справкаToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show(MainForm.HelpText, "Помощь");
        }
        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.clientsChecking.Dispose();
            this.controller.Dispose();
        }
        #endregion

        #region Assistants
        private bool CheckClientChanging(out string current)
        {
            object selected = this.clientsListBox.SelectedItem;
            current = selected != null
                ? selected.ToString()
                : String.Empty;
            string previous = this.previousDetails != null
                ? this.previousDetails.ToString()
                : String.Empty;
            return current != previous;
        }
        private void UpdateDataGridInfo(ClientInfo info)
        {
            this.clientDetailsGridView.DataSource = null;
            List<ApplicationInfo> list;
            switch (this.statusCombobox.SelectedIndex)
            {
                case 0:
                    list = info.CurrentState.ToList();
                    break;
                case 1:
                    list = info.CurrentState.Where(item => item.IsRunning).ToList();
                    break;
                case 2:
                    list = info.CurrentState.Where(item => !item.IsRunning).ToList();
                    break;
                default:
                    list = new List<ApplicationInfo>();
                    break;
            }

            list.Sort(new ApplicationInfoComparer());
            this.processBindingSource.DataSource = list;
            this.clientDetailsGridView.DataSource = this.processBindingSource;
        }
        #endregion
    }

    internal class ApplicationInfoComparer : IComparer<ApplicationInfo>
    {
        public int Compare(ApplicationInfo x, ApplicationInfo y)
        {
            return (x.StartTime > y.StartTime) ?
                -1 :
                (x.StartTime == y.StartTime) ? 0 : 1;
        }
    }
}
