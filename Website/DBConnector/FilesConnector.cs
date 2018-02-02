using System.Data;
using System.Linq;
using System.Text;
using System.Data.SqlClient;

using ResultReporter;
using System.Collections.Generic;
using System;

namespace DBConnector
{
    class FilesConnector : DBConnector
    {
        #region Members

        //public 

        public enum FieldNames
        {
            FileGUID = 0,
            FileName,
            FilePath,
            Created,
            LastModified,
            LastAccessed,
            OwnerId,
            Sharing,
        }

        //private 

        private string _Table_Name = "Files";
        private Dictionary<FieldNames, string> _Field_Name_Lookup;

        #endregion


        #region Constructor

        public FilesConnector()
        {
            _Field_Name_Lookup = new Dictionary<FieldNames, string>
            {
                { FieldNames.Created, "Created" },
                { FieldNames.OwnerId, "OwnerId" },
                { FieldNames.Sharing, "Sharing" },
                { FieldNames.FileGUID, "FileGUID" },
                { FieldNames.FileName, "FileName" },
                { FieldNames.FilePath, "FilePath" },
                { FieldNames.LastAccessed, "LastAccessed" },
                { FieldNames.LastModified, "LastModified" },
            };
        }

        #endregion


        #region Public Methods

        public ResultPackage<bool> Insert(Guid file_guid, string file_name, string file_path,
                                            long owner_id, bool sharing_enabled)
        {
            DateTime created, last_accessed, last_modified;
            SqlCommand command = new SqlCommand(
                $"INSERT INTO {_Table_Name} (FileGUID, FileName, FilePath, Created, LastModified, LastAccessed, OwnerId, Sharing) " +
                "VALUES(@FileGUID, @FileName, @FilePath, @Created, @LastModified, @LastAccessed, @OwnerId, @Sharing)");

            created = last_accessed = last_modified = DateTime.UtcNow;

            Add_Command_Parameter_With_Value(FieldNames.Created, created, ref command);
            Add_Command_Parameter_With_Value(FieldNames.OwnerId, owner_id, ref command);
            Add_Command_Parameter_With_Value(FieldNames.FileGUID, file_guid, ref command);
            Add_Command_Parameter_With_Value(FieldNames.FileName, file_name, ref command);
            Add_Command_Parameter_With_Value(FieldNames.FilePath, file_path, ref command);
            Add_Command_Parameter_With_Value(FieldNames.Sharing, sharing_enabled, ref command);
            Add_Command_Parameter_With_Value(FieldNames.LastAccessed, last_accessed, ref command);
            Add_Command_Parameter_With_Value(FieldNames.LastModified, last_modified, ref command);

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
                    case FieldNames.FileName:
                        valid_value = Verify_FileName(value);
                        if (string.IsNullOrEmpty(valid_value.ErrorMessage))
                        {
                            if (string.IsNullOrEmpty(preferred_name))
                                preferred_name = "@FileName";

                            command.Parameters.Add(preferred_name, SqlDbType.VarChar);
                            command.Parameters[preferred_name].Value = (string)value;
                        }
                        break;
                    case FieldNames.FilePath:
                        valid_value = Verify_FilePath(value);
                        if (string.IsNullOrEmpty(valid_value.ErrorMessage))
                        {
                            if (string.IsNullOrEmpty(preferred_name))
                                preferred_name = "@FilePath";

                            command.Parameters.Add(preferred_name, SqlDbType.VarChar);
                            command.Parameters[preferred_name].Value = (string)value;
                        }
                        break;
                    case FieldNames.OwnerId:
                        valid_value = Verify_OwnerId(value);
                        if (string.IsNullOrEmpty(valid_value.ErrorMessage))
                        {
                            if (string.IsNullOrEmpty(preferred_name))
                                preferred_name = "@OwnerId";

                            command.Parameters.Add(preferred_name, SqlDbType.BigInt);
                            command.Parameters[preferred_name].Value = (long)value;
                        }
                        break;
                    case FieldNames.Created:
                        valid_value = Verify_Created(value);
                        if (string.IsNullOrEmpty(valid_value.ErrorMessage))
                        {
                            if (string.IsNullOrEmpty(preferred_name))
                                preferred_name = "@Created";

                            command.Parameters.Add(preferred_name, SqlDbType.DateTime);
                            command.Parameters[preferred_name].Value = (DateTime)value;
                        }
                        break;
                    case FieldNames.LastAccessed:
                        valid_value = Verify_Accessed(value);
                        if (string.IsNullOrEmpty(valid_value.ErrorMessage))
                        {
                            if (string.IsNullOrEmpty(preferred_name))
                                preferred_name = "@LastAccessed";

                            command.Parameters.Add(preferred_name, SqlDbType.DateTime);
                            command.Parameters[preferred_name].Value = (DateTime)value;
                        }
                        break;
                    case FieldNames.LastModified:
                        valid_value = Verify_Modified(value);
                        if (string.IsNullOrEmpty(valid_value.ErrorMessage))
                        {
                            if (string.IsNullOrEmpty(preferred_name))
                                preferred_name = "@LastModified";

                            command.Parameters.Add(preferred_name, SqlDbType.DateTime);
                            command.Parameters[preferred_name].Value = (DateTime)value;
                        }
                        break;
                    case FieldNames.FileGUID:
                        valid_value = Verify_FileGuid(value);
                        if (string.IsNullOrEmpty(valid_value.ErrorMessage))
                        {
                            if (string.IsNullOrEmpty(preferred_name))
                                preferred_name = "@FileGUID";

                            command.Parameters.Add(preferred_name, SqlDbType.UniqueIdentifier);
                            command.Parameters[preferred_name].Value = (Guid)value;
                        }
                        break;
                    case FieldNames.Sharing:
                        valid_value = Verify_SharingEnabled(value);
                        if (string.IsNullOrEmpty(valid_value.ErrorMessage))
                        {
                            if (string.IsNullOrEmpty(preferred_name))
                                preferred_name = "@Sharing";

                            command.Parameters.Add(preferred_name, SqlDbType.Bit);
                            command.Parameters[preferred_name].Value = (bool)value;
                        }
                        break;
                }
            }
            else { valid_value.ErrorMessage = "Value cannot be null"; }

            valid_value.ReturnValue = (string.IsNullOrEmpty(valid_value.ErrorMessage));

            return valid_value;
        }

        private ResultPackage<bool> Verify_FileName(object file_name)
        {
            ResultPackage<bool> file_name_result = new ResultPackage<bool>();
            string file_name_str = string.Empty;

            if (file_name.GetType() == typeof(string))
            {
                file_name_str = (string)file_name;

                if (string.IsNullOrEmpty(file_name_str))
                    file_name_result.ErrorMessage = "FileName cannot be empty";
                else if (file_name_str.Length > 100)
                    file_name_result.ErrorMessage = "FileName cannot exceed 100 characters";
                else if (file_name_str.Any(c => c > 255))
                    file_name_result.ErrorMessage = "FileName cannot contain Unicode characters";

            }
            else { file_name_result.ErrorMessage = "FileName must be a string type"; }

            file_name_result.ReturnValue = (string.IsNullOrEmpty(file_name_result.ErrorMessage));

            return file_name_result;
        }

        private ResultPackage<bool> Verify_FilePath(object file_path)
        {
            ResultPackage<bool> file_path_result = new ResultPackage<bool>();
            string file_path_str = string.Empty;

            if (file_path.GetType() == typeof(string))
            {
                file_path_str = (string)file_path;

                if (string.IsNullOrEmpty(file_path_str))
                    file_path_result.ErrorMessage = "FilePath cannot be empty";
                else if (file_path_str.Length > 500)
                    file_path_result.ErrorMessage = "FilePath cannot exceed 500 characters";
                else if (file_path_str.Any(c => c > 255))
                    file_path_result.ErrorMessage = "FilePath cannot contain Unicode characters";

            }
            else { file_path_result.ErrorMessage = "FilePath must be a string type"; }

            file_path_result.ReturnValue = (string.IsNullOrEmpty(file_path_result.ErrorMessage));

            return file_path_result;
        }

        private ResultPackage<bool> Verify_Created(object created_time)
        {
            ResultPackage<bool> created_time_result = new ResultPackage<bool>();
            DateTime created_time_dt;

            if (created_time.GetType() == typeof(DateTime))
            {
                created_time_dt = (DateTime)created_time;
                created_time_dt = created_time_dt.ToUniversalTime();

                if (created_time_dt < DateTime.UtcNow.AddMinutes(-5))
                    created_time_result.ErrorMessage = "FileCreated time cannot be in the past";
            }
            else { created_time_result.ErrorMessage = "FileCreated must be a DateTime type"; }

            created_time_result.ReturnValue = (string.IsNullOrEmpty(created_time_result.ErrorMessage));

            return created_time_result;
        }

        private ResultPackage<bool> Verify_Accessed(object accessed_time)
        {
            ResultPackage<bool> accessed_time_result = new ResultPackage<bool>();
            DateTime accessed_time_dt;

            if (accessed_time.GetType() == typeof(DateTime))
            {
                accessed_time_dt = (DateTime)accessed_time;
                accessed_time_dt = accessed_time_dt.ToUniversalTime();

                if (accessed_time_dt < DateTime.UtcNow.AddMinutes(-5))
                    accessed_time_result.ErrorMessage = "FileAccessed time cannot be in the past";
            }
            else { accessed_time_result.ErrorMessage = "FileAccessed must be a DateTime type"; }

            accessed_time_result.ReturnValue = (string.IsNullOrEmpty(accessed_time_result.ErrorMessage));

            return accessed_time_result;
        }

        private ResultPackage<bool> Verify_Modified(object modified_time)
        {
            ResultPackage<bool> modified_time_result = new ResultPackage<bool>();
            DateTime modified_time_dt;

            if (modified_time.GetType() == typeof(DateTime))
            {
                modified_time_dt = (DateTime)modified_time;
                modified_time_dt = modified_time_dt.ToUniversalTime();

                if (modified_time_dt < DateTime.UtcNow.AddMinutes(-5))
                    modified_time_result.ErrorMessage = "FileModified time cannot be in the past";
            }
            else { modified_time_result.ErrorMessage = "FileModified must be a DateTime type"; }

            modified_time_result.ReturnValue = (string.IsNullOrEmpty(modified_time_result.ErrorMessage));

            return modified_time_result;
        }

        private ResultPackage<bool> Verify_OwnerId(object owner_id)
        {
            ResultPackage<bool> owner_id_result = new ResultPackage<bool>();
            long owner_id_long = -1;

            if (owner_id.GetType() == typeof(long))
            {
                owner_id_long = (long)owner_id;

                if (owner_id_long < 0)
                    owner_id_result.ErrorMessage = "OwnerId cannot be negative";
            }
            else { owner_id_result.ErrorMessage = "OwnerId must be a long type"; }

            owner_id_result.ReturnValue = string.IsNullOrEmpty(owner_id_result.ErrorMessage);

            return owner_id_result;
        }

        private ResultPackage<bool> Verify_FileGuid(object guid)
        {
            ResultPackage<bool> file_guid_result = new ResultPackage<bool>();
            Guid file_guid;
 
            if (guid.GetType() == typeof(long))
            {
                file_guid = (Guid)guid;

                if (file_guid == Guid.Empty)
                    file_guid_result.ErrorMessage = "FileGuid cannot be empty";
            }
            else { file_guid_result.ErrorMessage = "FileGuid must be a Guid type"; }

            file_guid_result.ReturnValue = string.IsNullOrEmpty(file_guid_result.ErrorMessage);

            return file_guid_result;
        }

        private ResultPackage<bool> Verify_SharingEnabled(object sharing_enabled)
        {
            ResultPackage<bool> sharing_enabled_result = new ResultPackage<bool>();

            if (sharing_enabled.GetType() != typeof(bool))
                  sharing_enabled_result.ErrorMessage = "SharingEnabled must be a bool type";

            sharing_enabled_result.ReturnValue = string.IsNullOrEmpty(sharing_enabled_result.ErrorMessage);

            return sharing_enabled_result;
        }

        #endregion
    }
}
