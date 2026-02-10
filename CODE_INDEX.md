# PupTrails V3 - Code Index & Architecture

**Project Type:** WPF Desktop Application (.NET 8.0-Windows)  
**Database:** SQLite with Entity Framework Core 8.0.0  
**Framework:** Community Toolkit MVVM 8.4.0  
**Date Generated:** February 4, 2026

---

## 📁 Project Structure Overview

```
PupTrailsV3/
├── Models/                  # Data models
│   └── DataModels.cs       # All entity classes
├── Data/                   # Database layer
│   └── PupTrailDbContext.cs  # EF Core DbContext
├── Services/               # Business logic & utilities
│   ├── BackupService.cs
│   ├── DatabaseService.cs
│   ├── LicenseManager.cs
│   ├── LicenseService.cs
│   ├── LoggingService.cs
│   ├── PathManager.cs      # Portable path management
│   ├── MachineIdGenerator.cs
│   ├── CsvExportService.cs
│   ├── MasterExportService.cs
│   ├── MasterResetService.cs
│   ├── SocialMediaExportService.cs
│   ├── ImageResizer.cs
│   └── ExportOptions.cs
├── ViewModels/             # MVVM ViewModels
│   ├── BaseViewModel.cs    # Base class with INotifyPropertyChanged
│   └── MainViewModel.cs    # Main application ViewModel (818 lines)
├── Views/                  # Dialog windows & UI components
│   ├── AddAnimalWindow.xaml(.cs)
│   ├── AddPersonWindow.xaml(.cs)
│   ├── AddVetVisitWindow.xaml(.cs)
│   ├── AddAdoptionWindow.xaml(.cs)
│   ├── AddTripWindow.xaml(.cs)
│   ├── AddExpenseWindow.xaml(.cs)
│   ├── AddIncomeWindow.xaml(.cs)
│   ├── AddIntakeWindow.xaml(.cs)
│   ├── AddMoneyOwedWindow.xaml(.cs)
│   ├── AdvancedSearchWindow.xaml(.cs)
│   ├── SelectPuppiesWindow.xaml(.cs)
│   ├── ViewGroupDetailsWindow.xaml(.cs)
│   ├── ExportSocialMediaWindow.xaml(.cs)
│   └── [More dialog windows...]
├── Converters/             # Value converters for WPF
│   └── StatusColorConverter.cs
├── Migrations/             # EF Core database migrations
│   ├── 20260130012206_InitialCreate
│   ├── 20260130030132_AddMoneyOwedTable
│   ├── 20260130061023_AddCollarColorToAnimal
│   ├── 20260130065035_AddPuppyGroupsTable
│   ├── 20260204000824_AddVaccinationFieldsToVetVisit
│   └── [More migrations...]
├── App.xaml(.cs)           # Application entry point
├── MainWindow.xaml(.cs)    # Main application window (605 lines)
├── DashboardPage.xaml(.cs) # Dashboard view
├── ActivationWindow.xaml(.cs)  # License activation dialog
├── PupTrailsV3.csproj      # Project configuration
├── PupTrailsV3.sln         # Solution file
└── LicenseGenerator/       # Utility project (excluded from main build)
```

---

## 🗄️ Data Models (DataModels.cs)

### Core Entities:

#### **Animal**
- Track rescue dogs/puppies
- Fields: Name, Breed, Sex, Colour, CollarColor, Weight, DOB, IntakeDate, Status
- Status values: Planned, In Transport, In Care, Vet Pending, Ready, Adopted, Transferred, Deceased
- Relations: TripAnimals (M-M), VetVisits, Adoptions, Expenses, Income, FileAttachments
- Soft delete support: `IsDeleted` boolean

#### **Person**
- Adopters, Vets, Contacts, Volunteers
- Fields: Name, Email, Phone, Address, Type, Notes
- Type values: Adopter, Vet, Contact, Volunteer
- Relations: VetVisits, Adoptions, Incomes

#### **Trip**
- Track transport/movement trips
- Fields: Date, Purpose, StartLocation, EndLocation, DistanceKm, FuelLitres, FuelCost
- Relations: TripAnimals (M-M), Expenses

#### **TripAnimal** (Many-to-Many)
- Join table between Trip and Animal
- Composite key: (TripId, AnimalId)

#### **VetVisit**
- Track veterinary visits
- Fields: AnimalId, PersonId (Vet), Date, TotalCost, Notes, InvoicePath, ReadyForAdoption
- Vaccination tracking: RabiesShotDate, DistemperDate, DAPPDate, VaccinationsGiven
- Medical procedures: WormingDate, DeFleeingDate, DentalDate, SpayedNeuteringDate
- Costs tracked per procedure
- Relations: Services (VetService collection)

#### **VetService**
- Individual services within a vet visit
- Fields: VetVisitId, ServiceName, Cost

#### **Adoption**
- Track animal adoptions
- Fields: AnimalId, PersonId (Adopter), Date, AgreedFee, PaidFee, Paid, ContractPath, Notes
- Relations: Animal, Person

#### **Expense**
- Track financial expenses
- Categories: Trip, Vet Visit, Supplies, Fuel, Tolls, Intake, Medication, Other
- Fields: Date, Amount, Category, Description, AnimalId, TripId, PersonId, ReceiptPath
- Relations: Animal (optional), Trip (optional), Person (optional)

#### **Income**
- Track financial income
- Sources: AdoptionFee, Donation, GroupAdoption, Other
- Fields: Date, Amount, Source, Description, AnimalId, PersonId, Notes
- Relations: Animal (optional), Person (optional)

#### **Intake**
- Track intake records
- Fields: IntakeDate, AnimalCount, Origin, Purpose, Cost, Notes
- Relations: Animal (optional)

#### **MoneyOwed**
- Track debts/money owed
- Fields: PersonId, Date, Amount, DueDate, Reason, Status, Notes
- Status values: Outstanding, Partial, Paid
- Relations: Person

#### **PuppyGroup**
- Track litter/sibling groups
- Fields: GroupName, Description, CreatedDate, IsDeleted

#### **FileAttachment**
- Track attached files (receipts, invoices, contracts, etc.)
- Fields: AnimalId, TripId, PersonId, FilePath, FileType, UploadDate, Description

#### **License**
- Store application license information
- Fields: LicenseeName, LicenseKey, Signature, MachineId, ActivationDate, IsActive, Notes

---

## 🗃️ Database Context (PupTrailDbContext.cs)

**Location:** [Data/PupTrailDbContext.cs](Data/PupTrailDbContext.cs)

- **DbSets:** All 13 entity types defined
- **Database:** SQLite at `{AppBaseDirectory}/PupTrails/data/PupTrail.db`
- **Features:**
  - Automatic directory creation
  - Portable path management (fully portable application)
  - Foreign key relationships configured with delete behaviors
  - Indexes on commonly queried fields (Status, IntakeDate, Date)
  - Soft delete support through `IsDeleted` property
  - Many-to-many relationships (Animal ↔ Trip)

**Key Relationships:**
- Animal → VetVisits (Cascade delete)
- Animal → Adoptions (Cascade delete)
- Animal → Expenses (Set to null)
- Animal → Incomes (Set to null)
- Trip → TripAnimals (Cascade delete)
- VetVisit → VetServices (Cascade delete)

---

## 🎯 ViewModels

### BaseViewModel.cs
- Implements `INotifyPropertyChanged` for MVVM binding
- Helper methods:
  - `OnPropertyChanged(propertyName)` - Raises PropertyChanged event
  - `SetProperty<T>(ref field, T value)` - Generic property setter with change notification

### MainViewModel.cs (818 lines)
**Location:** [ViewModels/MainViewModel.cs](ViewModels/MainViewModel.cs)

**Purpose:** Central ViewModel for main application window

**Key Properties:**
- Dashboard metrics:
  - `AnimalsInCare`, `AnimalsReadyForAdoption`, `AnimalsAdopted`
  - `MonthlyProfit`, `TotalIncome`, `TotalExpenses`
- Income breakdown:
  - `TotalAdoptionFees`, `TotalUnpaidAdoptionFees`
  - `TotalDonations`, `TotalGroupAdoptionIncome`, `TotalOtherIncome`
- Expense breakdown:
  - Vet costs (Visit, Worming, De-fleeing, Dental, Spay/Neuter, Vaccinations)
  - Trip costs (Fuel, Tolls)
  - `TotalIntakeCosts`, `TotalSuppliesExpenses`
- Puppy tracking:
  - `TotalPuppies`, `PuppiesAvailable`, `PuppiesAdopted`
  - `PuppiesAvailableForAdoption`, `PuppiesAlreadyAdopted` (Observable collections)

**Observable Collections:**
- Animals, People, Trips, VetVisits, Adoptions, Expenses, Incomes, Intakes, MoneyOwed

**Key Methods:**
- `LoadDataAsync()` - Loads all data from database
- `AddAnimal(Animal)` - Adds new animal to collection and database
- `AddPerson(Person)`, `AddVetVisit(VetVisit)`, `AddAdoption(Adoption)` - Similar add methods
- Various calculation methods for financial summaries

**Dependencies:**
- `DatabaseService _dbService` - For data access

---

## 🔌 Services

### DatabaseService.cs
**Purpose:** Data access and CRUD operations
- Works with `PupTrailDbContext`
- Methods for each entity type: Get, Add, Update, Delete
- Soft delete support (doesn't permanently delete marked records)

### LicenseManager.cs
**Purpose:** License validation and activation
- `IsApplicationLicensedAsync()` - Checks if app has valid license
- `ActivateLicenseAsync(licenseKey, licenseeName, requestedMachineId, createdDate)` - Activates license and binds to detected hardware
- License format: `XXXXX-XXXXX-XXXXX-XXXXX|SIGNATURE|DATE` (legacy keys may include a machine ID segment before the date)
- Stores bound licenses in database and enforces machine checks on startup

### LicenseService.cs
**Purpose:** License key cryptographic validation
- `ValidateLicense(keyPart, signature, licenseeName, createdDate)`
- Validates signatures and tampering using the embedded RSA public key

### MachineIdGenerator.cs
**Purpose:** Generate unique machine ID for license binding
- Uses system hardware info (processor, disk serial, etc.)
- Generates consistent ID for license validation

### PathManager.cs
**Purpose:** Manage portable file paths
- All data stored within `{AppBaseDirectory}/PupTrails/` directory
- Subdirectories:
  - `data/` - Database file
  - `attachments/` - Receipts, invoices, contracts
  - `attachments/group_images/` - Puppy group photos
  - `attachments/animal_photos/` - Individual animal photos
  - `backups/` - Database backups
- Methods:
  - `PupTrailsRoot` - Root directory
  - `DataDirectory` - Database directory
  - `AttachmentsDirectory` - Attachments directory
  - `BackupsDirectory` - Backups directory
  - `GetGroupImagesDirectory()`, `GetAnimalPhotosDirectory()` - Photo directories

### BackupService.cs
**Purpose:** Automatic backup management
- Creates database backups
- Called on startup
- Handles backup retention/cleanup

### LoggingService.cs
**Purpose:** Application logging
- `LogError(message, exception)` - Log errors
- Used throughout app for exception tracking

### CsvExportService.cs
**Purpose:** Export data to CSV format

### MasterExportService.cs
**Purpose:** Export entire dataset
- Comprehensive data export functionality

### SocialMediaExportService.cs
**Purpose:** Export animal data for social media
- Used by [Views/ExportSocialMediaWindow.xaml](Views/ExportSocialMediaWindow.xaml)

### MasterResetService.cs
**Purpose:** Reset application to factory settings
- Dangerous operation - probably admin/advanced only

### ImageResizer.cs
**Purpose:** Utility for image resizing/optimization

---

## 🎨 UI Layer

### Main Window
**File:** [MainWindow.xaml.cs](MainWindow.xaml.cs) (605 lines)
- Application main window
- Navigation between sections using buttons
- Views included:
  - DashboardView, AnimalsView, PuppiesView, PeopleView
  - VetVisitsView, AdoptionsView, IntakeView
  - ExpensesView, IncomeView, MoneyOwedView, ReportsView

### Dialog Windows (in Views/ folder)

#### Animal Management
- **AddAnimalWindow** - Create/edit animal records
- **SelectPuppiesWindow** - Manage puppy groups/litters
- **ViewGroupDetailsWindow** - View group information

#### Financial
- **AddExpenseWindow** - Record expenses
- **AddIncomeWindow** - Record income
- **AddMoneyOwedWindow** - Track debts
- **AddAdoptionWindow** - Record adoptions with fees

#### Operations
- **AddVetVisitWindow** - Record vet visits with procedures
- **AddTripWindow** - Record transport trips
- **AddIntakeWindow** - Record intake events
- **AddPersonWindow** - Add adopters, vets, contacts

#### Search & Export
- **AdvancedSearchWindow** - Search animals with filters
- **ExportSocialMediaWindow** - Export for social media

### Dashboard
**File:** [DashboardPage.xaml.cs](DashboardPage.xaml.cs)
- Overview of key metrics
- Displays data from MainViewModel

### Application Entry
**File:** [App.xaml.cs](App.xaml.cs) (169 lines)
**Startup Sequence:**
1. Set up global exception handlers
2. Ensure directories exist (PathManager)
3. Check license (LicenseManager → ActivationWindow if needed)
4. Perform automatic backup (BackupService)

---

## 🔄 Application Flow

```
App Launch
    ↓
App.xaml.cs OnStartup()
    ├─ PathManager.EnsureDirectoriesExist()
    ├─ LicenseManager.IsApplicationLicensedAsync()
    │   ├─ If Not Licensed → Show ActivationWindow
    │   └─ If License Invalid → Shutdown
    ├─ BackupService.PerformAutomaticBackup()
    └─ MainWindow Loads
        ↓
    MainWindow.xaml.cs
        ├─ DataContext = new MainViewModel()
        └─ MainViewModel.LoadDataAsync() → Loads all data
```

---

## 🛢️ Database Migrations

**Latest Migration:** `20260204050000_AddMachineIdToLicense`

**Key Migrations:**
| Date | Migration | Changes |
|------|-----------|---------|
| 2026-01-30 | InitialCreate | Base schema |
| 2026-01-30 | AddMoneyOwedTable | MoneyOwed entity |
| 2026-01-30 | AddCollarColorToAnimal | CollarColor field |
| 2026-01-30 | UpdateIntakeAndIncomeModels | Model refinements |
| 2026-01-30 | AddPuppyGroupsTable | PuppyGroup entity |
| 2026-01-30 | AddSoftDeleteAndValidation | IsDeleted support |
| 2026-01-30 | AddTagsToAnimal | Tags (later removed) |
| 2026-01-31 | AddWeightToAnimal | Weight tracking |
| 2026-02-04 | AddVaccinationFieldsToVetVisit | Vaccination tracking |
| 2026-02-04 | AddVaccinationCostFields | Vaccination costs |
| 2026-02-04 | AddMachineIdField | License machine binding |
| 2026-02-04 | AddMachineIdToLicense | License machine ID |

---

## ⚙️ Project Configuration

**Framework:** .NET 8.0-Windows  
**Output:** Self-contained single-file executable  
**Runtime:** win-x64

**NuGet Dependencies:**
- `CommunityToolkit.Mvvm` v8.4.0 - MVVM support
- `DocumentFormat.OpenXml` v3.0.0 - Excel/Office export
- `Microsoft.EntityFrameworkCore.Design` v8.0.0 - EF Core tools
- `Microsoft.EntityFrameworkCore.Sqlite` v8.0.0 - SQLite provider
- `System.Drawing.Common` v8.0.0 - Image handling
- `System.Management` v8.0.0 - System info (for machine ID)

**File:** [PupTrailsV3.csproj](PupTrailsV3.csproj)

---

## 🔐 License System

**Components:**
1. **LicenseGenerator** (Separate utility project)
   - Generates license keys with signatures
   - Creates machine ID-bound licenses

2. **License Activation Flow:**
   - User enters license key in ActivationWindow
   - LicenseService validates signature
   - MachineIdGenerator checks machine ID binding
   - License stored in database
   - App checks license on startup

3. **Machine Binding:**
   - Uses system hardware identifiers
   - Prevents license sharing across machines
   - Stored in License.MachineId field

---

## 🔄 Data Access Pattern

**Architecture:** Entity Framework Core with DbContext

**Pattern:**
```csharp
// Using DatabaseService (typical usage)
using (var db = new PupTrailDbContext())
{
    var animals = db.Animals.Where(a => !a.IsDeleted).ToList();
}

// Soft Delete Support
Animal.IsDeleted = true;  // Marks as deleted without removing from DB
```

**Key Features:**
- Async support throughout
- Soft delete through `IsDeleted` flag
- Cascade delete configured for related records
- Indices on commonly queried fields

---

## 📊 Key Business Logic

### Animal Status Workflow
```
Planned → In Transport → In Care → Vet Pending → Ready → Adopted
                                               → Transferred
                                               → Deceased
```

### Adoption Process
- Record adoption with adopter (Person)
- Track agreed fee vs. paid fee
- Store contract path
- Mark as "Paid" when fee received
- Track unpaid fees in MainViewModel

### Vet Visit Tracking
- Multiple procedures per visit
- Cost tracking per procedure
- Vaccination history
- Medical procedures (worming, dental, spay/neuter)
- Ready-for-adoption flag

### Financial Tracking
- Separate Income and Expense entities
- Category-based organization
- Receipt/invoice attachment support
- Monthly profit calculations
- Unpaid adoption fees tracking

---

## 🎯 Development Notes

### Portability
- All app data stored within `{exe_directory}/PupTrails/` folder
- No system registry usage
- Can be moved to any Windows machine
- Database automatically created on first run

### Thread Safety
- Uses async/await patterns
- Entity Framework handles threading
- UI updates on dispatcher thread

### Error Handling
- Global exception handlers in App.xaml.cs
- Logging to debug output
- User-friendly error messages

### Naming Conventions
- `_fieldName` - Private fields
- `PropertyName` - Public properties
- `MethodName()` - Methods
- `.xaml` for UI, `.xaml.cs` for code-behind
- `ViewModel` suffix for ViewModels

---

## 🚀 Build & Run

**Run Task:** "Run PupTrailsV3"
```bash
dotnet run --project PupTrailsV3.csproj
```

**Publish:**
```bash
dotnet publish -c Release
```
Outputs single executable: `PupTrails.exe` (~150MB+) with all runtime included

---

## 📝 Next Development Steps

When preparing for development:

1. **Understand the domain:** Puppy/dog rescue operation tracking
2. **Data entry points:** MainWindow buttons → Dialog windows → DatabaseService
3. **Key workflows:** Animal intake → Vet care → Adoption
4. **Financial tracking:** Expenses and Income entities
5. **License system:** Not needed for local development (comment out checks)
6. **Testing:** Use dummy database in dev mode

---

## 🔗 Key File Locations

| File | Purpose | Lines |
|------|---------|-------|
| [App.xaml.cs](App.xaml.cs) | Entry point, license check, startup | 169 |
| [MainWindow.xaml.cs](MainWindow.xaml.cs) | Main UI, navigation | 605 |
| [Models/DataModels.cs](Models/DataModels.cs) | All entity definitions | 371 |
| [Data/PupTrailDbContext.cs](Data/PupTrailDbContext.cs) | Database context | 123 |
| [ViewModels/MainViewModel.cs](ViewModels/MainViewModel.cs) | Main ViewModel | 818 |
| [Services/DatabaseService.cs](Services/DatabaseService.cs) | Data access | - |
| [Services/LicenseManager.cs](Services/LicenseManager.cs) | License validation | 132 |
| [Services/PathManager.cs](Services/PathManager.cs) | Portable paths | 143 |

---

**Index Complete & Ready for Development** ✅
