using Azure;
using Dapper;
using DesignTech_PLM_Entegrasyon_App.MVC.Models;
using DesignTech_PLM_Entegrasyon_App.MVC.Models.WindchillApiModel;
using DesignTech_PLM_Entegrasyon_App.MVC.Repository;
using DesignTech_PLM_Entegrasyon_App.MVC.Services.ApiServices;
using DesignTech_PLM_Entegrasyon_App.MVC.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using System.Data;

namespace DesignTech_PLM_Entegrasyon_App.MVC.Controllers.Modules.WTPartProgress
{
    [Authorize]
    public class PlmWTPartProgressController : Controller
    {
        //private readonly DTExcelDbContext _context;
        //private readonly IUygulamaDbContextFactory _dbContextFactory;
        private readonly IConfiguration _configuration;
		private readonly IWebHostEnvironment _env;

        private readonly IGenericRepository<WTPartMaster> _wtPartMaster;
        private readonly IGenericRepository<WTPart> _wtPart;
        private readonly IGenericRepository<PhaseLink> _phaseLink;
        private readonly IGenericRepository<PhaseTemplate> _phaseTemplate;
	

		public PlmWTPartProgressController(IGenericRepository<WTPartMaster> wtPartMaster, IGenericRepository<WTPart> wtPart, IGenericRepository<PhaseLink> phaseLink, IGenericRepository<PhaseTemplate> phaseTemplate, IConfiguration configuration, IWebHostEnvironment env = null)
		{
			_wtPartMaster = wtPartMaster;
			_wtPart = wtPart;
			_phaseLink = phaseLink;
			_phaseTemplate = phaseTemplate;
			_configuration = configuration;
			_env = env;

			//_context = context;
			//_dbContextFactory = dbContextFactory;
		}

		public async Task<IActionResult> Index(int page = 1, string search = "")
        {
            try
            {
    //            WindchillApiService windchillApiService = new WindchillApiService(_env);
    //           var json = await windchillApiService.GetApiData("192.168.1.11", "ProdMgmt/Parts");
				//var response = JsonConvert.DeserializeObject<ProdMgmtParts>(json);
                

				//string connectionString = _configuration.GetConnectionString("Plm");
                string schema = _configuration["Catalog"];
                //using IDbConnection connection = new SqlConnection(connectionString);


                // Sorguları optimize edin ve sadece gerekli sütunları çekin

                var WTPartMasterParameters = new DynamicParameters();
                WTPartMasterParameters.Add("endItem", "1");

				var WTPartListParameters = new DynamicParameters();
				WTPartListParameters.Add("latestIterationInfo", "1");
				WTPartListParameters.Add("stateCheckoutInfo", "c/i");

				var wtPartMasterList = (await _wtPartMaster.GetAllWithWhere(schema + ".WTPartMaster", WTPartMasterParameters)).ToList();
				var wtPartList = (await _wtPart.GetAllWithWhere(schema + ".WTPart", WTPartListParameters)).ToList();
				var phaseLinkList = await _phaseLink.GetAll(schema + ".PhaseLink");
				var phaseTemplateList = await _phaseTemplate.GetAll(schema + ".PhaseTemplate");


				//var wtPartMasterList = connection.Query<WTPartMaster>($"SELECT * FROM {schema}.WTPartMaster WHERE endItem = 1").ToList();
				//var wtPartList = connection.Query<WTPart>($"SELECT * FROM {schema}.WTPart WHERE latestIterationInfo = 1 AND stateCheckoutInfo = 'c/i'").ToList();
				//var phaseLinkList = connection.Query<PhaseLink>($"SELECT * FROM {schema}.PhaseLink").ToList();
				//var phaseTemplateList = connection.Query<PhaseTemplate>($"SELECT * FROM {schema}.PhaseTemplate").ToList();




				var PhaseLinkTemp = (from a in wtPartMasterList
                                     join b in wtPartList on a.idA2A2 equals b.idA3MasterReference
                                     where a.endItem == 1 && b.latestIterationInfo == 1 && b.stateCheckoutInfo == "c/i"
                                     join c in phaseLinkList on b.idA3A2State equals c.idA3A5
                                     join d in phaseTemplateList on c.idA3B5 equals d.idA2A2
                                     select new PHProcess
                                     {
                                         name = d.name,
                                         phaseState = d.phaseState
                                     }).Distinct().ToList();

                var wtPartsProcess = (from a in wtPartMasterList
                                      join b in wtPartList on a.idA2A2 equals b.idA3MasterReference
                                      where a.endItem == 1 && b.latestIterationInfo == 1 && b.stateCheckoutInfo == "c/i"
                                      select new WTProcess
                                      {
                                          name = a.name,
                                          endItem = a.endItem,
                                          idA3masterReference = b.idA3MasterReference,
                                          latestiterationInfo = b.latestIterationInfo,
                                          statecheckoutInfo = b.stateCheckoutInfo,
                                          statestate = b.stateState
                                      }).Distinct().ToList();

                // Aşamaların sırasını alın
                var phaseOrder = PhaseLinkTemp.Select(p => p.phaseState).Distinct().ToList();
                var phaseOrderDictionary = new Dictionary<string, int>();

                for (int i = 0; i < phaseOrder.Count; i++)
                {
                    phaseOrderDictionary[phaseOrder[i]] = i + 1;
                }
                // Ürünlerin yüzde değerini hesaplayın
                foreach (var process in wtPartsProcess)
                {
                    if (phaseOrderDictionary.ContainsKey(process.statestate))
                    {
                        int phaseIndex = phaseOrderDictionary[process.statestate];
                        double phasePercentage = (double)phaseIndex / (phaseOrder.Count - 1) * 100;
                        int roundedPercentage = (int)Math.Round(phasePercentage);

                        if (roundedPercentage < 0 || roundedPercentage > 100)
                        {
                            process.yuzdeOran = "Canceled";
                        }
                        else
                        {
                            process.yuzdeOran = roundedPercentage + "%";
                        }
                    }
                }


                int pageSize = 7; // Sayfa başına gösterilecek öğe sayısı
                int totalCount = wtPartsProcess.Count;
                int pageCount = (int)Math.Ceiling((double)totalCount / pageSize);

                // Geçerli sayfa numarasını kontrol et
                if (page < 1)
                    page = 1;
                else if (page > pageCount)
                    page = pageCount;

                // Sayfaya göre dosyaları al
                var paginatedFiles = wtPartsProcess.Skip((page - 1) * pageSize).Take(pageSize).ToList();


                ViewBag.wtPartsProcess = paginatedFiles;
                //PageSettings
                ViewBag.fileCount = totalCount;
                ViewBag.pageSize = pageSize; // Sayfa boyutunu view'a ekleyin
                ViewBag.page = page; // Sayfa numarasını view'a ekleyin
                ViewBag.search = search; // Arama terimini view'a ekleyin
                //PageSettings
                //PageSettings
                return View();
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return View();
            }
        }
    }
}
