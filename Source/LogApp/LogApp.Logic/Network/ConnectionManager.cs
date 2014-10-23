using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using LogApp.Common;
using LogApp.Common.Model;
using LogApp.Common.Serialization;


namespace LogApp.Logic.Network
{
    internal class ConnectionManager : IDisposable
    {
        #region Data Member
        private Socket clientSocket;
        private byte[] buffer;
        private bool isSocketDisposed = true;
        private ApplicationInfoCollection lastSentInfo;
        #endregion

        #region Properties
        public bool JustConnected { get; private set; }
        public ApplicationInfoCollection LastSentInfo
        {
            get { return this.lastSentInfo ?? new ApplicationInfoCollection(); }
        }
        public bool Connected
        {
            get
            {
                return this.clientSocket.Connected;
            }
        }
        public string ConnectionInfo
        {
            get
            {
                if (this.clientSocket != null && this.clientSocket.Connected)
                {
                    return String.Format("Установлено соединение с {0}", this.clientSocket.RemoteEndPoint);
                }
                return "Соединение не установлено!";
            }
        }
        #endregion

        #region Members
        public void Connect(string ip, int port)
        {
            try
            {
                if (!this.isSocketDisposed && this.clientSocket.Connected)
                {
                    Console.WriteLine("Соединение уже установлено!");
                    return;
                }

                this.clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                this.isSocketDisposed = false;
                this.clientSocket.BeginConnect(IPAddress.Parse(ip), port, this.ConnectCallback, this.clientSocket);
            }
            catch (SocketException exc)
            {
                Console.WriteLine(exc.Message);
                this.Dispose();
            }
        }
        public void Disconect()
        {
            this.Dispose();
            Console.WriteLine("Соединение с сервером разорвано;");
        }
        public void SendObject(object obj)
        {
            if (this.isSocketDisposed || !this.clientSocket.Connected)
            {
                Console.WriteLine("Нет соединения!");
                return;
            }

            try
            {
                String str = JsonSerializer.ConvertToJson(obj);
                str += CommonData.EndOfMessage;
                this.buffer = Encoding.Unicode.GetBytes(str);
                this.clientSocket.BeginSend(this.buffer,
                    0,
                    this.buffer.Length,
                    SocketFlags.None,
                    this.SendingCallback,
                    this.clientSocket
                    );
            }
            catch (Exception exc)
            {
                Console.WriteLine(exc.Message);
            }
        }
        #endregion

        #region IDisposable Members
        public void Dispose()
        {
            try
            {
                if (!this.isSocketDisposed)
                {
                    if (this.clientSocket.Connected)
                    {
                        this.clientSocket.Shutdown(SocketShutdown.Both);
                    }
                    this.clientSocket.Close();
                    this.isSocketDisposed = true;
                }
            }
            catch (Exception exc)
            {
                Console.WriteLine(exc.Message);
            }
        }
        #endregion

        #region Assistants
        private void ConnectCallback(IAsyncResult result)
        {
            try
            {
                Socket socket = (Socket)result.AsyncState;
                socket.EndConnect(result);
                this.JustConnected = true;
                Console.WriteLine("Соединение с сервером установлено!");
            }
            catch (Exception exc)
            {
                Console.WriteLine(exc.Message);
            }
        }
        private void SendingCallback(IAsyncResult result)
        {
            try
            {
                Socket socket = (Socket)result.AsyncState;
                int bytesSend = socket.EndSend(result);

                if (bytesSend != 0 && bytesSend == this.buffer.Length)
                {
                    Console.WriteLine("Сообщение успешно отправлено;");
                    this.JustConnected = false;
                    String json = Encoding.Unicode.GetString(this.buffer);
                    this.lastSentInfo = JsonSerializer.ConvertToObject(json);
                }
                else
                {
                    Console.WriteLine("Ничего не отправлено!");
                }
            }
            catch (Exception exc)
            {
                Console.WriteLine(exc.Message);
            }
        }
        #endregion
    }
}
