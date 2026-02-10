using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PupTrailsV3.Models
{
    // Animals
    public class Animal
    {
        public int Id { get; set; }
        
        [Required(ErrorMessage = "Animal name is required")]
        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
        public string Name { get; set; } = string.Empty;
        
        public string? TempName { get; set; }
        
        [StringLength(100)]
        public string? Breed { get; set; }
        
        [RegularExpression(@"^(M|F|Unknown)$", ErrorMessage = "Sex must be M, F, or Unknown")]
        public string? Sex { get; set; } // M, F, Unknown
        
        [StringLength(50)]
        public string? Colour { get; set; }
        
        [StringLength(50)]
        public string? CollarColor { get; set; }
        public decimal? Weight { get; set; }
        public DateTime? DOB { get; set; }
        public DateTime IntakeDate { get; set; } = DateTime.Now;
        public string Status { get; set; } = "In Care"; // Planned, In Transport, In Care, Vet Pending, Ready, Adopted, Transferred, Deceased
        public string? OriginLocation { get; set; }
        public string OriginCountry { get; set; } = "Canada"; // Canada, USA, Other
        public string? OriginNotes { get; set; }
        public string? Microchip { get; set; }
        public string? GroupName { get; set; } // Group name for litter/sibling groups
        
        public string? Notes { get; set; }
        public string? PhotoPath { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
        public bool IsDeleted { get; set; } = false;

        // Relations
        public ICollection<TripAnimal> TripAnimals { get; set; } = new List<TripAnimal>();
        public ICollection<VetVisit> VetVisits { get; set; } = new List<VetVisit>();
        public ICollection<Adoption> Adoptions { get; set; } = new List<Adoption>();
        public ICollection<Expense> Expenses { get; set; } = new List<Expense>();
        public ICollection<Income> Incomes { get; set; } = new List<Income>();
        public ICollection<FileAttachment> Files { get; set; } = new List<FileAttachment>();

        // Override ToString for ComboBox display
        public override string ToString() => Name;
    }

    // People (Adopters, Vets, Contacts)
    public class Person
    {
        public int Id { get; set; }
        
        [Required(ErrorMessage = "Name is required")]
        [StringLength(150, ErrorMessage = "Name cannot exceed 150 characters")]
        public string Name { get; set; } = string.Empty;
        
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [StringLength(200)]
        public string? Email { get; set; }
        
        [Phone(ErrorMessage = "Invalid phone number format")]
        [StringLength(20)]
        public string? Phone { get; set; }
        
        [StringLength(500)]
        public string? Address { get; set; }
        public string Type { get; set; } = "Adopter"; // Adopter, Vet, Contact, Volunteer
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public bool IsDeleted { get; set; } = false;

        // Relations
        public ICollection<VetVisit> VetVisits { get; set; } = new List<VetVisit>();
        public ICollection<Adoption> Adoptions { get; set; } = new List<Adoption>();
        public ICollection<Income> Incomes { get; set; } = new List<Income>();

        // Override ToString for ComboBox display
        public override string ToString() => Name;
    }

    // Trips
    public class Trip
    {
        public int Id { get; set; }
        public DateTime Date { get; set; } = DateTime.Now;
        public string Purpose { get; set; } = string.Empty;
        public string? StartLocation { get; set; }
        public string? EndLocation { get; set; }
        public decimal? DistanceKm { get; set; }
        public decimal? FuelLitres { get; set; }
        public decimal? FuelCost { get; set; }
        public string? Notes { get; set; }
        public string Country { get; set; } = "Canada";
        public bool IsDeleted { get; set; } = false;

        // Relations
        public ICollection<TripAnimal> TripAnimals { get; set; } = new List<TripAnimal>();
        public ICollection<Expense> Expenses { get; set; } = new List<Expense>();
    }

    // Trip-Animal (Many-to-Many)
    public class TripAnimal
    {
        public int TripId { get; set; }
        public int AnimalId { get; set; }

        public Trip Trip { get; set; } = null!;
        public Animal Animal { get; set; } = null!;
    }

    // Vet Visits
    public class VetVisit
    {
        public int Id { get; set; }
        public int AnimalId { get; set; }
        public int? PersonId { get; set; } // Vet
        public DateTime Date { get; set; } = DateTime.Now;
        public decimal TotalCost { get; set; } = 0;
        public string? Notes { get; set; }
        public string? InvoicePath { get; set; }
        public bool ReadyForAdoption { get; set; } = false;

        // Worming
        public DateTime? WormingDate { get; set; }
        public decimal? WormingCost { get; set; }

        // De-fleying (Flea treatment)
        public DateTime? DeFleeingDate { get; set; }
        public decimal? DeFleeingCost { get; set; }

        // Dental
        public DateTime? DentalDate { get; set; }
        public decimal? DentalCost { get; set; }

        // Spayed/Neutering
        public DateTime? SpayedNeuteringDate { get; set; }
        public decimal? SpayedNeuteringCost { get; set; }

        // Vaccinations
        public string? VaccinationsGiven { get; set; } // Comma-separated list: "Rabies shot, Distemper, DAPP"
        public DateTime? RabiesShotDate { get; set; }
        public decimal? RabiesShotCost { get; set; }
        public DateTime? DistemperDate { get; set; }
        public decimal? DistemperCost { get; set; }
        public DateTime? DAPPDate { get; set; }
        public decimal? DAPPCost { get; set; }
        
        public bool IsDeleted { get; set; } = false;

        // Relations
        public Animal Animal { get; set; } = null!;
        public Person? Person { get; set; }
        public ICollection<VetService> Services { get; set; } = new List<VetService>();
    }

    // Vet Services
    public class VetService
    {
        public int Id { get; set; }
        public int VetVisitId { get; set; }
        public string ServiceName { get; set; } = string.Empty;
        public decimal Cost { get; set; }

        public VetVisit VetVisit { get; set; } = null!;
    }

    // Adoptions
    public class Adoption
    {
        public int Id { get; set; }
        
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Please select an animal")]
        public int AnimalId { get; set; }
        
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Please select an adopter")]
        public int PersonId { get; set; } // Adopter
        
        [Required]
        public DateTime Date { get; set; } = DateTime.Now;
        
        [Range(0, double.MaxValue, ErrorMessage = "Fee must be a positive number")]
        public decimal? AgreedFee { get; set; }
        
        [Range(0, double.MaxValue, ErrorMessage = "Fee must be a positive number")]
        public decimal? PaidFee { get; set; }
        public bool Paid { get; set; } = false;
        public string? ContractPath { get; set; }
        public string? Notes { get; set; }
        public bool IsDeleted { get; set; } = false;

        // Relations
        public Animal Animal { get; set; } = null!;
        public Person Person { get; set; } = null!;
    }

    // Expenses
    public class Expense
    {
        public int Id { get; set; }
        
        [Required]
        public DateTime Date { get; set; } = DateTime.Now;
        
        [Required(ErrorMessage = "Category is required")]
        [StringLength(100)]
        public string Category { get; set; } = string.Empty; // Fuel, Tolls, Supplies, Vet, Other
        
        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "CAD";
        public int? TripId { get; set; }
        public int? AnimalId { get; set; }
        public string? Notes { get; set; }
        public string? ReceiptPath { get; set; }
        public bool IsDeleted { get; set; } = false;

        // Relations
        public Trip? Trip { get; set; }
        public Animal? Animal { get; set; }
    }

    // Income
    public class Income
    {
        public int Id { get; set; }
        
        [Required]
        public DateTime Date { get; set; } = DateTime.Now;
        
        [Required(ErrorMessage = "Income type is required")]
        [StringLength(100)]
        public string Type { get; set; } = string.Empty; // Adoption Fee, Donation, Other, Group Adoption
        
        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "CAD";
        public int? PersonId { get; set; }
        public int? AnimalId { get; set; }
        public string? GroupName { get; set; } // For group adoption income
        public string? Notes { get; set; }
        public bool IsDeleted { get; set; } = false;

        // Relations
        public Person? Person { get; set; }
        public Animal? Animal { get; set; }
    }

    // File Attachments
    public class FileAttachment
    {
        public int Id { get; set; }
        public string OwnerType { get; set; } = string.Empty; // animal, adoption, vet_visit, etc.
        public int OwnerId { get; set; }
        public string Path { get; set; } = string.Empty;
        public string? Type { get; set; } // photo, pdf, invoice
        public DateTime UploadedAt { get; set; } = DateTime.Now;

        // Relations
        public Animal? Animal { get; set; }
    }

    // Intake
    public class Intake
    {
        public int Id { get; set; }
        
        [Required]
        public DateTime Date { get; set; } = DateTime.Now;
        
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Puppy count must be at least 1")]
        public int PuppyCount { get; set; }
        
        [StringLength(200)]
        public string? Location { get; set; } // Where puppies were bought from
        
        [Range(0, double.MaxValue)]
        public decimal? CostPerLitter { get; set; } // Cost for the entire litter
        
        [Range(0, double.MaxValue)]
        public decimal CostPerPuppy { get; set; } // Cost per puppy (auto-calculated or manual)
        
        [Range(0, double.MaxValue)]
        public decimal TotalCost { get; set; } // Automatically calculated: PuppyCount * CostPerPuppy
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }

    // Money Owed
    public class MoneyOwed
    {
        public int Id { get; set; }
        
        [Required]
        public DateTime Date { get; set; } = DateTime.Now;
        
        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount owed must be greater than 0")]
        public decimal AmountOwed { get; set; }
        
        [Range(0, double.MaxValue, ErrorMessage = "Amount paid cannot be negative")]
        public decimal AmountPaid { get; set; } = 0;
        public DateTime? DatePaid { get; set; }
        public decimal TotalOwed
        {
            get { return AmountOwed - AmountPaid; }
        }
        public string? Debtor { get; set; } // Name of person who owes money
        public string? Reason { get; set; } // Reason for debt (e.g., "Adoption fee", "Veterinary services", etc.)
        public string? Notes { get; set; }
        public bool IsFullyPaid
        {
            get { return AmountPaid >= AmountOwed; }
        }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }

    // Puppy Groups
    public class PuppyGroup
    {
        public int Id { get; set; }
        public string GroupName { get; set; } = string.Empty;
        public DateTime? DateCreated { get; set; }
        public string? ImagePath { get; set; } // Path to group photo
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }

    // Licensing
    public class License
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(255)]
        public string LicenseeName { get; set; } = string.Empty;
        
        [Required]
        public string LicenseKey { get; set; } = string.Empty;
        
        [Required]
        public string Signature { get; set; } = string.Empty;
        
        [Required]
        [StringLength(32)]
        public string MachineId { get; set; } = string.Empty;
        
        public DateTime ActivationDate { get; set; } = DateTime.Now;
        
        public bool IsActive { get; set; } = true;
        
        public string? Notes { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }}