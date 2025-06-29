namespace Common.Models
{
    public sealed class BillingRecord
    {
        public string NPI { get; set; }
        public string ProviderName { get; set; }
        public string Specialty { get; set; }
        public string State { get; set; }
        public string HCPCScode { get; set; }
        public string PlaceOfService { get; set; }
        public decimal NumberOfServices { get; set; }
        public decimal TotalMedicarePayment { get; set; }
    }
}
