using System.Data;
using System.Linq;
using System.Text;
using System.Data.SqlClient;

using ResultReporter;
using System.Collections.Generic;

namespace DBConnector
{
    public class OrganizationUsersConnector : DBConnector
    {
        #region Members

        //public 

        public enum FieldNames
        {
            OrganizationId = 0,
            OrganizationUserId,
            PermissionLevelId,
            DepartmentId,
            FirstName,
            LastName,
        }

        //private 

        private string _Table_Name = "OrganizationUsers";
        private Dictionary<FieldNames, string> _Field_Name_Lookup;

        #endregion


        #region Constructor

        public OrganizationUsersConnector()
        {
            _Field_Name_Lookup = new Dictionary<FieldNames, string>
            {
                { FieldNames.OrganizationUserId, "OrganizationUserId" },
                { FieldNames.PermissionLevelId, "PermissionLevelId" },
                { FieldNames.OrganizationId, "OrganizationId" },
                { FieldNames.DepartmentId, "DepartmentId" },
                { FieldNames.FirstName, "FirstName" },
                { FieldNames.LastName, "LastName" },
            };
        }

        #endregion


        #region Public Methods

        public ResultPackage<bool> Insert(long org_id, long org_user_id, long permission_id, long dept_id, string f_name, string l_name)
        {
            SqlCommand command = new SqlCommand(
                $"INSERT INTO {_Table_Name} (OrganizationId, OrganizationUserId, PermissionLevelId, DepartmentId, FirstName, LastName) " +
                "VALUES(@OrganizationId, @OrganizationUserId, @PermissionLevelId, @DepartmentId, @FirstName, @LastName)");

            Add_Command_Parameter_With_Value(FieldNames.OrganizationUserId, org_user_id, ref command);
            Add_Command_Parameter_With_Value(FieldNames.PermissionLevelId, permission_id, ref command);
            Add_Command_Parameter_With_Value(FieldNames.OrganizationId, org_id, ref command);
            Add_Command_Parameter_With_Value(FieldNames.DepartmentId, dept_id, ref command);
            Add_Command_Parameter_With_Value(FieldNames.FirstName, f_name, ref command);
            Add_Command_Parameter_With_Value(FieldNames.LastName, l_name, ref command);

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
                    case FieldNames.DepartmentId:
                        valid_value = Verify_DepartmentId(value);
                        if (string.IsNullOrEmpty(valid_value.ErrorMessage))
                        {
                            if (string.IsNullOrEmpty(preferred_name))
                                preferred_name = "@DepartmentId";

                            command.Parameters.Add(preferred_name, SqlDbType.BigInt);
                            command.Parameters[preferred_name].Value = (long)value;
                        }
                        break;
                    case FieldNames.OrganizationUserId:
                        valid_value = Verify_OrganizationUserId(value);
                        if (string.IsNullOrEmpty(valid_value.ErrorMessage))
                        {
                            if (string.IsNullOrEmpty(preferred_name))
                                preferred_name = "@OrganizationUserId";

                            command.Parameters.Add(preferred_name, SqlDbType.BigInt);
                            command.Parameters[preferred_name].Value = (long)value;
                        }
                        break;
                    case FieldNames.PermissionLevelId:
                        valid_value = Verify_PermissionLevelId(value);
                        if(string.IsNullOrEmpty(valid_value.ErrorMessage))
                        {
                            if (string.IsNullOrEmpty(preferred_name))
                                preferred_name = "@PermissionLevelId";

                            command.Parameters.Add(preferred_name, SqlDbType.BigInt);
                            command.Parameters[preferred_name].Value = (long)value;
                        }
                        break;
                    case FieldNames.FirstName:
                        valid_value = Verify_FirstName(value);
                        if (string.IsNullOrEmpty(valid_value.ErrorMessage))
                        {
                            if (string.IsNullOrEmpty(preferred_name))
                                preferred_name = "@FirstName";

                            command.Parameters.Add(preferred_name, SqlDbType.NVarChar);
                            command.Parameters[preferred_name].Value = (string)value;
                        }
                        break;
                    case FieldNames.LastName:
                        valid_value = Verify_LastName(value);
                        if (string.IsNullOrEmpty(valid_value.ErrorMessage))
                        {
                            if (string.IsNullOrEmpty(preferred_name))
                                preferred_name = "@LastName";

                            command.Parameters.Add(preferred_name, SqlDbType.VarChar);
                            command.Parameters[preferred_name].Value = (string)value;
                        }
                        break;
                }
            }
            else { valid_value.ErrorMessage = "Value cannot be null"; }

            valid_value.ReturnValue = (string.IsNullOrEmpty(valid_value.ErrorMessage));

            return valid_value;
        }

        private ResultPackage<bool> Verify_FirstName(object first_name)
        {
            ResultPackage<bool> first_name_result = new ResultPackage<bool>();
            string first_name_str = string.Empty;

            if (first_name.GetType() == typeof(string))
            {
                first_name_str = (string)first_name;
                if (string.IsNullOrEmpty(first_name_str))
                    first_name_result.ErrorMessage = "FirstName cannot be empty";
                else if (first_name_str.Length > 30)
                    first_name_result.ErrorMessage = "FirstName cannot exceed 30 characters";
                else if (first_name_str.Any(c => c > 255))
                    first_name_result.ErrorMessage = "FirstName cannot contain Unicode characters";
            }
            else { first_name_result.ErrorMessage = "FirstName must be a string type"; }

            first_name_result.ReturnValue = (string.IsNullOrEmpty(first_name_result.ErrorMessage));

            return first_name_result;
        }

        private ResultPackage<bool> Verify_LastName(object last_name)
        {
            ResultPackage<bool> last_name_result = new ResultPackage<bool>();
            string last_name_str = string.Empty;

            if (last_name.GetType() == typeof(string))
            {
                last_name_str = (string)last_name;

                if (string.IsNullOrEmpty(last_name_str))
                    last_name_result.ErrorMessage = "LastName cannot be empty";
                else if (last_name_str.Length > 30)
                    last_name_result.ErrorMessage = "LastName cannot exceed 30 characters";
                else if (last_name_str.Any(c => c > 255))
                    last_name_result.ErrorMessage = "LastName cannot contain Unicode characters";
            }
            else { last_name_result.ErrorMessage = "LastName must be a string type"; }

            last_name_result.ReturnValue = (string.IsNullOrEmpty(last_name_result.ErrorMessage));

            return last_name_result;
        }

        private ResultPackage<bool> Verify_OrganizationId(object organization_id)
        {
            ResultPackage<bool> organizationId_result = new ResultPackage<bool>();
            long organization_id_long = -1;

            if (organization_id.GetType() == typeof(long))
            {
                organization_id_long = (long)organization_id;

                if (organization_id_long < 0)
                    organizationId_result.ErrorMessage = "OrganizationId cannot be negative";
            }
            else { organizationId_result.ErrorMessage = "OrganizationId must be a long type"; }

            organizationId_result.ReturnValue = string.IsNullOrEmpty(organizationId_result.ErrorMessage);

            return organizationId_result;
        }

        private ResultPackage<bool> Verify_DepartmentId(object dept_id)
        {
            ResultPackage<bool> dept_id_result = new ResultPackage<bool>();
            long dept_id_long = -1;

            if (dept_id.GetType() == typeof(long))
            {
                dept_id_long = (long)dept_id;

                if (dept_id_long < 0)
                    dept_id_result.ErrorMessage = "DepartmentId cannot be negative";
            }
            else { dept_id_result.ErrorMessage = "DepartmentId must be a long type"; }

            dept_id_result.ReturnValue = string.IsNullOrEmpty(dept_id_result.ErrorMessage);

            return dept_id_result;
        }

        private ResultPackage<bool> Verify_OrganizationUserId(object organization_user_id)
        {
            ResultPackage<bool> organization_user_id_result = new ResultPackage<bool>();
            long organization_user_id_long = -1;

            if (organization_user_id.GetType() == typeof(long))
            {
                organization_user_id_long = (long)organization_user_id;

                if (organization_user_id_long < 0)
                    organization_user_id_result.ErrorMessage = "OrganizationUserId cannot be negative";
            }
            else { organization_user_id_result.ErrorMessage = "OrganizationUserId must be a long type"; }

            organization_user_id_result.ReturnValue = string.IsNullOrEmpty(organization_user_id_result.ErrorMessage);

            return organization_user_id_result;
        }

        private ResultPackage<bool> Verify_PermissionLevelId(object permission_id)
        {
            ResultPackage<bool> permission_id_result = new ResultPackage<bool>();
            long permission_id_long = -1;

            if (permission_id.GetType() == typeof(long))
            {
                permission_id_long = (long)permission_id;

                if (permission_id_long < 0)
                    permission_id_result.ErrorMessage = "PermissionLevelId cannot be negative";
            }
            else { permission_id_result.ErrorMessage = "PermissionLevelId must be a long type"; }

            permission_id_result.ReturnValue = string.IsNullOrEmpty(permission_id_result.ErrorMessage);

            return permission_id_result;
        }

        #endregion
    }
}
