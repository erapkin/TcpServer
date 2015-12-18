using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Sockets;

namespace TCPServer
{
    class handleClient
    {
        TcpClient clientSocket;
        string clNo;
        static HashSet<TcpClient> ClientList = new HashSet<TcpClient>() { };
        static List<byte[]> ChatLog = new List<byte[]>() { };

        public void startClient(TcpClient inClientSocket, string clineNo)
        {
            ClientList.Add(inClientSocket);
            this.clientSocket = inClientSocket;
            this.clNo = clineNo;
            Thread ctThread = new Thread(doChat);
            ctThread.Start();
        }
        private void doChat()
        {
            string name;
            byte[] bytesFrom;
            string dataFromClient = null;
            Byte[] sendBytes = null;
            string serverResponse = null;
            foreach (byte[] message in ChatLog)
            {
                NetworkStream LogStream = clientSocket.GetStream();
                LogStream.Write(message, 0, message.Length);
                LogStream.Flush();
                Thread.Sleep(300);
            }
            while ((true))
            {
                try
                {
                    int receivedDataLength;
                    name = clientSocket.Client.RemoteEndPoint.ToString();
                    NetworkStream networkStream = clientSocket.GetStream();
                    bytesFrom = new byte[10025];
                    receivedDataLength = networkStream.Read(bytesFrom, 0, (int)clientSocket.ReceiveBufferSize) + name.Length + 2;
                    dataFromClient = System.Text.Encoding.ASCII.GetString(bytesFrom, 0, receivedDataLength);
                    Console.WriteLine(" >> " + "From client-" + clNo + ": " + dataFromClient);
                    byte [] bytesFromTemp = Encoding.ASCII.GetBytes(name + ": " + Encoding.ASCII.GetString(bytesFrom, 0, receivedDataLength));
                    ChatLog.Add(bytesFromTemp);

                    foreach (TcpClient client in ClientList)
                    {
                        if (client != this.clientSocket)
                        {
                            NetworkStream responseStream = client.GetStream();
                            responseStream.Write(bytesFromTemp, 0, receivedDataLength);
                            responseStream.Flush();
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    ClientList.Remove(clientSocket);
                    break;
                }
            }
        }
    }
}
