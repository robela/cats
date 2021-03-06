﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Cats.Models;
using Cats.Data;
using Cats.Models;
using Cats.Services.Procurement;
using Cats.Services.EarlyWarning;

using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using Cats.Areas.Procurement.Models;

namespace Cats.Areas.Procurement.Controllers
{
    public class TransportBidPlanController : Controller
    {
        private readonly ITransportBidPlanService _transportBidPlanService;
        private readonly IAdminUnitService _adminUnitService;
        private readonly IProgramService _programService;
        private readonly ITransportBidPlanDetailService _transportBidPlanDetailService;
        private readonly IHubService _hubService;

        public TransportBidPlanController(ITransportBidPlanService transportBidPlanServiceParam
                                          , IAdminUnitService adminUnitServiceParam
                                          , IProgramService programServiceParam
                                          , ITransportBidPlanDetailService transportBidPlanDetailServiceParam
                                            , IHubService hubServiceParam)
        {
            this._transportBidPlanService = transportBidPlanServiceParam;
            this._adminUnitService = adminUnitServiceParam;
            this._programService = programServiceParam;
            this._transportBidPlanDetailService = transportBidPlanDetailServiceParam;
            this._hubService = hubServiceParam;
        }
        public TransportBidPlan fetchFromDB(int id)
        {
            TransportBidPlan transportbidplan = _transportBidPlanService.FindById(id);
            return transportbidplan;
        }
        public TransportBidPlanViewModel CopyToView(TransportBidPlan transportbidplan)
        {
            return new TransportBidPlanViewModel{TransportBidPlanID=transportbidplan.TransportBidPlanID
                                                ,Year=transportbidplan.Year
                                                ,YearHalf=transportbidplan.YearHalf
                                                };
        }
        public IEnumerable<TransportBidPlanViewModel> CopyListToView(IEnumerable<Cats.Models.TransportBidPlan> bidplan)
        {
            List<TransportBidPlanViewModel> ret=new List<TransportBidPlanViewModel>();
            foreach (TransportBidPlan i in bidplan)
            {
                ret.Add(CopyToView(i));
            }
            return ret.ToList();
        }
        public bool loadLookups(TransportBidPlan transportbidplan)
        {
            ViewBag.ProgramID = new SelectList(_programService.GetAllProgram(), "ProgramID", "Name", transportbidplan.ProgramID);
            return true;

        }

        //
        // GET: /Procurement/TransportBidPlan/

        public ActionResult Index()
        {
            List<Cats.Models.TransportBidPlan> list = _transportBidPlanService.GetAllTransportBidPlan();
            return View(CopyListToView(list).ToList());
        }
        public ActionResult GetListJson([DataSourceRequest] DataSourceRequest request)
        {
            //JsonRequestBehavior.AllowGet ;
            IEnumerable<Cats.Models.TransportBidPlan> list = _transportBidPlanService.GetAllTransportBidPlan().OrderByDescending(m=>m.TransportBidPlanID);
            return Json(CopyListToView(list).ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }
        //
        // GET: /Procurement/TransportBidPlan/Details/5

        public ActionResult Details(int id = 0)
        {
            TransportBidPlan transportbidplan = fetchFromDB(id);
            if (transportbidplan == null)
            {
                return HttpNotFound();
            }
            return View(transportbidplan);
        }

        //
        // GET: /Procurement/TransportBidPlan/Create

        public ActionResult Create()
        {
            loadLookups(new TransportBidPlan());
            return View();
        }

        //
        // POST: /Procurement/TransportBidPlan/Create

        [HttpPost]
        public ActionResult Create(TransportBidPlan transportbidplan)
        {
            if (ModelState.IsValid)
            {
                ViewBag.Message = "Invalid Model";
                var transportPlan =_transportBidPlanService.FindBy( m => m.Year == transportbidplan.Year && m.YearHalf == transportbidplan.YearHalf).FirstOrDefault();
                if (transportPlan == null)
                {


                    var woredas = _adminUnitService.FindBy(m => m.AdminUnitTypeID == 4);
                    var psnptransportBidPlanDetail = (from detail in woredas
                                                      //where _transportBidPlanDetailService.GetWoredaGroupedPsnpAmount(detail.AdminUnitID)>0
                                                      select new TransportBidPlanDetail()
                                                          {
                                                              DestinationID = detail.AdminUnitID,
                                                              SourceID =
                                                                  _hubService.GetNearestWarehouse(detail.AdminUnitID).
                                                          HubID,
                                                              ProgramID = 2,
                                                              Quantity =
                                                                  _transportBidPlanDetailService.
                                                          GetWoredaGroupedPsnpAmount(detail.AdminUnitID)

                                                          }).ToList();

                    var relieftransportBidPlanDetail = (from detail in woredas
                                                        //where _transportBidPlanDetailService.GetHrdCommodityAmount(detail.AdminUnitID)>0
                                                        select new TransportBidPlanDetail()
                                                            {
                                                                DestinationID = detail.AdminUnitID,
                                                                SourceID =
                                                                    _hubService.GetNearestWarehouse(detail.AdminUnitID).
                                                            HubID,
                                                                ProgramID = 1,
                                                                Quantity =
                                                                    _transportBidPlanDetailService.GetHrdCommodityAmount
                                                            (detail.AdminUnitID)
                                                            }).ToList();
                    var transportbidplanDetail = psnptransportBidPlanDetail.Union(relieftransportBidPlanDetail).ToList();
                    transportbidplan.TransportBidPlanDetails = transportbidplanDetail;
                    _transportBidPlanService.AddTransportBidPlan(transportbidplan);
                    return RedirectToAction("Index");
                }
                ModelState.AddModelError("Errors",@"Bid Plan For this Year and Period already Exists");
            }

            loadLookups(transportbidplan);
            return View(transportbidplan);
        }

        //
        // GET: /Procurement/TransportBidPlan/Edit/5

        public ActionResult Edit(int id = 0)
        {
            TransportBidPlan transportbidplan = fetchFromDB(id);
            if (transportbidplan == null)
            {
                return HttpNotFound();
            }
            loadLookups(transportbidplan);

            return View(transportbidplan);
        }

        //
        // POST: /Procurement/TransportBidPlan/Edit/5

        [HttpPost]
        public ActionResult Edit(TransportBidPlan transportbidplan)
        {
            if (ModelState.IsValid)
            {
                _transportBidPlanService.UpdateTransportBidPlan(transportbidplan);
                return RedirectToAction("Index");
            }
            loadLookups(transportbidplan);
            return View(transportbidplan);
        }

        //
        // GET: /Procurement/TransportBidPlan/Delete/5

        public ActionResult Delete(int id = 0)
        {
            TransportBidPlan transportbidplan = fetchFromDB(id);
            if (transportbidplan == null)
            {
                return HttpNotFound();
            }
            return View(transportbidplan);
        }

        //
        // POST: /Procurement/TransportBidPlan/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            //TransportBidPlan transportbidplan = fetchFromDB(id);
            _transportBidPlanService.DeleteById(id);
            return RedirectToAction("Index");
        }

       

        public List<AdminUnit> GetAllWoredas(int regionID)
        {
            List<AdminUnit> AdminUnits = _adminUnitService.FindBy(t => t.AdminUnitTypeID == 3 && t.AdminUnit2.AdminUnitID==regionID);
            return AdminUnits;
            System.Collections.Generic.Dictionary<string, TransportBidPlanDetail> filled;

        }

        public List<Cats.Areas.Procurement.Models.WarehouseProgramViewModel> GetWoredaWarehouseProgram(int BidPlanID, int WoredaID)
        {
            //this._transportBidPlanDetailService.get
            List<TransportBidPlanDetail> bidDetails = _transportBidPlanDetailService.FindBy(t => t.BidPlanID == BidPlanID && t.DestinationID == WoredaID);
            var planDetail = _transportBidPlanDetailService.FindBy(m =>m.BidPlanID == BidPlanID && m.DestinationID == WoredaID && m.ProgramID==1);
            var hubs = (from hub in planDetail
                        select new Cats.Models.Hub
                            {
                                HubID = hub.SourceID

                            });
            List<WarehouseProgramViewModel> ret=
               ( from hub in hubs
                    select new WarehouseProgramViewModel
                    {
                        WarehouseID = hub.HubID,
                        WarehouseName = hub.Name,
                        PSNP =0,
                        Relief=0,
                        BidPlanID=BidPlanID,
                        WoredaID=WoredaID
                    }).ToList();
            
            System.Collections.Generic.Dictionary<string, TransportBidPlanDetail> filled = new Dictionary<string, TransportBidPlanDetail>();

            foreach (var i in bidDetails)
            {
                string hash = i.ProgramID + "_" + i.SourceID;
                filled.Add(hash, i);// = i;
            }
            foreach (WarehouseProgramViewModel i in ret)
            {
                string hash_psnp = "2_" + i.WarehouseID;
                string hash_relief = "1_" + i.WarehouseID;
                if (filled.ContainsKey(hash_psnp))
                {
                    TransportBidPlanDetail psnp = filled[hash_psnp];
                    i.PSNP = psnp.Quantity;
                }
                if (filled.ContainsKey(hash_relief))
                {
                    TransportBidPlanDetail releif = filled[hash_relief];
                    i.Relief = releif.Quantity;
                }
              //  ret.Add(filled, i);// = i;
            }
            return ret;
        }
       

        //
        // GET: /Procurement/TransportBidPlan/Details/5

        public ActionResult WarehouseSelection(int id = 0)
        {
            TransportBidPlan transportbidplan = fetchFromDB(id);
            ViewBag.WarehouseID = _hubService.GetAllHub();
            @ViewBag.bidPlan = transportbidplan;
            ViewBag.RegionCollection = _adminUnitService.FindBy(t => t.AdminUnitTypeID == 2);
            List<WarehouseProgramViewModel> table = GetWoredaWarehouseProgram(id,0);
            return View(table);

        }

        public ActionResult ReadJson( [DataSourceRequest] DataSourceRequest request,int TransportBidPlanID, int selectedWoreda=0)
        {
            List<WarehouseProgramViewModel> table = GetWoredaWarehouseProgram(TransportBidPlanID, selectedWoreda);
           // return View(table);
            return Json(table.ToDataSourceResult(request));
        }
        public ActionResult DeleteWarehouseSelectionAjax(int TransportBidPlanID, int selectedWoreda = 0,int sourceWarehouse=0)
        {
            List<TransportBidPlanDetail> bidDetails =_transportBidPlanDetailService.FindBy(t => t.BidPlanID == TransportBidPlanID 
                                                            && t.DestinationID == selectedWoreda 
                                                            && t.SourceID==sourceWarehouse);

            // return View(table);
            foreach (TransportBidPlanDetail i in bidDetails)
            {
                _transportBidPlanDetailService.DeleteTransportBidPlanDetail(i);
            }
            return Json("{}");
        }
        public ActionResult DeleteAjax2([DataSourceRequest] DataSourceRequest request, TransportBidPlan transportbidplan)
        {
            if (transportbidplan != null)
            {
                _transportBidPlanService.DeleteById(transportbidplan.TransportBidPlanID);
                //transportService.DeleteTransporter(transporter);
            }

            return Json(ModelState.ToDataSourceResult());
        }
        public ActionResult UpdateAjax([DataSourceRequest] DataSourceRequest request, TransportBidPlan transportbidplan)
        {
            _transportBidPlanService.UpdateTransportBidPlan(transportbidplan);
//            transportService.EditTransporter(transporter);
            return Json(ModelState.ToDataSourceResult(), JsonRequestBehavior.AllowGet);
        }
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult CreateAjax([DataSourceRequest] DataSourceRequest request, TransportBidPlan transportbidplan)
        {
            if (transportbidplan != null && ModelState.IsValid)
            {
                _transportBidPlanService.AddTransportBidPlan(transportbidplan);
            }

            return Json(new[] { transportbidplan }.ToDataSourceResult(request, ModelState));
        }
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult CreateWarehouseSelectionAjax([DataSourceRequest] DataSourceRequest request,WarehouseProgramViewModel warehouseAllocation,
                                                                                                      int transportBidPlanID, int selectedWoreda = 0)
        {
            if (warehouseAllocation!=null && ModelState.IsValid)
            {
                var reliefDetail = new TransportBidPlanDetail()
                {
                    BidPlanID = transportBidPlanID,
                    DestinationID = selectedWoreda,
                    SourceID = warehouseAllocation.WarehouseID,
                    ProgramID = 1,
                    Quantity = _transportBidPlanDetailService.GetHrdCommodityAmount(selectedWoreda),
                };
                UpdateBidPlanDetail(reliefDetail);
                var psnpDetail = new TransportBidPlanDetail()
                {
                    BidPlanID = transportBidPlanID,
                    DestinationID = selectedWoreda,
                    SourceID = warehouseAllocation.WarehouseID,
                    ProgramID = 2,
                    Quantity = _transportBidPlanDetailService.GetWoredaGroupedPsnpAmount(selectedWoreda),
                };
                UpdateBidPlanDetail(psnpDetail);
            }

            return Json(new[] { warehouseAllocation }.ToDataSourceResult(request, ModelState));
        }

        public ActionResult UpdateWarehouseSelectionAjax([DataSourceRequest] DataSourceRequest request, WarehouseProgramViewModel WarehouseAllocation)
        {
            if (WarehouseAllocation != null && ModelState.IsValid)
            {
                List<TransportBidPlanDetail> inDb = _transportBidPlanDetailService.FindBy(w =>
                                w.BidPlanID == WarehouseAllocation.BidPlanID
                                && w.DestinationID == WarehouseAllocation.WoredaID
                                && w.SourceID == WarehouseAllocation.WarehouseID
                                );
                TransportBidPlanDetail psnp = new TransportBidPlanDetail
                    {
                        BidPlanID = WarehouseAllocation.BidPlanID,
                        SourceID = WarehouseAllocation.WarehouseID,
                        DestinationID = WarehouseAllocation.WoredaID,
                        Quantity = WarehouseAllocation.PSNP,
                        ProgramID=2
                    };
                UpdateBidPlanDetail(psnp);

                TransportBidPlanDetail relief = new TransportBidPlanDetail
                {
                    BidPlanID = WarehouseAllocation.BidPlanID,
                    SourceID = WarehouseAllocation.WarehouseID,
                    DestinationID = WarehouseAllocation.WoredaID,
                    Quantity = WarehouseAllocation.Relief,
                    ProgramID = 1
                };
                UpdateBidPlanDetail(relief);
            }

            return Json(new[] { WarehouseAllocation }.ToDataSourceResult(request, ModelState));
        }
        public void UpdateBidPlanDetail(TransportBidPlanDetail bpd)
        {
            List<TransportBidPlanDetail> inDb = _transportBidPlanDetailService.FindBy(w =>
                                w.BidPlanID == bpd.BidPlanID
                                && w.DestinationID == bpd.DestinationID
                                && w.SourceID == bpd.SourceID
                                && w.ProgramID == bpd.ProgramID
                                );
            if (inDb.Count <= 0)
            {
                _transportBidPlanDetailService.AddTransportBidPlanDetail(bpd);
            }
            else
            {
                TransportBidPlanDetail bpd_orignal = inDb[0];
                bpd_orignal.Quantity = bpd.Quantity;
                _transportBidPlanDetailService.UpdateTransportBidPlanDetail(bpd_orignal);
            }
        }
        public ActionResult DeleteBidPlan(int id)
        {
            var bidPlan = _transportBidPlanService.FindById(id);
            if (bidPlan!=null)
            {
                _transportBidPlanService.DeleteTransportBidPlan(bidPlan);
                _transportBidPlanDetailService.DeleteByBidPlanID(bidPlan.TransportBidPlanID);
                return RedirectToAction("Index");
            }
           ModelState.AddModelError("Errors","Unable to delete Bid Plan");
           return RedirectToAction("Index");
        }
        
    }
}