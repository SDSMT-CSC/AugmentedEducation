using System.Data.SqlClient;

using ResultReporter;

namespace DBConnector
{
    interface IDBConnector
    {
        //CRUD - Create, Read, Update, Delete

        ResultPackage<bool> Insert(SqlCommand command);  //Create

        ResultPackage<string> Query(SqlCommand command);  //Read

        ResultPackage<bool> Update(SqlCommand command);

        ResultPackage<bool> Delete(SqlCommand command);
    }
}
