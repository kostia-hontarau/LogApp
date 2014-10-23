using System;
using System.IO;
using System.Timers;
using LogApp.Common.Model;
using LogApp.Common.Serialization;

namespace LogApp.Logic.Storage
{
    internal class ClientStorage : IDisposable
    {
        #region Data Members
        private Timer writeTimer = new Timer();
        #endregion

        #region Properties
        public ApplicationInfoCollection CurrentState { get; private set; }
        public string FilePath { get; set; }
        #endregion

        #region Constructors
        public ClientStorage(string path, double fileUpdateInterval)
        {
            try
            {
                this.FilePath = path;
                this.CurrentState = this.GetCurrentStorageInfo();

                this.writeTimer.Elapsed += writeTimer_Elapsed;
                this.writeTimer.Interval = fileUpdateInterval;
                this.writeTimer.Start();
            }
            catch (Exception)
            {
                Console.WriteLine("Проблемы с доступом к файлу. Запись в файл не будет производиться.");
            }
        }
        #endregion

        #region Members
        public void UpdateTo(ApplicationInfoCollection state)
        {
            this.CurrentState.MergeWith(state);
        }
        #endregion

        #region Event Handlers
        private void writeTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                ApplicationInfoCollection result = this.GetCurrentStorageInfo();
                result.MergeWith(this.CurrentState);
                this.WriteStorageInfo(result);
            }
            catch (Exception exc)
            {
                Console.WriteLine(exc.Message);
            }
        }
        #endregion

        #region Assistants
        private ApplicationInfoCollection GetCurrentStorageInfo()
        {
            try
            {
                this.CreateIfNotExist();
                ApplicationInfoCollection buffer = this.ReadStorageInfo();
                if (buffer == null)
                {
                    buffer = new ApplicationInfoCollection();
                    this.WriteStorageInfo(buffer);
                }
                return buffer;
            }
            catch (Exception exc)
            {
                Console.WriteLine(exc.Message);
                return new ApplicationInfoCollection();
            }
        }
        private void CreateIfNotExist()
        {
            if (!File.Exists(this.FilePath))
            {
                File.Create(this.FilePath);
            }
        }
        private ApplicationInfoCollection ReadStorageInfo()
        {

            String json = File.ReadAllText(this.FilePath);
            return JsonSerializer.ConvertToObject(json);

        }
        private void WriteStorageInfo(ApplicationInfoCollection result)
        {
            String json = JsonSerializer.ConvertToJson(result);
            File.WriteAllText(this.FilePath, json);
        }
        #endregion

        #region IDisposable Members
        public void Dispose()
        {
            this.writeTimer.Dispose();
        } 
        #endregion
    }
}
