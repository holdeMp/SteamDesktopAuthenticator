using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Steam_Desktop_Authenticator
{
    public partial class InputForm : Form
    {
        public bool Canceled = false;
        private bool userClosed = true;

        public InputForm(string label, bool password = false)
        {
            InitializeComponent();
            labelText.Text = label;

            if (password)
            {
                txtBox.PasswordChar = '*';
            }
        }

        private void btnAccept_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtBox.Text))
            {
                Canceled = true;
                userClosed = false;
                Close();
            }
            else
            {
                Canceled = false;
                userClosed = false;
                Close();
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            Canceled = true;
            userClosed = false;
            Close();
        }

        private void InputForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (userClosed)
            {
                // Set Canceled = true when the user hits the X button.
                Canceled = true;
            }
        }
    }
}
