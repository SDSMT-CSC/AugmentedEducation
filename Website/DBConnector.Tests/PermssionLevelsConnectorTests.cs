using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using ResultReporter;

using Fields = DBConnector.PermissionLevelsConnector.FieldNames;

namespace DBConnector.Tests
{
    [TestClass]
    public class PermssionLevelsConnectorTests
    {
        private class Test_PermissionLevelsConnecector: PermissionLevelsConnector
        {
            public Test_PermissionLevelsConnecector() { _Table_Name = $"Test_{_Table_Name}"; }
        }

        #region Init/Teardown

        //Init
        [ClassInitialize]
        public static void ClassInit(TestContext context)
        { BaseConnectorTests.Create_Test_Tables(); }

        [TestInitialize]
        public void TestInit()
        //if Connector == null, create new: else it is self
        { Connector = Connector ?? new Test_PermissionLevelsConnecector(); }


        //Teardown
        [ClassCleanup]
        public static void ClassTeardown()
        { BaseConnectorTests.Drop_Test_Tables(); }

        [TestCleanup]
        public void TestTeardown()
        { Connector.Delete_All(); }

        #endregion


        #region Properties

        Test_PermissionLevelsConnecector Connector { get; set; }

        #endregion


        [TestMethod]
        public void Test_Insert()
        {
            ResultPackage<bool> insert_result = Connector.Insert((long)1, "default");

            Assert.IsTrue(string.IsNullOrEmpty(insert_result.ErrorMessage));
            Assert.IsTrue(insert_result.ReturnValue);
        }

        [TestMethod]
        public void Test_Delete()
        {
            ResultPackage<bool> delete_result;
            Dictionary<Fields, object> delete_where;

            //DELETE FROM Test_Permission... WHERE PermissionId = 1;
            delete_where = new Dictionary<Fields, object>
            { { Fields.PermissionLevelId, (long)1 }, };

            //Insert Value for 1 to ensure we can delete
            Connector.Insert(1, "default");
            delete_result = Connector.Delete(delete_where);

            Assert.IsTrue(string.IsNullOrEmpty(delete_result.ErrorMessage));
            Assert.IsTrue(delete_result.ReturnValue);
        }

        [TestMethod]
        public void Test_Passing_All_Simple_Operations()
        {
            
        }
        

        private ResultPackage<string> Simple_Query_By_Id(PermissionLevelsConnector connector, long id, List<Fields> select_fields)
        {
            Dictionary<Fields, object> where_dict = new Dictionary<Fields, object>()
            {
                { Fields.PermissionLevelId, id },
            };

            return connector.Query(where_dict, select_fields);
        }
        
        //private ResultPackage<List<PermissionLevel>> Parse_Query(string query_result)
        //{
        //    long long_out;
        //    string trimmed_value;
        //    List<string> rows = new List<string>();
        //    char[] delimit_fields = new char[] { ',' };
        //    string[] delimit_rows = new string[] { "\r\n" };
        //    List<PermissionLevel> query_permission_list = new List<PermissionLevel>();
        //    ResultPackage<List<PermissionLevel>> result = new ResultPackage<List<PermissionLevel>>();

        //    rows = query_result.Split(delimit_rows, StringSplitOptions.RemoveEmptyEntries).ToList();
        //    foreach (string row in rows)
        //    {
        //        PermissionLevel permission = new PermissionLevel();
        //        List<string> field_values = row.Split(delimit_fields).ToList();

        //        foreach (string field_value in field_values)
        //        {
        //            if (field_value.Contains("PermissionLevelId::"))
        //            {
        //                trimmed_value = field_value.Replace("PermissionLevelId::", "");
        //                if (long.TryParse(trimmed_value, out long_out)) { permission.PermissionLevelId = long_out; }
        //                else
        //                {
        //                    result.ErrorMessage = "Unable to parse PermissionLevelId";
        //                    break;
        //                }
        //            }
        //            else if (field_value.Contains("PermissionLevelName::"))
        //            {
        //                permission.PermissionLevelName = field_value.Replace("PermissionLevelName::", "").Trim();
        //            }
        //        }

        //        if (!string.IsNullOrEmpty(result.ErrorMessage))
        //            break;

        //        query_permission_list.Add(permission);
        //    }

        //    result.ReturnValue = query_permission_list;

        //    return result;
        //}

        private ResultPackage<bool> Simple_Update_By_Id(PermissionLevelsConnector connector, long old_id, long new_id)
        {
            Dictionary<Fields, object> set_values;
            Dictionary<Fields, object> where_values;

            set_values = new Dictionary<Fields, object>
            { { Fields.PermissionLevelId, new_id } };

            where_values = new Dictionary<Fields, object>
            { { Fields.PermissionLevelId, old_id } };

            return connector.Update(set_values, where_values);
        }

        private ResultPackage<bool> Simple_Delete_By_Id(PermissionLevelsConnector connector, long id)
        {
            Dictionary<Fields, object> where_values;

            where_values = new Dictionary<Fields, object>
            { { Fields.PermissionLevelId, id } };

            return connector.Delete(where_values);
        }
    }
}
