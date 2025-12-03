# Vehicle Import System

A web-based Decision Support System (DSS) designed to automate customs duty calculations and analyze the economic feasibility of importing vehicles into Ukraine.

## Business Logic & Key Features

The system addresses the complexity and lack of transparency in manual customs clearance calculations ("turnkey price"). Its primary value lies in transforming dry legislative formulas into clear financial analytics.

### 1. Core Calculation Engine
Implements the complete algorithm based on current Ukrainian legislation (Laws No. 8487/8488, Tax Code):
* **Import Duty:** Automatic rate determination (10% standard or 0% for electric vehicles).
* **Excise Duty:**
    * **ICE (Petrol/Diesel):** Complex formula usage: `Base Rate * Volume * Age`, considering engine displacement coefficients.
    * **EV:** Calculated based on battery capacity (1 EUR per 1 kWh).
    * **Hybrid:** Fixed preferential rate.
* **VAT:** 20% applied to the sum of (Car Price + Duty + Excise), accounting for EV exemptions.
* **Pension Fund:** Dynamic calculation (3%, 4%, or 5%) based on the final vehicle value relative to the living wage threshold.

### 2. Market Intelligence Module
The system goes beyond tax calculation to answer the question: *"Is this import profitable?"*.
* **Auto.ria Integration:** Fetches the real-time average market price of a similar vehicle in Ukraine.
* **ROI Calculation:** Automatically compares the "Turnkey Import Price" with the local market price to display net savings.

### 3. Live Data Dynamics
* **NBU Integration:** Automatic updates of currency exchange rates (EUR, USD). This is critical for the accuracy of Pension Fund calculations (thresholds fixed in UAH) and Excise Duty (rates fixed in EUR).
* **Caching:** Implementation of `IMemoryCache` to optimize external API usage and reduce latency.

### 4. User Experience (UX)
* **Guest Mode:** History tracking without mandatory user registration (utilizing device identifiers).
* **Visualization:** Graphical representation of the cost structure (charts), highlighting the tax share in the final price.

## 🛠 Tech Stack

The project is built upon **Clean Architecture** principles to ensure flexibility, testability, and loose coupling of external services.

* **Backend:** .NET 8 (ASP.NET Core Minimal API).
* **Architecture:** Domain-Driven Design (DDD) elements, Clean Architecture layers (Domain, Application, Infrastructure, API).
* **Data Persistence:** MS SQL Server + Entity Framework Core.
* **Configuration:** Options Pattern (strict typing for financial parameters).
* **API Documentation:** Swagger / OpenAPI.

## Roadmap

### Sprint 1: Foundation & Core (Completed)
*Goal: Establish architecture and implement tax calculation logic.*
- [x] Clean Architecture setup.
- [x] Domain layer development (Entities, Enums, Settings).
- [x] Core calculation service implementation (`CustomsCalculatorService`).
- [x] Configuration setup via `appsettings.json` (Options Pattern).
- [x] Mock services for Currency and Market API (to save API limits).

### Sprint 2: Data & Infrastructure (In Progress)
*Goal: Connect the database and real data sources.*
- [ ] **Entity Framework Core** setup.
- [ ] Database structure creation (Code First Migrations).
- [ ] History saving implementation (Snapshot pattern).
- [ ] Real integration with **NBU API**.
- [ ] Real integration with **AUTO.RIA API**.

### Sprint 3: Basic Interface (Frontend Basic)
*Goal: Create a functional client application.*
- [ ] **Angular** project initialization.
- [ ] UI library setup (Angular Material).
- [ ] Input form development.
- [ ] Integration with .NET API (HTTP Client).

### Sprint 4: Analytics & Visualization (Frontend Polish)
*Goal: Finalize the product for defense.*
- [ ] Chart implementation (Chart.js / ng2-charts).
- [ ] "Savings" analytics block (UI logic).
- [ ] "Calculation History" page.
- [ ] Final testing and polishing.
