# 🧪 Blazor Interview Assignment

### **Title:** _Provider Billing Explorer_

---

## 🧩 Part 1 – Data Ingestion (/DataIngestionConsole)

### 📥 Dataset

**Medicare Physician & Other Practitioners – by Provider and Service** (Just get the latest data - 3GB) 
🔗 [https://data.cms.gov/provider-summary-by-type-of-service/medicare-physician-other-practitioners/medicare-physician-other-practitioners-by-provider-and-service](https://data.cms.gov/provider-summary-by-type-of-service/medicare-physician-other-practitioners/medicare-physician-other-practitioners-by-provider-and-service)

---

### 🧱 How to Ingest Data

1. **Download & Parse CSV**
    - Download the latest ZIP file containing the CSV
    - Extract and parse the CSV file
    - Rename csv to data.csv
    - Copy file to C:\data.csv
    - Alternitively you can adjust the file path and csv name at the top of Program.cs
2. **Run DataIngestionConsole**
    - Run the console app to ingest the data
    - Should take 5-7 minutes
    - If hardware is an issue adjust batch size to 1000 or 10000 to reduce memory usage
    - That will increase time to around 15-20 minutes


---

## 🌐 Part 2 – Blazor Server App (/ProviderBillingBlazor)

---

#### 1. **Provider List Page**

- Added virtualization for large result sets

#### 2. **Provider Detail Page**
    
- Navigate to from the table on the Providers page by clicking the link under details for the row you are accessing

---
