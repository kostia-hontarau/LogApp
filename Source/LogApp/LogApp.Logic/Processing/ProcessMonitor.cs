using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.Timers;
using LogApp.Common.Model;
using LogApp.Logic.Storage;


namespace LogApp.Logic.Processing
{
    internal class ProcessMonitor : IDisposable
    {
        #region P/Invoke Members
        private class NativeMethods
        {
            [DllImport("user32.dll")]
            public static extern IntPtr GetForegroundWindow();
            [DllImport("user32.dll", SetLastError = true)]
            public static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);
        }
        #endregion

        #region Constants
        private const string TargetInstanceDescription = "TargetInstance isa \"Win32_Process\"";
        #endregion

        #region Data Members
        private ClientStorage storage;
        private Timer refreshTimer = new Timer();
        private ManagementEventWatcher instanceDeletionWatcher;

        private System.Threading.Timer activityTimer;
        private uint lastProcess = 0;
        private Stopwatch activeProcessTime = new Stopwatch();

        #endregion

        #region Properties
        public ApplicationInfoCollection CollectedInfo
        {
            get { return this.storage.CurrentState; }
        }
        #endregion

        #region Constructors
        public ProcessMonitor(ClientStorage storage)
        {
            this.storage = storage;
            this.instanceDeletionWatcher = new ManagementEventWatcher();
            this.refreshTimer.Elapsed += refreshTimer_Elapsed;

            this.activityTimer = new System.Threading.Timer(this.MeasureActivity, null, TimeSpan.Zero, new TimeSpan(0, 0, 0, 0, 1000));
        }
        #endregion

        #region Members
        public void Start(int refreshTime)
        {
            this.instanceDeletionWatcher.Stop();
            TimeSpan updateInterval = new TimeSpan(0, 0, 0, 0, refreshTime);
            this.SetupDeletionMonitor(updateInterval);
            this.instanceDeletionWatcher.Start();

            this.refreshTimer.Stop();
            this.refreshTimer.Interval = refreshTime;
            this.refreshTimer.Start();


        }
        public void Stop()
        {
            this.instanceDeletionWatcher.Stop();
            this.instanceDeletionWatcher.EventArrived -= ProcessEndEvent;

            this.refreshTimer.Stop();
        }
        #endregion

        #region Event Handlers
        private void refreshTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            ApplicationInfoCollection result = new ApplicationInfoCollection();
            result.AddRange(ProcessMonitor.GetProcesses());
            foreach (ApplicationInfo info in this.CollectedInfo)
            {
                ApplicationInfo running = result
                    .FirstOrDefault(item => ApplicationInfo.AreEqualsByProcess(item, info));
                if (running == null)
                {
                    info.IsRunning = false;
                }
            }
            this.storage.CurrentState.MergeWith(result);
        }

        private void ProcessEndEvent(object sender, EventArrivedEventArgs e)
        {
            ManagementBaseObject newEvent = e.NewEvent;
            ManagementBaseObject targetInstance = (ManagementBaseObject)newEvent["TargetInstance"];
            ApplicationInfo info = new ApplicationInfo
            {
                Name = this.GetProcessName(targetInstance),
                PPID = (int)(UInt32)targetInstance.Properties["ProcessId"].Value,
                StartTime = null,
                EndTime = DateTime.Now,
                IsRunning = false
            };

            this.storage.CurrentState.MergeWith(info);
        }
        #endregion

        #region Assistants
        private void MeasureActivity(object state)
        {
            lock (this.activeProcessTime)
            {
                uint pid = GetActiveProcessId();

                if (pid != 0)
                {
                    if (this.lastProcess != 0)
                    {
                        this.UpdateLastProcessActivity();
                    }
                    this.lastProcess = pid;
                    this.activeProcessTime.Restart();
                }
            }
        }

        private void UpdateLastProcessActivity()
        {
            Process process = Process.GetProcesses()
                .FirstOrDefault(item => item.Id == this.lastProcess);
            if (process != null)
            {
                ApplicationInfo info = this.storage.CurrentState
                    .FirstOrDefault(item => (item.PPID == process.Id) && (item.Name == process.ProcessName));

                if (info != null)
                {
                    String time = this.activeProcessTime.Elapsed.ToString(@"hh\:mm\:ss");
                    info.ActivityTime = info.ActivityTime.HasValue
                        ? info.ActivityTime.Value.Add(TimeSpan.Parse(time))
                        : new TimeSpan();
                    this.storage.CurrentState.MergeWith(info);
                }
            }
        }
        private static uint GetActiveProcessId()
        {
            try
            {
                IntPtr hWnd = NativeMethods.GetForegroundWindow();
                uint pid;
                NativeMethods.GetWindowThreadProcessId(hWnd, out pid);
                return pid;
            }
            catch (Exception)
            {
                return 0;
            }
        }
        private string GetProcessName(ManagementBaseObject targetInstance)
        {
            string buffer = (string)targetInstance.Properties["Name"].Value;
            int position = buffer.LastIndexOf('.');
            return buffer.Substring(0, position);
        }
        private void SetupDeletionMonitor(TimeSpan updateInterval)
        {
            WqlEventQuery instanceDeletionQuery = new WqlEventQuery("__InstanceDeletionEvent", updateInterval, TargetInstanceDescription);
            this.instanceDeletionWatcher.Query = instanceDeletionQuery;
            this.instanceDeletionWatcher.EventArrived += ProcessEndEvent;
        }
        #endregion

        #region Static Members
        private static IEnumerable<ApplicationInfo> GetProcesses()
        {
            return Process.GetProcesses()
                .Select(process => new ApplicationInfo(process) { IsRunning = true })
                .ToList();
        }
        #endregion

        #region IDisposable Members
        public void Dispose()
        {
            this.instanceDeletionWatcher.Dispose();
            this.refreshTimer.Dispose();
            this.activityTimer.Dispose();
        } 
        #endregion
    }
}
