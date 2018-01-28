using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using ResultReporter;

using Fields = DBConnector.SubscriptionTypesConnector.FieldNames;

namespace DBConnector.Tests
{
    [TestClass]
    public class SubscriptionTypesConnectorTests
    {
        private class SubscriptionType
        {
            public SubscriptionType() { }
            public SubscriptionType(long id, string name)
            {
                SubscriptionTypeId = id;
                SubscriptionTypeName = name;
            }

            public long SubscriptionTypeId { get; set; }
            public string SubscriptionTypeName { get; set; }
        }

        [TestMethod]
        public void Test_Passing_All_Simple_Operations()
        {
            SubscriptionType subscription, from_query;
            List<Fields> select_fields = new List<Fields>();
            ResultPackage<bool> bool_result = new ResultPackage<bool>();
            ResultPackage<string> string_result = new ResultPackage<string>();
            Dictionary<Fields, object> set_fields = new Dictionary<Fields, object>();
            SubscriptionTypesConnector subscriptionConnector = new SubscriptionTypesConnector();
            ResultPackage<List<SubscriptionType>> query_result = new ResultPackage<List<SubscriptionType>>();

            subscription = new SubscriptionType(1, "default");

            //Insert user record
            bool_result = Simple_Insert(subscriptionConnector, subscription);
            Assert.IsTrue(bool_result.ReturnValue, bool_result.ErrorMessage);


            //Query for insert
            string_result = Simple_Query_By_Id(subscriptionConnector, 1, new List<Fields>());
            Assert.IsTrue(string.IsNullOrEmpty(string_result.ErrorMessage));
            //Parse query and verify insert
            query_result = Parse_Query(string_result.ReturnValue);
            Assert.IsTrue(string.IsNullOrEmpty(query_result.ErrorMessage));
            Assert.IsTrue(query_result.ReturnValue.Count == 1);
            from_query = query_result.ReturnValue[0];
            Assert.IsTrue(subscription.SubscriptionTypeId == from_query.SubscriptionTypeId);
            Assert.IsTrue(subscription.SubscriptionTypeName == from_query.SubscriptionTypeName);



            bool_result = Simple_Update_By_Id(subscriptionConnector, 1, 2);
            Assert.IsTrue(bool_result.ReturnValue);


            //Query for original value to show it's not there
            string_result = Simple_Query_By_Id(subscriptionConnector, 1, new List<Fields>());
            //No error, but also no result
            Assert.IsTrue(string.IsNullOrEmpty(string_result.ErrorMessage));
            Assert.IsTrue(string.IsNullOrEmpty(string_result.ReturnValue));


            //Update Id back to 1
            bool_result = Simple_Update_By_Id(subscriptionConnector, 2, 1);
            Assert.IsTrue(bool_result.ReturnValue);


            //Query for update
            string_result = Simple_Query_By_Id(subscriptionConnector, 1, new List<Fields>());
            Assert.IsTrue(string.IsNullOrEmpty(string_result.ErrorMessage));
            //Parse query and verify it matches original
            query_result = Parse_Query(string_result.ReturnValue);
            Assert.IsTrue(string.IsNullOrEmpty(query_result.ErrorMessage));
            Assert.IsTrue(query_result.ReturnValue.Count == 1);
            from_query = query_result.ReturnValue[0];
            Assert.IsTrue(subscription.SubscriptionTypeId == from_query.SubscriptionTypeId);
            Assert.IsTrue(subscription.SubscriptionTypeName == from_query.SubscriptionTypeName);


            //Delete test record
            bool_result = Simple_Delete_By_Id(subscriptionConnector, 1);
            Assert.IsTrue(bool_result.ReturnValue, bool_result.ErrorMessage);
        }

        private ResultPackage<bool> Simple_Insert(SubscriptionTypesConnector connector, SubscriptionType subscription)
        {
            return connector.Insert(subscription.SubscriptionTypeId, subscription.SubscriptionTypeName);
        }

        private ResultPackage<string> Simple_Query_By_Id(SubscriptionTypesConnector connector, long id, List<Fields> select_fields)
        {
            Dictionary<Fields, object> where_dict = new Dictionary<Fields, object>()
            {
                { Fields.SubscriptionTypeId, id },
            };

            return connector.Query(where_dict, select_fields);
        }

        private ResultPackage<List<SubscriptionType>> Parse_Query(string query_result)
        {
            long long_out;
            string trimmed_value;
            List<string> rows = new List<string>();
            char[] delimit_fields = new char[] { ',' };
            string[] delimit_rows = new string[] { "\r\n" };
            List<SubscriptionType> query_subscriptions_list = new List<SubscriptionType>();
            ResultPackage<List<SubscriptionType>> result = new ResultPackage<List<SubscriptionType>>();

            rows = query_result.Split(delimit_rows, StringSplitOptions.RemoveEmptyEntries).ToList();
            foreach (string row in rows)
            {
                SubscriptionType subscription = new SubscriptionType();
                List<string> field_values = row.Split(delimit_fields).ToList();

                foreach (string field_value in field_values)
                {
                    if (field_value.Contains("SubscriptionTypeId::"))
                    {
                        trimmed_value = field_value.Replace("SubscriptionTypeId::", "");
                        if (long.TryParse(trimmed_value, out long_out)) { subscription.SubscriptionTypeId = long_out; }
                        else
                        {
                            result.ErrorMessage = "Unable to parse SubscriptionTypeId";
                            break;
                        }
                    }
                    else if (field_value.Contains("SubscriptionTypeName::"))
                    {
                        subscription.SubscriptionTypeName = field_value.Replace("SubscriptionTypeName::", "").Trim();
                    }
                }

                if (!string.IsNullOrEmpty(result.ErrorMessage))
                    break;

                query_subscriptions_list.Add(subscription);
            }

            result.ReturnValue = query_subscriptions_list;

            return result;
        }

        private ResultPackage<bool> Simple_Update_By_Id(SubscriptionTypesConnector connector, long old_id, long new_id)
        {
            Dictionary<Fields, object> set_values;
            Dictionary<Fields, object> where_values;

            set_values = new Dictionary<Fields, object>
            { { Fields.SubscriptionTypeId, new_id } };

            where_values = new Dictionary<Fields, object>
            { { Fields.SubscriptionTypeId, old_id } };

            return connector.Update(set_values, where_values);
        }

        private ResultPackage<bool> Simple_Delete_By_Id(SubscriptionTypesConnector connector, long id)
        {
            Dictionary<Fields, object> where_values;

            where_values = new Dictionary<Fields, object>
            { { Fields.SubscriptionTypeId, id } };

            return connector.Delete(where_values);
        }

    }
}
