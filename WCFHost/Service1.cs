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
    [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Reentrant)]//,InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple
    public class Service1 : IService1
    {
        private List<string> _userList = new List<string>();
        private string _filePath = "";
        private IService1Callback _callBack;

        public static Dictionary<string, IService1Callback> DicIDAndCB = new Dictionary<string, IService1Callback>();// Record client's session id and CallBack
        public static Dictionary<string, string> DicNameAndID = new Dictionary<string,string>();// Record client's user name and session id

        public static bool isNewUser = false;
        public static bool isSendMessage = false;
        public static bool isSendFile = false;

        public static bool isTest = false;
        public static string clientNameTest;

        public static Person SendMessenger;
        public static string ReceiveMessenger;


        public delegate void ListenerHandler_ReceiveFile(string clientName, string fileName, bool isChangeFileName);
        public static event ListenerHandler_ReceiveFile listenerHandler_ReceiveFile = null;

        public delegate void ListenerHandler_SendFile(ServiceFile serviceFile, double currentProgress, bool isFirstTime);
        public static event ListenerHandler_SendFile listenerHandler_SendFile = null;

        public delegate void ListenerHandler_Test(string clientName);
        public static event ListenerHandler_Test listenerHandler_Test = null;

        public delegate void ListenerHandler_SendToOtherClients(Person person, string specificClient);
        public static event ListenerHandler_SendToOtherClients listenerHandler_sendToOtherClients = null;

        public FileStream fileStream;

        public Service1()
        {
            DicNameAndID = new Dictionary<string, string>();
            DicIDAndCB = new Dictionary<string, IService1Callback>();
        }
        /// <summary>
        /// Register client's name and closing event
        /// </summary>
        /// <param name="userName">Client's userName</param>
        /// 
        public void Register(string userName)
        {
            IService1Callback callBack = OperationContext.Current.GetCallbackChannel<IService1Callback>();
            string sessionid = OperationContext.Current.SessionId;
            OperationContext.Current.Channel.Closing += new EventHandler(Channel_Closing); // Regist client's closing event
            
            DicNameAndID.Add(userName, sessionid);
            DicIDAndCB.Add(sessionid, callBack);//If type is seesion id, add it

            isNewUser = true;
        }

        /// <summary>
        /// Close client's event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Channel_Closing(object sender, EventArgs e)
        {
            if (DicIDAndCB != null && DicIDAndCB.Count > 0)
            {
                isNewUser = true;
                foreach (var dic in DicNameAndID)
                    if (DicIDAndCB[dic.Value] == (IService1Callback)sender)//Remove the client's data
                    {
                        DicIDAndCB.Remove(DicNameAndID[dic.Key]);
                        DicNameAndID.Remove(dic.Key);
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
            listenerHandler_sendToOtherClients(SendMessenger, ReceiveMessenger);
            //isSendMessage = true;
        }
        /// <summary>
        /// Get File data with type of list
        /// </summary>
        /// <param name="filePath">File path, ex:C:\\picDemo</param>
        /// <returns>DataTable, include fileName, fileExtension, and fileSize</returns>
        public DataSet GetFileList(string filePath)
        {
            DataTable dataTable = new DataTable();
            dataTable.Columns.Add("FileName");
            dataTable.Columns.Add("FileExtension");
            dataTable.Columns.Add("FileSize");
            DirectoryInfo directoryInfo = new DirectoryInfo(filePath); // Get path folder
            foreach (FileSystemInfo fileSystemInfo in directoryInfo.GetFileSystemInfos())
            {
                FileInfo fileInfo = new FileInfo(fileSystemInfo.FullName); // Get file's information
                DataRow dataRow = dataTable.NewRow();
                dataRow["FileName"] = fileSystemInfo.FullName;
                dataRow["FileExtension"] = fileSystemInfo.Extension;
                dataRow["FileSize"] = fileInfo.Length;
                dataTable.Rows.Add(dataRow);
            }
            DataSet dataSet = new DataSet();
            dataSet.Tables.Add(dataTable);
            return dataSet;
        }

        ///<summary>
        /// Get picture Data
        /// </summary> 
        /// <param name="fileName">Downloading file name</param> 
        /// <param name="offSet">Location</param> 
        /// <param name="blockSize">Download data size per once</param> 
        /// <returns>byte[], </returns> 
        public byte[] GetPic(string filePath, string fileName, int offSet, int blockSize)
        {
            try
            {
                byte[] picByte;
                using(FileStream fileStream = new FileStream(Path.Combine(@filePath, fileName),FileMode.Open,FileAccess.Read,FileShare.ReadWrite))
                {
                    fileStream.Seek(offSet, SeekOrigin.Begin);

                    if ((offSet + blockSize) > fileStream.Length) // If asked download size is bigger than fileStream's size, reduce asked size.
                        blockSize = Convert.ToInt32(fileStream.Length - offSet);
                    if (blockSize <= 0) // If ask nothing, return 0
                    {
                        picByte = new byte[0];
                        return picByte;
                    }
                    picByte = new byte[blockSize];
                    fileStream.Read(picByte, 0, blockSize); // Read a block of bytes size (blocksize) from fileStream and writes the data into buffer (picByte)
                    fileStream.Close(); // Close fileStream
                }
                    return picByte;
            }
            catch (Exception ex)
            {
                throw ex;
            } 
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
        public static IService1Callback callBackkk;
        public void SendFile(string clientName)
        {
            //_filePath = "D:\\Folder2\\AddNewUser.JPG";
            //int bufferSize = 1;
            //int tillCurrentBytesRead = 0;
            double currentProgress = 0;
            bool isFirstTime = true;
            ServiceFile serviceFile = new ServiceFile();

            ////_callBack = OperationContext.Current.GetCallbackChannel<ICallBackServices>();
            //string[] splitString = _filePath.Split('\\');
            //FileInfo fileInfo = new FileInfo(_filePath); // Get file Length
            //MyFileInfo sendFileInfo = new MyFileInfo(File.OpenRead(_filePath), splitString[splitString.Length - 1], fileInfo.Length);

            //serviceFile.Buffer = new byte[bufferSize];
            //serviceFile.BufferSize = bufferSize;
            //serviceFile.TotalBytes = sendFileInfo.FileSize;
            serviceFile.ClientName = clientName;

            //do
            //{
                //serviceFile.BytesRead = sendFileInfo.Stream.Read(serviceFile.Buffer, 0, serviceFile.Buffer.Length);
                //serviceFile.FileName = sendFileInfo.FileName;
                //serviceFile.isFinsishFlag = false;

                //tillCurrentBytesRead += serviceFile.BytesRead;

                //if (sendFileInfo.FileSize != 0)
                //    currentProgress = (((double)tillCurrentBytesRead) / sendFileInfo.FileSize) * 100;
                //else
                    currentProgress = 100;

                //listenerHandler_ReceiveFile(clientName, "123", false);
                listenerHandler_SendFile(serviceFile, currentProgress, isFirstTime);
                //DicIDAndCB[DicNameAndID[serviceFile.ClientName]].UpdateDownloadFile(serviceFile, currentProgress, isFirstTime);
                //_callBack.SendMessage(person, 1);
                //string sessionID = DicNameAndID[clientName];
                //DicIDAndCB[sessionID].test();

                //callBackkk = OperationContext.Current.GetCallbackChannel<IService1Callback>();
                //callBackkk.test();

                isFirstTime = false;

            //    if (currentProgress == 100)
            //    {
            //        serviceFile.isFinsishFlag = true;
            //        string sessionID = DicNameAndID[serviceFile.ClientName];
            //        listenerHandler_SendFile(serviceFile, currentProgress, isFirstTime);
            //        //DicIDAndCB[sessionID].UpdateDownloadFile(serviceFile, currentProgress, isFirstTime); // for file closing
            //    }

            //} while (currentProgress != 100);

        }
        public void Test(string clientName)
        {
            clientNameTest = clientName;
            listenerHandler_Test(clientName);
        }
        private string CheckFileName(string filePath)
        {
            string[] splitPath = filePath.Split('.');
            string extensionName = "." + splitPath[splitPath.Length - 1];
            string newFilePath = filePath;
            int nameCount = 0;
            while (true)
            {
                if (File.Exists(newFilePath) == true)
                {
                    nameCount++;
                    newFilePath = filePath.Substring(0, filePath.Length - extensionName.Length) + "_" + nameCount.ToString() + extensionName;// - 1 for counting position
                }
                else
                    return newFilePath;
            }
        }

        
        
    }
}
