using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Common.Models
{
    public sealed class BillingRecord
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string NPI { get; set; } = default!;
        [ForeignKey(nameof(NPI))]
        public Provider? Provider { get; set; }
        public string HCPCScode { get; set; } = default!;
        public string HCPCSdesc { get; set;} = default!;
        public string PlaceOfService { get; set; } = default!;
        public decimal NumberOfServices { get; set; }
        public decimal TotalMedicarePayment { get; set; }
    }
}
