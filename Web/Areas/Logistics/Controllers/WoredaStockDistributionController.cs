﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Cats.Areas.Logistics.Models;
using Cats.Helpers;
using Cats.Models;
using Cats.Models.Constant;
using Cats.Services.Common;
using Cats.Services.Logistics;
using Cats.Services.EarlyWarning;
using Cats.Services.Security;
using Cats.ViewModelBinder;
using Kendo.Mvc.UI;
using Kendo.Mvc.Extensions;

namespace Cats.Areas.Logistics.Controllers
{
    public class WoredaStockDistributionController : Controller
    {

        private readonly IUtilizationHeaderSerivce _utilizationService;
        private readonly IUtilizationDetailSerivce _utilizationDetailSerivce;
        private readonly IReliefRequisitionService _reliefRequisitionService;
        private readonly UserAccountService _userAccountService;
        private readonly IWorkflowStatusService _workflowStatusService;
        private readonly ICommonService _commonService;
        private readonly IRegionalRequestService _regionalRequestService;
        private readonly IDistributionByAgeDetailService _distributionByAgeDetailService;
        public WoredaStockDistributionController(IUtilizationHeaderSerivce utilizationService, IUtilizationDetailSerivce utilizationDetailSerivce, 
                       UserAccountService userAccountService, IWorkflowStatusService workflowStatusService, ICommonService commonService, 
                        IRegionalRequestService regionalRequestService,IDistributionByAgeDetailService distributionByAgeDetailService)
        {
            _utilizationService = utilizationService;
            _utilizationDetailSerivce = utilizationDetailSerivce;
            _userAccountService = userAccountService;
            _workflowStatusService = workflowStatusService;
            _commonService = commonService;
            _regionalRequestService = regionalRequestService;
            _distributionByAgeDetailService = distributionByAgeDetailService;
        }

        //
        // GET: /Logistics/Utilization/

        public ActionResult Index()
        {
           
            ViewBag.RegionCollection = _commonService.GetAminUnits(t => t.AdminUnitTypeID == 2);
           
            ViewBag.ProgramID = new SelectList(_commonService.GetPrograms().Take(2),"ProgramId","Name");

            return View();
        }

        public ActionResult Create()
        {
            var utilizationHeader = new WoredaStockDistribution();
            ViewBag.Region = new SelectList(_commonService.GetAminUnits(m => m.AdminUnitTypeID == 2), "AdminUnitID","Name");
            ViewBag.Zone = new SelectList(_commonService.FindBy(m => m.AdminUnitTypeID == 3 && m.ParentID == 3),"AdminUnitID", "Name");
            ViewBag.Woreda = new SelectList(_commonService.FindBy(m => m.AdminUnitTypeID == 4 && m.ParentID == 19),"AdminUnitID", "Name");
            ViewBag.ProgramID = new SelectList(_commonService.GetPrograms(), "ProgramID", "Name");
            ViewBag.Month = new SelectList(RequestHelper.GetMonthList(),"Id","Name");
            return View(utilizationHeader);
        }
        [HttpPost]
        public ActionResult Create(WoredaStockDistribution woredaStockDistribution)
        {
            if (woredaStockDistribution!=null && ModelState.IsValid)
            {
                var utilization = _utilizationService.FindBy(m => m.WoredaID == woredaStockDistribution.WoredaID && m.WoredaStockDistributionID == woredaStockDistribution.WoredaStockDistributionID).FirstOrDefault();
                if (utilization==null)
                {
                    var userProfileId = _userAccountService.GetUserInfo(HttpContext.User.Identity.Name).UserProfileID;
                    woredaStockDistribution.DistributedBy = userProfileId;
                    woredaStockDistribution.DistributionDate = DateTime.Now;
                    _utilizationService.AddHeaderDistribution(woredaStockDistribution);
                    return RedirectToAction("Index");
                }
                ModelState.AddModelError("Errors",@"Distribution Information for this Woreda already exist.You can Edit the fields");
                return RedirectToAction("Edit", new {id = woredaStockDistribution.WoredaStockDistributionID});
            }
            ModelState.AddModelError("Errors",@"Unable to Save Distribution Information");
            return View();
        }
        public ActionResult ReadRequestionNumbers([DataSourceRequest] DataSourceRequest request, 
                                                  int zoneId=-1, 
                                                  int programId = -1,
                                                  int planId = -1,
                                                  int round =-1,
                                                   int month=-1)
        {
            if (zoneId == -1 || programId ==-1 || planId ==-1)
                return null;
            if (programId == 1 && (month == -1 && round == -1))
                return null;
            if (programId == 2 && round == -1)
                return null;

           

            
            var datePref = _userAccountService.GetUserInfo(HttpContext.User.Identity.Name).DatePreference;
            var requisition = _utilizationService.GetRequisitions(zoneId,programId,planId,6,month,round);
            var requisitionViewModel =UtilizationViewModelBinder.GetUtilizationViewModel(requisition);
            return Json(requisitionViewModel.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        public ActionResult WoredaStockDetail_Read([DataSourceRequest] DataSourceRequest request, int woredaID=-1, int woredaStockDistributionID=-1)
        {
            if (woredaID==-1) return null;
            if (woredaStockDistributionID!=-1)
            {
                var woredaStockDistribution =
                    _utilizationDetailSerivce.FindBy(
                        m => m.FDP.AdminUnitID == woredaID && m.WoredaStockDistributionID == woredaStockDistributionID);
                var woredaDistributionDetail = GetWoredaStockDistributionDetail(woredaStockDistribution);
                return Json(woredaDistributionDetail.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
            }
           

                var fdpStockDistribution = _commonService.GetFDPs(woredaID);
                var woredaStockDistributionDetail = GetWoredaStockDistribution(fdpStockDistribution);
                return Json(woredaStockDistributionDetail.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
           

        }

        private IEnumerable<WoredaDistributionDetailViewModel> GetWoredaStockDistributionDetail(IEnumerable<WoredaStockDistributionDetail> woredaStockDistributionDetails)
        {
            return (from woredaStockDistributionDetail in woredaStockDistributionDetails
                    select new WoredaDistributionDetailViewModel()
                        {
                            FdpId = woredaStockDistributionDetail.FdpId,
                            FDP = woredaStockDistributionDetail.FDP.Name,
                            WoredaStockDistributionID = woredaStockDistributionDetail.WoredaStockDistributionID,
                            WoredaStockDistributionDetailID = woredaStockDistributionDetail.WoredaStockDistributionID,
                            DistributedAmount = woredaStockDistributionDetail.DistributedAmount
                            
                        });

        }

        private IEnumerable<WoredaDistributionDetailViewModel>  GetWoredaStockDistribution(IEnumerable<FDP> fdps)
        {
            return (from fdp in fdps
                    select new WoredaDistributionDetailViewModel()
                        {
                            FdpId = fdp.FDPID,
                            FDP = fdp.Name,
                            DistributedAmount = 0
                        });
        }
        //private  bool IsSaved(int planId, int month, int round)
        //{
        //    try
        //    {
        //        var utilization =
        //            _utilizationService.FindBy(u => u.PlanId == planId && u.Month == month && u.Round == round).ToList();
        //        if (utilization.Count > 0)
        //            return true;
        //        else
        //            return false;
        //    }
        //    catch (Exception)
        //    {

        //        return false;
        //    }
        //}
        public ActionResult ReadRequisitionDetail([DataSourceRequest] DataSourceRequest request,   int requisitionId = -1 )
        {
            if (requisitionId == -1)
                return null;
            var datePref = _userAccountService.GetUserInfo(HttpContext.User.Identity.Name).DatePreference;
            var requisitionDetail = _utilizationService.GetReliefRequisitions(requisitionId);
            //var requisitionViewModel = UtilizationViewModelBinder.GetUtilizationDetailViewModel(requisition);
            var distributionByAgeDetailViewModel = GetDistributionByAgeDetail(requisitionDetail);
            return Json(distributionByAgeDetailViewModel.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);

        }
        private IEnumerable<WoredaDistributionDetailViewModel> GetDistributionByAgeDetail(IEnumerable<ReliefRequisitionDetail> reliefRequisitionDetails)
        {
            var woredaGrouped = (from reliefRequisitionDetail in reliefRequisitionDetails
                                 group reliefRequisitionDetail by reliefRequisitionDetail.FDP.AdminUnit
                                 into woredaDetail
                                 select woredaDetail);
            return (from groupedDetail in woredaGrouped
                    select new WoredaDistributionDetailViewModel()
                        {
                            WoredaID = groupedDetail.Key.AdminUnitID,
                            WoredaName = groupedDetail.Key.Name
                           
                        });

        }
        public ActionResult DistributionByAge_Read([DataSourceRequest] DataSourceRequest request, int requisitionId = -1)
        {
            if (requisitionId == -1)
                return null;
            var datePref = _userAccountService.GetUserInfo(HttpContext.User.Identity.Name).DatePreference;
            var requisitionDetail = _utilizationService.GetReliefRequisitions(requisitionId);
            var distributionByAgeDetailViewModel = UtilizationViewModelBinder.GetDistributionByAgeDetail(requisitionDetail);
            return Json(distributionByAgeDetailViewModel.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);

        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Update([DataSourceRequest] DataSourceRequest request, IEnumerable<Models.UtilizationDetailViewModel> utilizationDetailViewModels)
        {
            var results = new List<Models.UtilizationDetailViewModel>();
            return Json(results.ToDataSourceResult(request, ModelState));
        }

        //[AcceptVerbs(HttpVerbs.Post)]
        //public ActionResult Utilization_Update([DataSourceRequest] DataSourceRequest request, [Bind(Prefix = "models")]IEnumerable<Models.UtilizationDetailViewModel> utilizationDetailViewModels)
        //{


        //           var userProfileId = _userAccountService.GetUserInfo(HttpContext.User.Identity.Name).UserProfileID;
        //            var planId = utilizationDetailViewModels.FirstOrDefault().PlanId;
        //            var month = utilizationDetailViewModels.FirstOrDefault().Month;
        //            var round = utilizationDetailViewModels.FirstOrDefault().Round;
        //            var requisitionID = utilizationDetailViewModels.FirstOrDefault().RequisitionId;
                   
                
        //        foreach (var utilizationDetailViewModel in utilizationDetailViewModels)
        //        {


        //            var utilizationHeader =
        //                _utilizationService.FindBy(m => m.RequisitionId == requisitionID && m.PlanId == planId &&
        //                                                m.Month == month && m.Round == round).FirstOrDefault();
        //            if (utilizationHeader != null)
        //            {
        //                var utilizationDetail =
        //                    _utilizationDetailSerivce.FindBy(
        //                        m => m.WoredaStockDistributionID == utilizationHeader.WoredaStockDistributionID
        //                             && m.FdpId == utilizationDetailViewModel.FdpId).FirstOrDefault();
        //                var distributionByAgeDetail =
        //                    _distributionByAgeDetailService.FindBy(
        //                        m => m.DistributionHeaderID == utilizationHeader.WoredaStockDistributionID
        //                             && m.FDPID == utilizationDetailViewModel.FdpId).FirstOrDefault();
        //                if (utilizationDetail != null)
        //                {
        //                    utilizationDetail.DistributedAmount = utilizationDetailViewModel.DistributedQuantity;
        //                    _utilizationDetailSerivce.EditDetailDistribution(utilizationDetail);
        //                }
        //                else
        //                {
        //                    var utilizationDetailModel = new WoredaStockDistributionDetail
        //                        {
        //                            DistributedAmount = utilizationDetailViewModel.DistributedQuantity,
        //                            FdpId = utilizationDetailViewModel.FdpId,
        //                            WoredaStockDistributionID = utilizationHeader.WoredaStockDistributionID
        //                        };
        //                    _utilizationDetailSerivce.AddDetailDistribution(utilizationDetailModel);
        //                }
        //                if (distributionByAgeDetail != null)
        //                {
        //                    distributionByAgeDetail.FemaleLessThan5Years =
        //                        utilizationDetailViewModel.FemaleLessThan5Years;
        //                    distributionByAgeDetail.MaleLessThan5Years = utilizationDetailViewModel.MaleLessThan5Years;
        //                    distributionByAgeDetail.FemaleBetween5And18Years =
        //                        utilizationDetailViewModel.FemaleBetween5And18Years;
        //                    distributionByAgeDetail.MaleBetween5And18Years =
        //                        utilizationDetailViewModel.MaleBetween5And18Years;
        //                    distributionByAgeDetail.FemaleAbove18Years = utilizationDetailViewModel.FemaleAbove18Years;
        //                    distributionByAgeDetail.MaleAbove18Years = utilizationDetailViewModel.MaleAbove18Years;
        //                    _distributionByAgeDetailService.EditDistributionByAgeDetail(distributionByAgeDetail);

        //                }
        //                else
        //                {
        //                    var distributionDetailByAgeModel = new DistributionByAgeDetail()
        //                        {
        //                            FDPID = utilizationDetailViewModel.FdpId,
        //                            FemaleLessThan5Years = utilizationDetailViewModel.FemaleLessThan5Years,
        //                            MaleLessThan5Years = utilizationDetailViewModel.MaleLessThan5Years,
        //                            FemaleBetween5And18Years = utilizationDetailViewModel.FemaleBetween5And18Years,
        //                            MaleBetween5And18Years = utilizationDetailViewModel.MaleBetween5And18Years,
        //                            FemaleAbove18Years = utilizationDetailViewModel.FemaleAbove18Years,
        //                            MaleAbove18Years = utilizationDetailViewModel.MaleAbove18Years,
        //                            DistributionHeaderID = utilizationHeader.WoredaStockDistributionID
        //                        };
        //                    _distributionByAgeDetailService.AddDistributionByAgeDetail(distributionDetailByAgeModel);
        //                }
        //            }
        //            else
        //            {
        //                var utilization = new WoredaStockDistribution()
        //                    {
        //                        //RequisitionId = requisitionID,
        //                        //PlanId = planId,
        //                        Month = month,
        //                        //Round = round,
        //                        DistributionDate = DateTime.Now,
        //                        DistributedBy = userProfileId
        //                    };

        //                _utilizationService.AddHeaderDistribution(utilization);
        //            }
        //        }
            
        //    return Json(utilizationDetailViewModels.ToDataSourceResult(request, ModelState));
        //}

        //[AcceptVerbs(HttpVerbs.Post)]
        //public ActionResult Create([DataSourceRequest] DataSourceRequest request,
        //    [Bind(Prefix = "models")]IEnumerable<Models.UtilizationDetailViewModel> utilizationDetailViewModels, FormCollection collection)
        //{

        //    int planId = 0;
        //    int month = 0;
        //    int round = 0;
            

        //    foreach (var utilizationDetailViewModel in utilizationDetailViewModels)
        //    {
        //       planId = utilizationDetailViewModel.PlanId;
        //       month = utilizationDetailViewModel.Month;
        //       round = utilizationDetailViewModel.Round;
        //        break;
        //    }


        //    var userProfileId = _userAccountService.GetUserInfo(HttpContext.User.Identity.Name).UserProfileID;
        //    var results = new List<Models.UtilizationDetailViewModel>();


            
        //    var utilization = new Cats.Models.WoredaStockDistribution();
                                 
            
        //       WoredaStockDistribution utilizationToBeSaved =_utilizationService.FindBy(u => u.PlanId == planId && u.Month == month && u.Round == round).SingleOrDefault();
        //        if (utilizationToBeSaved != null)
        //        {
        //            foreach (var utilizationDetailViewModel in utilizationDetailViewModels)
        //            {
        //                UtilizationDetailViewModel model = utilizationDetailViewModel;
        //                var utilDetail =
        //                    _utilizationDetailSerivce.FindBy(
        //                        u =>
        //                        u.FdpId == model.FdpId && u.WoredaStockDistribution.PlanId == planId &&
        //                        u.WoredaStockDistribution.Month == month && u.WoredaStockDistribution.Round == round).SingleOrDefault();

        //                var distributionByAgeDetail=_distributionByAgeDetailService.FindBy(m=>m.FDPID==model.FdpId && m.WoredaStockDistribution.PlanId == planId &&
        //                        m.WoredaStockDistribution.Month == month && m.WoredaStockDistribution.Round == round).SingleOrDefault();
        //                //if (utilDetail == null &&) continue;
        //                if (utilDetail != null)
        //                {
        //                    utilDetail.DistributedQuantity = utilizationDetailViewModel.DistributedQuantity;
        //                    _utilizationDetailSerivce.EditDetailDistribution(utilDetail);
        //                }
        //               if(distributionByAgeDetail!=null)
        //                {
        //                    distributionByAgeDetail.FemaleAbove18Years = model.FemaleAbove18Years;
        //                    distributionByAgeDetail.FemaleBetween5And18Years = model.FemaleBetween5And18Years;
        //                    distributionByAgeDetail.FemaleAbove18Years = model.FemaleAbove18Years;
        //                }
        //            }
        //        }
        //        else
        //        {

        //             foreach (var utilizationDetailViewModel in utilizationDetailViewModels)
        //             {
        //                  var utilizationDetail = new Cats.Models.WoredaStockDistributionDetail
        //                                            {
        //                                                DistributedQuantity =
        //                                                utilizationDetailViewModel.DistributedQuantity,
        //                                                FdpId = utilizationDetailViewModel.FdpId,
        //                                                WoredaStockDistribution = utilization
        //                                            };
        //                  var distributionByAgeDetail = new DistributionByAgeDetail()
        //                      {
        //                          FDPID = utilizationDetailViewModel.FdpId,
        //                          FemaleLessThan5Years = utilizationDetailViewModel.FemaleLessThan5Years,
        //                          MaleLessThan5Years = utilizationDetailViewModel.MaleLessThan5Years,
        //                          FemaleBetween5And18Years = utilizationDetailViewModel.FemaleBetween5And18Years,
        //                          MaleBetween5And18Years = utilizationDetailViewModel.MaleBetween5And18Years,
        //                          FemaleAbove18Years = utilizationDetailViewModel.FemaleAbove18Years,
        //                          MaleAbove18Years = utilizationDetailViewModel.MaleAbove18Years,
        //                          WoredaStockDistribution = utilization

        //                      };

        //                 utilization.RequisitionId = utilizationDetailViewModel.RequisitionId;
        //                 utilization.PlanId = utilizationDetailViewModel.PlanId;
        //                 utilization.Month = utilizationDetailViewModel.Month;
        //                 utilization.Round = utilizationDetailViewModel.Round;
        //                 utilization.DistributionDate = DateTime.Now;
        //                 utilization.DistributedBy = userProfileId;
        //                 _utilizationDetailSerivce.AddDetailDistribution(utilizationDetail);
        //                 _distributionByAgeDetailService.AddDistributionByAgeDetail(distributionByAgeDetail);

        //             }

                                    
        //    }

        //    return Json(results.ToDataSourceResult(request, ModelState));
        //}
                


        public JsonResult GetPlans(string id)
        {
            var programId = int.Parse(id);
            var plans = _commonService.GetPlan(programId);
            return Json(new SelectList(plans.ToList(), "PlanID", "PlanName"), JsonRequestBehavior.AllowGet);
        }
       
        public JsonResult GetMonth(string id)
        {
            try
            {
                var planid = int.Parse(id);

                var months = _regionalRequestService.FindBy(r => r.PlanID == planid).ToList();
                var month = from m in months
                             select new {month = m.Month};
                var distinctMonth = month.Distinct();
                return Json(new SelectList(distinctMonth, "month", "month"), JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {

                return null;
            }
          
        }
        public JsonResult GetRound(string id)
        {
            try
            {
                var planid = int.Parse(id);
                var rounds = _regionalRequestService.FindBy(r => r.PlanID == planid).ToList();
                var round = from r in rounds
                            where r.Round != null
                            select new {round = r.Round};
                var distinctRound = round.Distinct();
                return Json(new SelectList(distinctRound, "round", "round"), JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {

                return null;
            }
           
        }
        public JsonResult GetAdminUnits()
        {
            var r = (from region in _commonService.GetRegions()
                     select new
                     {

                         RegionID = region.AdminUnitID,
                         RegionName = region.Name,
                         Zones = from zone in _commonService.GetZones(region.AdminUnitID)
                                 select new
                                 {
                                     ZoneID = zone.AdminUnitID,
                                     ZoneName = zone.Name,
                                     Woredas = from woreda in _commonService.GetWoreda(zone.AdminUnitID)
                                               select new
                                               {
                                                   WoredaID = woreda.AdminUnitID,
                                                   WoredaName = woreda.Name
                                               }
                                 }
                     }
                    );
            return Json(r, JsonRequestBehavior.AllowGet);
        }
    }
}