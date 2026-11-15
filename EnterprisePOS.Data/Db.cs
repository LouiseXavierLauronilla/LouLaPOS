using System;
using System.Configuration;
using System.Data;
using System.Data.Linq;
using System.Data.SqlClient;
namespace EnterprisePOS.Data
{
    public sealed class PosDataContext : DataContext 
    { 
        public PosDataContext(string cs) : base(cs) 
        { ObjectTrackingEnabled = true; 
            DeferredLoadingEnabled = false; 
        } 
    }
    public static class Db
    {
        private const string DefaultConnectionString ="Data Source=LAPTOP-KMGS7E5Q\\SQLEXPRESS;Initial Catalog=PosDB;Integrated Security=True;Encrypt=True;TrustServerCertificate=True;";

        public static string ConnectionString
        {
            get
            {
                var configured = ConfigurationManager.ConnectionStrings["PosDB"]?.ConnectionString;
                return string.IsNullOrWhiteSpace(configured) ? DefaultConnectionString : configured;
            }
        }

        public static SqlConnection Open()
        {
            var connection = new SqlConnection(ConnectionString);

            try
            {
                connection.Open();
                return connection;
            }
            catch
            {
                connection.Dispose();
                throw new InvalidOperationException(
                    "Could not connect to the SQL Server database. Check that SQL Server is running and that the PosDB connection string matches the database shown in Server Explorer.");
            }
        }

        public static int Execute(string sql, params SqlParameter[] p)
        {
            using (var c = Open())
            using (var x = new SqlCommand(sql, c))
            {
                x.Parameters.AddRange(p);
                return x.ExecuteNonQuery();
            }
        }
    }
}
