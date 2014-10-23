using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Timers;
using LogApp.Common;
using LogApp.Common.Model;
using LogApp.Common.Serialization;
using LogAppServer.Logic.Processing;

namespace LogAppServer.Logic.Network
{
    internal class ConnectionManager : IDisposable
    {
        #region Constants
        private const int MaxQueueLength = 10;
        private const int ConnectionCleaningTimer = 5000;
        #endregion

        #region Data Members
        private Socket serverSocket;

        public Timer connectionsCleaningTimer = new Timer();

        private readonly object locker = new object();
        private bool isSocketDisposed = true;

        private readonly List<ConnectionInfo> connections = new List<ConnectionInfo>();
        public IReadOnlyList<ClientInfo> Clients
        {
            get
            {
                return this.connections
                    .Select(item => item.Details)
                    .ToList()
                    .AsReadOnly();
            }
        }
        #endregion

        #region Properties
        public bool IsRunning
        {
            get { return !this.isSocketDisposed && this.serverSocket.IsBound; }
        }
        #endregion

        #region Events
        public event EventHandler StatusChanged;
        #endregion

        #region Members
        public void StartServer(IPEndPoint endPoint, double refreshTime)
        {
            try
            {
                this.SetupServerSocket(endPoint);
                Console.WriteLine("Начинаю слушать сокет {0}", this.serverSocket.LocalEndPoint);
                this.serverSocket.Listen(ConnectionManager.MaxQueueLength);
                this.serverSocket.BeginAccept(this.AcceptCallback, this.serverSocket);

                this.OnStatusChanged();

                this.connectionsCleaningTimer.Interval = ConnectionManager.ConnectionCleaningTimer;
                this.connectionsCleaningTimer.Elapsed += connectionsCleaningTimer_Elapsed;
                this.connectionsCleaningTimer.Start();
            }
            catch (SocketException exc)
            {
                Console.WriteLine("Socket exception: " + exc.SocketErrorCode);
            }
            catch (Exception exc)
            {
                Console.WriteLine("Exception: " + exc);
            }
        }
        public void ShotDownServer()
        {
            if (this.isSocketDisposed)
            {
                Console.WriteLine("Сервер уже выключен!");
                return;
            }

            this.CloseAllConnections();
            this.DisposeSocket();

        }

        public void CloseConnection(ClientInfo connection)
        {
            try
            {
                ConnectionInfo info = this.connections
                    .FirstOrDefault(item => item.Details.Name == connection.Name);
                this.CloseConnection(info);
            }
            catch (Exception exc)
            {
                Console.WriteLine("Exception: " + exc.Message);
            }
        }
        #endregion

        #region IDisposable Members
        public void Dispose()
        {
            this.connectionsCleaningTimer.Dispose();
            this.ShotDownServer();
        }
        #endregion

        #region Event Handlers
        private void connectionsCleaningTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                lock (this.locker)
                {
                    this.connections
                        .RemoveAll(connection => !connection.IsSocketConnected);
                }
            }
            catch (Exception exc)
            {
                Console.WriteLine("Exception: " + exc.Message);
            }
        }
        #endregion

        #region Assistants
        private void SetupServerSocket(IPEndPoint endPoint)
        {
            this.DisposeSocket();
            this.serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            this.isSocketDisposed = false;
            this.serverSocket.Bind(endPoint);
        }

        private void AcceptCallback(IAsyncResult result)
        {
            ConnectionInfo connection = null;
            try
            {
                Socket socket = (Socket)result.AsyncState;
                Socket clientSocket = socket.EndAccept(result);
                ClientInfo info = new ClientInfo(clientSocket.RemoteEndPoint.ToString());
                connection = new ConnectionInfo(info, clientSocket);
                lock (this.locker)
                {
                    this.connections.Add(connection);
                }
                Console.WriteLine("Подключен новый клиент: {0}", connection.Socket.RemoteEndPoint);

                socket.BeginAccept(this.AcceptCallback, socket);

                connection.Buffer = new byte[1024];
                connection.Socket.BeginReceive(connection.Buffer, 0, connection.Buffer.Length, SocketFlags.None, this.ReceiveCallback, connection);
            }
            catch (SocketException exc)
            {
                Console.WriteLine("Socket exception: " + exc.Message);
                if (connection != null)
                {
                    this.CloseConnection(connection);
                }
            }
            catch (Exception exc)
            {
                Console.WriteLine("Exception: " + exc.Message);
            }
        }
        private void ReceiveCallback(IAsyncResult result)
        {
            ConnectionInfo connection = null;
            try
            {
                connection = (ConnectionInfo)result.AsyncState;
                int bytesRead = connection.Socket.EndReceive(result);
                if (bytesRead != 0)
                {
                    Console.WriteLine("Получено сообщение от {0}:", connection.Socket.RemoteEndPoint);
                    this.ProceedReceivedInfo(connection, bytesRead);
                    connection.Socket.BeginReceive(connection.Buffer, 0, connection.Buffer.Length, SocketFlags.None,
                        this.ReceiveCallback, connection);
                }
                else
                {
                    this.CloseConnection(connection);
                }
            }
            catch (SocketException exc)
            {
                Console.WriteLine("Socket exception: " + exc.SocketErrorCode);
                this.CloseConnection(connection);
            }
            catch (Exception exc)
            {
                Console.WriteLine("Exception: " + exc.Message);
            }
        }
        private void ProceedReceivedInfo(ConnectionInfo client, int bytesRead)
        {
            String buffer = Encoding.Unicode.GetString(client.Buffer, 0, bytesRead);
            client.LastMessage.Append(buffer);

            ApplicationInfoCollection message;
            bool transferCompleted = this.CheckEndOfMessage(client, out message);
            if (transferCompleted)
            {
                client.LastMessage.Clear();
                client.Details.CurrentState.MergeWith(message);
            }
        }
        private bool CheckEndOfMessage(ConnectionInfo client, out ApplicationInfoCollection message)
        {
            try
            {
                String json = client.LastMessage.ToString();
                message = null;
                if (json.Contains(CommonData.EndOfMessage))
                {
                    json = json.Substring(0, json.IndexOf(CommonData.EndOfMessage, System.StringComparison.Ordinal));
                    message = JsonSerializer.ConvertToObject(json);
                    return true;
                }
                return false;
            }
            catch (Exception)
            {
                message = null;
                return false;
            }
        }

        private void CloseAllConnections()
        {
            lock (this.locker)
            {
                foreach (ConnectionInfo connection in this.connections)
                {
                    this.CloseConnection(connection);
                }
                this.connections.Clear();
            }
        }
        private void CloseConnection(ConnectionInfo connection)
        {
            try
            {
                Console.WriteLine("Работа с {0} закончена.", connection.Socket.RemoteEndPoint);
                connection.Dispose();
            }
            catch (Exception exc)
            {
                Console.WriteLine("Exception: " + exc.Message);
            }
        }

        private void OnStatusChanged()
        {
            EventHandler handler = StatusChanged;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        private void DisposeSocket()
        {
            if (!this.isSocketDisposed)
            {
                this.serverSocket.Close();
                this.isSocketDisposed = true;
                this.OnStatusChanged();
            }
        }
        #endregion
    }
}
