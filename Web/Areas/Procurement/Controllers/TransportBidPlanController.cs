﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Cats.Models;
using Cats.Data;
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
            return View(CopyListToView(list));
        }
        public ActionResult GetListJson([DataSourceRequest] DataSourceRequest request)
        {
            //JsonRequestBehavior.AllowGet ;
            IEnumerable<Cats.Models.TransportBidPlan> list = _transportBidPlanService.GetAllTransportBidPlan();
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
                _transportBidPlanService.AddTransportBidPlan(transportbidplan);
                return RedirectToAction("Index");
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
             List<Hub> hubs = _hubService.GetAllHub();
            
            List<WarehouseProgramViewModel> ret=
               ( from hub in hubs
                    select new WarehouseProgramViewModel
                    {
                        WarehouseID = hub.HubId,
                        WarehouseName = hub.Name,
                        PSNP=0,
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
        

       
    }
}