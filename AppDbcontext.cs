using Dapper;
using Microsoft.Data.SqlClient;
using System.Data;

namespace EventMgmt
{
    public class AppDbcontext
    {
        private readonly IConfiguration _configuration;
        public AppDbcontext(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public IDbConnection Createconnection()
        {

            return new SqlConnection(_configuration.GetConnectionString("connectionstr"));

        }

        public SqlConnection sqlconnection()
        {
            SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("connectionstr"));
            return connection;
        }

        public async Task Init()
        {
            var conn = Createconnection();

            await _initcustomer();

            async Task _initcustomer()
            {
                var sqlstr = "CREATE TABLE IF NOT EXISTS Employees(ID INTEGER NOT NULL PRIMARY KEY,FirstName TEXT,LastName TEXT,Address TEXT,Email TEXT,Company TEXT);";

                await conn.ExecuteAsync(sqlstr);

            }
        }
    }
}
