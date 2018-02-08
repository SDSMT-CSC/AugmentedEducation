﻿using System;
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
        public void Test_Insert_And_Query()
        {
            ResultPackage<string> query_result;
            Dictionary<Fields, object> find_where;
            ResultPackage<List<Tuple<Fields, object>>> parsed_query;
            ResultPackage<bool> insert_result = Connector.Insert(1, "default");

            Assert.IsTrue(string.IsNullOrEmpty(insert_result.ErrorMessage));
            Assert.IsTrue(insert_result.ReturnValue);

            find_where = new Dictionary<Fields, object>
            { { Fields.PermissionLevelId, (long)1 }, };

            // SELECT PermissionLevelID From Test_Permissions WHERE id = 1...
            query_result = Connector.Query(find_where, new List<Fields> { Fields.PermissionLevelId });
            Assert.IsTrue(string.IsNullOrEmpty(query_result.ErrorMessage));
            Assert.IsFalse(string.IsNullOrEmpty(query_result.ReturnValue));

            parsed_query = Parse_Query(query_result.ReturnValue);
            Assert.IsTrue(string.IsNullOrEmpty(parsed_query.ErrorMessage));
            Assert.IsTrue(parsed_query.ReturnValue.Count == 1);
            Assert.IsTrue(parsed_query.ReturnValue[0].Item1 == Fields.PermissionLevelId);
            Assert.IsTrue(parsed_query.ReturnValue[0].Item2.Equals((long)1));


            // SELECT PermissionLevelName From Test_Permissions WHERE id = 1 ...
            query_result = Connector.Query(find_where, new List<Fields> { Fields.PermissionLevelName });
            Assert.IsTrue(string.IsNullOrEmpty(query_result.ErrorMessage));
            Assert.IsFalse(string.IsNullOrEmpty(query_result.ReturnValue));

            parsed_query = Parse_Query(query_result.ReturnValue);
            Assert.IsTrue(string.IsNullOrEmpty(parsed_query.ErrorMessage));
            Assert.IsTrue(parsed_query.ReturnValue.Count == 1);
            Assert.IsTrue(parsed_query.ReturnValue[0].Item1 == Fields.PermissionLevelName);
            Assert.IsTrue(parsed_query.ReturnValue[0].Item2.Equals("default"));


            // SELECT * From Test_Permissions WHERE id = 1 ...
            query_result = Connector.Query(find_where, new List<Fields> ());
            Assert.IsTrue(string.IsNullOrEmpty(query_result.ErrorMessage));
            Assert.IsFalse(string.IsNullOrEmpty(query_result.ReturnValue));

            parsed_query = Parse_Query(query_result.ReturnValue);
            Assert.IsTrue(string.IsNullOrEmpty(parsed_query.ErrorMessage));
            Assert.IsTrue(parsed_query.ReturnValue.Count == 2);

            foreach(Tuple<Fields, object> item in parsed_query.ReturnValue)
            {
                if(item.Item1 == Fields.PermissionLevelId) { Assert.IsTrue(item.Item2.Equals((long)1)); }
                else if (item.Item1 == Fields.PermissionLevelName) { Assert.IsTrue(item.Item2.Equals("default")); }
                else { Assert.IsTrue(false); }
            }
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
        public void Test_Delete_And_Query()
        {
            ResultPackage<bool> delete_result;
            ResultPackage<string> query_result;
            Dictionary<Fields, object> delete_where, find_where;

            //DELETE FROM Test_Permission... WHERE PermissionId = 1;
            delete_where = new Dictionary<Fields, object>
            { { Fields.PermissionLevelId, (long)1 }, };

            //Insert Value for 1 to ensure we can delete
            Connector.Insert(1, "default");
            delete_result = Connector.Delete(delete_where);

            Assert.IsTrue(string.IsNullOrEmpty(delete_result.ErrorMessage));
            Assert.IsTrue(delete_result.ReturnValue);

            find_where = delete_where;

            // SELECT * From Test_Permissions WHERE id = 1...
            query_result = Connector.Query(find_where, new List<Fields>());
            //No error for empty result but also no return value
            Assert.IsTrue(string.IsNullOrEmpty(query_result.ErrorMessage));
            Assert.IsTrue(string.IsNullOrEmpty(query_result.ReturnValue));
        }

        [TestMethod]
        public void Test_Update()
        {
            ResultPackage<bool> update_result;
            Dictionary<Fields, object> set_values, where_values;

            set_values = new Dictionary<Fields, object>
            { {Fields.PermissionLevelId, (long)2}, };
            //set id = 2 where id = 1;
            where_values = new Dictionary<Fields, object>
            { { Fields.PermissionLevelId, (long)1 }, };

            //Insert Value for 1 to ensure we can delete
            Connector.Insert(1, "default");
            update_result = Connector.Update(set_values, where_values);

            Assert.IsTrue(string.IsNullOrEmpty(update_result.ErrorMessage));
            Assert.IsTrue(update_result.ReturnValue);
        }

        [TestMethod]
        public void Test_Update_And_Query()
        {
            ResultPackage<bool> update_result;
            ResultPackage<string> query_result;
            ResultPackage<List<Tuple<Fields, object>>> parsed_query;
            Dictionary<Fields, object> find_where, set_values, where_values;

            set_values = new Dictionary<Fields, object>
            { {Fields.PermissionLevelId, (long)2}, };
            //set id = 2 where id = 1;
            where_values = new Dictionary<Fields, object>
            { { Fields.PermissionLevelId, (long)1 }, };

            //Insert Value for 1 to ensure we can delete
            Connector.Insert(1, "default");
            update_result = Connector.Update(set_values, where_values);

            Assert.IsTrue(string.IsNullOrEmpty(update_result.ErrorMessage));
            Assert.IsTrue(update_result.ReturnValue);

            //Try to query original value should fail
            find_where = where_values;

            // SELECT * From Test_Permissions WHERE id = 1...
            query_result = Connector.Query(find_where, new List<Fields>());
            //No error for empty result but also no return value
            Assert.IsTrue(string.IsNullOrEmpty(query_result.ErrorMessage));
            Assert.IsTrue(string.IsNullOrEmpty(query_result.ReturnValue));

            //Try to query updated values should succeed
            find_where = set_values;

            // SELECT PermissionLevelID From Test_Permissions WHERE id = 2...
            query_result = Connector.Query(find_where, new List<Fields> { Fields.PermissionLevelId });
            Assert.IsTrue(string.IsNullOrEmpty(query_result.ErrorMessage));
            Assert.IsFalse(string.IsNullOrEmpty(query_result.ReturnValue));

            parsed_query = Parse_Query(query_result.ReturnValue);
            Assert.IsTrue(string.IsNullOrEmpty(parsed_query.ErrorMessage));
            Assert.IsTrue(parsed_query.ReturnValue.Count == 1);
            Assert.IsTrue(parsed_query.ReturnValue[0].Item1 == Fields.PermissionLevelId);
            Assert.IsTrue(parsed_query.ReturnValue[0].Item2.Equals((long)2));
        }

        private ResultPackage<List<Tuple<Fields, object>>> Parse_Query(string query_result)
        {
            List<string> rows = new List<string>();
            char[] delimit_fields = new char[] { ',' };
            string[] delimit_rows = new string[] { "\r\n" };
            List<Tuple<Fields, object>> query_permission_list = new List<Tuple<Fields, object>>();
            ResultPackage<List<Tuple<Fields, object>>> result = new ResultPackage<List<Tuple<Fields, object>>>();

            rows = query_result.Split(delimit_rows, StringSplitOptions.RemoveEmptyEntries).ToList();
            foreach (string row in rows)
            {
                List<string> field_values = row.Split(delimit_fields).ToList();

                foreach (string field_value in field_values)
                {
                    if (field_value.Contains("PermissionLevelId::"))
                    {
                        if (long.TryParse(field_value.Replace("PermissionLevelId::", ""), out long long_out))
                        {
                            query_permission_list.Add(new Tuple<Fields, object>(Fields.PermissionLevelId, long_out));
                        }
                        else
                        {
                            result.ErrorMessage = "Unable to parse PermissionLevelId";
                            break;
                        }
                    }
                    else if (field_value.Contains("PermissionLevelName::"))
                    {
                        string value = field_value.Replace("PermissionLevelName::", "").Trim();
                        query_permission_list.Add(new Tuple<Fields, object>(Fields.PermissionLevelName, value));
                    }
                    else
                    {
                        result.ErrorMessage = "Unable to parse PermissionLevelId";
                        break;
                    }
                }

                if (!string.IsNullOrEmpty(result.ErrorMessage)) { break; }
            }

            result.ReturnValue = query_permission_list;

            return result;
        }
    }
}
