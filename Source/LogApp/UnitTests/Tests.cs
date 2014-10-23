using System;
using System.Collections.Generic;
using System.Linq;
using LogApp.Common.Model;
using LogApp.Common.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests
{
    [TestClass]
    public class Tests
    {
        [TestMethod]
        public void GettingDifferenceTest()
        {
            ApplicationInfo info1 = new ApplicationInfo { Name = "proc1" };
            ApplicationInfo info2 = new ApplicationInfo { Name = "proc2" };

            ApplicationInfoCollection a = new ApplicationInfoCollection
            {
                info1,
                info2
            };
            ApplicationInfoCollection b = new ApplicationInfoCollection
            {
                info1
            };

            ApplicationInfoCollection c = a.GetDifference(b);

            Assert.AreEqual(c.Count, 1);
            Assert.AreEqual(c.First().Name, info2.Name);
        }

        [TestMethod]
        public void MergingTest()
        {
            ApplicationInfo info1 = new ApplicationInfo { Name = "proc1" };
            ApplicationInfo info2 = new ApplicationInfo { Name = "proc2" };

            ApplicationInfoCollection a = new ApplicationInfoCollection
            {
                info1
            };
            ApplicationInfoCollection b = new ApplicationInfoCollection
            {
                info2
            };

            a.MergeWith(b);

            Assert.AreEqual(a.Count, 2);
            Assert.AreEqual(a.First().Name, info1.Name);
            Assert.AreEqual(a.Skip(1).Take(1).First().Name, info2.Name);

        }

        [TestMethod]
        public void SerializeTest()
        {
            ApplicationInfoCollection collection = new ApplicationInfoCollection();
            ApplicationInfo info = new ApplicationInfo
            {
                Name = "proc1", 
                PPID = 3333, 
                IsRunning = true, 
                Modules = new List<ModuleInfo> {new ModuleInfo {Name = "lib.dll"}}
            };
            collection.Add(info);
            string result = JsonSerializer.ConvertToJson(collection);
            const string expectedResult = "[{\"Name\":\"proc1\",\"PPID\":3333,\"StartTime\":null,\"ActivityTime\":null,\"EndTime\":null,\"IsRunning\":true,\"Modules\":[{\"Name\":\"lib.dll\"}]}]";
            Assert.AreEqual(result, expectedResult);
        }

        [TestMethod]
        public void DeserializeTest()
        {
            const string json = "[{\"Name\":\"proc1\",\"PPID\":3333,\"StartTime\":null,\"ActivityTime\":null,\"EndTime\":null,\"IsRunning\":true,\"Modules\":[{\"Name\":\"lib.dll\"}]}]";

            ApplicationInfoCollection result = JsonSerializer.ConvertToObject(json);
            ApplicationInfo info = result.First();

            Assert.AreEqual(info.Name, "proc1");
            Assert.AreEqual(info.PPID, 3333);

            bool libExists = info.Modules.Any(item => item.Name == "lib.dll");
            bool libIsSingle = info.Modules.Count == 1;
            Assert.IsTrue(libExists && libIsSingle);

        }

        [TestMethod]
        public void ApplicationInfoComparingTest()
        {
            ApplicationInfo info1 = new ApplicationInfo {Name = "proc1", PPID = 1231, EndTime = DateTime.Now };
            ApplicationInfo info2 = new ApplicationInfo { Name = "proc1", PPID = 1231 };

            Assert.IsTrue(ApplicationInfo.AreEqualsByProcess(info1, info2));
            Assert.IsFalse(ApplicationInfo.AreEqual(info1, info2));
        }
    }
}
