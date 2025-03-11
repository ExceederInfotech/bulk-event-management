using Microsoft.Data.SqlClient;

namespace EventMgmt
{
    public class AppDbcontext
    {
        private readonly IConfiguration _configuration;
        public AppDbcontext(IConfiguration configuration)
        {
            _configuration = configuration;
        }     
        public SqlConnection Sqlconnection()
        {
            SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("connectionstr"));
            return connection;
        }        
    }
}
