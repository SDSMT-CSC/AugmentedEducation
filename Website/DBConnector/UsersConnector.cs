using System;
using System.Data;
using System.Linq;
using System.Text;
using System.Data.SqlClient;

using ResultReporter;
using System.Collections.Generic;

namespace DBConnector
{
    public class UsersConnector : DBConnector
    {
        #region Members

        //public 

        public enum FieldNames
        {
            Email = 0,
            UserId,
            UserName,
            UserPassword,
            OrganizationId,
        }

        //private 

        private string _Table_Name = "Users";
        private Dictionary<FieldNames, string> _Field_Name_Lookup;

        #endregion


        #region Constructor

        public UsersConnector()
        {
            _Field_Name_Lookup = new Dictionary<FieldNames, string>
            {
                { FieldNames.Email, "Email" },
                { FieldNames.UserId, "UserId" },
                { FieldNames.UserName, "UserName" },
                { FieldNames.UserPassword, "UserPassword" },
                { FieldNames.OrganizationId, "OrganizationId" }
            };
        }

        #endregion


        #region Public Methods

        public ResultPackage<bool> Insert(long user_id, long organization_id, string user_name, string email, string password)
        {
            SqlCommand command = new SqlCommand(
                $"INSERT INTO {_Table_Name} (UserId, OrganizationId, UserName, UserPassword, Email) " +
                "VALUES(@UserId, @OrganizationId, @UserName, @UserPassword, @Email)");

            Add_Command_Parameter_With_Value(FieldNames.Email, email, ref command);
            Add_Command_Parameter_With_Value(FieldNames.UserId, user_id, ref command);
            Add_Command_Parameter_With_Value(FieldNames.UserName, user_name, ref command);
            Add_Command_Parameter_With_Value(FieldNames.UserPassword, password, ref command);
            Add_Command_Parameter_With_Value(FieldNames.OrganizationId, organization_id, ref command);

            return Insert(command);
        }

        public ResultPackage<string> Query(Dictionary<FieldNames, object> where_values, List<FieldNames> select_values)
        {
            SqlCommand command = new SqlCommand();
            StringBuilder commandText = new StringBuilder();
            ResultPackage<string> result = new ResultPackage<string>();

            commandText.Append("SELECT ");
            if (select_values.Count > 0)
            {
                //Build string to say 'SELECT field1, field2, ... , fieldn '
                foreach (FieldNames field in select_values)
                {
                    commandText.Append($"{field}");
                    if (field != select_values.Last()) { commandText.Append(", "); }
                }
            }
            else { commandText.Append("*"); }

            commandText.Append($" FROM {_Table_Name} WHERE ");
            result = And_Values(where_values, ref command);
            if (string.IsNullOrEmpty(result.ErrorMessage)) { commandText.Append(result.ReturnValue); }

            if (string.IsNullOrEmpty(result.ErrorMessage))
            {
                //No errors in building command so execute
                command.CommandText = commandText.ToString();
                result = Query(command);
            }

            return result;
        }

        public ResultPackage<bool> Update(Dictionary<FieldNames, object> set_values, Dictionary<FieldNames, object> where_values)
        {
            SqlCommand command = new SqlCommand();
            StringBuilder commandText = new StringBuilder();
            ResultPackage<bool> result = new ResultPackage<bool>();
            ResultPackage<string> anded_values = new ResultPackage<string>();

            commandText.Append($"UPDATE {_Table_Name} SET ");
            foreach (FieldNames field in set_values.Keys)
            {
                result = Add_Command_Parameter_With_Value(field, set_values[field], ref command, $"@set{field}");

                if (result.ReturnValue)
                {
                    commandText.Append($"{field} = @set{field}");
                    if (field != set_values.Keys.Last()) { commandText.Append(", "); }
                    else { commandText.Append(" "); }
                }
                else { break; }
            }

            if (result.ReturnValue)
            {
                commandText.Append("WHERE ");
                anded_values = And_Values(where_values, ref command);
                result.ReturnValue = string.IsNullOrEmpty(anded_values.ErrorMessage);

                if (result.ReturnValue) { commandText.Append(anded_values.ReturnValue); }
                else { result.ErrorMessage = anded_values.ErrorMessage; }
            }

            if (result.ReturnValue)
            {
                command.CommandText = commandText.ToString();
                result = Update(command);
            }

            return result;
        }

        public ResultPackage<bool> Delete_Single_Compare(FieldNames field_name, object field_value)
        {
            return Delete(new Dictionary<FieldNames, object> { { field_name, field_value }, });
        }

        public ResultPackage<bool> Delete(Dictionary<FieldNames, object> where_values)
        {
            SqlCommand command = new SqlCommand();
            StringBuilder commandText = new StringBuilder();
            ResultPackage<bool> result = new ResultPackage<bool>();
            ResultPackage<string> anded_values = new ResultPackage<string>();

            commandText.Append($"DELETE FROM {_Table_Name} WHERE ");
            anded_values = And_Values(where_values, ref command);
            result.ReturnValue = string.IsNullOrEmpty(anded_values.ErrorMessage);

            if (result.ReturnValue) { commandText.Append(anded_values.ReturnValue); }
            else { result.ErrorMessage = anded_values.ErrorMessage; }

            if (result.ReturnValue)
            {
                command.CommandText = commandText.ToString();
                result = Delete(command);
            }

            return result;
        }

        #endregion


        #region Private Methods

        private ResultPackage<string> And_Values(Dictionary<FieldNames, object> values, ref SqlCommand command)
        {
            StringBuilder and_string = new StringBuilder();
            ResultPackage<string> result = new ResultPackage<string>();
            ResultPackage<bool> valid_values = new ResultPackage<bool>();

            foreach (FieldNames field in values.Keys)
            {
                //Set values for SQL variables @field1, @field2, ..., @fieldn
                valid_values = Add_Command_Parameter_With_Value(field, values[field], ref command);

                if (valid_values.ReturnValue)
                {
                    and_string.Append($"{field} = @{field}");
                    if (field != values.Keys.Last()) { and_string.Append(" AND "); }
                }
                else
                {
                    result.ErrorMessage = valid_values.ErrorMessage;
                    break;
                }
            }

            if (string.IsNullOrEmpty(result.ErrorMessage))
                result.ReturnValue = and_string.ToString();

            return result;
        }

        private ResultPackage<bool> Add_Command_Parameter_With_Value(FieldNames field_name, object value, ref SqlCommand command, string preferred_name = null)
        {
            ResultPackage<bool> valid_value = new ResultPackage<bool>();

            if (value != null)
            {
                switch (field_name)
                {
                    case FieldNames.Email:
                        valid_value = Verify_Email(value);
                        if (string.IsNullOrEmpty(valid_value.ErrorMessage))
                        {
                            if (string.IsNullOrEmpty(preferred_name))
                                preferred_name = "@Email";

                            command.Parameters.Add(preferred_name, SqlDbType.VarChar);
                            command.Parameters[preferred_name].Value = (string)value;
                        }
                        break;
                    case FieldNames.UserId:
                        valid_value = Verify_UserId(value);
                        if (string.IsNullOrEmpty(valid_value.ErrorMessage))
                        {
                            if (string.IsNullOrEmpty(preferred_name))
                                preferred_name = "@UserId";

                            command.Parameters.Add(preferred_name, SqlDbType.BigInt);
                            command.Parameters[preferred_name].Value = (long)value;
                        }
                        break;
                    case FieldNames.UserName:
                        valid_value = Verify_UserName(value);
                        if (string.IsNullOrEmpty(valid_value.ErrorMessage))
                        {
                            if (string.IsNullOrEmpty(preferred_name))
                                preferred_name = "@UserName";

                            command.Parameters.Add(preferred_name, SqlDbType.NVarChar);
                            command.Parameters[preferred_name].Value = (string)value;
                        }
                        break;
                    case FieldNames.UserPassword:
                        valid_value = Verify_Password(value);
                        if (string.IsNullOrEmpty(valid_value.ErrorMessage))
                        {
                            if (string.IsNullOrEmpty(preferred_name))
                                preferred_name = "@UserPassword";

                            command.Parameters.Add(preferred_name, SqlDbType.VarChar);
                            command.Parameters[preferred_name].Value = (string)value;
                        }
                        break;
                    case FieldNames.OrganizationId:
                        valid_value = Verify_OrganizationId(value);
                        if (string.IsNullOrEmpty(valid_value.ErrorMessage))
                        {
                            if (string.IsNullOrEmpty(preferred_name))
                                preferred_name = "@OrganizationId";

                            command.Parameters.Add(preferred_name, SqlDbType.BigInt);
                            command.Parameters[preferred_name].Value = (long)value;
                        }
                        break;
                }
            }
            else { valid_value.ErrorMessage = "Value cannot be null"; }

            valid_value.ReturnValue = (string.IsNullOrEmpty(valid_value.ErrorMessage));

            return valid_value;
        }

        private ResultPackage<bool> Verify_Email(object email)
        {
            ResultPackage<bool> email_result = new ResultPackage<bool>();
            string email_str = string.Empty;

            if (email.GetType() == typeof(string))
            {
                email_str = (string)email;

                if (string.IsNullOrEmpty(email_str))
                    email_result.ErrorMessage = "Email cannot be empty";
                else if (email_str.Length > 100)
                    email_result.ErrorMessage = "Email cannot exceed 100 characters";
                else if (email_str.Any(c => c > 255))
                    email_result.ErrorMessage = "Email cannot contain Unicode characters";

            }
            else { email_result.ErrorMessage = "Email must be a string type"; }

            email_result.ReturnValue = (string.IsNullOrEmpty(email_result.ErrorMessage));

            return email_result;
        }

        private ResultPackage<bool> Verify_Password(object password)
        {
            ResultPackage<bool> password_result = new ResultPackage<bool>();
            string password_str = string.Empty;

            if (password.GetType() == typeof(string))
            {
                password_str = (string)password;

                if (string.IsNullOrEmpty(password_str))
                    password_result.ErrorMessage = "Password cannot be empty";
                else if (password_str.Length > 100)
                    password_result.ErrorMessage = "Password cannot exceed 100 characters";
                else if (password_str.Any(c => c > 255))
                    password_result.ErrorMessage = "Password cannot contain Unicode characters";
            }
            else { password_result.ErrorMessage = "Password must be a string type"; }

            password_result.ReturnValue = (string.IsNullOrEmpty(password_result.ErrorMessage));

            return password_result;
        }

        private ResultPackage<bool> Verify_UserName(object user_name)
        {
            ResultPackage<bool> userName_result = new ResultPackage<bool>();
            string user_name_str = string.Empty;

            if (user_name.GetType() == typeof(string))
            {
                user_name_str = (string)user_name;
                if (string.IsNullOrEmpty(user_name_str))
                    userName_result.ErrorMessage = "UserName cannot be empty";
                else if (user_name_str.Length > 100)
                    userName_result.ErrorMessage = "UserName cannot exceed 100 characters";
            }
            else { userName_result.ErrorMessage = "UserName must be a string type"; }

            userName_result.ReturnValue = (string.IsNullOrEmpty(userName_result.ErrorMessage));

            return userName_result;
        }

        private ResultPackage<bool> Verify_UserId(object user_id)
        {
            ResultPackage<bool> userId_result = new ResultPackage<bool>();
            long user_id_long = -1;

            if (user_id.GetType() == typeof(long))
            {
                user_id_long = (long)user_id;

                if (user_id_long < 0)
                    userId_result.ErrorMessage = "UserId cannot be negative";
            }
            else { userId_result.ErrorMessage = "UserId must be a long type"; }

            userId_result.ReturnValue = string.IsNullOrEmpty(userId_result.ErrorMessage);

            return userId_result;
        }

        private ResultPackage<bool> Verify_OrganizationId(object organization_id)
        {
            ResultPackage<bool> organizationId_result = new ResultPackage<bool>();
            long organization_id_long = -1;

            if (organization_id.GetType() == typeof(long))
            {
                organization_id_long = (long)organization_id;

                if (organization_id_long < 0)
                    organizationId_result.ErrorMessage = "UserId cannot be negative";
            }
            else { organizationId_result.ErrorMessage = "UserId must be a long type"; }

            organizationId_result.ReturnValue = string.IsNullOrEmpty(organizationId_result.ErrorMessage);

            return organizationId_result;
        }

        #endregion
    }
}
