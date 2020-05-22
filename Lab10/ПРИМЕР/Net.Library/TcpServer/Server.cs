﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SomeProject.Library.Server
{
    public class Server
    {
        /// <summary>
        /// Слушатель
        /// </summary>
        TcpListener serverListener;
        /// <summary>
        /// Номер файла
        /// </summary>
        private int FileId = 0;
        /// <summary>
        /// Количество клиентов
        /// </summary>
        private int ClientCount = 0;
        /// <summary>
        /// Максимальное число клиентов
        /// </summary>
        private const int MaxClients = 1;

        public Server()
        {
            serverListener = new TcpListener(IPAddress.Loopback, 8080);
        }

        /// <summary>
        /// Отключение слушателя
        /// </summary>
        /// <returns>Результат истина - успех</returns>
        public bool TurnOffListener()
        {
            try
            {
                if (serverListener != null)
                    serverListener.Stop();
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("Cannot turn off listener: " + e.Message);
                return false;
            }
        }

        /// <summary>
        /// Включение слушателя
        /// </summary>
        /// <returns></returns>
        public async Task TurnOnListener()
        {
            try
            {
                if (serverListener != null)
                    serverListener.Start();
                while (true)
                {
                    OperationResult result = await ReceiveMessageFromClient();
                    
                    if (result.Result == Result.Fail)
                    {
                        var text = "Unexpected error: " + result.Message;
                        SendMessageToClient(text);
                        Console.WriteLine(text);
                    }
                    else
                    {
                        if (result.Message.Length > ServerCommands.GetFile.Length && result.Message.Substring(0, ServerCommands.GetFile.Length) == ServerCommands.GetFile)
                        {
                            GetFile(result.Message.Substring(ServerCommands.GetFile.Length));
                            continue;
                        }

                        var text = "New message from client: " + result.Message;
                        SendMessageToClient(text);
                        Console.WriteLine(text);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Cannot turn on listener: " + e.Message);
            }
        }

        /// <summary>
        /// Метод подтверждения клиента
        /// </summary>
        /// <returns>клиента</returns>
        private TcpClient AcceptClient()
        {
            if (ClientCount == MaxClients)
            {
                return null;
            }

            Interlocked.Increment(ref ClientCount);
            Console.WriteLine($"Connect client #{ClientCount}");
            return serverListener.AcceptTcpClient();
        }

        /// <summary>
        /// Получение файла от клиента
        /// </summary>
        /// <param name="fileDim">расширение файла</param>
        private async void GetFile(string fileDim)
        {
            OperationResult result = await ReceiveFileFromClient(fileDim);
            if (result.Result == Result.Fail)
            {
                var text = "Unexpected error: " + result.Message;
                SendMessageToClient(text);
                Console.WriteLine(text);
            }
            else
            {
                var text = "New file from client: " + result.Message;
                SendMessageToClient(text);
                Console.WriteLine(text);
            }
        }

        /// <summary>
        /// Получить сообщение от клиента
        /// </summary>
        /// <returns>Результат операции</returns>
        public async Task<OperationResult> ReceiveMessageFromClient()
        {
            try
            {
                Console.WriteLine("Waiting for connections...");
                StringBuilder recievedMessage = new StringBuilder();
                TcpClient client = AcceptClient();
                if (client == null)
                {
                    return new OperationResult(Result.Fail, "Can't accept client!");
                }

                byte[] data = new byte[256];
                NetworkStream stream = client.GetStream();

                do
                {
                    int bytes = stream.Read(data, 0, data.Length);
                    recievedMessage.Append(Encoding.UTF8.GetString(data, 0, bytes));
                } while (stream.DataAvailable);

                stream.Close();
                client.Close();
                Console.WriteLine("Disconnect client");
                Interlocked.Decrement(ref ClientCount);

                return new OperationResult(Result.OK, recievedMessage.ToString());
            }
            catch (Exception e)
            {
                return new OperationResult(Result.Fail, e.Message);
            }
        }

        /// <summary>
        /// Получить файл от клиента
        /// </summary>
        /// <param name="fileDim">расширение файла</param>
        /// <returns>Результат операции</returns>
        public async Task<OperationResult> ReceiveFileFromClient(string fileDim)
        {
            try
            {
                Console.WriteLine("Waiting for connections...");
                TcpClient client = AcceptClient();
                if (client == null)
                {
                    return new OperationResult(Result.Fail, "Can't accept client!");
                }

                NetworkStream stream = client.GetStream();

                do
                {
                    try
                    {
                        var data = new List<byte>();
                        int bt = stream.ReadByte();
                        while (bt != -1)
                        {
                            data.Add((byte) bt);
                            bt = stream.ReadByte();
                        }

                        var today = DateTime.Today;
                        var path = @"C:\Dron\" + $"{today.Year}-{today.Month}-{today.Day}" + @"\File" +
                                   Interlocked.Increment(ref FileId) + "." + fileDim;
                        Directory.CreateDirectory(Path.GetDirectoryName(path));
                        File.WriteAllBytes(path, data.ToArray());
                    }
                    catch (SerializationException e)
                    {
                        Console.WriteLine("Failed to deserialize. Reason: " + e.Message);
                        throw;
                    }
                } while (stream.DataAvailable);

                stream.Close();
                client.Close();
                Console.WriteLine("Disconnect client");
                Interlocked.Decrement(ref ClientCount);

                return new OperationResult(Result.OK, "File recieved");
            }
            catch (Exception e)
            {
                return new OperationResult(Result.Fail, e.Message);
            }
        }

        /// <summary>
        /// Отправить сообщение клиенту
        /// </summary>
        /// <param name="message">сообщение</param>
        /// <returns>Результат</returns>
        public OperationResult SendMessageToClient(string message)
        {
            try
            {
                TcpClient client = serverListener.AcceptTcpClient();
                NetworkStream stream = client.GetStream();

                byte[] data = Encoding.UTF8.GetBytes(message);
                stream.Write(data, 0, data.Length);

                stream.Close();
                client.Close();
            }
            catch (Exception e)
            {
                return new OperationResult(Result.Fail, e.Message);
            }

            return new OperationResult(Result.OK, "");
        }
    }
}