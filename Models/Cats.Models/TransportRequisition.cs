﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cats.Models
{
    public partial class TransportRequisition
    {
        public TransportRequisition()
        {
            this.TransportRequisitionDetails = new List<TransportRequisitionDetail>();
            //this.TransReqWithoutTransporters = new List<TransReqWithoutTransporter>();
        }

        public int TransportRequisitionID { get; set; }
        [Display(Name="Transport Requisition No")]
        public string TransportRequisitionNo { get; set; }
        public int RegionID { get; set; }
        public int ProgramID { get; set; }
        [Display(Name="Requested By")]
        public int RequestedBy { get; set; }
        [Display(Name="Requested Date")]
        public System.DateTime RequestedDate { get; set; }
        public int CertifiedBy { get; set; }
        [Display(Name="Certified Date")]
        public System.DateTime CertifiedDate { get; set; }
        public string Remark { get; set; }
        public int Status { get; set; }
        public virtual AdminUnit AdminUnit { get; set; }
        public virtual Program Program { get; set; }
        public virtual ICollection<TransportRequisitionDetail> TransportRequisitionDetails { get; set; }
        //public virtual ICollection<TransReqWithoutTransporter> TransReqWithoutTransporters { get; set; } 
    }
}
