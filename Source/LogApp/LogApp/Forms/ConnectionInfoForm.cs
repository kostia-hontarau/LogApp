using System;
using System.Windows.Forms;

namespace LogApp.Forms
{
    public partial class ConnectionInfoForm : Form
    {
        #region Constructors
        public ConnectionInfoForm()
        {
            this.InitializeComponent();
        } 
        #endregion

        #region Properties
        public string ConnectionInfo { get; set; }
        #endregion

        #region Event Handlers
        private void okButton_Click(object sender, EventArgs e)
        {
            this.Hide();
        } 
        #endregion

        private void ConnectionInfoForm_Shown(object sender, EventArgs e)
        {
            this.label1.Text = this.ConnectionInfo;
        }
    }
}
