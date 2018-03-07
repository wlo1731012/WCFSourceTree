using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.ServiceModel;
using UseWCF2.netTCPServiceReference;

namespace UseWCF2
{
    public partial class WCF2Form : Form
    {
        private string _userName;
        private int selectedFlag = 0;
        private int selectedIndex = -1;

        static CallBack back = new CallBack();
        static InstanceContext context = new InstanceContext(back);
        netTCPServiceReference.Service1Client client = new netTCPServiceReference.Service1Client(context);

        public WCF2Form()
        {
            InitializeComponent();

            btnLogin.Enabled = false;
            btnChat.Enabled = false;
            rtbHistory.Enabled = false;
            lsbUserList.Enabled = false;
            txtChat.Enabled = false;

            back.SetForm(this);
        }
                
        private void btnLogin_Click(object sender, EventArgs e)
        {
            txtUserName.Enabled = false;
            btnLogin.Enabled = false;

            btnChat.Enabled = true;
            rtbHistory.Enabled = true;
            lsbUserList.Enabled = true;
            txtChat.Enabled = true;

            _userName += txtUserName.Text;
            lsbUserList.Items.Add("Service");
            lsbUserList.Items.Add(_userName);
            rtbHistory_ShowLoginMessage(_userName);

            client.Register(_userName);

            this.Text = _userName;
        }

        private void btnChat_Click(object sender, EventArgs e)
        {
            Person person = new Person();
            string selectedItem = "";
            person._userName = _userName;
            person._chatContent = txtChat.Text;
            //rtbHistory.AppendText(client.BasicTalking(person) + "\n");

            if (lsbUserList.SelectedItem != null)
            {
                selectedItem = lsbUserList.SelectedItem.ToString();

                client.SendToOtherClients(person, selectedItem);

                rtbHistory.SelectionColor = Color.Green;
                if (selectedItem == "Service")
                {
                    rtbHistory.AppendText(person._userName + "[to Service] : " + person._chatContent + "\n");
                }
                else
                {
                    rtbHistory.AppendText(person._userName + "[to " + selectedItem + "] : " + person._chatContent + "\n");
                }
                rtbHistory.SelectionColor = Color.Black;
            }
            else
            {
                client.SendToOtherClients(person, "");
                rtbHistory.AppendText(person._userName + " : " + person._chatContent + "\n");
            }
        }

        private void txtUserName_TextChanged(object sender, EventArgs e)
        {
            if (txtUserName.Text != "")
            {
                btnLogin.Enabled = true;
            }
            else
            {
                btnLogin.Enabled = false;
            }
        }

        private void rtbHistory_TextChanged(object sender, EventArgs e)
        {
            rtbHistory.SelectionStart = rtbHistory.TextLength;
            rtbHistory.ScrollToCaret();
        }

        private void lsbUserList_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lsbUserList.SelectedItem != null && lsbUserList.SelectedItem.ToString() == _userName)
                lsbUserList.ClearSelected();
            else if (lsbUserList.SelectedIndex == selectedIndex && selectedFlag == 1)
            {
                lsbUserList.SelectedIndex = -1;
                selectedFlag = 0;
            }
            else
            {
                selectedIndex = lsbUserList.SelectedIndex;
                selectedFlag = 1;
            }
        }

        private void rtbHistory_ShowLoginMessage(string userName)
        {
            if (userName != "")
            {
                rtbHistory.SelectionColor = Color.ForestGreen;
                rtbHistory.AppendText("User ");
                rtbHistory.SelectionColor = Color.DarkBlue;
                rtbHistory.AppendText(userName);
                rtbHistory.SelectionColor = Color.ForestGreen;
                rtbHistory.AppendText(" has been login.\n");
                rtbHistory.SelectionColor = Color.Black;
            }
        }

        #region Send Message Type
        public void ServiceBroadCastMessage(Person person)
        {
           rtbHistory.SelectionColor = Color.Blue;
           rtbHistory.AppendText(person._userName + " : " + person._chatContent + "\n");
           rtbHistory.SelectionColor = Color.Black;
        }
        public void ServiceToSpecificClientMessage(Person person)
        {
            rtbHistory.SelectionColor = Color.Red;
            rtbHistory.AppendText(person._userName + "[to you] : " + person._chatContent + "\n");
            rtbHistory.SelectionColor = Color.Black;
        }
        public void ClientBroadCastMessage(Person person)
        {
            rtbHistory.AppendText(person._userName + " : " + person._chatContent + "\n");
        }
        public void ClientPersonalMessage(Person person)
        {
            rtbHistory.SelectionColor = Color.Green;
            rtbHistory.AppendText(person._userName + "[to you] : " + person._chatContent + "\n");
            rtbHistory.SelectionColor = Color.Black;
        }
        #endregion

        public void UpdateUserList(string[] userList)
        {
            lsbUserList.Items.Clear();
            lsbUserList.Items.Add("Service");
            foreach (string user in userList)
            {
                if (!lsbUserList.Items.Contains(user)) // Avoid add userName repeatly
                    lsbUserList.Items.Add(user);
            }
        }

    }
    public class CallBack : netTCPServiceReference.IService1Callback
    {
        #region IServicesCallback Member
        WCF2Form _wcfForm;

        public void SetForm(WCF2Form wcfForm) // Let CallBack knows what is WCFForm
        {
            _wcfForm = wcfForm;
        }

        // contentType
        // 0 = Service to everyone
        // 1 = Service to Specific client
        // 2 = Client to everyone
        // 3 = Client to Specific client or Service
        public void SendMessage(Person person, int contentType)
        {
            switch (contentType)
            {
                case 0:
                    _wcfForm.ServiceBroadCastMessage(person);
                    break;
                case 1:
                    _wcfForm.ServiceToSpecificClientMessage(person);
                    break;
                case 2:
                    _wcfForm.ClientBroadCastMessage(person);
                    break;
                case 3:
                    _wcfForm.ClientPersonalMessage(person);
                    break;
            }
        }

        public void UpdateUserList(string[] userList)
        {
            _wcfForm.UpdateUserList(userList);
        }
        #endregion
    }
}
