using System;
using System.Collections.Generic;
using LogApp.Common.Model;
using LogApp.Logic.Network;
using LogApp.Logic.Processing;
using LogApp.Logic.Storage;


namespace LogApp.Logic
{
    public sealed class LogAppController : IDisposable
    {
        #region Data Members
        private readonly IReadOnlyDictionary<String, String> settings;
        private readonly ConnectionManager connectionManager;
        private readonly ProcessMonitor monitor;
        #endregion

        #region Properties
        public bool IsConnectedToServer
        {
            get { return this.connectionManager.Connected; }
        }
        public string ConnectionInfo
        {
            get { return this.connectionManager.ConnectionInfo; }
        }
        #endregion

        #region Constructors
        public LogAppController(IReadOnlyDictionary<String, String> settings)
        {
            this.settings = settings;
            this.connectionManager = new ConnectionManager();

            string filepath = this.settings[LogAppSettings.StorageFilePath];
            double rewritingTime = double.Parse(settings[LogAppSettings.FileUpdateTime]);
            ClientStorage storage = new ClientStorage(filepath, rewritingTime);
            this.monitor = new ProcessMonitor(storage);
        }
        #endregion

        #region Members
        public void StartLogging()
        {
            int processUpdateTime =
                int.Parse(settings[LogAppSettings.ProcessStateUpdateTime]);
            this.monitor.Start(processUpdateTime);
        }
        public void StopLogging()
        {
            this.monitor.Stop();
        }
        public void ResetLogging()
        {
            this.StopLogging();
            this.StartLogging();
        }

        public void ConnectToServer()
        {
            String ip = this.settings[LogAppSettings.ServerIp];
            int port;
            int.TryParse(this.settings[LogAppSettings.ServerPort], out port);
            this.connectionManager.Connect(ip, port);
        }
        public void DisconnectFromServer()
        {
            this.connectionManager.Disconect();
        }
        public void ResetConnection()
        {
            this.DisconnectFromServer();
            this.ConnectToServer();
        }

        public void SendInfoToServer()
        {
            if (this.IsConnectedToServer)
            {
                ApplicationInfoCollection info = this.connectionManager.JustConnected ? 
                    this.monitor.CollectedInfo :
                    this.monitor.CollectedInfo.GetDifference(this.connectionManager.LastSentInfo);

                if (info.Count > 0)
                {
                    this.connectionManager.SendObject(info);
                }
            }
        }
        #endregion

        #region IDisposable Members
        public void Dispose()
        {
            this.connectionManager.Dispose();
            this.monitor.Dispose();
        } 
        #endregion
    }
}
