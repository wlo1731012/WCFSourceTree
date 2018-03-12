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
        public static Dictionary<string, ICallBackServices> _dicHostSessionid = null;// Record client's session id and CallBack
        public static Dictionary<string, string> _dicHostUserid = null;// Record client's user name and session id
        public static bool _isNewUser = false;
        public static bool _isSendMessage = false;

        public static Person _sendMessenger;
        public static string _receiveMessenger;

        public delegate void ListenerHandler_ReceiveFile(string clientName, string fileName, bool isChangeFileName);
        public static event ListenerHandler_ReceiveFile listenerHandler_ReceiveFile = null;

        private string filePath;

        public Service1()
        {
            _dicHostSessionid = new Dictionary<string, ICallBackServices>();
            _dicHostUserid = new Dictionary<string, string>();
        }
        /// <summary>
        /// Register client's name and closing event
        /// </summary>
        /// <param name="userName">Client's userName</param>
        /// 
        public void Register(string userName)
        {
            ICallBackServices client = OperationContext.Current.GetCallbackChannel<ICallBackServices>();
            string sessionid = OperationContext.Current.SessionId;
            OperationContext.Current.Channel.Closing += new EventHandler(Channel_Closing); // Regist client's closing event

            _dicHostSessionid.Add(sessionid, client);//If type is seesion id, add it
            _dicHostUserid.Add(userName, sessionid);

            _isNewUser = true;
        }

        /// <summary>
        /// Close client's event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Channel_Closing(object sender, EventArgs e)
        {
            if (_dicHostSessionid != null && _dicHostSessionid.Count > 0)
            {
                _isNewUser = true;
                foreach (var dic in _dicHostUserid)
                    if (_dicHostSessionid[dic.Value] == (ICallBackServices)sender)//Remove the client's data
                    {
                        _dicHostSessionid.Remove(_dicHostUserid[dic.Key]);
                        _dicHostUserid.Remove(dic.Key);
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
            _sendMessenger = person;
            _receiveMessenger = specificClient;

            _isSendMessage = true;
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
            if (listenerHandler_ReceiveFile != null && isChangeFileName == false)
            {
                listenerHandler_ReceiveFile(clientFile.ClientName, clientFile.FileName, isChangeFileName); // Make GUI display text
                filePath = "D:\\Folder2\\" + clientFile.ClientName + "_" + clientFile.FileName;
                filePath = CheckFileName(filePath);
            }
            clientFile.ReceiveFileStream = File.Open(filePath, FileMode.Append);
            clientFile.ReceiveFileStream.Write(clientFile.Buffer, 0, clientFile.BytesRead);
            clientFile.ReceiveFileStream.Close();
        }
        private string CheckFileName(string filePath)
        {
            string[] splitPath = filePath.Split('.');
            string extensionName = "." + splitPath[splitPath.Length - 1];
            int nameCount = 0;
            while (true)
            {
                if (File.Exists(filePath) == true)
                {
                    nameCount++;
                    filePath = filePath.Substring(0, filePath.Length - extensionName.Length) + "_" + nameCount.ToString() + extensionName;// - 1 for counting position
                }
                else
                    return filePath;
            }
        }
    }
}
