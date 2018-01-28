using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using ResultReporter;

using Fields = DBConnector.PermissionLevelsConnector.FieldNames;

namespace DBConnector.Tests
{
    [TestClass]
    public class PermssionLevelsConnectorTests
    {
        private class PermissionLevel
        {
            public PermissionLevel() { }
            public PermissionLevel(long id, string name)
            {
                PermissionLevelId = id;
                PermissionLevelName = name;
            }

            public long PermissionLevelId { get; set; }
            public string PermissionLevelName { get; set; }
        }

        [TestMethod]
        public void Test_Passing_All_Simple_Operations()
        {
            PermissionLevel permission, from_query;
            List<Fields> select_fields = new List<Fields>();
            ResultPackage<bool> bool_result = new ResultPackage<bool>();
            ResultPackage<string> string_result = new ResultPackage<string>();
            Dictionary<Fields, object> set_fields = new Dictionary<Fields, object>();
            PermissionLevelsConnector permissionConnector = new PermissionLevelsConnector();
            ResultPackage<List<PermissionLevel>> query_result = new ResultPackage<List<PermissionLevel>>();

            permission = new PermissionLevel(1, "default");

            //Insert user record
            bool_result = Simple_Insert(permissionConnector, permission);
            Assert.IsTrue(bool_result.ReturnValue, bool_result.ErrorMessage);


            //Query for insert
            string_result = Simple_Query_By_Id(permissionConnector, 1, new List<Fields>());
            Assert.IsTrue(string.IsNullOrEmpty(string_result.ErrorMessage));
            //Parse query and verify insert
            query_result = Parse_Query(string_result.ReturnValue);
            Assert.IsTrue(string.IsNullOrEmpty(query_result.ErrorMessage));
            Assert.IsTrue(query_result.ReturnValue.Count == 1);
            from_query = query_result.ReturnValue[0];
            Assert.IsTrue(permission.PermissionLevelId == from_query.PermissionLevelId);
            Assert.IsTrue(permission.PermissionLevelName == from_query.PermissionLevelName);


            
            bool_result = Simple_Update_By_Id(permissionConnector, 1, 2);
            Assert.IsTrue(bool_result.ReturnValue);


            //Query for original value to show it's not there
            string_result = Simple_Query_By_Id(permissionConnector, 1, new List<Fields>());
            //No error, but also no result
            Assert.IsTrue(string.IsNullOrEmpty(string_result.ErrorMessage));
            Assert.IsTrue(string.IsNullOrEmpty(string_result.ReturnValue));


            //Update Id back to 1
            bool_result = Simple_Update_By_Id(permissionConnector, 2, 1);
            Assert.IsTrue(bool_result.ReturnValue);


            //Query for update
            string_result = Simple_Query_By_Id(permissionConnector, 1, new List<Fields>());
            Assert.IsTrue(string.IsNullOrEmpty(string_result.ErrorMessage));
            //Parse query and verify it matches original
            query_result = Parse_Query(string_result.ReturnValue);
            Assert.IsTrue(string.IsNullOrEmpty(query_result.ErrorMessage));
            Assert.IsTrue(query_result.ReturnValue.Count == 1);
            from_query = query_result.ReturnValue[0];
            Assert.IsTrue(permission.PermissionLevelId == from_query.PermissionLevelId);
            Assert.IsTrue(permission.PermissionLevelName == from_query.PermissionLevelName);


            //Delete test record
            bool_result = Simple_Delete_By_Id(permissionConnector, 1);
            Assert.IsTrue(bool_result.ReturnValue, bool_result.ErrorMessage);
        }

        private ResultPackage<bool> Simple_Insert(PermissionLevelsConnector connector, PermissionLevel permission)
        {
            return connector.Insert(permission.PermissionLevelId, permission.PermissionLevelName);
        }

        private ResultPackage<string> Simple_Query_By_Id(PermissionLevelsConnector connector, long id, List<Fields> select_fields)
        {
            Dictionary<Fields, object> where_dict = new Dictionary<Fields, object>()
            {
                { Fields.PermissionLevelId, id },
            };

            return connector.Query(where_dict, select_fields);
        }
        
        private ResultPackage<List<PermissionLevel>> Parse_Query(string query_result)
        {
            long long_out;
            string trimmed_value;
            List<string> rows = new List<string>();
            char[] delimit_fields = new char[] { ',' };
            string[] delimit_rows = new string[] { "\r\n" };
            List<PermissionLevel> query_permission_list = new List<PermissionLevel>();
            ResultPackage<List<PermissionLevel>> result = new ResultPackage<List<PermissionLevel>>();

            rows = query_result.Split(delimit_rows, StringSplitOptions.RemoveEmptyEntries).ToList();
            foreach (string row in rows)
            {
                PermissionLevel permission = new PermissionLevel();
                List<string> field_values = row.Split(delimit_fields).ToList();

                foreach (string field_value in field_values)
                {
                    if (field_value.Contains("PermissionLevelId::"))
                    {
                        trimmed_value = field_value.Replace("PermissionLevelId::", "");
                        if (long.TryParse(trimmed_value, out long_out)) { permission.PermissionLevelId = long_out; }
                        else
                        {
                            result.ErrorMessage = "Unable to parse PermissionLevelId";
                            break;
                        }
                    }
                    else if (field_value.Contains("PermissionLevelName::"))
                    {
                        permission.PermissionLevelName = field_value.Replace("PermissionLevelName::", "").Trim();
                    }
                }

                if (!string.IsNullOrEmpty(result.ErrorMessage))
                    break;

                query_permission_list.Add(permission);
            }

            result.ReturnValue = query_permission_list;

            return result;
        }

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
