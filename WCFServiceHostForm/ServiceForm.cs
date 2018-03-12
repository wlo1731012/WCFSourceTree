using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.ServiceModel;
using WCFService;
using System.Threading;
using System.IO;

namespace WCFServiceHostForm//WCFServiceHostForm
{
    public partial class ServiceForm : Form
    {
        public ServiceForm()
        {
            InitializeComponent();
        }
        private static object InstObj = new object();
        private int selectedFlag = 0;
        private int selectedIndex = -1;

        private void Form1_Load(object sender, EventArgs e)
        {
            ServiceHost host = new ServiceHost(typeof(Service1));
            host.Open(); // Start Listening
            
            Service1.listenerHandler_ReceiveFile += new Service1.ListenerHandler_ReceiveFile(ReceiveFile);
            Service1.listenerHandler_SendFile += new Service1.ListenerHandler_SendFile(SendFile);
            Service1.listenerHandler_Test += new Service1.ListenerHandler_Test(Test);
            Service1.listenerHandler_sendToOtherClients += new Service1.ListenerHandler_SendToOtherClients(SendToOtherClient);
            #region Thread Working

            Thread threadReceiveMessage = new Thread(new ThreadStart(delegate   //Listen all clients chat content
            {
                lock (InstObj)//Lock for multiple
                {
                    while (true)
                    {
                        if (Service1.isSendMessage == true) // Listen chat
                        {
                            SendToOtherClient(Service1.SendMessenger, Service1.ReceiveMessenger);
                        }
                        else if (Service1.isNewUser == true) // Listen login
                        {
                            AddNewUser();
                        }
                        //else if (Service1.isTest == true)
                        //{
                        //    Test();
                        //}

                        Thread.Sleep(100);
                    }
                }
            }));
            threadReceiveMessage.IsBackground = true;
            threadReceiveMessage.Start();

            #endregion
        }
        
        private void AddNewUser()
        {
            string sessionID = "";
            List<string> userList = new List<string>();
            if (Service1.DicIDAndCB != null || Service1.DicIDAndCB.Count > 0)
            {
                this.Invoke(new MethodInvoker(delegate { this.lsbUserList.Items.Clear(); })); // Clear listbox
                foreach (var userName in Service1.DicNameAndID) // id.key = userName, id.Value = sessionID
                {
                    userList.Add(userName.Key);

                    this.Invoke(new MethodInvoker(delegate
                    {
                        this.lsbUserList.Items.Add(userName.Key); // UserList put in Service
                    }));
                }

                foreach (var users in Service1.DicIDAndCB) // Update all client's list
                {
                    sessionID = users.Key;
                    Service1.DicIDAndCB[sessionID].UpdateUserList(userList);
                }
            }
            Service1.isNewUser = false; // Initialize
        }

        private void SendToOtherClient(Person person, string specificClient)
        {
            string sessionID = Service1.DicNameAndID[person.UserName];
            if (specificClient != "")
            {
                if (specificClient == "Service")
                {
                    this.Invoke(new MethodInvoker(delegate
                    {
                        this.rtbHistory.SelectionColor = Color.Green;
                        this.rtbHistory.AppendText(person.UserName + "[to you] : " + person.ChatContent + "\n");
                        this.rtbHistory.SelectionColor = Color.White;
                    }));
                }
                else 
                {
                    sessionID = Service1.DicNameAndID[specificClient];
                    Service1.DicIDAndCB[sessionID].SendMessage(person, 3);
                }
            }
            else
            {
                foreach (var id in Service1.DicIDAndCB) // Broadcast message to other clients without the one who send this message
                {
                    if (id.Key.ToString() != sessionID)
                        id.Value.SendMessage(person, 2);
                }

                this.Invoke(new MethodInvoker(delegate
                { 
                    this.rtbHistory.AppendText(person.UserName + " : " + person.ChatContent + "\n"); 
                }));
            }
            Service1.isSendMessage = false; // Initialize
        }

        private void ReceiveFile(string clientName, string fileName, bool isChangeFileName)
        {
            if (isChangeFileName == false) // Only active once
            {
                this.Invoke(new MethodInvoker(delegate
                {
                    this.rtbHistory.SelectionColor = Color.Goldenrod;
                    this.rtbHistory.AppendText(clientName + " transport a file \"" + fileName + "\" to you.\n");
                    this.rtbHistory.SelectionColor = Color.White;
                }));
            }
        }
        private void SendFile(ServiceFile serviceFile, double currentProgress, bool isFirstTime)
        {
            string sessionID = Service1.DicNameAndID[serviceFile.ClientName];
            //Service1.DicIDAndCB[sessionID].UpdateDownloadFile(serviceFile, currentProgress, isFirstTime);
             //Service1.DicIDAndCB[sessionID].test();
        }
        private void Test(string clientName)
        {
            string sessionID = Service1.DicNameAndID[Service1.clientNameTest];
            Person person = new Person { UserName = "Yo", ChatContent = "What's up!" };
            Service1.DicIDAndCB[sessionID].SendMessage(person, 1);
            //Service1.DicIDAndCB[sessionID].test();
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

        #region Toolbox Event
        private void btnBroadCast_Click(object sender, EventArgs e)
        {
            Person person = new Person();
            person.UserName = "Service";
            person.ChatContent = this.txtChat.Text;
            if (txtChat != null && lsbUserList.SelectedItem != null)
            {
                string selectedItem = lsbUserList.SelectedItem.ToString();
                if (Service1.DicNameAndID.ContainsKey(selectedItem))
                {
                    string sessionID = Service1.DicNameAndID[selectedItem];
                    Service1.DicIDAndCB[sessionID].SendMessage(person, 1); // Service to specific client

                    rtbHistory.SelectionColor = Color.Red;
                    rtbHistory.AppendText(person.UserName + "[to " + selectedItem + "] : " + person.ChatContent + "\n");
                    rtbHistory.SelectionColor = Color.White;
                }
            }
            else
            {
                foreach (var id in Service1.DicIDAndCB)
                {
                    id.Value.SendMessage(person, 0);// Broadcast message to every client
                }
                this.rtbHistory.AppendText(person.UserName + " : " + person.ChatContent + "\n"); // Add text into ChatBoard
            }
            
        }

        private void lsbUserList_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lsbUserList.SelectedIndex == selectedIndex && selectedFlag == 1)
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
        #endregion
    }
}
