﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Cats.Models;
using Cats.Models.ViewModels.HRD;
using Cats.Services.EarlyWarning;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;

namespace Cats.Areas.EarlyWarning.Controllers
{
    public class HRDController : Controller
    {
        //
        // GET: /EarlyWarning/HRD/
        private IAdminUnitService _adminUnitService;
        private IHRDService _hrdService;
        private IRationService _rationService;
        private IHRDDetailService _hrdDetailService;
        private ICommodityService _commodityService;

        public HRDController(IAdminUnitService adminUnitService, IHRDService hrdService, 
                             IHRDDetailService hrdDetailService,ICommodityService commodityService)
        {
            _adminUnitService = adminUnitService;
            _hrdService = hrdService;
            _hrdDetailService = hrdDetailService;
            _commodityService = commodityService;
        }

        public ActionResult Index()
        {

            var hrd = _hrdService.GetAllHRD();
            return View(hrd);
        }
        public ActionResult HRDDetail(int id = 0)
        {
            var hrd = _hrdService.Get(m => m.HRDID == id, null, "HRDDetails,HRDDetails.HRDCommodityDetails").FirstOrDefault();

            if (hrd != null)
            {
                return View(hrd);
            }
            return RedirectToAction("Index");
        }

        public ActionResult HRD_Read([DataSourceRequest] DataSourceRequest request)
        {

            var hrds = _hrdService.GetAllHRD().OrderByDescending(m => m.HRDID);
            var hrdsToDisplay = GetHrds(hrds).ToList();
            return Json(hrdsToDisplay.ToDataSourceResult(request));
        }

        public ActionResult HRDDetail_Read([DataSourceRequest] DataSourceRequest request, int id = 0)
        {
            var hrdDetail = _hrdService.GetHRDDetailByHRDID(id);
            if (hrdDetail != null)
            {
                var detailsToDisplay = GetHRDDetails(hrdDetail).ToList();
                return Json(detailsToDisplay.ToDataSourceResult(request));
            }
            return RedirectToAction("Index");
        }
        //gets hrd information
        private IEnumerable<HRDViewModel> GetHrds(IEnumerable<HRD> hrds)
        {
            return (from hrd in hrds
                    select new HRDViewModel()
                    {
                        HRDID = hrd.HRDID,
                        Month = hrd.Month,
                        Year = hrd.Year,
                        CreatedDate = DateTime.Now,
                        PublishedDate = hrd.PublishedDate

                    });
        }

        //gets hrd Detail Information
        private IEnumerable<HRDDetailViewModel> GetHRDDetails(IEnumerable<HRDDetail> hrdDetails)
        {
            return (from hrdDetail in hrdDetails
                    select new HRDDetailViewModel()
                        {
                            HRDDetailID = hrdDetail.HRDDetailID,
                            HRDID = hrdDetail.HRDID,
                            WoredaID = hrdDetail.WoredaID,
                            NumberOfBeneficiaries = hrdDetail.NumberOfBeneficiaries,
                            StartingMonth = hrdDetail.StartingMonth,
                            DurationOfAssistance = hrdDetail.DurationOfAssistance,
                            
                            CSB = (int) hrdDetail.HRDCommodityDetails.Single(m => m.CommodityID == 8).Amount,
                            Pulse = (int)hrdDetail.HRDCommodityDetails.Single(m => m.CommodityID == 2).Amount,
                            Cereal = (int)hrdDetail.HRDCommodityDetails.Single(m => m.CommodityID == 1).Amount,
                            Oil = (int)hrdDetail.HRDCommodityDetails.Single(m => m.CommodityID == 4).Amount

                            


                        });
        }
        public ActionResult Create(int id = 0)
        {
            var hrd = new HRD();
            hrd.HRDDetails = new List<HRDDetail>();
            var woredas = _adminUnitService.FindBy(m => m.AdminUnitTypeID == 3);
             var commodities = _commodityService.GetAllCommodity();

            var hrdDetails = (from detail in woredas
                              select new HRDDetail()
                              {
                                  WoredaID = detail.AdminUnitID,
                                  NumberOfBeneficiaries = 0
                                 
                              }).ToList();
            hrd.HRDDetails = hrdDetails;


            //ViewBag.Ration = _rationService.GetAllRation();
            //foreach (var woreda in woredas)
            //{
            //    var detail = new HRDDetail();
            //    detail.Woreda = woreda;
            //    hrd.HRDDetails.Add(detail);
            //}

            //var viewModel = new CreateHumanitarianRequirementViewModel(hrd);
            //ViewData["HRDDetail"] = viewModel;
            return View(hrd);
        }

        [HttpPost]
        public ActionResult Create(HRD hrd)
        {

            if (ModelState.IsValid)
            {
                
                var woredas = _adminUnitService.FindBy(m => m.AdminUnitTypeID == 3);
                var commodities = _commodityService.Get(m=>m.CommodityID==1 && m.CommodityID==2 && m.CommodityID==4 && m.CommodityID==8);

                var hrdDetails = (from detail in woredas
                                  select new HRDDetail()
                                  {
                                      WoredaID = detail.AdminUnitID,
                                      NumberOfBeneficiaries = 0,
                                      DurationOfAssistance = 0,
                                      HRDCommodityDetails = (from commodity in commodities
                                                             select new HRDCommodityDetail()
                                                             {
                                                                 CommodityID = commodity.CommodityID,
                                                                 Amount = 0
                                                             }).ToList()
                                    
                                
                                  }).ToList();
                
                hrd.HRDDetails = hrdDetails;
                _hrdService.AddHRD(hrd);

                // RouteValueDictionary routeValues = this.GridRouteValues();
                //return RedirectToAction("Edit","Bid", routeValues);
                //return RedirectToAction("Edit", "Bid", new { id = bid.BidID });
                return RedirectToAction("Index");
            }




            //var requirement = viewModel.Hrd;
            //_hrdService.AddHRD(requirement);
            return RedirectToAction("Index");
        }
    }
}
