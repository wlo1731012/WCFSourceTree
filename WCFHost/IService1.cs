using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Data;
using System.IO;

namespace WCFService
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "IService1" in both code and config file together.
    [ServiceContract(SessionMode = SessionMode.Required, CallbackContract = typeof(ICallBackServices))]
    public interface IService1
    {
        [OperationContract(IsOneWay = false)]
        void Register(string userName); // Register client
        [OperationContract]
        void SendToOtherClients(Person person, string specificClient); // Send message to other clients

        [OperationContract]
        DataSet GetFileList(string filePath); 

        [OperationContract]
        void ReceiveFile(ClientFile clientFile, bool isChangeFileName);
        [OperationContract]
        void SendFile(ref ServiceFile ClientAsked);
        [OperationContract]
        ServiceFile Test(ServiceFile ClientAsked);

    }
    public interface ICallBackServices
    {
        // contentType
        // 0 = Service to everyone
        // 1 = Service to Specific client
        // 2 = Client to everyone
        // 3 = Client to Specific client or Service
        [OperationContract(IsOneWay = true)]
        void SendMessage(Person person, int contentType);
        [OperationContract(IsOneWay = true)]
        void UpdateUserList(List<string> userList);
        [OperationContract(IsOneWay = true)]
        void UpdateDownloadFile(ServiceFile serviceFile, double currentProgress);
    }

    [DataContract] // Add a new type
    public class Person
    {
        [DataMember]
        public string UserName;
        [DataMember]
        public string ChatContent;
    }

    [MessageContract] // Could use this to change SOAP
    public class MyFileInfo
    {
        [MessageHeader]
        public string FileName;
        [MessageHeader]
        public long FileSize;
        [MessageBodyMember]
        public Stream Stream;
        //public MyFileInfo() { }
        public MyFileInfo(Stream stream, string fileName, long fileSize)
        {
            this.Stream = stream;
            this.FileSize = fileSize;
            this.FileName = fileName;
        }
    }

    [DataContract] // Add a new type
    public class ClientFile
    {
        [DataMember]
        public string FileName;
        [DataMember]
        public string ClientName;
        [DataMember]
        public int BytesRead;
        [DataMember]
        public byte[] Buffer; // Tranported onformation once
        [DataMember]
        public bool isFinsishFlag;
    }

    [DataContract] // Add a new type
    public class ServiceFile
    {
        [DataMember]
        public string FilePath;
        [DataMember]
        public int BytesRead;
        [DataMember]
        public long FileSize;
        [DataMember]
        public byte[] Buffer; // Tranported onformation once
        [DataMember]
        public int BufferSize; // Size of tranported onformation once
        [DataMember]
        public bool isFirstTime;
    }
}
