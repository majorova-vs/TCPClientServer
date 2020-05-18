using System;
using System.Text;
using System.Net.Sockets;
using System.IO;
using System.Collections.Generic;

namespace SomeProject.Library.Client
{
    public class Client
    {
        public TcpClient tcpClient;

        public byte clientSessionID = 0;

      

        enum MessageType { Message, File, NewClientID, Delete }

        /// <summary>
        /// Конструктор. Инициализирует ИД нового клиента.
        /// </summary>
        /// <param name="ID"></param>
        public Client(byte ID)
        {
            clientSessionID = ID;
        }

        /// <summary>
        /// Получает сообщения от сервера
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

                if(stream.ReadByte() == (byte)MessageType.NewClientID)
                {
                    clientSessionID = (byte)stream.ReadByte();

                    if(clientSessionID == 0)
                    {
                        return new OperationResult(Result.Fail, "Connection rejected: too many clients.") ;
                    }

                    return new OperationResult(Result.OK, "Connected to server.");
                }
                else
                {
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
            }
            catch (Exception e)
            {
                return new OperationResult(Result.Fail, e.ToString());
            }
        }

        /// <summary>
        /// Отправляет сообщение серверу
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

                stream.WriteByte(clientSessionID);
                stream.WriteByte((byte)MessageType.Message);
                
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
        /// Отправляет серверу ИД отключившегося клиента
        /// </summary>
        public void DisconnectFromServer()
        {

            tcpClient = new TcpClient("127.0.0.1", 8080);
            NetworkStream stream = tcpClient.GetStream();

            stream.WriteByte(clientSessionID);
            stream.WriteByte((byte)MessageType.Delete);

            stream.Close();
            tcpClient.Close();

        }

        /// <summary>
        /// Отправляет файл на сервер
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public OperationResult SendFileToServer(string path)
        {
            try
            {
                if (File.Exists(path))
                {

                    tcpClient = new TcpClient("127.0.0.1", 8080);
                    NetworkStream stream = tcpClient.GetStream();

                    byte[] extention = System.Text.Encoding.UTF8.GetBytes(Path.GetExtension(path));
                    byte[] data = File.ReadAllBytes(path);

                    stream.WriteByte(clientSessionID);
                    stream.WriteByte((byte)MessageType.File);

                    stream.WriteByte(Convert.ToByte(extention.Length));

                    stream.Write(extention, 0, extention.Length);
                    stream.Write(data, 0, data.Length);

                    stream.Close();
                    tcpClient.Close();
                    return new OperationResult(Result.OK, "");
                }
                else
                {
                    return new OperationResult(Result.Fail, "File does not exist");
                }
            }
            catch (Exception e)
            {
                return new OperationResult(Result.Fail, e.Message);
            }
        }

    }
}
