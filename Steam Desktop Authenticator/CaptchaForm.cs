using System;
using System.Windows.Forms;

namespace Steam_Desktop_Authenticator
{
    public partial class CaptchaForm : Form
    {
        public bool Canceled = false;
        public string CaptchaGID = "";
        public string CaptchaURL = "";
        public string CaptchaCode
        {
            get
            {
                return txtBox.Text;
            }
        }

        public CaptchaForm(string GID)
        {
            CaptchaGID = GID;
            CaptchaURL = "https://steamcommunity.com/public/captcha.php?gid=" + GID;
            InitializeComponent();
            pictureBoxCaptcha.Load(CaptchaURL);
        }

        private void btnAccept_Click(object sender, EventArgs e)
        {
            Canceled = false;
            Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            Canceled = true;
            Close();
        }
    }
}
