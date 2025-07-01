using CsvHelper.Configuration;
using System.Globalization;

namespace Common.Models
{
    // Class map for CsvHelper to handle header mapping
    public sealed class BillingRecordMap : ClassMap<BillingRecord>
    {
        public BillingRecordMap()
        {
            Map(m => m.NPI).Name("Rndrng_NPI");
            Map(m => m.Provider.ProviderName).Name("Rndrng_Prvdr_Last_Org_Name");
            Map(m => m.Provider.Specialty).Name("Rndrng_Prvdr_Crdntls");
            Map(m => m.Provider.State).Name("Rndrng_Prvdr_State_Abrvtn");
            Map(m => m.HCPCScode).Name("HCPCS_Cd");
            Map(m => m.HCPCSdesc).Name("HCPCS_Desc");
            Map(m => m.PlaceOfService).Name("Place_Of_Srvc");
            Map(m => m.NumberOfServices).Name("Tot_Srvcs").TypeConverterOption.NumberStyles(NumberStyles.AllowDecimalPoint); // Assumed partial services when populating data, can adjust on front end if not needed
            Map(m => m.TotalMedicarePayment).Name("Avg_Mdcr_Pymt_Amt").TypeConverterOption.NumberStyles(NumberStyles.AllowDecimalPoint);
        }
    }

}
