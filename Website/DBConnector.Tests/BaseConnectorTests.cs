using System.Data.SqlClient;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using ResultReporter;

namespace DBConnector.Tests
{
    [TestClass]
    public class BaseConnectorTests
    {
        [ClassInitialize]
        public static void ClassInit(TestContext context)
        { Create_Test_Tables(); }

        [ClassCleanup]
        public static void ClassTeardown()
        { Drop_Test_Tables(); }


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

        #region Create/Drop Test Tables

        public static bool Create_Test_Tables()
        {
            bool created = true;

            created = created && Create_Test_Subscriptions_Table();
            created = created && Create_Test_Permissions_Table();
            created = created && Create_Test_Organizations_Table();
            created = created && Create_Test_Departments_Table();
            created = created && Create_Test_Users_Table();
            created = created && Create_Test_OrganizationUsers_Table();
            created = created && Create_Test_Files_Table();

            return created;
        }

        public static bool Drop_Test_Tables()
        {
            bool dropped = true;
            DBConnector connector = new DBConnector();

            //In this order
            dropped = dropped && (connector.Drop(new SqlCommand { CommandText = "DROP TABLE Test_Files;" })).ReturnValue;
            dropped = dropped && (connector.Drop(new SqlCommand { CommandText = "DROP TABLE Test_OrganizationUsers;" })).ReturnValue;
            dropped = dropped && (connector.Drop(new SqlCommand { CommandText = "DROP TABLE Test_Users;" })).ReturnValue;
            dropped = dropped && (connector.Drop(new SqlCommand { CommandText = "DROP TABLE Test_Departments;" })).ReturnValue;
            dropped = dropped && (connector.Drop(new SqlCommand { CommandText = "DROP TABLE Test_Organizations;" })).ReturnValue;
            dropped = dropped && (connector.Drop(new SqlCommand { CommandText = "DROP TABLE Test_PermissionLevelsHC;" })).ReturnValue;
            dropped = dropped && (connector.Drop(new SqlCommand { CommandText = "DROP TABLE Test_SubscriptionTypesHC;" })).ReturnValue;

            return dropped;
        }


        public static bool Create_Test_Files_Table()
        {
            DBConnector connector = new DBConnector();
            SqlCommand command = new SqlCommand
            {
                CommandText = "CREATE TABLE Test_Files(" +
                "FileGUID UNIQUEIDENTIFIER NOT NULL," +
                "FileName VARCHAR(100) NOT NULL," +
                "FilePath VARCHAR(500) NOT NULL," +
                "Created DATETIME NOT NULL DEFAULT GetUTCDate(), " +
                "LastModified DATETIME NOT NULL DEFAULT GetUTCDate()," +
                "LastAccessed DATETIME NOT NULL DEFAULT GetUTCDate()," +
                "OwnerId BIGINT NOT NULL," +
                "Sharing BIT DEFAULT 0," +
                "CONSTRAINT U_OwnerTest UNIQUE(OwnerId)," +
                "CONSTRAINT U_PathTest UNIQUE(FilePath, FileName)," +
                "FOREIGN KEY(OwnerId) REFERENCES Test_Users(UserId)," +
                "PRIMARY KEY(FileGUID),);"
            };

            return (connector.Create(command)).ReturnValue;
        }

        public static bool Create_Test_Users_Table()
        {
            DBConnector connector = new DBConnector();
            SqlCommand command = new SqlCommand
            {
                CommandText = "CREATE TABLE Test_Users(" +
                "UserId BIGINT NOT NULL," +
                "UserName NVARCHAR(100) NOT NULL, " +
                "UserPassword VARCHAR(100) NOT NULL, " +
                "Email VARCHAR(100) NOT NULL," +
                "OrganizationId BIGINT NOT NULL," +
                "CONSTRAINT U_UserNameTest UNIQUE(UserName)," +
                "CONSTRAINT U_UserEmailTest UNIQUE(Email)," +
                "FOREIGN KEY(OrganizationId) REFERENCES Test_Organizations(OrganizationId)," +
                "PRIMARY KEY(UserId),);"
            };

            return (connector.Create(command)).ReturnValue;
        }

        public static bool Create_Test_Organizations_Table()
        {
            DBConnector connector = new DBConnector();
            SqlCommand command = new SqlCommand
            {
                CommandText = "CREATE TABLE Test_Organizations(" +
                "OrganizationId BIGINT NOT NULL," +
                "OrganizationName VARCHAR(100) NOT NULL," +
                "SubscriptionTypeId BIGINT NOT NULL," +
                "CONSTRAINT U_OrgNameTest UNIQUE(OrganizationName)," +
                "FOREIGN KEY(SubscriptionTypeId) REFERENCES Test_SubscriptionTypesHC(SubscriptionTypeId)," +
                "PRIMARY KEY(OrganizationId),); "
            };

            return (connector.Create(command)).ReturnValue;
        }

        public static bool Create_Test_OrganizationUsers_Table()
        {
            DBConnector connector = new DBConnector();
            SqlCommand command = new SqlCommand
            {
                CommandText = "CREATE TABLE Test_OrganizationUsers(" +
                "OrganizationId BIGINT NOT NULL," +
                "OrganizationUserId BIGINT NOT NULL," +
                "FirstName VARCHAR(30) NOT NULL," +
                "LastName VARCHAR(30) NOT NULL, " +
                "DepartmentId BIGINT NOT NULL," +
                "PermissionLevelId BIGINT NOT NULL," +
                "FOREIGN KEY(PermissionLevelId) REFERENCES Test_PermissionLevelsHC(PermissionLevelId)," +
                "FOREIGN KEY(OrganizationId) REFERENCES Test_Organizations(OrganizationId)," +
                "FOREIGN KEY(OrganizationUserId) REFERENCES Test_Users(UserId)," +
                "PRIMARY KEY(OrganizationUserId, OrganizationId),);"
            };

            return (connector.Create(command)).ReturnValue;
        }

        public static bool Create_Test_Departments_Table()
        {
            DBConnector connector = new DBConnector();
            SqlCommand command = new SqlCommand
            {
                CommandText = "CREATE TABLE Test_Departments(" +
                "DepartmentId BIGINT NOT NULL," +
                "DepartmentName VARCHAR(100) NOT NULL, " +
                "OrganizationId BIGINT NOT NULL," +
                "CONSTRAINT U_OrgDeptTest UNIQUE(OrganizationId, DepartmentName)," +
                "FOREIGN KEY(OrganizationId) REFERENCES Test_Organizations(OrganizationId)," +
                "PRIMARY KEY(DepartmentId),); "
            };

            return (connector.Create(command)).ReturnValue;
        }

        public static bool Create_Test_Permissions_Table()
        {
            DBConnector connector = new DBConnector();
            SqlCommand command = new SqlCommand
            {
                CommandText = "CREATE TABLE Test_PermissionLevelsHC(" +
                "PermissionLevelId BIGINT NOT NULL," +
                "PermissionLevelName VARCHAR(100) NOT NULL," +
                "CONSTRAINT U_PermissionLevelNameTest UNIQUE(PermissionLevelName)," +
                "PRIMARY KEY(PermissionLevelId),); "
            };

            return (connector.Create(command)).ReturnValue;
        }

        public static bool Create_Test_Subscriptions_Table()
        {
            DBConnector connector = new DBConnector();
            SqlCommand command = new SqlCommand
            {
                CommandText = "CREATE TABLE Test_SubscriptionTypesHC(" +
                "SubscriptionTypeId BIGINT NOT NULL," +
                "SubscriptionTypeName VARCHAR(100) NOT NULL," +
                "CONSTRAINT U_SubscriptionNameTest UNIQUE(SubscriptionTypeName), " +
                "PRIMARY KEY(SubscriptionTypeId),);"
            };

            return (connector.Create(command)).ReturnValue;
        }

        #endregion
    }
}
