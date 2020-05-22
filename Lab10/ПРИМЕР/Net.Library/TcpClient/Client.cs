using System;
using System.IO;
using System.Text;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;

namespace SomeProject.Library.Client
{
    public class Client
    {
        public TcpClient tcpClient;
        
        /// <summary>
        /// Получение собщения от сервера
        /// </summary>
        /// <returns></returns>
        public OperationResult ReceiveMessageFromServer()
        {
            try
            {
                tcpClient = new TcpClient("127.0.0.1", 8080);
                StringBuilder recievedMessage = new StringBuilder();
                byte[] data = new byte[256];
                NetworkStream stream = tcpClient.GetStream();

                do
                {
                    int bytes = stream.Read(data, 0, data.Length);
                    recievedMessage.Append(Encoding.UTF8.GetString(data, 0, bytes));
                }
                while (stream.DataAvailable);
                stream.Close();
                tcpClient.Close();

                return new OperationResult(Result.OK, recievedMessage.ToString());
            }
            catch (Exception e)
            {
                return new OperationResult(Result.Fail, e.ToString());
            }
        }

        /// <summary>
        /// Отправка сообщения на сервер
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public OperationResult SendMessageToServer(string message)
        {
            try
            {
                tcpClient = new TcpClient("127.0.0.1", 8080);
                NetworkStream stream = tcpClient.GetStream();
                byte[] data = System.Text.Encoding.UTF8.GetBytes(message);
                stream.Write(data, 0, data.Length);
                stream.Close();
                tcpClient.Close();
                return new OperationResult(Result.OK, "") ;
            }
            catch (Exception e)
            {
                return new OperationResult(Result.Fail, e.Message);
            }
        }

        /// <summary>
        /// Отправка файла на сервер
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public OperationResult SendFileToServer(string path)
        {
            var fileName = Path.GetFileName(path);
            var result = SendMessageToServer(ServerCommands.GetFile + fileName.Substring(fileName.IndexOf('.') + 1));
            if (result.Result == Result.Fail)
                return new OperationResult(Result.Fail, "Can't send message!");
            try
            {
                tcpClient = new TcpClient("127.0.0.1", 8080);
                NetworkStream stream = tcpClient.GetStream();
                
                var data = File.ReadAllBytes(path);
                stream.Write(data, 0, data.Length);
                stream.Close();
                tcpClient.Close();
                return new OperationResult(Result.OK, "") ;
            }
            catch (Exception e)
            {
                return new OperationResult(Result.Fail, e.Message);
            }
        }
    }
}
