using System;
using System.Collections.Generic;
using System.Net;
using LogAppServer.Logic.Network;
using LogAppServer.Logic.Processing;

namespace LogAppServer.Logic
{
    public sealed class LogAppServerController : IDisposable
    {
        #region Data Members
        private readonly IReadOnlyDictionary<String, String> settings;
        private readonly ConnectionManager connectionManager;
        #endregion

        #region Properties
        public bool IsRunning
        {
            get { return this.connectionManager.IsRunning; }    
        }
        public IReadOnlyList<ClientInfo> Clients
        {
            get { return this.connectionManager.Clients; }
        }
        #endregion

        #region Events
        public event EventHandler ServerStatusChanged;
        #endregion

        #region Constructors
        public LogAppServerController(IReadOnlyDictionary<String, String> settings)
        {
            this.settings = settings;
            this.connectionManager = new ConnectionManager();

            this.connectionManager.StatusChanged += connectionManager_StatusChanged;
        }
        #endregion

        #region Event Handlers
        private void connectionManager_StatusChanged(object sender, EventArgs e)
        {
            this.OnServerStatusChanged();
        }
        #endregion

        #region Members
        public void StartServer()
        {
            IPEndPoint endPoint = this.GetIpEndPoint();
            double connectionCheckingTime = this.GetConnectionCheckingTime();

            this.connectionManager.StartServer(endPoint, connectionCheckingTime);
        }
        public void StopServer()
        {
            this.connectionManager.ShotDownServer();
        }
        public void ResetServer()
        {
            this.StopServer();
            this.StartServer();
        }

        public void DisconnectClient(ClientInfo info)
        {
            this.connectionManager.CloseConnection(info);
        }
        #endregion

        #region IDisposable Members
        public void Dispose()
        {
            this.connectionManager.Dispose();
        }
        #endregion
        
        #region Assistants
        private IPEndPoint GetIpEndPoint()
        {
            IPAddress address;
            IPAddress.TryParse(this.settings[LogAppServerSettings.ServerIp], out address);
            int port;
            int.TryParse(this.settings[LogAppServerSettings.ServerPort], out port);

            IPEndPoint endPoint = new IPEndPoint(address ?? IPAddress.Any, port);
            return endPoint;
        }
        private double GetConnectionCheckingTime()
        {
            double connectionCheckingTime;
            double.TryParse(this.settings[LogAppServerSettings.ConnectionCheckingTime], out connectionCheckingTime);
            return connectionCheckingTime;
        }

        private void OnServerStatusChanged()
        {
            EventHandler handler = ServerStatusChanged;
            if (handler != null) handler(this, EventArgs.Empty);
        }
        #endregion
    }
}
