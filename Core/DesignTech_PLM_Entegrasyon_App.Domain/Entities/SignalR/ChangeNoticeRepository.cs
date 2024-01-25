using Dapper;
using DesignTech_PLM_Entegrasyon_App.MVC.Dtos;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;

namespace DesignTech_PLM_Entegrasyon_App.MVC.Models.SignalR
{
    public class ChangeNoticeRepository
    {
        private readonly ChangeNoticeContext _context;
        private readonly IConfiguration _configuration;

        public ChangeNoticeRepository(ChangeNoticeContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public IEnumerable<WTChangeOrder2Master> GetResolvedCNs()
        {
            var catalogValue = _configuration["Catalog"];
            using (var connection = new SqlConnection(_configuration.GetConnectionString("Plm")))
            {
                connection.Open();

                var resolvedCNs = connection.Query<WTChangeOrder2Master>($"select * from {catalogValue}.dbo.Change_Notice where STATE = 'RESOLVED'");

                return resolvedCNs;
            }
        }
    }
}
