using Dapper;
using DesignTech_PLM_Entegrasyon_App.MVC.Helper;
using DesignTech_PLM_Entegrasyon_App.MVC.Models;
using DesignTech_PLM_Entegrasyon_App.MVC.Models.SignalR;
using DesignTech_PLM_Entegrasyon_App.MVC.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Generic;
using System.Data;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using System.Security.Cryptography;
using DesignTech_PLM_Entegrasyon_App.MVC.Models.Views;
using Newtonsoft.Json;

namespace DesignTech_PLM_Entegrasyon_App.MVC.Controllers.Modules.ReadAndUpdateDocument
{
    [Authorize]
    public class ReadAndUpdateDocumentController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly IDbConnection _connection;
        //private readonly IHubContext<StatusHub> _hubContext;

        public ReadAndUpdateDocumentController(IConfiguration configuration, IDbConnection connection)
        {
            _configuration = configuration;
            _connection = connection;
            //_hubContext = hubContext;
        }

        public async Task<IActionResult> Index()
        {
            try
            {

                //var catalogValue = _configuration["Catalog"];

                //// WTDocument sorgusu
                //var wtDocuments = await _connection.QueryAsync<WTDocument>($"SELECT * FROM {catalogValue}.WTDocument");
                //var wtDocumentIdA2A2 = wtDocuments.Select(a => a.IdA2A2).ToList();

                //// HolderToContent sorgusu
                //var holderToContents = await _connection.QueryAsync<HolderToContent>($"SELECT * FROM {catalogValue}.HolderToContent WHERE idA3A5 IN @Ids", new { Ids = wtDocumentIdA2A2 });

                //// ApplicationData sorgusu
                //var applicationDatas = await _connection.QueryAsync<ApplicationData>($"SELECT * FROM {catalogValue}.ApplicationData WHERE idA2A2 IN @Ids", new { Ids = holderToContents.Select(item => item.idA3B5) });

                //// EPMDocumentMaster sorgusu
                //var epmDocumentMasters = await _connection.QueryAsync<EPMDocumentMaster>($"SELECT * FROM {catalogValue}.EPMDocumentMaster WHERE documentNumber IN @FileNames", new { FileNames = applicationDatas.Select(item => item.fileName.ToUpper()) });

                //// EPMDocument sorgusu
                //var epmDocuments = await _connection.QueryAsync<EPMDocument>($"SELECT * FROM {catalogValue}.EPMDocument WHERE idA3masterReference IN @Ids", new { Ids = epmDocumentMasters.Select(item => item.IdA2A2) });
                LogService logService = new LogService(_configuration);
                var loggedInUsername = HttpContext.User.Identity.Name;

                var catalogValue = _configuration["Catalog"];
                var WTDocument = await _connection.QueryAsync<WTDocument>($"SELECT * FROM {catalogValue}.WTDocument");
                var WTDocumentIdA2A2 = (from a in WTDocument select a.IdA2A2).ToList();

                List<HolderToContent> holderToContents = new List<HolderToContent>();
                foreach (var item in WTDocumentIdA2A2)
                {

                    var HolderToContent = await _connection.QueryAsync<HolderToContent>($"SELECT * FROM {catalogValue}.HolderToContent WHERE idA3A5 IN ({item})");

                    holderToContents.AddRange(HolderToContent);
                }

                List<ApplicationDataViewModel> applicationDatas = new List<ApplicationDataViewModel>();
                foreach (var item in holderToContents)
                {

                    var ApplicationData = await _connection.QueryAsync<ApplicationDataViewModel>($"SELECT * FROM {catalogValue}.ApplicationData WHERE  idA2A2 IN ({item.idA3B5}) ");


                    foreach (var item2 in ApplicationData)
                    {

                        item2.HolderToContent_ClassnamekeyroleAObjectRef = item.classnamekeyroleAObjectRef;
                        item2.HolderToContent_IdA3A5 = item.idA3A5;
                        item2.HolderToContent_IdA3B5 = item.idA3B5;
                    }
                    applicationDatas.AddRange(ApplicationData);



                }

                List<EPMDocumentMasterViewModel> ePMDocumentMasters = new List<EPMDocumentMasterViewModel>();

                foreach (var item in applicationDatas)
                {
                    var EPMDocumentMaster = await _connection.QueryAsync<EPMDocumentMasterViewModel>($"SELECT * FROM {catalogValue}.EPMDocumentMaster WHERE documentNumber IN ('{item.fileName.ToUpper().Split(".")[0]}') ");

                    foreach (var item2 in EPMDocumentMaster)
                    {
                        item2.HolderToContent_ClassnamekeyroleAObjectRef = item.HolderToContent_ClassnamekeyroleAObjectRef;
                        item2.HolderToContent_IdA3A5 = item.HolderToContent_IdA3A5;
                        item2.HolderToContent_IdA3B5 = item.HolderToContent_IdA3B5;
                    }

                    ePMDocumentMasters.AddRange(EPMDocumentMaster);
                }

                List<EPMDocumentViewModel> ePMDocuments = new List<EPMDocumentViewModel>();

                foreach (var item in ePMDocumentMasters)
                {
                    var EPMDocument = await _connection.QueryAsync<EPMDocumentViewModel>($"SELECT TOP (1) * FROM {catalogValue}.EPMDocument WHERE  latestiterationInfo = 1 and statecheckoutInfo = 'c/i' and idA3masterReference IN ({item.IdA2A2}) ");

                    foreach (var item2 in EPMDocument)
                    {
                        item2.HolderToContent_ClassnamekeyroleAObjectRef = item.HolderToContent_ClassnamekeyroleAObjectRef;
                        item2.HolderToContent_IdA3A5 = item.HolderToContent_IdA3A5;
                        item2.HolderToContent_IdA3B5 = item.HolderToContent_IdA3B5;
                    }

                    ePMDocuments.AddRange(EPMDocument);
                }
                //AllUpdateDocumentErrorManagementViewModel

                var newApplicationDatas = (from a in applicationDatas where a.fileName.EndsWith(".pdf") select a).ToList();

                var NewEPMDocuments = (from a in await _connection.QueryAsync<EPMDocumentMasterViewModel>($"SELECT * FROM {catalogValue}.EPMDocumentMaster")
                                       join
                                       b in ePMDocuments on a.IdA2A2 equals b.IdA3MasterReference
                                       select a.DocumentNumber).ToList();


                List<AllUpdateDocumentErrorManagementViewModel> allUpdateDocumentErrorManagementViewModel = new List<AllUpdateDocumentErrorManagementViewModel>();

                foreach (var item in newApplicationDatas)
                {

                    var documentRest = NewEPMDocuments.Any(item2 => item.fileName.ToUpper().Split(".")[0] == item2);

                    if (!documentRest)
                    {
                        allUpdateDocumentErrorManagementViewModel.Add(new AllUpdateDocumentErrorManagementViewModel
                        {
                            FileName = item.fileName,
                            Status = 0
                        });
                    }
                    else
                    {
                        allUpdateDocumentErrorManagementViewModel.Add(new AllUpdateDocumentErrorManagementViewModel
                        {
                            FileName = item.fileName,
                            Status = 1
                        });
                    }


                }


                ViewBag.WTDocument = WTDocument;
                ViewBag.RESP = allUpdateDocumentErrorManagementViewModel;

                return View();
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "HATA!" + ex.Message;
                return View();
            }

        }

        [HttpPost]
        public async Task<IActionResult> UpdateAllDocument()
        {
            try
            {
                LogService logService = new LogService(_configuration);
                var loggedInUsername = HttpContext.User.Identity.Name;

                var catalogValue = _configuration["Catalog"];
                var WTDocument = await _connection.QueryAsync<WTDocument>($"SELECT * FROM {catalogValue}.WTDocument");
                var WTDocumentIdA2A2 = (from a in WTDocument select a.IdA2A2).ToList();

                List<HolderToContent> holderToContents = new List<HolderToContent>();
                foreach (var item in WTDocumentIdA2A2)
                {
                    var HolderToContent = await _connection.QueryAsync<HolderToContent>($"SELECT * FROM {catalogValue}.HolderToContent WHERE idA3A5 IN ({item})");
                    holderToContents.AddRange(HolderToContent);
                }

                List<ApplicationDataViewModel> applicationDatas = new List<ApplicationDataViewModel>();
                foreach (var item in holderToContents)
                {
                    var ApplicationData = await _connection.QueryAsync<ApplicationDataViewModel>($"SELECT * FROM {catalogValue}.ApplicationData WHERE  idA2A2 IN ({item.idA3B5}) ");

                    foreach (var item2 in ApplicationData)
                    {
                        item2.HolderToContent_ClassnamekeyroleAObjectRef = item.classnamekeyroleAObjectRef;
                        item2.HolderToContent_IdA3A5 = item.idA3A5;
                        item2.HolderToContent_IdA3B5 = item.idA3B5;
                    }
                    applicationDatas.AddRange(ApplicationData);



                }

                List<EPMDocumentMasterViewModel> ePMDocumentMasters = new List<EPMDocumentMasterViewModel>();

                foreach (var item in applicationDatas)
                {
                    var EPMDocumentMaster = await _connection.QueryAsync<EPMDocumentMasterViewModel>($"SELECT * FROM {catalogValue}.EPMDocumentMaster WHERE documentNumber IN ('{item.fileName.ToUpper().Split(".")[0]}') ");

                    foreach (var item2 in EPMDocumentMaster)
                    {
                        item2.HolderToContent_ClassnamekeyroleAObjectRef = item.HolderToContent_ClassnamekeyroleAObjectRef;
                        item2.HolderToContent_IdA3A5 = item.HolderToContent_IdA3A5;
                        item2.HolderToContent_IdA3B5 = item.HolderToContent_IdA3B5;
                    }

                    ePMDocumentMasters.AddRange(EPMDocumentMaster);
                }

                List<EPMDocumentViewModel> ePMDocuments = new List<EPMDocumentViewModel>();

                foreach (var item in ePMDocumentMasters)
                {
                    var EPMDocument = await _connection.QueryAsync<EPMDocumentViewModel>($"SELECT TOP (1) * FROM {catalogValue}.EPMDocument WHERE  latestiterationInfo = 1 and statecheckoutInfo = 'c/i' and idA3masterReference IN ({item.IdA2A2}) ");

                    foreach (var item2 in EPMDocument)
                    {
                        item2.HolderToContent_ClassnamekeyroleAObjectRef = item.HolderToContent_ClassnamekeyroleAObjectRef;
                        item2.HolderToContent_IdA3A5 = item.HolderToContent_IdA3A5;
                        item2.HolderToContent_IdA3B5 = item.HolderToContent_IdA3B5;
                    }

                    ePMDocuments.AddRange(EPMDocument);
                }




                foreach (var item in ePMDocuments)
                {
                    try
                    {

                        var updateQuery = $@"
UPDATE {catalogValue}.HolderToContent
SET
   classnamekeyroleAObjectRef = 'wt.epm.EPMDocument',
   idA3A5 = @NewId
WHERE
   idA3A5 = @OldId;

 UPDATE {catalogValue}.ApplicationData
                        SET
   role = 'SECONDARY'
WHERE
   idA2A2 = @OtherId
";

                        await _connection.ExecuteAsync(updateQuery, new { NewId = item.IdA2A2, OldId = item.HolderToContent_IdA3A5, OtherId = item.HolderToContent_IdA3B5 });
                        logService.AddNewLogEntry(item.HolderToContent_IdA3A5 + " => " + item.IdA2A2 + "&&" + item.HolderToContent_ClassnamekeyroleAObjectRef + " => wt.epm.EPMDocument "+ " && " + "Role => SECONDARY İle Güncellendi.", null, "Güncellendi", loggedInUsername);
                        TempData["SuccessMessage"] = "Güncelleme işlemi gerçekleştirildi.";
                    }
                    catch (Exception ex)
                    {
                        logService.AddNewLogEntry(item.HolderToContent_IdA3A5 + " X " + item.IdA2A2 + "&&" + item.HolderToContent_ClassnamekeyroleAObjectRef + " X wt.epm.EPMDocument İle Güncellenmedi." + " HATA! : " + ex.Message, null, "Güncellenmedi", loggedInUsername);
                        TempData["ErrorMessage"] = "HATA!" + ex.Message;
                    }
                }



                var newApplicationDatas = (from a in applicationDatas where a.fileName.EndsWith(".pdf") select a).ToList();

                var NewEPMDocuments = (from a in await _connection.QueryAsync<EPMDocumentMasterViewModel>($"SELECT * FROM {catalogValue}.EPMDocumentMaster")
                                       join
                                       b in ePMDocuments on a.IdA2A2 equals b.IdA3MasterReference
                                       select a.DocumentNumber).ToList();


                List<AllUpdateDocumentErrorManagementViewModel> allUpdateDocumentErrorManagementViewModel = new List<AllUpdateDocumentErrorManagementViewModel>();

                foreach (var item in newApplicationDatas)
                {

                    var documentRest = NewEPMDocuments.Any(item2 => item.fileName.ToUpper().Split(".")[0] == item2);

                    if (!documentRest)
                    {

                        logService.AddNewLogEntry(item.fileName + " dosyasına uygun döküman yok", null, "Güncellenmedi", loggedInUsername);
                        allUpdateDocumentErrorManagementViewModel.Add(new AllUpdateDocumentErrorManagementViewModel
                        {
                            FileName = item.fileName,
                            Status = 0
                        });
                    }
                    else
                    {
                        allUpdateDocumentErrorManagementViewModel.Add(new AllUpdateDocumentErrorManagementViewModel
                        {
                            FileName = item.fileName,
                            Status = 1
                        });
                    }


                }

                string jsonResult = JsonConvert.SerializeObject(allUpdateDocumentErrorManagementViewModel);
                TempData["rest"] = jsonResult;

                return RedirectToAction("UpdateAllDocumentControlPage");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "HATA!" + ex.Message;
                return RedirectToAction("Index");
            }

        }

       


        public async Task<IActionResult> UpdateAllDocumentControlPage()
        {
            try
            {
                string jsonResult = TempData["rest"] as string;

                List<AllUpdateDocumentErrorManagementViewModel> resultList = JsonConvert.DeserializeObject<List<AllUpdateDocumentErrorManagementViewModel>>(jsonResult);

                var succeeded = resultList.Count(x => x.Status == 1);
                ViewBag.SucceededCount = succeeded;
                var failed = resultList.Count(x => x.Status == 0);
                ViewBag.FailedCount = failed;
                var total = resultList.Count;
                ViewBag.TotalCount = total;

                ViewBag.resp = resultList;
                return View();
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "HATA!" + ex.Message;
                return RedirectToAction("Index", "Home");
            }
 
        }

        //public async Task<IActionResult> Index()
        //{
        //	var documents = await _connection.QueryAsync<WTDocument>("SELECT * FROM WTDocument");

        //	// SignalR üzerinden clients'a gönder
        //	await _hubContext.Clients.All.SendAsync("ReceiveDocuments", documents);
        //	return View();
        //}
    }


}

