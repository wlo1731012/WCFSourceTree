using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Data;
using System.IO;
using System.ServiceModel.Channels;
using System.Threading;

namespace WCFService
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "Service1" in both code and config file together.
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class Service1 : IService1
    {
        private List<string> _userList = new List<string>();
        private string _filePath = "";

        public static Dictionary<string, ICallBackServices> DicHostSessionid = new Dictionary<string,ICallBackServices>();// Record client's session id and CallBack
        public static Dictionary<string, string> DicHostUserid = new Dictionary<string,string>();// Record client's user name and session id

        public static bool isNewUser = false;
        public static bool isSendMessage = false;
        public static bool isSendFile = false;

        public static Person SendMessenger;
        public static string ReceiveMessenger;


        public delegate void ListenerHandler_ReceiveFile(string clientName, string fileName, bool isChangeFileName);
        public static event ListenerHandler_ReceiveFile listenerHandler_ReceiveFile = null;

        public FileStream fileStream;
        public static MemoryStream ms;

        public Service1()
        {
            DicHostSessionid = new Dictionary<string, ICallBackServices>();
            DicHostUserid = new Dictionary<string, string>();
        }
        /// <summary>
        /// Register client's name and closing event
        /// </summary>
        /// <param name="userName">Client's userName</param>
        /// 
        public void Register(string userName)
        {
            ICallBackServices callBack = OperationContext.Current.GetCallbackChannel<ICallBackServices>();
            string sessionid = OperationContext.Current.SessionId;
            OperationContext.Current.Channel.Closing += new EventHandler(Channel_Closing); // Regist client's closing event

            DicHostSessionid.Add(sessionid, callBack);//If type is seesion id, add it
            DicHostUserid.Add(userName, sessionid);

            isNewUser = true;
        }

        /// <summary>
        /// Close client's event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Channel_Closing(object sender, EventArgs e)
        {
            if (DicHostSessionid != null && DicHostSessionid.Count > 0)
            {
                isNewUser = true;
                foreach (var dic in DicHostUserid)
                    if (DicHostSessionid[dic.Value] == (ICallBackServices)sender)//Remove the client's data
                    {
                        DicHostSessionid.Remove(DicHostUserid[dic.Key]);
                        DicHostUserid.Remove(dic.Key);
                        break;
                    }
            }
        }

        /// <summary>
        /// Decide who send and send to who
        /// </summary>
        /// <param name="person">Who send message</param>
        /// <param name="specificClient">Send message to who</param>
        public void SendToOtherClients(Person person, string specificClient)
        {
            SendMessenger = person;
            ReceiveMessenger = specificClient;

            isSendMessage = true;
        }

        /// <summary>
        /// Get File data with type of list
        /// </summary>
        /// <param name="filePath">File path, ex:C:\\picDemo</param>
        /// <returns>DataTable, include fileName, fileExtension, and fileSize</returns>
        public DataSet GetFileList(string filePath)
        {
            DataTable dataTable = new DataTable();
            dataTable.Columns.Add("FileFullPath");
            dataTable.Columns.Add("FileName");
            dataTable.Columns.Add("FileExtension");
            dataTable.Columns.Add("FileSize");
            DirectoryInfo directoryInfo = new DirectoryInfo(filePath); // Get path folder
            foreach (FileSystemInfo fileSystemInfo in directoryInfo.GetFileSystemInfos())
            {
                FileInfo fileInfo = new FileInfo(fileSystemInfo.FullName); // Get file's information
                DataRow dataRow = dataTable.NewRow();
                dataRow["FileFullPath"] = fileSystemInfo.FullName;
                dataRow["FileName"] = fileSystemInfo.Name;
                dataRow["FileExtension"] = fileSystemInfo.Extension;
                dataRow["FileSize"] = fileInfo.Length;
                dataTable.Rows.Add(dataRow);
            }
            DataSet dataSet = new DataSet();
            dataSet.Tables.Add(dataTable);
            return dataSet;
        }


        #region A way to avoid Application Configuration File
        //public class MyHost
        //{
        //    static ServiceHost host = null;
        //    public static void Open()
        //    {
        //        string baseAddress = "net.tcp://localhost:2008/FileService";
        //        host = new ServiceHost(typeof(Service1), new Uri(baseAddress));
        //        host.AddServiceEndpoint(typeof(IService1), GetTcpBinding(), "");
        //        host.Open();
        //    }
        //    public static void Close()
        //    {
        //        if (host != null && host.State == CommunicationState.Opened)
        //        {
        //            host.Close();
        //        }
        //        host = null;
        //    }
        //    public static Binding GetTcpBinding()
        //    {
        //        NetTcpBinding binding = new NetTcpBinding();
        //        binding.TransferMode = TransferMode.Streamed;
        //        binding.MaxReceivedMessageSize = int.MaxValue;
        //        return binding;
        //    }
        //}
        #endregion



        public void ReceiveFile(ClientFile clientFile, bool isChangeFileName)
        {
            if (clientFile.isFinsishFlag == true)
            {
                fileStream.Close();
            }
            else
            {
                if (listenerHandler_ReceiveFile != null && isChangeFileName == false) // Check if this file is first time transport to Service then give it name
                {
                    listenerHandler_ReceiveFile(clientFile.ClientName, clientFile.FileName, isChangeFileName); // Make GUI display text
                    _filePath = "D:\\Folder2\\" + clientFile.ClientName + "_" + clientFile.FileName;
                    _filePath = CheckFileName(_filePath);

                    //fileStream = File.Open(_filePath, FileMode.Append);
                    fileStream = new FileStream(_filePath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
                }
                fileStream.Write(clientFile.Buffer, 0, clientFile.BytesRead);
            }
        }
                
        private MyFileInfo _sendFileInfo;
        public void SendFile(ref ServiceFile ClientAsked)
        {
            if (ClientAsked.isFirstTime == true) // Active once
            {
                _filePath = ClientAsked.FilePath; // FilePath that client want
                string[] splitString = _filePath.Split('\\');
                FileInfo fileInfo = new FileInfo(_filePath); // Get file Length
                _sendFileInfo = new MyFileInfo(File.OpenRead(_filePath), splitString[splitString.Length - 1], fileInfo.Length);
                ClientAsked.isFirstTime = false;
            }
            
            ClientAsked.Buffer = new byte[ClientAsked.BufferSize];
            ClientAsked.FileSize = _sendFileInfo.FileSize; // For progress bar value count

            ClientAsked.BytesRead = _sendFileInfo.Stream.Read(ClientAsked.Buffer, 0, ClientAsked.BufferSize); // Read part of file save into buffer and return how much bytes be read this time into ClientAsked.BytesRead
            
        }

        private string CheckFileName(string filePath)
        {
            string[] splitPath = filePath.Split('.');
            string extensionName = "." + splitPath[splitPath.Length - 1];
            string _filePath = filePath;
            int nameCount = 0;
            while (true)
            {
                if (File.Exists(_filePath) == true)
                {
                    nameCount++;
                    _filePath = filePath.Substring(0, filePath.Length - extensionName.Length) + "_" + nameCount.ToString() + extensionName;// - 1 for counting position
                }
                else
                    return _filePath;
            }
        }



        public ServiceFile Test(ServiceFile ClientAsked)
        {
            if (ClientAsked.isFirstTime == true) // Active once
            {
                _filePath = ClientAsked.FilePath; // FilePath that client want
                string[] splitString = _filePath.Split('\\');
                FileInfo fileInfo = new FileInfo(_filePath); // Get file Length
                _sendFileInfo = new MyFileInfo(File.OpenRead(_filePath), splitString[splitString.Length - 1], fileInfo.Length);
                ClientAsked.isFirstTime = false;
            }

            ClientAsked.Buffer = new byte[ClientAsked.BufferSize];
            ClientAsked.FileSize = _sendFileInfo.FileSize; // For progress bar value count

            ClientAsked.BytesRead = _sendFileInfo.Stream.Read(ClientAsked.Buffer, 0, ClientAsked.BufferSize); // Read part of file save into buffer and return how much bytes be read this time into ClientAsked.BytesRead
            
            return ClientAsked;
        }
    }
}
