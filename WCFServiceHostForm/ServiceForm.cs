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

namespace WCFServiceHostForm
{
    public partial class ServiceForm : Form
    {
        public ServiceForm()
        {
            InitializeComponent();
        }
        private int selectedFlag = 0;
        private int selectedIndex = -1;
        string filePath = "D:\\Folder2\\Client_";

        private void Form1_Load(object sender, EventArgs e)
        {
            ServiceHost host = new ServiceHost(typeof(Service1));
            host.Open(); // Start Listening
            
            Service1.listenerHandler_ReceiveFile += new Service1.ListenerHandler_ReceiveFile(ReceiveFile);

            #region Thread Working

            Thread threadReceiveMessage = new Thread(new ThreadStart(delegate   //Listen all clients chat content
            {
                //lock (InstObj)//Lock for multiple
                //{
                
                while (true)
                {
                    if (Service1._isSendMessage == true) // Listen chat
                    {
                        SendToOtherClient(Service1._sendMessenger, Service1._receiveMessenger);
                    }
                    else if (Service1._isNewUser == true) // Listen login
                    {
                        AddNewUser();
                    }
                    //else if (Service1._isReceiveFile == true)
                    //{
                    //    ReceiveFile(Service1._clientFile);
                    //}

                    Thread.Sleep(10);
                }
                //}
            }));
            threadReceiveMessage.IsBackground = true;
            threadReceiveMessage.Start();

            #endregion
        }
        
        private void AddNewUser()
        {
            string sessionID = "";
            List<string> userList = new List<string>();
            if (Service1._dicHostSessionid != null || Service1._dicHostSessionid.Count > 0)
            {
                this.Invoke(new MethodInvoker(delegate { this.lsbUserList.Items.Clear(); }));
                foreach (var clientList in Service1._dicHostUserid) // id.key = userName, id.Value = sessionID
                {
                    userList.Add(clientList.Key);

                    this.Invoke(new MethodInvoker(delegate
                    {
                        this.lsbUserList.Items.Add(clientList.Key); // UserList put in Service
                    }));
                }

                foreach (var users in Service1._dicHostSessionid) // Update all client's list
                {
                    sessionID = users.Key;
                    Service1._dicHostSessionid[sessionID].UpdateUserList(userList);
                }
            }
            Service1._isNewUser = false; // Initialize
        }

        private void SendToOtherClient(Person person, string specificClient)
        {
            string sessionID = Service1._dicHostUserid[person.UserName];
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
                    sessionID = Service1._dicHostUserid[specificClient];
                    Service1._dicHostSessionid[sessionID].SendMessage(person, 3);
                }
            }
            else
            {
                foreach (var id in Service1._dicHostSessionid) // Broadcast message to other clients without the one who send this message
                {
                    if (id.Key.ToString() != sessionID)
                        id.Value.SendMessage(person, 2);
                }

                this.Invoke(new MethodInvoker(delegate
                { 
                    this.rtbHistory.AppendText(person.UserName + " : " + person.ChatContent + "\n"); 
                }));
            }
            Service1._isSendMessage = false; // Initialize
        }

        private void ReceiveFile(ClientFile clientFile, bool isChangeFileName)
        {
            if (isChangeFileName == false) // Only active once
            {
                this.Invoke(new MethodInvoker(delegate
                {
                    this.rtbHistory.SelectionColor = Color.Goldenrod;
                    this.rtbHistory.AppendText(clientFile.ClientName + " transport a file \"" + clientFile.FileName + "\" to you.\n");
                    this.rtbHistory.SelectionColor = Color.White;
                }));
                filePath = "D:\\Folder2\\Client_" + clientFile.FileName;
                filePath = CheckFilePath(filePath);
            }
            
            clientFile.ReceiveFileStream = File.Open(filePath, FileMode.Append);
            clientFile.ReceiveFileStream.Write(clientFile.Buffer, 0, clientFile.BytesRead);
            clientFile.ReceiveFileStream.Close();
        }
        private string CheckFilePath(string filePath)
        {
            int nameCount = 0;
            string[] splitPath = filePath.Split('.');
            while (true)
            {
                if (File.Exists(filePath) == true)
                {
                    nameCount++;
                    filePath = splitPath[0] + "_" + nameCount.ToString() + "." + splitPath[1]; 
                }
                else
                    break;
            }
            return filePath;
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
                if (Service1._dicHostUserid.ContainsKey(selectedItem))
                {
                    string sessionID = Service1._dicHostUserid[selectedItem];
                    Service1._dicHostSessionid[sessionID].SendMessage(person, 1); // Service to specific client

                    rtbHistory.SelectionColor = Color.Red;
                    rtbHistory.AppendText(person.UserName + "[to " + selectedItem + "] : " + person.ChatContent + "\n");
                    rtbHistory.SelectionColor = Color.White;
                }
            }
            else
            {
                foreach (var id in Service1._dicHostSessionid)
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
