using System;
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

        public enum UsersTableFieldNames
        {
            Email = 0,
            UserId,
            UserName,
            UserPassword,
            OrganizationId,
        }

        //private 

        private Dictionary<UsersTableFieldNames, string> _Field_Name_Lookup;

        #endregion


        #region Constructor

        public UsersConnector()
        {
            _Field_Name_Lookup = new Dictionary<UsersTableFieldNames, string>();
            _Field_Name_Lookup.Add(UsersTableFieldNames.Email, "Email");
            _Field_Name_Lookup.Add(UsersTableFieldNames.UserId, "UserId");
            _Field_Name_Lookup.Add(UsersTableFieldNames.UserName, "UserName");
            _Field_Name_Lookup.Add(UsersTableFieldNames.UserPassword, "UserPassword");
            _Field_Name_Lookup.Add(UsersTableFieldNames.OrganizationId, "OrganizationId");
        }

        #endregion


        #region Public Methods

        public ResultPackage<bool> Insert(long user_id, long organization_id, string user_name, string email, string password)
        {
            SqlCommand command = new SqlCommand(
                "INSERT INTO Users (UserId, OrganizationId, UserName, UserPassword, Email) " +
                "VALUES(@UserId, @OrganizationId, @UserName, @UserPassword, @Email)");

            // create your parameters
            command.Parameters.Add("@UserId", System.Data.SqlDbType.BigInt);
            command.Parameters.Add("@OrganizationId", System.Data.SqlDbType.BigInt);
            command.Parameters.Add("@UserName", System.Data.SqlDbType.NVarChar);
            command.Parameters.Add("@UserPassword", System.Data.SqlDbType.VarChar);
            command.Parameters.Add("@Email", System.Data.SqlDbType.VarChar);

            // set values to parameters from textboxes
            command.Parameters["@UserId"].Value = user_id;
            command.Parameters["@OrganizationId"].Value = organization_id;
            command.Parameters["@UserName"].Value = user_name;
            command.Parameters["@UserPassword"].Value = password;
            command.Parameters["@Email"].Value = email;

            return Insert(command);
        }


        public ResultPackage<bool> Delete(UsersTableFieldNames field_name, object field_value)
        {
            string field;
            SqlCommand command = new SqlCommand();
            ResultPackage<bool> result = new ResultPackage<bool>();

            command.CommandText = "DELETE FROM Users WHERE";

            if(_Field_Name_Lookup.TryGetValue(field_name, out field))
            {
                command.CommandText += $" {field} = @{field}";
                command.Parameters.AddWithValue($"@{field}", field_value);
            }
            else { result.ErrorMessage = $"Bad field name: {field_name}"; }

            /*
            switch(field_name)
            {
                case UsersTableFieldNames.Email:
                    command.CommandText += " Email = @email";
                    command.Parameters.AddWithValue("@email", field_value);
                    break;
                case UsersTableFieldNames.UserId:
                    command.CommandText += " UserId = @userId";
                    command.Parameters.AddWithValue("@userId", field_value);
                    break;
                case UsersTableFieldNames.UserName:
                    command.CommandText += " UserName = @userName";
                    command.Parameters.AddWithValue("@userName", field_value);
                    break;
                case UsersTableFieldNames.UserPassword:
                    command.CommandText += " UserPassword = @password";
                    command.Parameters.AddWithValue("@password", field_value);
                    break;
                case UsersTableFieldNames.OrganizationId:
                    command.CommandText += " OrganizationId = @organizationId";
                    command.Parameters.AddWithValue("@organizationId", field_value);
                    break;
            }
            */

            return ((string.IsNullOrEmpty(result.ErrorMessage))
                ? Delete(command)
                : result);
        }

        //Delete Multiple - pass Dictionary<UsersTableFieldNames, object(values)>

        #endregion
    }
}
