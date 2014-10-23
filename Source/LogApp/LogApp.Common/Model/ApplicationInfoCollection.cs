using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace LogApp.Common.Model
{
    [Serializable]
    public class ApplicationInfoCollection :
        ICollection<ApplicationInfo>, IEnumerable<ApplicationInfo>
    {
        #region Data Members
        private readonly List<ApplicationInfo> data = new List<ApplicationInfo>();
        private object locker = new object();
        #endregion

        #region Properties
        public int Count
        {
            get { return this.data.Count; }
        }
        public bool IsReadOnly { get; private set; }
        #endregion

        #region Events
        public event EventHandler Changed;
        #endregion

        #region ICollection Members
        public void Add(ApplicationInfo item)
        {
            this.data.Add(item);
            this.OnChanged();
        }
        public void Clear()
        {
            this.data.Clear();
            this.OnChanged();
        }
        public bool Contains(ApplicationInfo item)
        {
            return this.data.Any(info => ApplicationInfo.AreEqual(info, item));
        }

        public void CopyTo(ApplicationInfo[] array, int arrayIndex)
        {
            this.data.CopyTo(array, arrayIndex);
        }
        public bool Remove(ApplicationInfo item)
        {
            bool removed = this.data.Remove(item);
            if (removed)
            {
                this.OnChanged();
            }
            return removed;
        }
        #endregion

        #region IEnumerable Members
        public IEnumerator<ApplicationInfo> GetEnumerator()
        {
            return this.data.GetEnumerator();
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.data.GetEnumerator();
        }
        #endregion

        #region Members
        public void AddRange(IEnumerable<ApplicationInfo> range)
        {
            foreach (ApplicationInfo info in range)
            {
                this.data.Add(info);
            }
        }
        public ApplicationInfoCollection GetDifference(ApplicationInfoCollection minuend)
        {
            ApplicationInfoCollection difference = new ApplicationInfoCollection();
            lock (this.locker)
            {
                IEnumerable<ApplicationInfo> buffer = this.data
                    .Where(item => IsInDifference(item, minuend))
                    .ToList();
                difference.AddRange(buffer);
            }
            return difference;
        }
        public void MergeWith(ApplicationInfoCollection collection)
        {
            lock (this.locker)
            {
                foreach (ApplicationInfo info in collection)
                {
                    this.MergeWith(info);
                }
                this.OnChanged();
            }
        }
        public void MergeWith(ApplicationInfo info)
        {
            lock (this.locker)
            {
                ApplicationInfo applicationInfo = this.data
                    .FirstOrDefault(item => ApplicationInfo.AreEqualsByProcess(info, item));

                if (applicationInfo != null)
                {
                    applicationInfo.UpdateTo(info);
                }
                else
                {
                    this.data.Add(info);
                }
            }
        }
        #endregion

        #region Assistants
        private void OnChanged()
        {
            EventHandler handler = Changed;
            if (handler != null) handler(this, EventArgs.Empty);
        }
        #endregion

        #region Static Members
        private static bool IsInDifference(ApplicationInfo item, IEnumerable<ApplicationInfo> minuend)
        {
            return minuend.All(info => !ApplicationInfo.AreEqual(info, item));
        }
        #endregion
    }
}
