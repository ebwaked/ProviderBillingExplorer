using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Models
{
    public class Provider
    {
        [Key]
        public string NPI { get; set; } = default!;
        public string ProviderName { get; set; } = default!;
        public string Specialty { get; set; } = default!;
        public string State { get; set; } = default!;

        public ICollection<BillingRecord> BillingRecords { get; set; } = new List<BillingRecord>();
    }
}
