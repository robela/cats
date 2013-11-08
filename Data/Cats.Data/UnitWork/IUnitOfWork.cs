using System;
using Cats.Models;
using Cats.Data.Repository;

namespace Cats.Data.UnitWork
{
    public interface IUnitOfWork : IDisposable
    {
        // TODO: Add properties to be implemented by UnitOfWork class for each repository

        IGenericRepository<DashboardWidget> DashboardWidgetRepository { get; }
        IGenericRepository<UserDashboardPreference> UserDashboardPreferenceRepository { get; }
        IGenericRepository<HubOwner> HubOwnerRepository { get; }
        IGenericRepository<Log> LogRepository { get; }
        IGenericRepository<RegionalPSNPPledge> RegionalPSNPPledgeRepository { get; }

        IGenericRepository<BusinessProcessState> BusinessProcessStateRepository { get; }
        IGenericRepository<BusinessProcess> BusinessProcessRepository { get; }
        IGenericRepository<ProcessTemplate> ProcessTemplateRepository { get; }
        IGenericRepository<StateTemplate> StateTemplateRepository { get; }
        IGenericRepository<FlowTemplate> FlowTemplateRepository { get; }

        IGenericRepository<RegionalRequest> RegionalRequestRepository { get; }
        IGenericRepository<RegionalRequestDetail> RegionalRequestDetailRepository { get; }
        IGenericRepository<AdminUnit> AdminUnitRepository { get; }
        IGenericRepository<AdminUnitType> AdminUnitTypeRepository { get; }
        IGenericRepository<Commodity> CommodityRepository { get; }
        IGenericRepository<CommodityType> CommodityTypeRepository { get; }
        IGenericRepository<FDP> FDPRepository { get; }
        IGenericRepository<Program> ProgramRepository { get; }
        IGenericRepository<Hub> HubRepository { get; }
        IGenericRepository<ReliefRequisitionDetail> ReliefRequisitionDetailRepository { get; }
        IGenericRepository<ReliefRequisition> ReliefRequisitionRepository { get; }
        IGenericRepository<TransportRequisition> TransportRequisitionRepository { get; }
        IGenericRepository<HubAllocation> HubAllocationRepository { get;}
        IGenericRepository<ProjectCode> ProjectCodeRepository { get; }

        IGenericRepository<UserDashboard> UserDashboardRepository { get; }
        IGenericRepository<BidWinner> BidWinnerRepository { get; } 

        IGenericRepository<ProjectCodeAllocation> ProjectCodeAllocationRepository { get; }

        IGenericRepository<ShippingInstruction> ShippingInstructionRepository { get; }
        IGenericRepository<Transporter> TransporterRepository { get; } 


        //IGenericRepository<Transporter> TransporterRepository { get; }
        IGenericRepository<TransportBidPlan> TransportBidPlanRepository { get; }
        IGenericRepository<TransportBidPlanDetail> TransportBidPlanDetailRepository { get; }

        IGenericRepository<Bid> BidRepository { get; }
        IGenericRepository<BidDetail> BidDetailRepository { get; }
        IGenericRepository<Status> StatusRepository { get; }


        //IGenericRepository<DispatchAllocationDetail> DispatchAllocationRepository { get; }
        IGenericRepository<DispatchAllocation> DispatchAllocationRepository { get; }
        IGenericRepository<TransportOrder> TransportOrderRepository { get; }

        IGenericRepository<TransportOrderDetail> TransportOrderDetailRepository { get; }
       // IGenericRepository<TransportBidWinnerDetail> TransportBidWinnerDetailRepository { get; }

        IGenericRepository<vwTransportOrder> VwTransportOrderRepository { get; }
        //IGenericRepository<TransportRequisition> TransportRequisitionRepository { get; }
        IGenericRepository<TransportRequisitionDetail> TransportRequisitionDetailRepository { get; }
        IGenericRepository<Transaction> TransactionRepository { get; }
        IGenericRepository<ReceiptAllocation> ReceiptAllocationReository { get; }

        IGenericRepository<WorkflowStatus> WorkflowStatusRepository { get; }

        IGenericRepository<TransportBidQuotation> TransportBidQuotationRepository { get; }
        IGenericRepository<ApplicationSetting> ApplicationSettingRepository { get; }

        IGenericRepository<Ration> RationRepository { get; }

        IGenericRepository<NeedAssessment> NeedAssessmentRepository { get; } 
        IGenericRepository<NeedAssessmentHeader> NeedAssessmentHeaderRepository { get; }
        IGenericRepository<NeedAssessmentDetail> NeedAssessmentDetailRepository { get; }

        IGenericRepository<RationDetail> RationDetailRepository { get; } 
        IGenericRepository<HRD> HRDRepository { get; }
        IGenericRepository<HRDDetail> HRDDetailRepository { get; }


        IGenericRepository<UserProfile> UserProfileRepository { get; } 

        IGenericRepository<RegionalPSNPPlan> RegionalPSNPPlanRepository { get; }
        IGenericRepository<RegionalPSNPPlanDetail> RegionalPSNPPlanDetailRepository { get; } 
        
        



        IGenericRepository<RequestDetailCommodity> RequestDetailCommodityRepository { get; }

        IGenericRepository<GiftCertificate> GiftCertificateRepository { get; }
        IGenericRepository<GiftCertificateDetail> GiftCertificateDetailRepository { get; }



        IGenericRepository<vwPSNPAnnualPlan> VwPSNPAnnualPlanRepository { get; }
        IGenericRepository<Unit> UnitRepository { get; }
        IGenericRepository<Season> SeasonRepository { get; }

        IGenericRepository<Contribution> ContributionRepository { get; }
        IGenericRepository<ContributionDetail> ContributionDetailRepository { get; }
        IGenericRepository<Donor> DonorRepository { get; } 

 
        IGenericRepository<Detail> DetailRepository { get; }

        IGenericRepository<TypeOfNeedAssessment> TypeOfNeedAssessmentRepository { get; }

        IGenericRepository<NeedAssessmentSummary> NeedAssessmetSummaryRepository { get;}

        IGenericRepository<LetterTemplate> LetterTemplateRepository { get; }
        IGenericRepository<Currency> CurrencyRepository { get; } 
        IGenericRepository<TransactionGroup> TransactionGroupRepository { get; }

        IGenericRepository<InKindContributionDetail> InKindContributionDetailRepository { get; }

      

        IGenericRepository<Store> StoreRepository { get; } 

        IGenericRepository<CommoditySource> CommoditySourceRepository { get; }
        IGenericRepository<Audit> AuditRepository { get; }
        IGenericRepository<CommodityGrade> CommodityGradeRepository { get; }
        IGenericRepository<UserHub> UserHubRepository { get; }
        IGenericRepository<TransReqWithoutTransporter> TransReqWithoutTransporterRepository { get; }
        IGenericRepository<AllocationByRegion> AllocationByRegionRepository { get; }

        IGenericRepository<PromisedContribution> PromisedContributionRepository { get; }


        void Save();

    }
}
