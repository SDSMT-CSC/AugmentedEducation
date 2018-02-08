using System.Data;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Collections.Generic;

using ResultReporter;

namespace DBConnector
{
    public class PermissionLevelsConnector : DBConnector
    {
        #region Members

        //public 

        public enum FieldNames
        {
            PermissionLevelId = 0,
            PermissionLevelName,
        }

        //private 

        private Dictionary<FieldNames, string> _Field_Name_Lookup;

        //protected

        protected string _Table_Name = "PermissionLevelsHC";



        #endregion


        #region Constructor

        public PermissionLevelsConnector()
        {
            _Field_Name_Lookup = new Dictionary<FieldNames, string>
            {
                { FieldNames.PermissionLevelId, "PermissionLevelId" },
                { FieldNames.PermissionLevelName, "PermissionLevelName" },
            };
        }

        #endregion


        #region Public Methods

        public ResultPackage<bool> Insert(long permission_level_id, string permission_level_name)
        {
            SqlCommand command = new SqlCommand(
                $"INSERT INTO {_Table_Name} (PermissionLevelId, PermissionLevelName) " +
                "VALUES(@PermissionLevelId, @PermissionLevelName)");

            Add_Command_Parameter_With_Value(FieldNames.PermissionLevelId, permission_level_id, ref command);
            Add_Command_Parameter_With_Value(FieldNames.PermissionLevelName, permission_level_name, ref command);

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

        public ResultPackage<bool> Delete_All()
        {
            SqlCommand command = new SqlCommand() { CommandText = $"DELETE FROM {_Table_Name};" };

            return Delete(command);
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
                    case FieldNames.PermissionLevelId:
                        valid_value = Verify_PerimissionId(value);
                        if (string.IsNullOrEmpty(valid_value.ErrorMessage))
                        {
                            if (string.IsNullOrEmpty(preferred_name))
                                preferred_name = "@PermissionLevelId";

                            command.Parameters.Add(preferred_name, SqlDbType.BigInt);
                            command.Parameters[preferred_name].Value = (long)value;
                        }
                        break;
                    case FieldNames.PermissionLevelName:
                        valid_value = Verify_PermissionName(value);
                        if (string.IsNullOrEmpty(valid_value.ErrorMessage))
                        {
                            if (string.IsNullOrEmpty(preferred_name))
                                preferred_name = "@PermissionLevelName";

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

        private ResultPackage<bool> Verify_PerimissionId(object permission_id)
        {
            ResultPackage<bool> permission_id_result = new ResultPackage<bool>();
            long permission_id_long = -1;

            if (permission_id.GetType() == typeof(long) ||
                permission_id.GetType() == typeof(int) )
            {
                permission_id_long = (long)permission_id;

                if (permission_id_long < 0)
                    permission_id_result.ErrorMessage = "PermissionId must be greater than 0";
            }
            else { permission_id_result.ErrorMessage = "PermisisonId must be a long type"; }

            permission_id_result.ReturnValue = (string.IsNullOrEmpty(permission_id_result.ErrorMessage));

            return permission_id_result;
        }

        private ResultPackage<bool> Verify_PermissionName(object permission_name)
        {
            ResultPackage<bool> permission_name_result = new ResultPackage<bool>();
            string permission_name_str = string.Empty;

            if (permission_name.GetType() == typeof(string))
            {
                permission_name_str = (string)permission_name;

                if (string.IsNullOrEmpty(permission_name_str))
                    permission_name_result.ErrorMessage = "PermissionName cannot be empty";
                else if (permission_name_str.Length > 100)
                    permission_name_result.ErrorMessage = "PermissionName cannot exceed 100 characters";
                else if (permission_name_str.Any(c => c > 255))
                    permission_name_result.ErrorMessage = "PermissionName cannot contain Unicode characters";
            }
            else { permission_name_result.ErrorMessage = "PermissionName must be a string type"; }

            permission_name_result.ReturnValue = (string.IsNullOrEmpty(permission_name_result.ErrorMessage));

            return permission_name_result;
        }

        #endregion
    }
}