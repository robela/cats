﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Cats.Areas.Logistics.Models
{
    public class DonationHeaderViewModel
    {
        public int ReceiptHeaderId { get; set; }
        public int? GiftCertificateDetailId { get; set; }
        public Nullable<System.DateTime> ReceiptDate { get; set; }
        public Nullable<int> EnteredBy { get; set; }
        public bool? IsClosed { get; set; }
        public string Remark { get; set; }

        public int?  ProgramId { get; set; }
        public string Program { get; set; }
        public string Donor { get; set; }
        public int? DonorId { get; set; }
        public int? Unit { get; set; }
        public DateTime? ETA { get; set; }
        public string SINumber { get; set; }
        public int? ShippingInstructionId { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public string Commodity { get; set; }
        public int? CommodityId { get; set; }
        public string CommodityType { get; set; }
        public decimal? WieghtInMT { get; set; }
        public string Date { get; set; }
    }
}