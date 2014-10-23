using System.Collections.Generic;
using System.Linq;

namespace LogApp.Common.Model
{
    internal static class ModuleCollectionComparer
    {
        public static bool AreEqual(IList<ModuleInfo> a, IList<ModuleInfo> b)
        {
            if (a == null && b == null)
                return true;
            if (a == null || b == null)
                return false;

            return (a.Count() == b.Count()) &&
                (a.All(b.Contains));
        }
    }
}
