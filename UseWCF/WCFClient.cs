using System;
using System.Drawing;
using System.Windows.Forms;
using System.ServiceModel;
using WCFService;
using System.IO;
using System.Threading;

namespace UseWCF
{
    public partial class WCFClient : Form
    {
        private string _userName;
        private int selectedFlag = 0;
        private int selectedIndex = -1;

        static CallBack back = new CallBack();
        static InstanceContext context = new InstanceContext(back);
        netTCPServiceReference.Service1Client service = new netTCPServiceReference.Service1Client(context);
        
        public WCFClient()
        {
            InitializeComponent();

            btnLogin.Enabled = false;
            btnChat.Enabled = false;
            btnFile.Enabled = false;
            rtbHistory.Enabled = false;
            lsbUserList.Enabled = false;
            txtChat.Enabled = false;

            back.SetForm(this); // Let CallBack class konw what it is
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            txtUserName.Enabled = false;
            btnLogin.Enabled = false;

            btnChat.Enabled = true;
            btnFile.Enabled = true;
            rtbHistory.Enabled = true;
            lsbUserList.Enabled = true;
            txtChat.Enabled = true;

            _userName += txtUserName.Text;
            lsbUserList.Items.Add("Service");
            lsbUserList.Items.Add(_userName);
            rtbHistory_ShowLoginMessage(_userName);           

            service.Register(_userName);

            this.Text = _userName;
        }

        private void btnChat_Click(object sender, EventArgs e)
        {
            Person person = new Person();
            string selectedItem = "";

            person.UserName = _userName;
            person.ChatContent = txtChat.Text;

            if (lsbUserList.SelectedItem != null)
            {
                selectedItem = lsbUserList.SelectedItem.ToString();

                service.SendToOtherClients(person, selectedItem);

                rtbHistory.SelectionColor = Color.Green;
                if (selectedItem == "Service")
                {
                    rtbHistory.AppendText(person.UserName + "[to Service] : " + person.ChatContent + "\n");
                }
                else
                {
                    rtbHistory.AppendText(person.UserName + "[to " + selectedItem + "] : " + person.ChatContent + "\n");
                }
                rtbHistory.SelectionColor = Color.Black;
            }
            else
            {
                service.SendToOtherClients(person, "");
                rtbHistory.AppendText(person.UserName + " : " + person.ChatContent + "\n");
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
            rtbHistory.ReadOnly = true;
            rtbHistory.BackColor = Color.White;
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

        private void btnFile_Click(object sender, EventArgs e)
        {
            string filePath = OpenDialog();
            if (filePath != "")
            {
                ClientFile clientFile = new ClientFile();
                clientFile.ClientName = txtUserName.Text;
                clientFile.Buffer = new byte[100000];
                string[] splitString = filePath.Split('\\');
                FileInfo fileInfo = new FileInfo(filePath); // Get file Length
                MyFileInfo sendFileInfo = new MyFileInfo(File.OpenRead(filePath), splitString[splitString.Length - 1], fileInfo.Length);
                bool isChangeFileName = false;
                long totalBytesRead = 0;
                
                do
                {
                    clientFile.BytesRead = sendFileInfo.Stream.Read(clientFile.Buffer, 0, clientFile.Buffer.Length);
                    clientFile.FileName = sendFileInfo.FileName;
                    clientFile.BufferSize = clientFile.Buffer.Length;

                    service.ReceiveFile(clientFile, isChangeFileName);

                    isChangeFileName = true;
                    totalBytesRead += clientFile.BytesRead;
                    double currentProgress = (((double)totalBytesRead) / sendFileInfo.FileSize) * 100;
                    pgbReadFile.Value = Convert.ToInt32(currentProgress);
                    
                } while (totalBytesRead != sendFileInfo.FileSize);//== buffer.Length

                rtbHistory.AppendText("Transport successful\n");
                //service.ReceiveFile(clientFile, finishFlag);
            }
        }

        #region Send Message Type
        public void ServiceBroadCastMessage(Person person)
        {
           rtbHistory.SelectionColor = Color.Blue;
           rtbHistory.AppendText(person.UserName + " : " + person.ChatContent + "\n");
           rtbHistory.SelectionColor = Color.Black;
        }
        public void ServiceToSpecificClientMessage(Person person)
        {
            rtbHistory.SelectionColor = Color.Red;
            rtbHistory.AppendText(person.UserName + "[to you] : " + person.ChatContent + "\n");
            rtbHistory.SelectionColor = Color.Black;
        }
        public void ClientBroadCastMessage(Person person)
        {
            rtbHistory.AppendText(person.UserName + " : " + person.ChatContent + "\n");
        }
        public void ClientPersonalMessage(Person person)
        {
            rtbHistory.SelectionColor = Color.Green;
            rtbHistory.AppendText(person.UserName + "[to you] : " + person.ChatContent + "\n");
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

        private string OpenDialog()
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Title = "Select file";
            dialog.InitialDirectory = ".\\";
            dialog.Filter = "all files (*.*)|*.*";

            if (dialog.ShowDialog() == DialogResult.OK)
                return dialog.FileName;
            else
                return "";
        }
    }

    public class CallBack : netTCPServiceReference.IService1Callback
    {
        #region IServicesCallback Member
        WCFClient _wcfForm;

        public void SetForm(WCFClient wcfForm) // Let CallBack knows what is WCFForm
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
            switch(contentType)
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
