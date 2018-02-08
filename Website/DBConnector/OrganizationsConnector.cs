using System.Data;
using System.Linq;
using System.Text;
using System.Data.SqlClient;

using ResultReporter;
using System.Collections.Generic;

namespace DBConnector
{
    public class OrganizationsConnector : DBConnector
    {
        #region Members

        //public 
        public enum FieldNames
        {
            OrganizationId = 0,
            OrganizationName,
            SubscriptionTypeId,
        }

        //private 
        private Dictionary<FieldNames, string> _Field_Name_Lookup;

        //protected
        protected string _Table_Name = "Organizations";

        #endregion


        #region Constructor

        public OrganizationsConnector()
        {
            _Field_Name_Lookup = new Dictionary<FieldNames, string>
            {
                { FieldNames.OrganizationId, "OrganizationId" },
                { FieldNames.OrganizationName, "OrganizationName" },
                { FieldNames.SubscriptionTypeId, "SubscriptionTypeId" },
            };
        }

        #endregion


        #region Public Methods

        public ResultPackage<bool> Insert(long organization_id, long subscription_type_id, string organization_name)
        {
            SqlCommand command = new SqlCommand(
                $"INSERT INTO {_Table_Name} (OrganizationId, OrganizationName, SubscriptionTypeId) " +
                "VALUES(@OrganizationId, @OrganizationName, @SubscriptionTypeId)");

            Add_Command_Parameter_With_Value(FieldNames.OrganizationId, organization_id, ref command);
            Add_Command_Parameter_With_Value(FieldNames.OrganizationName, organization_name, ref command);
            Add_Command_Parameter_With_Value(FieldNames.SubscriptionTypeId, subscription_type_id, ref command);

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
            return Delete(new SqlCommand { CommandText = $"DELETE FROM $Test_{_Table_Name};" });
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
                    case FieldNames.OrganizationName:
                        valid_value = Verify_OrganizationName(value);
                        if (string.IsNullOrEmpty(valid_value.ErrorMessage))
                        {
                            if (string.IsNullOrEmpty(preferred_name))
                                preferred_name = "@OrganizationName";

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
                    case FieldNames.SubscriptionTypeId:
                        valid_value = Verify_SubscriptionTypeId(value);
                        if (string.IsNullOrEmpty(valid_value.ErrorMessage))
                        {
                            if (string.IsNullOrEmpty(preferred_name))
                                preferred_name = "@SubscriptionTypeId";

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

        private ResultPackage<bool> Verify_OrganizationName(object org_name)
        {
            ResultPackage<bool> org_name_result = new ResultPackage<bool>();
            string org_name_str = string.Empty;

            if (org_name.GetType() == typeof(string))
            {
                org_name_str = (string)org_name;

                if (string.IsNullOrEmpty(org_name_str))
                    org_name_result.ErrorMessage = "OrganizationName cannot be empty";
                else if (org_name_str.Length > 100)
                    org_name_result.ErrorMessage = "OrganizationName cannot exceed 100 characters";
                else if (org_name_str.Any(c => c > 255))
                    org_name_result.ErrorMessage = "OrganizationName cannot contain Unicode characters";

            }
            else { org_name_result.ErrorMessage = "OrganizationName must be a string type"; }

            org_name_result.ReturnValue = (string.IsNullOrEmpty(org_name_result.ErrorMessage));

            return org_name_result;
        }

        private ResultPackage<bool> Verify_OrganizationId(object org_id)
        {
            ResultPackage<bool> org_id_result = new ResultPackage<bool>();
            long org_id_long = -1;

            if (org_id.GetType() == typeof(long))
            {
                org_id_long = (long)org_id;

                if (org_id_long < 0)
                    org_id_result.ErrorMessage = "OrganizationId cannot be negative";
            }
            else { org_id_result.ErrorMessage = "OrganizationId must be a long type"; }

            org_id_result.ReturnValue = string.IsNullOrEmpty(org_id_result.ErrorMessage);

            return org_id_result;
        }

        private ResultPackage<bool> Verify_SubscriptionTypeId(object sub_id)
        {
            ResultPackage<bool> sub_id_result = new ResultPackage<bool>();
            long sub_id_long = -1;

            if (sub_id.GetType() == typeof(long))
            {
                sub_id_long = (long)sub_id;

                if (sub_id_long < 0)
                    sub_id_result.ErrorMessage = "SubscriptionTypeId cannot be negative";
            }
            else { sub_id_result.ErrorMessage = "SubscriptionTypeId must be a long type"; }

            sub_id_result.ReturnValue = string.IsNullOrEmpty(sub_id_result.ErrorMessage);

            return sub_id_result;
        }

        #endregion
    }
}
