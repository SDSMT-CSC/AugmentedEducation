using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DBConnector.Tests
{
    [TestClass]
    public class DBTests
    {
        [TestMethod]
        public void Test_Connector_Constructor()
        {
            DBConnector connector = null;
            Assert.IsNull(connector);

            connector = new DBConnector();
            Assert.IsNotNull(connector);
        }

        [TestMethod]
        public void Test_Connector_Establish_Connection()
        {
            DBConnector connector = new DBConnector();

            Assert.IsTrue(connector.Connection_Established());
        }
    }
}
