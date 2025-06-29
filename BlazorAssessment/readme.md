# 🧪 Blazor Interview Assignment

### **Title:** _Provider Billing Explorer_

---

## 🧩 Part 1 – Data Ingestion (/DataIngestionConsole)

### 📥 Dataset

**Medicare Physician & Other Practitioners – by Provider and Service** (Just get the latest data - 3GB) 
🔗 [https://data.cms.gov/provider-summary-by-type-of-service/medicare-physician-other-practitioners/medicare-physician-other-practitioners-by-provider-and-service](https://data.cms.gov/provider-summary-by-type-of-service/medicare-physician-other-practitioners/medicare-physician-other-practitioners-by-provider-and-service)

---

### 🧱 Requirements

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

### 🎯 Objective

Use the SQLite database from Part 1 to create a **Blazor Server** app that allows users to explore provider billing data.

---

### 🧱 Requirements

#### 1. **Provider List Page**

- Display: 
    - NPI
    - Name
    - Specialty
    - State
    
- Filters:
    - Specialty (dropdown)
    - State (dropdown)
    
- Search by NPI or Name (with debounced input)
- Optional: paging or virtualization for large result sets

#### 2. **Provider Detail Page**

- Shows **top 10 HCPCS codes** by total Medicare payment
- Display in a table:
    - HCPCS Code
    - Description
    - Number of Services
    - Total Medicare Payment
    
- Optional: Include a **bar chart** (e.g. ChartJs.Blazor) to visualize payments by HCPCS code

---

### 💡 Optional Bonus Features

- Filter detail view by **Place of Service**
- Include **"Export to CSV"** button for current view
- Show **national average** for selected HCPCS codes (across all providers)
- Use EF-Core in a shared libary to handle the db operations


---

### 📦 Deliverables for Part 2

- Blazor Server app (`.NET 8`) 
- README with:
    - Setup instructions
    - Navigation overview
    - Any optional features implemented

---

### 🧪 Evaluation Criteria (Across Both Parts)

| Category             | What to Look For                                        |
| -------------------- | ------------------------------------------------------- |
| **Data Engineering** | CSV handling, schema normalization, efficient ingestion |
| **Blazor Skills**    | Component structure, routing, state handling            |
| **Query Design**     | LINQ or SQL performance, filtering, grouping            |
| **UI/UX**            | Usability, filter interactivity, responsive layout      |
| **Code Quality**     | Clarity, modularity, appropriate abstraction            |
| **Optional Extras**  | Charting, export, comparison logic                      |
