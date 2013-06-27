﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cats.Models
{
    public class Donor
    {
        public int DonorID { get; set; }
        public string Name { get; set; }
        public string DonorCode { get; set; }
        public bool IsResponsibleDonor { get; set; }
        public bool IsSourceDonor { get; set; }
        public string LongName { get; set; }
    }
}
