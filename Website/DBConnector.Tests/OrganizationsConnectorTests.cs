using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using ResultReporter;

using Fields = DBConnector.OrganizationsConnector.FieldNames;

namespace DBConnector.Tests
{
    [TestClass]
    public class OrganizationsConnectorTests
    {
        private class Test_OrganizationsConnector : OrganizationsConnector
        {
            public Test_OrganizationsConnector() { _Table_Name = $"Test_{_Table_Name}"; }
        }


        #region Init/Teardown

        //Init
        [ClassInitialize]
        public static void ClassInit(TestContext context)
        {
            BaseConnectorTests.Create_Test_Tables();
            SubscriptionTypesConnectorTests.Insert_Test_Value();
        }

        [TestInitialize]
        public void TestInit()
        //if Connector == null, create new: else it is self
        { Connector = Connector ?? new Test_OrganizationsConnector(); }


        //Teardown
        [ClassCleanup]
        public static void ClassTeardown()
        { BaseConnectorTests.Drop_Test_Tables(); }

        [TestCleanup]
        public void TestTeardown()
        { Connector.Delete_All(); }

        #endregion


        #region Properties

        private static Test_OrganizationsConnector Connector { get; set; }

        #endregion


        #region Tests

        [TestMethod]
        public void Test_Insert()
        {
            ResultPackage<bool> insert_result = Insert_Test_Value();
            Assert.IsTrue(string.IsNullOrEmpty(insert_result.ErrorMessage));
            Assert.IsTrue(insert_result.ReturnValue);
        }

        [TestMethod]
        public void Test_Insert_Missing_Dependency()
        {
            //remove dependency added in ClassInit()
            SubscriptionTypesConnectorTests.Delete_Test_Value();

            ResultPackage<bool> insert_result = Insert_Test_Value();
            Assert.IsFalse(string.IsNullOrEmpty(insert_result.ErrorMessage));
            Assert.IsFalse(insert_result.ReturnValue);

            //re-add dependency for following tests
            SubscriptionTypesConnectorTests.Insert_Test_Value();
        }

        [TestMethod]
        public void Test_Insert_And_Query_By_OrganizationId()
        {
            ResultPackage<string> query_result;
            Dictionary<Fields, object> find_where;
            ResultPackage<List<Tuple<Fields, object>>> parsed_query;
            ResultPackage<bool> insert_result = Insert_Test_Value();

            Assert.IsTrue(string.IsNullOrEmpty(insert_result.ErrorMessage));
            Assert.IsTrue(insert_result.ReturnValue);

            find_where = new Dictionary<Fields, object>
            { { Fields.OrganizationId, (long)1 }, };

            //
            // SELECT SubscriptionTypeID From Test_subscriptions WHERE id = 1...
            query_result = Connector.Query(find_where, new List<Fields> { Fields.OrganizationId });
            Assert.IsTrue(string.IsNullOrEmpty(query_result.ErrorMessage));
            Assert.IsFalse(string.IsNullOrEmpty(query_result.ReturnValue));

            parsed_query = Parse_Query(query_result.ReturnValue);
            Assert.IsTrue(string.IsNullOrEmpty(parsed_query.ErrorMessage));
            Assert.IsTrue(parsed_query.ReturnValue.Count == 1);
            Assert.IsTrue(parsed_query.ReturnValue[0].Item1 == Fields.OrganizationId);
            Assert.IsTrue(parsed_query.ReturnValue[0].Item2.Equals((long)1));


            //
            // SELECT SubscriptionTypeID From Test_subscriptions WHERE id = 1...
            query_result = Connector.Query(find_where, new List<Fields> { Fields.SubscriptionTypeId });
            Assert.IsTrue(string.IsNullOrEmpty(query_result.ErrorMessage));
            Assert.IsFalse(string.IsNullOrEmpty(query_result.ReturnValue));

            parsed_query = Parse_Query(query_result.ReturnValue);
            Assert.IsTrue(string.IsNullOrEmpty(parsed_query.ErrorMessage));
            Assert.IsTrue(parsed_query.ReturnValue.Count == 1);
            Assert.IsTrue(parsed_query.ReturnValue[0].Item1 == Fields.SubscriptionTypeId);
            Assert.IsTrue(parsed_query.ReturnValue[0].Item2.Equals((long)1));


            //
            // SELECT SubscriptionTypeName From Test_subscriptions WHERE id = 1 ...
            query_result = Connector.Query(find_where, new List<Fields> { Fields.OrganizationName });
            Assert.IsTrue(string.IsNullOrEmpty(query_result.ErrorMessage));
            Assert.IsFalse(string.IsNullOrEmpty(query_result.ReturnValue));

            parsed_query = Parse_Query(query_result.ReturnValue);
            Assert.IsTrue(string.IsNullOrEmpty(parsed_query.ErrorMessage));
            Assert.IsTrue(parsed_query.ReturnValue.Count == 1);
            Assert.IsTrue(parsed_query.ReturnValue[0].Item1 == Fields.OrganizationName);
            Assert.IsTrue(parsed_query.ReturnValue[0].Item2.Equals("Organization"));


            //
            // SELECT * From Test_subscriptions WHERE id = 1 ...
            query_result = Connector.Query(find_where, new List<Fields>());
            Assert.IsTrue(string.IsNullOrEmpty(query_result.ErrorMessage));
            Assert.IsFalse(string.IsNullOrEmpty(query_result.ReturnValue));

            parsed_query = Parse_Query(query_result.ReturnValue);
            Assert.IsTrue(string.IsNullOrEmpty(parsed_query.ErrorMessage));
            Assert.IsTrue(parsed_query.ReturnValue.Count == 3);

            foreach (Tuple<Fields, object> item in parsed_query.ReturnValue)
            {
                if (item.Item1 == Fields.OrganizationId) { Assert.IsTrue(item.Item2.Equals((long)1)); }
                else if (item.Item1 == Fields.OrganizationName) { Assert.IsTrue(item.Item2.Equals("Organization"));}
                else if (item.Item1 == Fields.SubscriptionTypeId) { Assert.IsTrue(item.Item2.Equals((long)1)); }
                else { Assert.IsTrue(false); }
            }
        }

        [TestMethod]
        public void Test_Delete()
        {
            Insert_Test_Value();
            ResultPackage<bool> delete_result = Delete_Test_Value();

            Assert.IsTrue(delete_result.ReturnValue);
            Assert.IsTrue(string.IsNullOrEmpty(delete_result.ErrorMessage));
        }

        [TestMethod]
        public void Test_Delete_And_Query_By_OrganizationId()
        {
            ResultPackage<bool> delete_result;
            ResultPackage<string> query_result;
            Dictionary<Fields, object> find_where;

            //Insert Value for 1 to ensure we can delete
            Insert_Test_Value();
            delete_result = Delete_Test_Value();

            Assert.IsTrue(string.IsNullOrEmpty(delete_result.ErrorMessage));
            Assert.IsTrue(delete_result.ReturnValue);

            find_where = new Dictionary<Fields, object>
            { { Fields.OrganizationId, (long)1 }, };

            // SELECT * From Test_Organizations WHERE id = 1...
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
            { {Fields.OrganizationId, (long)2}, };
            //set id = 2 where id = 1;
            where_values = new Dictionary<Fields, object>
            { { Fields.OrganizationId, (long)1 }, };

            //Insert Value for 1 to ensure we can delete
            Insert_Test_Value();
            update_result = Connector.Update(set_values, where_values);

            Assert.IsTrue(string.IsNullOrEmpty(update_result.ErrorMessage));
            Assert.IsTrue(update_result.ReturnValue);
        }

        [TestMethod]
        public void Test_Update_And_Query()
        {/*
            ResultPackage<bool> update_result;
            ResultPackage<string> query_result;
            ResultPackage<List<Tuple<Fields, object>>> parsed_query;
            Dictionary<Fields, object> find_where, set_values, where_values;

            set_values = new Dictionary<Fields, object>
            { {Fields.SubscriptionTypeId, (long)2}, };
            //set id = 2 where id = 1;
            where_values = new Dictionary<Fields, object>
            { { Fields.SubscriptionTypeId, (long)1 }, };

            //Insert Value for 1 to ensure we can delete
            Connector.Insert(1, "default");
            update_result = Connector.Update(set_values, where_values);

            Assert.IsTrue(string.IsNullOrEmpty(update_result.ErrorMessage));
            Assert.IsTrue(update_result.ReturnValue);

            //Try to query original value should fail
            find_where = where_values;

            // SELECT * From Test_subscriptions WHERE id = 1...
            query_result = Connector.Query(find_where, new List<Fields>());
            //No error for empty result but also no return value
            Assert.IsTrue(string.IsNullOrEmpty(query_result.ErrorMessage));
            Assert.IsTrue(string.IsNullOrEmpty(query_result.ReturnValue));

            //Try to query updated values should succeed
            find_where = set_values;

            // SELECT SubscriptionTypeID From Test_subscriptions WHERE id = 2...
            query_result = Connector.Query(find_where, new List<Fields> { Fields.SubscriptionTypeId });
            Assert.IsTrue(string.IsNullOrEmpty(query_result.ErrorMessage));
            Assert.IsFalse(string.IsNullOrEmpty(query_result.ReturnValue));

            parsed_query = Parse_Query(query_result.ReturnValue);
            Assert.IsTrue(string.IsNullOrEmpty(parsed_query.ErrorMessage));
            Assert.IsTrue(parsed_query.ReturnValue.Count == 1);
            Assert.IsTrue(parsed_query.ReturnValue[0].Item1 == Fields.SubscriptionTypeId);
            Assert.IsTrue(parsed_query.ReturnValue[0].Item2.Equals((long)2));
        */}

        #endregion


        #region Helper methods

        private static ResultPackage<bool> Insert_Test_Value()
        {
            Connector = Connector ?? new Test_OrganizationsConnector();
            return Connector.Insert(1, 1, "Organization");
        }

        private static ResultPackage<bool> Delete_Test_Value()
        {
            Dictionary<Fields, object> delete_where = new Dictionary<Fields, object>
            { {Fields.OrganizationId, (long)1}, };

            Connector = Connector ?? new Test_OrganizationsConnector();

            return Connector.Delete(delete_where);
        }

        private ResultPackage<List<Tuple<Fields, object>>> Parse_Query(string query_result)
        {
            List<string> rows = new List<string>();
            char[] delimit_fields = new char[] { ',' };
            string[] delimit_rows = new string[] { "\r\n" };
            List<Tuple<Fields, object>> query_subscription_list = new List<Tuple<Fields, object>>();
            ResultPackage<List<Tuple<Fields, object>>> result = new ResultPackage<List<Tuple<Fields, object>>>();

            rows = query_result.Split(delimit_rows, StringSplitOptions.RemoveEmptyEntries).ToList();
            foreach (string row in rows)
            {
                List<string> field_values = row.Split(delimit_fields).ToList();

                foreach (string field_value in field_values)
                {
                    if (field_value.Contains("SubscriptionTypeId::"))
                    {
                        if (long.TryParse(field_value.Replace("SubscriptionTypeId::", ""), out long long_out))
                        {
                            query_subscription_list.Add(new Tuple<Fields, object>(Fields.SubscriptionTypeId, long_out));
                        }
                        else
                        {
                            result.ErrorMessage = "Unable to parse SubscriptionTypeId";
                            break;
                        }
                    }
                    else if (field_value.Contains("OrganizationId::"))
                    {
                        if (long.TryParse(field_value.Replace("OrganizationId::", ""), out long long_out))
                        {
                            query_subscription_list.Add(new Tuple<Fields, object>(Fields.OrganizationId, long_out));
                        }
                        else
                        {
                            result.ErrorMessage = "Unable to parse OrganizationId";
                            break;
                        }
                    }
                    else if (field_value.Contains("OrganizationName::"))
                    {
                        string value = field_value.Replace("OrganizationName::", "").Trim();
                        query_subscription_list.Add(new Tuple<Fields, object>(Fields.OrganizationName, value));
                    }
                    else
                    {
                        result.ErrorMessage = "Unable to parse Field";
                        break;
                    }
                }

                if (!string.IsNullOrEmpty(result.ErrorMessage)) { break; }
            }

            result.ReturnValue = query_subscription_list;

            return result;
        }

        #endregion
    }
}
