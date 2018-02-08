using System;
using System.Text;
using System.Data.SqlClient;

using ResultReporter;

namespace DBConnector
{
    public class DBConnector : IDBConnector
    {
        #region Members

        private static int s_Timeout = 30;
        private static string s_User_Name = "aeadmin";
        private static string s_Password = "SeniorDesign2018";
        private static string s_Database_Name = "AugmentedEducationDB";
        private static string s_Server_URL = "augmentededucationserver.database.windows.net";
        private static SqlConnectionStringBuilder s_Connection_Builder = new SqlConnectionStringBuilder();

        #endregion


        #region Constructor

        public DBConnector()
        {
            s_Connection_Builder.UserID = s_User_Name;
            s_Connection_Builder.Password = s_Password;
            s_Connection_Builder.DataSource = s_Server_URL;
            s_Connection_Builder.ConnectTimeout = s_Timeout;
            s_Connection_Builder.InitialCatalog = s_Database_Name;
        }

        #endregion


        #region Properties

        public string Connection_String => s_Connection_Builder.ConnectionString;

        #endregion


        #region Public Methods

        public bool Check_Can_Conect()
        {
            bool successful = true;

            try
            {
                using (SqlConnection conn = Get_Connection())
                {
                    conn.Open();
                    conn.Close();
                }
            }
            catch (Exception e)
            {
                successful = false;
                Console.WriteLine(e.ToString());
            }

            return successful;
        }

        public ResultPackage<bool> Insert(SqlCommand command)
        {
            return Execute_With_Connection<bool>(() =>
            {
                int rows_affected = 0;

                try { rows_affected = command.ExecuteNonQuery(); }
                catch (Exception ex) { throw new InvalidOperationException($"Unable to Insert: \r\n{ex.ToString()}"); }

                return (rows_affected > 0);
            }, command);
        }

        public ResultPackage<string> Query(SqlCommand command)
        {
            return Execute_With_Connection<string>(() =>
            {
                int field_index = 0;
                StringBuilder query_result;

                try
                {
                    using (SqlDataReader dataReader = command.ExecuteReader())
                    {
                        query_result = new StringBuilder();
                        while (dataReader.Read())
                        {
                            for (field_index = 0; field_index < dataReader.FieldCount; field_index++)
                            {
                                query_result.Append($"{dataReader.GetName(field_index)}::");
                                query_result.Append($"{dataReader[field_index].ToString()}");
                                if (field_index != dataReader.FieldCount - 1) { query_result.Append(", "); }
                            }
                            query_result.AppendLine();
                        }
                    }
                }
                catch (Exception ex) { throw new InvalidOperationException($"Unable to Query: \r\n{ex.ToString()}"); }

                return query_result.ToString();
            }, command);
        }

        public ResultPackage<bool> Update(SqlCommand command)
        {
            return Execute_With_Connection<bool>(() =>
            {
                int rows_affected = 0;

                try { rows_affected = command.ExecuteNonQuery(); }
                catch (Exception ex) { throw new InvalidOperationException($"Unable to Update: \r\n{ex.ToString()}"); }

                return (rows_affected > 0);
            }, command);
        }

        public ResultPackage<bool> Delete(SqlCommand command)
        {
            return Execute_With_Connection<bool>(() =>
            {
                int rows_affected = 0;

                try { rows_affected = command.ExecuteNonQuery(); }
                catch (Exception ex) { throw new InvalidOperationException($"Unable to Delete: \r\n{ex.ToString()}"); }

                return (rows_affected > 0);
            }, command);
        }

        public ResultPackage<bool> Create(SqlCommand command)
        {
            return Execute_With_Connection<bool>(() =>
            {
                int rows_affected = 0;

                try { rows_affected = command.ExecuteNonQuery(); }
                catch (Exception ex) { throw new InvalidOperationException($"Unable to Create Table: \r\n{ex.ToString()}"); }

                return (rows_affected > 0);
            }, command);
        }

        public ResultPackage<bool> Drop(SqlCommand command)
        {
            return Execute_With_Connection<bool>(() =>
            {
                int rows_affected = 0;

                try { rows_affected = command.ExecuteNonQuery(); }
                catch (Exception ex) { throw new InvalidOperationException($"Unable to Drop Table: \r\n{ex.ToString()}"); }

                return (rows_affected > 0);
            }, command);
        }

        #endregion


        #region Protected Mehtods

        protected SqlConnection Get_Connection()
        {
            return new SqlConnection(Connection_String);
        }

        protected ResultPackage<T> Execute_With_Connection<T>(Func<T> exec, SqlCommand command)
        {
            ResultPackage<T> return_value = new ResultPackage<T>();

            try
            {
                using (SqlConnection conn = Get_Connection())
                {
                    conn.Open();
                    command.Connection = conn;
                    return_value.ReturnValue = exec();
                }
            }
            catch (InvalidOperationException ex) { return_value.ErrorMessage = ex.ToString(); }
            catch (Exception e) { return_value.ErrorMessage = $"Unexpected Error: \r\n{e.ToString()}"; }

            return return_value;
        }

        #endregion
    }
}
