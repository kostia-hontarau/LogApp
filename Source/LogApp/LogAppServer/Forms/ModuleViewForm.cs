using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using LogApp.Common.Model;

namespace LogAppServer.Forms
{
    public partial class ModuleViewForm : Form
    {
        #region Properties
        public List<ModuleInfo> Modules { get; set; }
        #endregion

        #region Constructors
        public ModuleViewForm()
        {
            InitializeComponent();
        } 
        #endregion

        #region Event Handlers
        private void okButton_Click(object sender, EventArgs e)
        {
            this.Hide();
        }
        private void ModuleViewForm_Shown(object sender, EventArgs e)
        {
            this.modulesListBox.Items.Clear();
            if (this.Modules != null)
            {
                this.modulesListBox.Items.AddRange(this.Modules
                    .Select(item => item.Name)
                    .ToArray()
                    );
            }
            else
            {
                this.modulesListBox.Items.Add("Нет подключенных библиотек!");
            }
        } 
        #endregion
    }
}
