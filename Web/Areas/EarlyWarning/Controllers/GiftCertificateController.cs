﻿
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cats.Areas.GiftCertificate.Models;
using Cats.Services.EarlyWarning;
using System.Web.Mvc;
using Cats.Services.Transaction;
using Cats.ViewModelBinder;
using Kendo.Mvc.UI;
using Kendo.Mvc.Extensions;
using Cats.Services.Common;
using Master = Cats.Models.Constant.Master;
using Cats.Helpers;
using Cats.Data.UnitWork;

namespace Cats.Areas.EarlyWarning.Controllers
{
    public class GiftCertificateController : Controller
    {
        private readonly IGiftCertificateService _giftCertificateService;
        private readonly IGiftCertificateDetailService _giftCertificateDetailService;
        private readonly ICommonService _commonService;
        private readonly ITransactionService _transactionService;
        private readonly ILetterTemplateService _letterTemplateService;
        private readonly IUnitOfWork _unitofwork;
        public GiftCertificateController(IGiftCertificateService giftCertificateService, IGiftCertificateDetailService giftCertificateDetailService, ICommonService commonService,ITransactionService transactionService, ILetterTemplateService letterTemplateService, IUnitOfWork unitofwork)
        {
            _giftCertificateService = giftCertificateService;
            _giftCertificateDetailService = giftCertificateDetailService;
            _commonService = commonService;
            _transactionService = transactionService;
            _letterTemplateService = letterTemplateService;
            _unitofwork = unitofwork;
        }

        public ActionResult Index(int id=1)
        {
            ViewBag.Title = id == 1 ? "Draft Gift Certificates" : "Approved Gift Certificates";
            var gifts = _giftCertificateService.Get(t=>t.StatusID==id,null, "GiftCertificateDetails,Donor,GiftCertificateDetails.Detail,GiftCertificateDetails.Commodity");
            var giftsViewModel = GiftCertificateViewModelBinder.BindListGiftCertificateViewModel(gifts.ToList(),true);
            return View(giftsViewModel);

        }

        public JsonResult GetListOfCertificate([DataSourceRequest] DataSourceRequest request)
        {
            var giftCertList = _giftCertificateDetailService.GetAllGiftCertificateDetail();

            var result = giftCertList.ToList().Select(item => new GiftCertificateViewModel
                                                                  {
                                                                      CommodityName = item.Commodity.Name,
                                                                      GiftDate = item.GiftCertificate.GiftDate,
                                                                      Donor = item.GiftCertificate.Donor.Name,
                                                                      Program = item.GiftCertificate.Program.Name,
                                                                      ReferenceNo = item.GiftCertificate.ReferenceNo,
                                                                      GiftCertificateID = item.GiftCertificateID,
                                                                      SINumber = item.GiftCertificate.SINumber,
                                                                      PortName = item.GiftCertificate.PortName
                                                                  }).ToList();

            return Json(result.ToDataSourceResult(request, ModelState));
        }

        public ActionResult GenerateTemplate(int id)
        {
            return RedirectToAction("LetterTemplate", new {giftceritificateId = id});
            
         
        }
     

        public virtual ActionResult NotUnique(string siNumber, int giftCertificateId)
        {


            if (_giftCertificateService.IsSINumberNewOrEdit(siNumber, giftCertificateId))
            {
                return Json(true, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(string.Format("{0} is invalid, there is an existing record with the same SI Number ", siNumber),
                        JsonRequestBehavior.AllowGet);

            }
        }

        public ActionResult Create()
        {
            PopulateLookup();
            var gift = new GiftCertificateViewModel();
            gift.GiftDate = DateTime.Today;
            gift.ETA = DateTime.Today;
            gift.CommodityTypeID = 1;
            return View(gift);
        }
        [HttpPost]
        public ActionResult Create(GiftCertificateViewModel giftcertificateViewModel)
        {
            if (ModelState.IsValid && giftcertificateViewModel != null)
            {
                var giftCertificate = GiftCertificateViewModelBinder.BindGiftCertificate(giftcertificateViewModel);
                giftCertificate.StatusID = 1;

                _giftCertificateService.AddGiftCertificate(giftCertificate);
                return RedirectToAction("Edit", new { id=giftCertificate.GiftCertificateID});
            }

            PopulateLookup();

            return Create(); //GiftCertificateViewModel.GiftCertificateModel(giftcertificate));
        }
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult GiftCertificateDetail_Create([DataSourceRequest] DataSourceRequest request, GiftCertificateDetailsViewModel giftCertificateDetailsViewModel, int? id)
        {
            if (giftCertificateDetailsViewModel != null && ModelState.IsValid && id.HasValue)
            {
                giftCertificateDetailsViewModel.GiftCertificateID = id.Value;
                var giftcertifiateDtail = GiftCertificateViewModelBinder.BindGiftCertificateDetail(giftCertificateDetailsViewModel);
                _giftCertificateDetailService.AddGiftCertificateDetail(giftcertifiateDtail);
            }

            return Json(new[] { giftCertificateDetailsViewModel }.ToDataSourceResult(request, ModelState));
        }
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult GiftCertificateDetail_Read([DataSourceRequest] DataSourceRequest request, int? id)
        {
            if (!id.HasValue)
            {
                return Json((new List<GiftCertificateDetailsViewModel>()).ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
            }
            else
            {
                var gc = _giftCertificateService.FindById(id.Value);
                if (gc != null)
                {
                    var gC =
                        GiftCertificateViewModelBinder.BindListOfGiftCertificateDetailsViewModel(
                            gc.GiftCertificateDetails.ToList());

                    return Json(gC.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json((new List<GiftCertificateDetailsViewModel>()).ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
                }
            }
        }
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult GiftCertificateDetail_Update([DataSourceRequest]DataSourceRequest request, GiftCertificateDetailsViewModel giftCertificateDetailsViewModel)
        {
            if (giftCertificateDetailsViewModel != null && ModelState.IsValid)
            {
                var target = _giftCertificateDetailService.FindById(giftCertificateDetailsViewModel.GiftCertificateDetailID);
                if (target != null)
                {
                    target = GiftCertificateViewModelBinder.BindGiftCertificateDetail(target, giftCertificateDetailsViewModel);

                    _giftCertificateDetailService.EditGiftCertificateDetail(target);
                }
            }

            return Json(new[] { giftCertificateDetailsViewModel }.ToDataSourceResult(request, ModelState));
        }
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult GiftCertificateDetail_Destroy([DataSourceRequest] DataSourceRequest request,
                                                  GiftCertificateDetailsViewModel giftCertificateDetailsViewModel)
        {
            if (giftCertificateDetailsViewModel != null)
            {
                _giftCertificateDetailService.DeleteById(giftCertificateDetailsViewModel.GiftCertificateDetailID);
            }

            return Json(ModelState.ToDataSourceResult());
        }
    
        public ActionResult Edit(int id)
        {
            var giftcertificate = _giftCertificateService.Get(t => t.GiftCertificateID == id, null, "GiftCertificateDetails,GiftCertificateDetails.Commodity").FirstOrDefault();
            PopulateLookup(false, giftcertificate);
            var giftCertificateViewModel = GiftCertificateViewModelBinder.BindGiftCertificateViewModel(giftcertificate);
            return View(giftCertificateViewModel);
        }

        [HttpPost]
        public ActionResult Edit(GiftCertificateViewModel giftcertificate)
        {
            //just incase the user meses with the the hidden GiftCertificateID field
            var giftcert = _giftCertificateService.FindById(giftcertificate.GiftCertificateID);

            if (ModelState.IsValid && giftcert != null)
            {

                giftcert = GiftCertificateViewModelBinder.BindGiftCertificate(giftcert, giftcertificate);

                _giftCertificateService.EditGiftCertificate(giftcert);

                return RedirectToAction("Index");
            }
            PopulateLookup(false, giftcert);


            return View(giftcertificate);
        }
        public ActionResult Delete(int id)
        {
            var giftcertificate = _giftCertificateService.FindById(id);
            return View(GiftCertificateViewModelBinder.BindGiftCertificateViewModel(giftcertificate));
        }


        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            _giftCertificateService.DeleteById(id);
            return RedirectToAction("Index");
        }

        public ActionResult IsBillOfLoadingDuplicate(string BillOfLoading)
        {
            return Json(_giftCertificateService.IsBillOfLoadingDuplicate(BillOfLoading), JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
       public ActionResult Approved(int id)
       {
           var giftCertificate = _giftCertificateService.FindById(id);
           var giftCertificateViewModel = GiftCertificateViewModelBinder.BindGiftCertificateViewModel(giftCertificate);
           return PartialView("_Approve", giftCertificateViewModel);
       }
        [HttpPost]
       public ActionResult Approve(int GiftCertificateID)
        {
            var result = _transactionService.PostGiftCertificate(GiftCertificateID);
            return RedirectToAction("Index");
        }
        private void PopulateLookup(bool isNew = true, Cats.Models.GiftCertificate giftCertificate = null)
        {
            ViewData["Commodities"] = _commonService.GetCommodities(null, t => t.OrderBy(o => o.Name));

            ViewBag.DCurrencies = _commonService.GetDetails(d => d.MasterID == Master.Constants.CURRENCY, t => t.OrderBy(o => o.SortOrder));
            ViewBag.DFundSources = _commonService.GetDetails(d => d.MasterID == Master.Constants.FUND_SOURCE, t => t.OrderBy(o => o.SortOrder));
            ViewBag.DBudgetTypes = _commonService.GetDetails(d => d.MasterID == Master.Constants.BUDGET_TYPE, t => t.OrderBy(o => o.SortOrder));

             var giftCertificateDetails = new List<GiftCertificateDetailsViewModel>();
            ViewBag.GiftCertificateDetails = giftCertificateDetails;
            if (isNew && giftCertificate == null)
            {
                ViewBag.DonorID = new SelectList(_commonService.GetDonors(null, t => t.OrderBy(o => o.Name)), "DonorID",
                                                 "Name");
                ViewBag.CommodityTypeID =
                    new SelectList(_commonService.GetCommodityTypes(null, t => t.OrderBy(o => o.Name)),
                                   "CommodityTypeID", "Name",1);
                ViewBag.ProgramID = new SelectList(_commonService.GetPrograms(), "ProgramID", "Name");
                ViewBag.DModeOfTransport = new SelectList(_commonService.GetDetails(d => d.MasterID == Master.Constants.TRANSPORT_MODE, t => t.OrderBy(o => o.SortOrder)), "DetailID", "Name");
        
            }
            else
            {
                var giftDetails = giftCertificate.GiftCertificateDetails.FirstOrDefault();
                ViewBag.DonorID = new SelectList(_commonService.GetDonors(null, t => t.OrderBy(o => o.Name)), "DonorID",
                                                "Name",giftCertificate.DonorID);
                ViewBag.CommodityTypeID =
                    new SelectList(_commonService.GetCommodityTypes(null, t => t.OrderBy(o => o.Name)),
                                   "CommodityTypeID", "Name", giftDetails == null ? "1" : giftDetails.Commodity.CommodityTypeID.ToString());
                ViewBag.ProgramID = new SelectList(_commonService.GetPrograms(), "ProgramID", "Name",giftCertificate.ProgramID);
                ViewBag.DModeOfTransport = new SelectList(_commonService.GetDetails(d => d.MasterID == Master.Constants.TRANSPORT_MODE, t => t.OrderBy(o => o.SortOrder)), "DetailID", "Name",giftCertificate.DModeOfTransport);
        
            }
        }

        public ActionResult LetterTemplate(int giftceritificateId)
        {
            ViewData["giftcertficateId"] = giftceritificateId;
            return View();
        }

       public ActionResult ShowLetterTemplates([DataSourceRequest] DataSourceRequest request)
       {
           return Json(_letterTemplateService.GetAllLetterTemplates().ToDataSourceResult(request));

       }
        public void ShowTemplate(string fileName, int giftCertificateId)
        {
            // TODO: Make sure to use DI to get the template generator instance
            try
            {
                var template = new TemplateHelper(_unitofwork);
                string filePath = template.GenerateTemplate(giftCertificateId, 1, fileName); //here you have to send the name of the tempalte and the id of the giftcertificate
                Response.ContentType = "application/text";
                Response.AddHeader("Content-Disposition", @"filename= " + fileName + ".docx");
                Response.TransmitFile(filePath);
            }
            catch 
            {
                
            }
           
        }
        protected override void Dispose(bool disposing)
        {
            _giftCertificateService.Dispose();
        }

    }
}
