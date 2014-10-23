using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;


namespace LogApp.Common.Model
{
    [Serializable]
    public class ApplicationInfo
    {
        #region Properties
        public string Name { get; set; }
        public int PPID { get; set; }
        public DateTime? StartTime { get; set; }
        public TimeSpan? ActivityTime { get; set; }
        public DateTime? EndTime { get; set; }
        public bool IsRunning { get; set; }
        public List<ModuleInfo> Modules { get; set; }
        #endregion

        #region Constructors
        public ApplicationInfo() { }
        public ApplicationInfo(Process process)
        {
            this.Name = process.ProcessName;
            this.PPID = process.Id;
            this.SetupStartTime(process);
            this.SetupModules(process);
        }

        #endregion

        #region Members
        public void UpdateTo(ApplicationInfo info)
        {
            this.StartTime = info.StartTime ?? this.StartTime;
            this.EndTime = info.EndTime ?? this.EndTime;
            this.ActivityTime = info.ActivityTime ?? this.ActivityTime;
            this.Modules = info.Modules;
            this.IsRunning = info.IsRunning;
        }

        public override string ToString()
        {
            return String.Format("{0}:{1}", this.Name, this.PPID);
        }
        #endregion

        #region Static Members
        public static bool AreEqual(ApplicationInfo a, ApplicationInfo b)
        {
            return (a.Name == b.Name) &&
                   (a.PPID == b.PPID) &&
                   (a.StartTime == b.StartTime) &&
                   (a.EndTime == b.EndTime) &&
                   (ModuleCollectionComparer.AreEqual(a.Modules, b.Modules));
        }
        public static bool AreEqualsByProcess(ApplicationInfo a, ApplicationInfo b)
        {
            return (a.Name == b.Name) && (a.PPID == b.PPID);
        }
        #endregion

        #region Assistants
        private void SetupModules(Process process)
        {
            try
            {
                this.Modules = process.Modules
                    .Cast<ProcessModule>()
                    .Select(item => new ModuleInfo(){Name = item.ModuleName})
                    .ToList();
            }
            catch (Exception)
            {
                this.Modules = null;
            }
        }
        private void SetupStartTime(Process process)
        {
            try
            {
                this.StartTime = process.StartTime;
            }
            catch (Exception)
            {
                this.StartTime = null;
            }
        }
        #endregion
    }
}
