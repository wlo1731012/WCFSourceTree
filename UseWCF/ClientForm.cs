using System;
using System.Drawing;
using System.Windows.Forms;
using System.ServiceModel;
using WCFService;
using System.IO;
using System.Threading;
using System.Collections.Generic;
using System.Diagnostics;
using System.Data;
using WCFClient;

namespace WCFClient
{
    public partial class ClientForm : Form//WCFClient
    {
        private string _userName;
        private int selectedFlag = 0;
        private int selectedIndex = -1;

        public FileStream fileStream;

        static CallBack back = new CallBack();
        static InstanceContext context = new InstanceContext(back);
        netTCPServiceReference.Service1Client service = new netTCPServiceReference.Service1Client(context);
        
        public ClientForm()
        {
            InitializeComponent();

            //threadA = Service1.;

            btnLogin.Enabled = false;
            btnChat.Enabled = false;
            btnUpload.Enabled = false;
            btnDownload.Enabled = false;
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
            btnUpload.Enabled = true;
            btnDownload.Enabled = true;
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

        private void btnUpload_Click(object sender, EventArgs e)
        {
            string filePath = OpenDialog();
            
            if (filePath != "")
            {
                int roundCount = 0;
                int timesCount = 0;
                Stopwatch sw_total = new Stopwatch();
                Stopwatch sw_step = new Stopwatch();
                string usingTime = "";
                List<string> usingTimeList = new List<string>();

                ClientFile clientFile = new ClientFile();
                clientFile.ClientName = txtUserName.Text;
                int bufferSize = 3000000;

                for (int i = 0; i < 3; i++)
                {
                    sw_total.Reset();
                    sw_step.Reset();
                    sw_total.Start();
                    sw_step.Start();

                    roundCount = 0;

                    bool isChangeFileName = false;
                    long totalBytesRead = 0;
                    double currentProgress = 0;
                    
                    string[] splitString = filePath.Split('\\');
                    FileInfo fileInfo = new FileInfo(filePath); // Get file Length
                    MyFileInfo sendFileInfo = new MyFileInfo(File.OpenRead(filePath), splitString[splitString.Length - 1], fileInfo.Length);

                    clientFile.Buffer = new byte[bufferSize]; // In C#, if u give byte array a new assign, it will release orgin space that u used automatically by GC(Garbage Collection).
                    
                    timesCount++;
                    rtbHistory.AppendText("\n-----No. " + timesCount.ToString() + " transport " + sendFileInfo.FileName + "-----\n\n");
                    sw_step.Stop();
                    usingTime = sw_step.Elapsed.TotalMilliseconds.ToString();
                    rtbHistory.AppendText("Assign Time : "+ usingTime + "\n");

                    do
                    {
                        sw_step.Reset();
                        sw_step.Start();

                        clientFile.BytesRead = sendFileInfo.Stream.Read(clientFile.Buffer, 0, clientFile.Buffer.Length);
                        clientFile.FileName = sendFileInfo.FileName;
                        clientFile.isFinsishFlag = false;

                        service.ReceiveFile(clientFile, isChangeFileName);

                        isChangeFileName = true;
                        totalBytesRead += clientFile.BytesRead;

                        if (sendFileInfo.FileSize != 0)
                            currentProgress = (((double)totalBytesRead) / sendFileInfo.FileSize) * 100;
                        else
                            currentProgress = 100;

                        pgbReadFile.Value = Convert.ToInt32(currentProgress);
                        if (currentProgress == 100)
                        {
                            clientFile.isFinsishFlag = true;
                            service.ReceiveFile(clientFile, isChangeFileName);
                        }

                        roundCount++;
                        sw_step.Stop();
                        usingTime = sw_step.Elapsed.TotalMilliseconds.ToString();
                        rtbHistory.AppendText("Round " + roundCount.ToString() + " Time : " + usingTime + "\n");

                    } while (currentProgress != 100);//== buffer.Length
                    
                    sw_total.Stop();
                    usingTime = sw_total.Elapsed.TotalMilliseconds.ToString();
                    rtbHistory.AppendText("\nUsing total time : " + usingTime + "\n");//"Transport" + clientFile .FileName + " successful\nTotal using time : " +  ms
                    usingTimeList.Add(usingTime);
                    //Array.Clear(clientFile.Buffer, 0, bufferSize);
                }

                rtbHistory.AppendText("\n---------------------------------------------------------\n");
                for (int i = 1; i < usingTimeList.Count+1; i++)
                {
                    rtbHistory.AppendText("-No. " + i.ToString() + " transport " + usingTimeList[i-1] + " ms-\n");
                }

                back.SetForm(this);
                //service.ReceiveFile(clientFile, finishFlag);
            }
        }

        public static DataSet serviceFileList = new DataSet();
        private void btnDownload_Click(object sender, EventArgs e)
        {
            ServiceFile ClientAsked = new ServiceFile();
            serviceFileList = service.GetFileList("D:\\Folder1"); // Set folder that make client READ

            ServiceFileListForm serviceFileListForm = new ServiceFileListForm();
            serviceFileListForm.ShowDialog();

            if (serviceFileListForm.selectedFile != "")
            {
                ClientAsked.FilePath = "D:\\Folder1\\" + serviceFileListForm.selectedFile;

                FolderBrowserDialog browsePath = new FolderBrowserDialog(); // Open a dialog make client select where to save file
                browsePath.ShowDialog();
                string saveFilePath = browsePath.SelectedPath;

                if (saveFilePath != "")
                {
                    saveFilePath += "\\" + serviceFileListForm.selectedFile;
                    double currentProgress = 0;
                    int tillCurrentBytesRead = 0; // Current sum of bytes been read
                    ClientAsked.BufferSize = 30000;
                    ClientAsked.isFirstTime = true; // Make file open until read file over

                    fileStream = new FileStream(saveFilePath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite); // Accoring to saveFilePath create a file
                    do
                    {
                        service.SendFile(ref ClientAsked); // Call Service1.cs function make ClientAsked.Buffer get part of file information
                        fileStream.Write(ClientAsked.Buffer, 0, ClientAsked.Buffer.Length); // Write  part of file information into File

                        tillCurrentBytesRead += ClientAsked.BytesRead;
                        currentProgress = (((double)tillCurrentBytesRead) / ClientAsked.FileSize) * 100;
                        pgbReadFile.Value = Convert.ToInt32(currentProgress);
                    } while (currentProgress != 100);

                    fileStream.Close(); // Close File makes it rest in peace
                }
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

        private string OpenDialog()
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Title = "Select file";
            dialog.InitialDirectory = @"D:\";
            dialog.Filter = "all files(*.*)|*.*";

            if (dialog.ShowDialog() == DialogResult.OK)
                return dialog.FileName;
            else
                return "";
        }
       
        #region CallBack function using

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

        public void UpdateDownloadFile(ServiceFile serviceFile, double currentProgress)
        {
            pgbReadFile.Value = Convert.ToInt32(currentProgress);
        }

        #endregion
    }

    public class CallBack : netTCPServiceReference.IService1Callback
    {
        #region IServicesCallback Member
        ClientForm _wcfForm;

        public void SetForm(ClientForm wcfForm) // Let CallBack knows what is WCFForm
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

        public void UpdateDownloadFile(ServiceFile serviceFile, double currentProgress)
        {
            _wcfForm.UpdateDownloadFile(serviceFile, currentProgress);
        }

        #endregion
    }
}
