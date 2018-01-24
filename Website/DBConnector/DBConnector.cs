using System;
using System.Data.SqlClient;

namespace DBConnector
{
    public class DBConnector
    {
        #region Members

        private static int s_Timeout = 30;
        private static bool s_Encrypt = true;
        private static bool s_Trust_Cert = false;
        private static bool s_Trust_Connection = true;
        private static bool s_Persist_Security = false;
        private static string s_User_Name = "aeadmin";
        private static string s_Password = "SeniorDesign2018";
        private static string s_Database_Name = "AugmentedEducationDB";
        private static string s_Server_URL = "augmentededucationserver.database.windows.net";

        //Server=tcp:augmentededucationserver.database.windows.net,1433;
        //Initial Catalog = AugmentedEducationDB;
        //MultipleActiveResultSets=False;
        //TrustServerCertificate=False;
        //Persist Security Info=False;
        //User ID = { your_username };
        //Password={your_password};
        //Connection Timeout = 30;
        //Encrypt=True;

        #endregion


        #region Constructor

        public DBConnector() { }

        #endregion


        #region Properties

        public string Connection_String => $"User ID={s_User_Name};Password={s_Password};" +
                                        $"Server={s_Server_URL};Database={s_Database_Name};" +
                                        $"Trusted_Connection={s_Trust_Connection}Connection_Timeout={s_Timeout}";

        #endregion


        #region Public Methods

        public bool Connection_Established()
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
            catch(Exception e)
            {
                successful = false;
                Console.WriteLine(e.ToString());
            }

            return successful;
        }

        #endregion

        #region Private Mehtods

        private SqlConnection Get_Connection()
        {
            return new SqlConnection(Connection_String);
        }

        #endregion

    }
}
