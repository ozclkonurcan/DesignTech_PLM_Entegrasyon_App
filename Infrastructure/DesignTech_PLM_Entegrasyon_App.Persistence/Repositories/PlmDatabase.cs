using Microsoft.Extensions.Configuration;
using Microsoft.Data.SqlClient;
using SqlKata.Compilers;
using SqlKata.Execution;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace DesignTech_PLM_Entegrasyon_App.Persistence.Repositories
{
    public class PlmDatabase
    {
        private readonly IConfiguration _configuration;

        public PlmDatabase(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public QueryFactory Connect()
        {
            string connectionString = _configuration.GetConnectionString("Plm");
            var connection = new System.Data.SqlClient.SqlConnection(connectionString);
            var compiler = new SqlServerCompiler();
            
            var db = new QueryFactory(connection, compiler);
            return db;
        }
    }
}
