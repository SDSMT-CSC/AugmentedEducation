using System.Data.SqlClient;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using ResultReporter;

namespace DBConnector.Tests
{
    [TestClass]
    public class BaseConnectorTests
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

            Assert.IsTrue(connector.Check_Can_Conect());
        }

        [TestMethod]
        public void Test_Insert_No_Command_Fails()
        {
            SqlCommand command = new SqlCommand();
            DBConnector connector = new DBConnector();
            ResultPackage<bool> result = connector.Insert(command);

            Assert.IsFalse(result.ReturnValue, result.ErrorMessage);
        }

        [TestMethod]
        public void Test_Query_No_Command_Fails()
        {
            SqlCommand command = new SqlCommand();
            DBConnector connector = new DBConnector();
            ResultPackage<string> result = connector.Query(command);

            Assert.IsTrue(string.IsNullOrEmpty(result.ReturnValue), result.ErrorMessage);
        }

        [TestMethod]
        public void Test_Update_No_Command_Fails()
        {
            SqlCommand command = new SqlCommand();
            DBConnector connector = new DBConnector();
            ResultPackage<bool> result = connector.Update(command);

            Assert.IsFalse(result.ReturnValue, result.ErrorMessage);
        }

        [TestMethod]
        public void Test_Delete_No_Command_Fails()
        {
            SqlCommand command = new SqlCommand();
            DBConnector connector = new DBConnector();
            ResultPackage<bool> result = connector.Delete(command);

            Assert.IsFalse(result.ReturnValue, result.ErrorMessage);
        }
    }
}
