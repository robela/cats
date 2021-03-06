﻿

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Cats.Data.UnitWork;
using Cats.Models;
using Cats.Models.Constant;
using Cats.Models.ViewModels;


namespace Cats.Services.EarlyWarning
{

    public class RegionalRequestService : IRegionalRequestService
    {
        private readonly IUnitOfWork _unitOfWork;


        public RegionalRequestService(IUnitOfWork unitOfWork)
        {
            this._unitOfWork = unitOfWork;
        }

        #region Default Service Implementation

        public bool AddRegionalRequest(RegionalRequest regionalRequest)
        {
            // regionalRequest.RegionalRequestDetails = CreateRequestDetail(regionalRequest.RegionID);
            regionalRequest.Status = (int) RegionalRequestStatus.Draft;
            // regionalRequest.RationID = 2;//TODO:SET DEFAULT Ration
            regionalRequest.RequistionDate = DateTime.Today;
            regionalRequest.ReferenceNumber = DateTime.Today.ToLongTimeString();
            _unitOfWork.RegionalRequestRepository.Add(regionalRequest);
            _unitOfWork.Save();
            regionalRequest.ReferenceNumber = "ref-00" + regionalRequest.RegionalRequestID;
            _unitOfWork.Save();
            return true;

        }

        private List<RegionalRequestDetail> CreateRequestDetail(int regionId)
        {
            //TODO:Filter with selected region
            var fdpList = _unitOfWork.FDPRepository.FindBy(t => t.AdminUnit.AdminUnit2.ParentID == regionId);
            var requestDetail = (from fdp in fdpList
                                 select new RegionalRequestDetail()
                                     {
                                         Beneficiaries = 0,
                                         Fdpid = fdp.FDPID

                                     });
            return requestDetail.ToList();
        }

        public bool EditRegionalRequest(RegionalRequest reliefRequistion)
        {
            _unitOfWork.RegionalRequestRepository.Edit(reliefRequistion);
            CalculateAllocation(reliefRequistion.RegionalRequestID);
            _unitOfWork.Save();
            return true;

        }

        private bool CalculateAllocation(int requestId)
        {
            var requestDetails =
                _unitOfWork.RegionalRequestDetailRepository.Get(t => t.RegionalRequestID == requestId, null,
                                                                "RequestDetailCommodities").ToList();
            foreach (var requestDetail in requestDetails)
            {



                foreach (var requestCommodity in requestDetail.RequestDetailCommodities)
                {
                    var rationAmount = GetCommodityRation(requestDetail.RegionalRequestID, requestCommodity.CommodityID);
                    var target =
                        _unitOfWork.RequestDetailCommodityRepository.FindById(requestCommodity.RequestCommodityID);

                    target.Amount = requestDetail.Beneficiaries*rationAmount;
                }
            }
            return true;
        }

        private decimal GetCommodityRation(int requestId, int commodityId)
        {
            var rationID = _unitOfWork.RegionalRequestRepository.FindById(requestId).RationID;
            var ration =
                _unitOfWork.RationDetailRepository.FindBy(t => t.RationID == rationID && t.CommodityID == commodityId).
                    FirstOrDefault();
            if (ration == null) return 0;
            return ration.Amount;
        }

        public bool DeleteRegionalRequest(RegionalRequest reliefRequistion)
        {
            if (reliefRequistion == null) return false;
            _unitOfWork.RegionalRequestRepository.Delete(reliefRequistion);
            _unitOfWork.Save();
            return true;
        }

        public bool DeleteById(int id)
        {
            var entity = _unitOfWork.RegionalRequestRepository.FindById(id);
            if (entity == null) return false;
            _unitOfWork.RegionalRequestRepository.Delete(entity);
            _unitOfWork.Save();
            return true;
        }

        public List<RegionalRequest> GetAllRegionalRequest()
        {
            return _unitOfWork.RegionalRequestRepository.GetAll();
        }

        public RegionalRequest FindById(int id)
        {
            return _unitOfWork.RegionalRequestRepository.FindById(id);
        }

        public List<RegionalRequest> FindBy(Expression<Func<RegionalRequest, bool>> predicate)
        {
            return _unitOfWork.RegionalRequestRepository.FindBy(predicate);
        }

        public IEnumerable<RegionalRequest> Get(
            Expression<Func<RegionalRequest, bool>> filter = null,
            Func<IQueryable<RegionalRequest>, IOrderedQueryable<RegionalRequest>> orderBy = null,
            string includeProperties = "")
        {
            var requisitions = _unitOfWork.RegionalRequestRepository.Get(filter, orderBy, includeProperties);
            //var regionalRequests=(from itm in requisitions select new RequestView
            //                                                          {
            //                                                              ProgramID=itm.ProgramId ,
            //                                                              Program=itm.Program.Name,
            //                                                              S
            //                                                          })
            return requisitions;
        }

        #endregion

        public void Dispose()
        {
            _unitOfWork.Dispose();

        }



        public List<int?> GetZonesFoodRequested(int requestId)
        {
            var regionalRequestDetails =
                _unitOfWork.RegionalRequestDetailRepository.Get(t => t.RegionalRequestID == requestId, null,
                                                                "FDP,FDP.AdminUnit");
            var zones =
                (from requestDetail in regionalRequestDetails
                 where requestDetail.Fdp.AdminUnit.ParentID != null
                 select requestDetail.Fdp.AdminUnit.ParentID).Distinct();
            return zones.ToList();

        }


        public List<RegionalRequest> GetSubmittedRequest(int region, int month, int status)
        {


            if (region != 0)
            {
                return month != 0
                           ? _unitOfWork.RegionalRequestRepository.Get(
                               r => r.RegionID == region && r.RequistionDate.Month == month && r.Status == status,
                               null,
                               "AdminUnit,Program").ToList()
                           : _unitOfWork.RegionalRequestRepository.Get(r => r.RegionID == region && r.Status == status,
                                                                       null,
                                                                       "AdminUnit,Program").ToList();
            }

            return month != 0
                       ? _unitOfWork.RegionalRequestRepository.Get(
                           r => r.RequistionDate.Month == month && r.Status == status, null,
                           "AdminUnit,Program").ToList()
                       : _unitOfWork.RegionalRequestRepository.Get(r => r.Status == status, null, "AdminUnit,Program").
                             ToList();
        }


        public bool ApproveRequest(int id)
        {
            var req = _unitOfWork.RegionalRequestRepository.FindById(id);
            req.Status = (int) RegionalRequestStatus.Approved;
            _unitOfWork.Save();
            return true;
        }

        public HRDPSNPPlanInfo PlanToRequest(HRDPSNPPlan plan)
        {
            if (CheckDurationOfAssisstance(plan))
            {
                HRDPSNPPlanInfo result = new HRDPSNPPlanInfo();
                List<BeneficiaryInfo> beneficiaryInfos = new List<BeneficiaryInfo>();
                result.HRDPSNPPlan = plan;
                if (plan.ProgramID == 2)
                {
                    RegionalPSNPPlan psnpplan =
                        _unitOfWork.RegionalPSNPPlanRepository.FindBy(r=>r.PlanId == plan.PSNPPlanID).FirstOrDefault();

                    if (psnpplan != null)
                    {
                        result.HRDPSNPPlan.RationID = psnpplan.RationID;
                        beneficiaryInfos = PSNPToRequest(psnpplan,plan.RegionID);
                    }
                }
                else if (plan.ProgramID == 1)
                {
                    HRD hrd = _unitOfWork.HRDRepository.FindBy(r => r.PlanID == plan.PlanID).LastOrDefault();

                    var lastRequest =_unitOfWork.RegionalRequestRepository.FindBy(r => r.RegionID == plan.RegionID && r.ProgramId == 1 && r.PlanID == plan.PlanID).LastOrDefault();

                    if (lastRequest != null)
                    {
                        var requests = _unitOfWork.RegionalRequestRepository.FindBy(r => r.RegionID == plan.RegionID && r.ProgramId == 1 && r.PlanID == plan.PlanID);
                        var numberOfRequestsPerRegion = requests.Count;
                        var applicableWoredas = (from detail in hrd.HRDDetails
                                                 where
                                                     detail.AdminUnit.AdminUnit2.AdminUnit2.AdminUnitID == plan.RegionID &&
                                                     detail.DurationOfAssistance > numberOfRequestsPerRegion
                                                 select detail.WoredaID).ToList();
                        beneficiaryInfos = LastReliefRequest(lastRequest, applicableWoredas);
                       // var lastRequestDetail = LastReliefRequest(lastRequest);



                    }
                    else
                    {
                        result.HRDPSNPPlan.RationID = hrd.RationID;
                        List<HRDDetail> hrddetail =
                            (from woreda in hrd.HRDDetails
                             where woreda.AdminUnit.AdminUnit2.AdminUnit2.AdminUnitID == plan.RegionID && woreda.DurationOfAssistance>0
                             select woreda).ToList();
                        beneficiaryInfos = HRDToRequest(hrddetail);
                    }
                }
                else
                {
                    //if program is IDPS
                    List<BeneficiaryInfo> benficiaries = new List<BeneficiaryInfo>();
                    List<AdminUnit> woredas = new List<AdminUnit>();
                    var zones =
                        _unitOfWork.AdminUnitRepository.FindBy(
                            w => w.AdminUnitTypeID == 3 && w.ParentID == plan.RegionID);
                    foreach (var zone in zones)
                    {
                        AdminUnit zone1 = zone;
                        woredas.AddRange(_unitOfWork.AdminUnitRepository.FindBy(w => w.ParentID == zone1.AdminUnitID));
                    }
                    //var 
                    foreach (var woreda in woredas)
                    {
                        AdminUnit woreda1 = woreda;
                        List<FDP> WoredaFDPs =
                            _unitOfWork.FDPRepository.FindBy(w => w.AdminUnitID == woreda1.AdminUnitID);
                        ICollection<BeneficiaryInfo> woredabeneficiaries =
                            (from FDP fdp in WoredaFDPs
                             select new BeneficiaryInfo {FDPID = fdp.FDPID, FDPName = fdp.Name, Beneficiaries = 0}).
                                ToList();
                        benficiaries.AddRange(woredabeneficiaries);
                    }

                    //beneficiaryInfos = benficiaries;
                    //beneficiaryInfos = null; 
                }
                result.BeneficiaryInfos = beneficiaryInfos;
                return result;
            }
            return null;
        }

        private List<BeneficiaryInfo> HRDToRequest(List<HRDDetail> plandetail)
        {
            List<BeneficiaryInfo> benficiaries = new List<BeneficiaryInfo>();
            foreach (HRDDetail d in plandetail)
            {
                List<FDP> WoredaFDPs = _unitOfWork.FDPRepository.FindBy(w => w.AdminUnitID == d.AdminUnit.AdminUnitID);
                ICollection<BeneficiaryInfo> woredabeneficiaries =
                    (from FDP fdp in WoredaFDPs
                     select new BeneficiaryInfo {FDPID = fdp.FDPID, FDPName = fdp.Name, Beneficiaries = 0}).ToList();
                benficiaries.AddRange(woredabeneficiaries);
            }
            return benficiaries;
        }

        private List<BeneficiaryInfo> PSNPToRequest(RegionalPSNPPlan plan,int regionID)
        {
            List<BeneficiaryInfo> benficiaries =
                (from RegionalPSNPPlanDetail pd in plan.RegionalPSNPPlanDetails
                 where pd.PlanedFDP.AdminUnit.AdminUnit2.AdminUnit2.AdminUnitID==regionID
                 select
                     new BeneficiaryInfo
                         {FDPID = pd.PlanedFDP.FDPID, FDPName = pd.PlanedFDP.Name, Beneficiaries = pd.BeneficiaryCount})
                    .ToList();
            return benficiaries;

        }

        private List<BeneficiaryInfo> LastReliefRequest(RegionalRequest request,List<int> woredas )
        {
            var benficiaries = (from lastRequestDetail in request.RegionalRequestDetails 
                                where woredas.Contains(lastRequestDetail.Fdp.AdminUnitID)
                                select
                                    new BeneficiaryInfo
                                        {
                                            FDPID = lastRequestDetail.Fdpid,
                                            FDPName = lastRequestDetail.Fdp.Name,
                                            Beneficiaries = lastRequestDetail.Beneficiaries
                                        }).ToList();
            return benficiaries;
        }
       
        private bool CheckDurationOfAssisstance(HRDPSNPPlan plan)
        {
            if (plan.ProgramID == 1)
            {

                var hrd = _unitOfWork.HRDRepository.FindBy(m => m.PlanID == plan.PlanID).LastOrDefault();
                //var woredas=new List<>();
                if (hrd != null)
                {
                    var requests =_unitOfWork.RegionalRequestRepository.FindBy(r => r.RegionID == plan.RegionID && r.ProgramId == 1 && r.PlanID == plan.PlanID);
                   
                    var hrdDetails = (from region in hrd.HRDDetails
                                      where region.AdminUnit.AdminUnit2.AdminUnit2.AdminUnitID == plan.RegionID
                                      select new
                                          {
                                              region.WoredaID,
                                              region.DurationOfAssistance
                                          }).ToList();
                   
                    if (requests.Count >= hrdDetails.Max(m => m.DurationOfAssistance))
                        return false;
                    return true;
                }
                
            }
            return true;
        }
        
    }
}




