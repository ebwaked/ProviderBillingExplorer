using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Models
{
    public sealed class HcpcsSummary
    {
        public string HcpcsCode { get; set; }
        public string Description { get; set; }
        public int ServiceCount { get; set; }
        public decimal TotalPayment { get; set; }
    }
}
