using System;
using System.Net.Sockets;
using System.Text;
using LogAppServer.Logic.Processing;

namespace LogAppServer.Logic.Network
{
    internal class ConnectionInfo : IDisposable
    {
        #region Data Members
        private bool isSocketDisposed;
        private readonly StringBuilder lastMessage = new StringBuilder();
        #endregion

        #region Properties
        public ClientInfo Details { get; private set; }
        public Socket Socket { get; private set; }
        public byte[] Buffer { get; set; }
        public StringBuilder LastMessage
        {
            get { return this.lastMessage; }
        }

        public bool IsSocketConnected
        {
            get { return this.Socket.Connected; }
        }
        #endregion

        #region Constructors
        public ConnectionInfo(ClientInfo details, Socket socket)
        {
            this.Socket = socket;
            this.Details = details;
        }

        #endregion

        #region Members
        public void Dispose()
        {
            if (!this.isSocketDisposed)
            {
                this.Socket.Shutdown(SocketShutdown.Both);
                this.Socket.Close();
                this.isSocketDisposed = true;
            }
        }
        #endregion

        #region Overriden Members
        public override string ToString()
        {
            return !this.isSocketDisposed ?
                this.Socket.RemoteEndPoint.ToString() :
                String.Empty;
        }
        #endregion
    }
}
