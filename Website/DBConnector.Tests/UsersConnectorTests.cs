using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using ResultReporter;
using Fields = DBConnector.UsersConnector.FieldNames;

namespace DBConnector.Tests
{
    /// <summary>
    /// Summary description for UsersConnectorTests
    /// </summary>
    [TestClass]
    public class UsersConnectorTests
    {
        #region Inner Classes

        private class User
        {
            #region Constructors

            public User() { }

            public User(long user_id, long org_id, string name, string email, string password)
            {
                Email = email;
                UserName = name;
                UserId = user_id;
                Password = password;
                OrganizationId = org_id;
            }

            #endregion


            #region Properties

            public long UserId { get; set; }
            public long OrganizationId { get; set; }
            public string UserName { get; set; }
            public string Email { get; set; }
            public string Password { get; set; }

            #endregion


            #region Public Methods

            public static bool CompareUsers(User left, User right)
            {
                bool equal = true;
                equal = equal && left.UserId == right.UserId;
                equal = equal && left.Email.Equals(right.Email);
                equal = equal && left.UserName.Equals(right.UserName);
                equal = equal && left.Password.Equals(right.Password);
                equal = equal && left.OrganizationId == right.OrganizationId;
                return equal;
            }

            public object GetValueByConnectorEnum(Fields field)
            {
                object return_val = null;

                switch (field)
                {
                    case Fields.Email:
                        return_val = Email;
                        break;
                    case Fields.UserId:
                        return_val = UserId;
                        break;
                    case Fields.UserName:
                        return_val = UserName;
                        break;
                    case Fields.UserPassword:
                        return_val = Password;
                        break;
                    case Fields.OrganizationId:
                        return_val = OrganizationId;
                        break;
                }

                return return_val;
            }

            #endregion
        }

        #endregion


        #region Memebers

        private TestContext testContextInstance;

        #endregion


        #region Constructor

        public UsersConnectorTests()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        #endregion


        #region Properties
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #endregion

        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion


        [TestMethod]
        public void Test_Passing_All_Simple_Operations()
        {
            User user;
            List<Fields> select_fields = new List<Fields>();
            UsersConnector usersConnector = new UsersConnector();
            ResultPackage<bool> bool_result = new ResultPackage<bool>();
            ResultPackage<string> string_result = new ResultPackage<string>();
            Dictionary<Fields, object> set_fields = new Dictionary<Fields, object>();
            ResultPackage<List<User>> user_list_result = new ResultPackage<List<User>>();

            //userId, orgId, name, email, password
            user = new User(1, 1, "UserName", "Email", "Password");

            //Insert user record
            bool_result = Simple_Insert(usersConnector, user);
            Assert.IsTrue(bool_result.ReturnValue, bool_result.ErrorMessage);


            //Query for insert
            string_result = Simple_Query(usersConnector, user, Fields.UserId, new List<Fields>());
            Assert.IsTrue(string.IsNullOrEmpty(string_result.ErrorMessage));
            //Parse query and verify insert
            user_list_result = Parse_Query(string_result.ReturnValue);
            Assert.IsTrue(string.IsNullOrEmpty(user_list_result.ErrorMessage));
            Assert.IsTrue(user_list_result.ReturnValue.Count == 1);
            Assert.IsTrue(User.CompareUsers(user, user_list_result.ReturnValue[0]));


            //Update UserId to 2
            set_fields.Add(Fields.UserId, (long)2);
            select_fields.Add(Fields.UserId);
            bool_result = Simple_Update(usersConnector, user, set_fields, select_fields);
            Assert.IsTrue(bool_result.ReturnValue);
         

            //Query for change
            string_result = Simple_Query(usersConnector, user, Fields.UserId, new List<Fields>());
            //No error, but also no result
            Assert.IsTrue(string.IsNullOrEmpty(string_result.ErrorMessage));
            Assert.IsTrue(string.IsNullOrEmpty(string_result.ReturnValue));


            //Update UserId back to 1
            //modify the user.UserId for comparisons
            user.UserId = 2;
            set_fields.Remove(Fields.UserId);
            set_fields.Add(Fields.UserId, (long)1);
            bool_result = Simple_Update(usersConnector, user, set_fields, select_fields);
            Assert.IsTrue(bool_result.ReturnValue);
            user.UserId = 1;


            //Query for update
            string_result = Simple_Query(usersConnector, user, Fields.UserId, new List<Fields>());
            Assert.IsTrue(string.IsNullOrEmpty(string_result.ErrorMessage));
            //Parse query and verify update matches original
            user_list_result = Parse_Query(string_result.ReturnValue);
            Assert.IsTrue(string.IsNullOrEmpty(user_list_result.ErrorMessage));
            Assert.IsTrue(user_list_result.ReturnValue.Count == 1);
            Assert.IsTrue(User.CompareUsers(user, user_list_result.ReturnValue[0]));


            //Delete test record
            bool_result = Simple_Delete(usersConnector, user, Fields.UserId);
            Assert.IsTrue(bool_result.ReturnValue, bool_result.ErrorMessage);
        }

        #region Insert Methods

        private ResultPackage<bool> Simple_Insert(UsersConnector connector, User user)
        {
            return connector.Insert(user.UserId, user.OrganizationId, user.UserName, user.Email, user.Password);
        }

        #endregion


        #region Query Methods

        private ResultPackage<string> Simple_Query(UsersConnector connector, User user,
                                                Fields by_field, List<Fields> select_fields)
        {
            Dictionary<Fields, object> where_dict = new Dictionary<Fields, object>
            {
                { by_field, user.GetValueByConnectorEnum(by_field) },
            };

            return connector.Query(where_dict, select_fields);
        }

        private ResultPackage<List<User>> Parse_Query(string query_result)
        {
            long long_out;
            string trimmed_value;
            char[] delimit_fields = new char[] { ',' };
            List<User> query_user_list = new List<User>();
            List<string> user_fields = new List<string>();
            string[] delimit_rows = new string[] { "\r\n" };
            ResultPackage<List<User>> result = new ResultPackage<List<User>>();

            user_fields = query_result.Split(delimit_rows, StringSplitOptions.RemoveEmptyEntries).ToList();
            foreach (string user_result in user_fields)
            {
                User user = new User();
                List<string> field_values = user_result.Split(delimit_fields).ToList();

                foreach (string field_value in field_values)
                {
                    if (field_value.Contains("UserId::"))
                    {
                        trimmed_value = field_value.Replace("UserId::", "");
                        if (long.TryParse(trimmed_value, out long_out)) { user.UserId = long_out; }
                        else
                        {
                            result.ErrorMessage = "Unable to parse UserId";
                            break;
                        }
                    }
                    else if (field_value.Contains("Email::"))
                    {
                        user.Email = field_value.Replace("Email::", "").Trim();
                    }
                    else if (field_value.Contains("UserName::"))
                    {
                        user.UserName = field_value.Replace("UserName::", "").Trim();
                    }
                    else if (field_value.Contains("UserPassword::"))
                    {
                        user.Password = field_value.Replace("UserPassword::", "").Trim();
                    }
                    else if (field_value.Contains("OrganizationId::"))
                    {
                        trimmed_value = field_value.Replace("OrganizationId::", "");
                        if (long.TryParse(trimmed_value, out long_out)) { user.OrganizationId = long_out; }
                        else
                        {
                            result.ErrorMessage = "Unable to parse OrganizationId";
                            break;
                        }
                    }
                }

                if (!string.IsNullOrEmpty(result.ErrorMessage))
                    break;

                query_user_list.Add(user);
            }

            result.ReturnValue = query_user_list;

            return result;
        }

        #endregion


        #region Update Methods

        private ResultPackage<bool> Simple_Update(UsersConnector connector, User user,
                                                Dictionary<Fields, object> set_dict, List<Fields> select_fields)
        {
            Dictionary<Fields, object> where_dict = new Dictionary<Fields, object>();

            foreach(Fields field in select_fields)
            {
                if(!where_dict.TryGetValue(field, out object val))
                {
                    where_dict.Add(field, user.GetValueByConnectorEnum(field));
                }
            }

            return connector.Update(set_dict, where_dict);
        }

        #endregion


        #region Delete Methods

        private ResultPackage<bool> Simple_Delete(UsersConnector connector, User user, Fields by_field)
        {
            Dictionary<Fields, object> where_dict = new Dictionary<Fields, object>
            {
                { by_field, user.GetValueByConnectorEnum(by_field) },
            };

            return connector.Delete(where_dict);
        }

        #endregion
    }
}
