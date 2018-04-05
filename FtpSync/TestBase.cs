using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NUnit.Framework;

namespace FtpSync
{
    [TestFixture]
    class TestBase
    {
        public void AreEqual<T1, T2>(T1 obj1, T2 obj2)
        {
            string json1 = JsonConvert.SerializeObject(obj1);
            string json2 = JsonConvert.SerializeObject(obj2);
            Assert.AreEqual(json1, json2);
        }
    }
}
