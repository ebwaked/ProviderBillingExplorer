# 🧪 Blazor Interview Assignment

### **Title:** _Provider Billing Explorer_

---

## 📕 Background and Rationale

At CompIQ, one of our primary responsibilities is ingesting data from disparate sources and displaying that data to our customers, both internal and external.

This assessment tests your ability to ingest data, create efficient indexes on that data and display it to a potential customer. The best solutions will utilize common logic between the DataIngestionConsole and the ProviderBillingBlazor projects. You may add additional class libraries as needed.

You should be able to complete this assessment in roughly 6 hours.

---

## 📩 Submission

Fork this repository and submit a link to your completed fork via email.

---

## 🧩 Part 1 – Data Ingestion (/DataIngestionConsole)

### 🎯 Objective

Write a standalone C# console app (or script) that:

- Downloads and parses Medicare billing data
- Creates a normalized **SQLite database**
- Populates provider and billing tables efficiently

### 📥 Dataset

**Medicare Physician & Other Practitioners – by Provider and Service** (Just get the latest data - 3GB) 
🔗 [https://data.cms.gov/provider-summary-by-type-of-service/medicare-physician-other-practitioners/medicare-physician-other-practitioners-by-provider-and-service](https://data.cms.gov/provider-summary-by-type-of-service/medicare-physician-other-practitioners/medicare-physician-other-practitioners-by-provider-and-service)

---

### 🧱 Requirements

1. **Download & Parse CSV**
    - Download the latest ZIP file containing the CSV
    - Extract and parse the CSV file
2. **Create SQLite Schema**  
    Suggested structure:
    ```sql
    CREATE TABLE Provider (
      NPI TEXT PRIMARY KEY,
      ProviderName TEXT,
      Specialty TEXT,
      State TEXT
    );
    
    CREATE TABLE BillingRecord (
      Id INTEGER PRIMARY KEY AUTOINCREMENT,
      NPI TEXT,
      HCPCSCode TEXT,
      HCPCSDescription TEXT,
      PlaceOfService TEXT,
      NumberOfServices INTEGER,
      TotalMedicarePayment REAL,
      FOREIGN KEY (NPI) REFERENCES Provider(NPI)
    );
    ```
    
3. **Populate Database**
    - Insert all distinct providers first
    - Insert billing records in bulk (efficient inserts are encouraged)
    - Optional: add indexes on `NPI`, `Specialty`, `State`, `HCPCSCode`

---

### ✅ Deliverables for Part 1

- C# Console app or script (e.g., `.csproj`)    
- README describing:
    - How to run the script
    - Any assumptions or transformations made

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
