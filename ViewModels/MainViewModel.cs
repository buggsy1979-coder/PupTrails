using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using PupTrailsV3.Models;
using PupTrailsV3.Services;

namespace PupTrailsV3.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        private readonly DatabaseService _dbService;
        private string _statusMessage = "Ready";
        private int _animalsInCare = 0;
        private int _animalsReadyForAdoption = 0;
        private int _animalsAdopted = 0;
        private decimal _monthlyProfit = 0;
        private decimal _yearlyIncome = 0;
        private decimal _yearlyExpenses = 0;
        private decimal _yearlyProfit = 0;
        private decimal _totalAdoptionAgreedFees = 0;
        private decimal _totalIncomeAdoptionFee = 0;
        private decimal _totalGrantIncome = 0;
        private decimal _totalSponsorshipIncome = 0;
        private decimal _totalSingleAdoptionIncome = 0;
        private decimal _totalFuelIncome = 0;
        private decimal _totalCleaningIncome = 0;
        private decimal _totalFoodExpenses = 0;
        private decimal _totalLaundryExpenses = 0;
        private decimal _totalVeterinaryExpenses = 0;
        private decimal _totalMoneyOwedAmount = 0;
        private decimal _totalMoneyPaid = 0;
        private int _totalIntakePuppies = 0;
        private decimal _averageIntakeCostPerPuppy = 0;
        private decimal _averageIntakeCostPerLitter = 0;
        private decimal _totalIncome = 0;
        private decimal _totalExpenses = 0;
        private decimal _totalAdoptionFees = 0;
        private decimal _totalUnpaidAdoptionFees = 0;
        private decimal _totalDonations = 0;
        private decimal _totalGroupAdoptionIncome = 0;
        private decimal _totalOtherIncome = 0;
        private decimal _totalVetCosts = 0;
        private decimal _totalVisitCost = 0;
        private decimal _totalWormingCosts = 0;
        private decimal _totalDeFleeingCosts = 0;
        private decimal _totalDentalCosts = 0;
        private decimal _totalSpayedNeuteringCosts = 0;
        private decimal _totalRabiesShotCosts = 0;
        private decimal _totalDistemperCosts = 0;
        private decimal _totalDAPPCosts = 0;
        private decimal _totalTripCosts = 0;
        private decimal _totalIntakeCosts = 0;
        private decimal _totalOtherExpenses = 0;
        private decimal _totalFuelExpenses = 0;
        private decimal _totalTollsExpenses = 0;
        private decimal _totalSuppliesExpenses = 0;
        private decimal _totalVetExpenses = 0;
        private decimal _totalOtherCategoryExpenses = 0;
        private decimal _totalMoneyOwed = 0;
        private int _totalPuppies = 0;
        private int _puppiesAvailable = 0;
        private int _puppiesAdopted = 0;
        private ObservableCollection<Animal> _puppiesAvailableForAdoption = new();
        private ObservableCollection<Animal> _puppiesAlreadyAdopted = new();
        private ObservableCollection<Animal> _animals = new();
        private ObservableCollection<Person> _people = new();
        private ObservableCollection<Trip> _trips = new();
        private ObservableCollection<VetVisit> _vetVisits = new();
        private ObservableCollection<Adoption> _adoptions = new();
        private ObservableCollection<Expense> _expenses = new();
        private ObservableCollection<Income> _incomes = new();
        private ObservableCollection<Intake> _intakes = new();
        private ObservableCollection<MoneyOwed> _moneyOwed = new();

        public DatabaseService DatabaseService => _dbService;

        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        public int AnimalsInCare
        {
            get => _animalsInCare;
            set => SetProperty(ref _animalsInCare, value);
        }

        public int AnimalsReadyForAdoption
        {
            get => _animalsReadyForAdoption;
            set => SetProperty(ref _animalsReadyForAdoption, value);
        }

        public int AnimalsAdopted
        {
            get => _animalsAdopted;
            set => SetProperty(ref _animalsAdopted, value);
        }

        public decimal MonthlyProfit
        {
            get => _monthlyProfit;
            set => SetProperty(ref _monthlyProfit, value);
        }

        public decimal YearlyIncome
        {
            get => _yearlyIncome;
            set => SetProperty(ref _yearlyIncome, value);
        }

        public decimal YearlyExpenses
        {
            get => _yearlyExpenses;
            set => SetProperty(ref _yearlyExpenses, value);
        }

        public decimal YearlyProfit
        {
            get => _yearlyProfit;
            set => SetProperty(ref _yearlyProfit, value);
        }

        public decimal TotalAdoptionAgreedFees
        {
            get => _totalAdoptionAgreedFees;
            set => SetProperty(ref _totalAdoptionAgreedFees, value);
        }

        public decimal TotalIncomeAdoptionFee
        {
            get => _totalIncomeAdoptionFee;
            set => SetProperty(ref _totalIncomeAdoptionFee, value);
        }

        public decimal TotalGrantIncome
        {
            get => _totalGrantIncome;
            set => SetProperty(ref _totalGrantIncome, value);
        }

        public decimal TotalSponsorshipIncome
        {
            get => _totalSponsorshipIncome;
            set => SetProperty(ref _totalSponsorshipIncome, value);
        }

        public decimal TotalSingleAdoptionIncome
        {
            get => _totalSingleAdoptionIncome;
            set => SetProperty(ref _totalSingleAdoptionIncome, value);
        }

        public decimal TotalFuelIncome
        {
            get => _totalFuelIncome;
            set => SetProperty(ref _totalFuelIncome, value);
        }

        public decimal TotalCleaningIncome
        {
            get => _totalCleaningIncome;
            set => SetProperty(ref _totalCleaningIncome, value);
        }

        public decimal TotalFoodExpenses
        {
            get => _totalFoodExpenses;
            set => SetProperty(ref _totalFoodExpenses, value);
        }

        public decimal TotalLaundryExpenses
        {
            get => _totalLaundryExpenses;
            set => SetProperty(ref _totalLaundryExpenses, value);
        }

        public decimal TotalVeterinaryExpenses
        {
            get => _totalVeterinaryExpenses;
            set => SetProperty(ref _totalVeterinaryExpenses, value);
        }

        public decimal TotalMoneyOwedAmount
        {
            get => _totalMoneyOwedAmount;
            set => SetProperty(ref _totalMoneyOwedAmount, value);
        }

        public decimal TotalMoneyPaid
        {
            get => _totalMoneyPaid;
            set => SetProperty(ref _totalMoneyPaid, value);
        }

        public int TotalIntakePuppies
        {
            get => _totalIntakePuppies;
            set => SetProperty(ref _totalIntakePuppies, value);
        }

        public decimal AverageIntakeCostPerPuppy
        {
            get => _averageIntakeCostPerPuppy;
            set => SetProperty(ref _averageIntakeCostPerPuppy, value);
        }

        public decimal AverageIntakeCostPerLitter
        {
            get => _averageIntakeCostPerLitter;
            set => SetProperty(ref _averageIntakeCostPerLitter, value);
        }

        public decimal TotalIncome
        {
            get => _totalIncome;
            set => SetProperty(ref _totalIncome, value);
        }

        public decimal TotalExpenses
        {
            get => _totalExpenses;
            set => SetProperty(ref _totalExpenses, value);
        }

        public decimal TotalAdoptionFees
        {
            get => _totalAdoptionFees;
            set => SetProperty(ref _totalAdoptionFees, value);
        }

        public decimal TotalUnpaidAdoptionFees
        {
            get => _totalUnpaidAdoptionFees;
            set => SetProperty(ref _totalUnpaidAdoptionFees, value);
        }

        public decimal TotalDonations
        {
            get => _totalDonations;
            set => SetProperty(ref _totalDonations, value);
        }

        public decimal TotalGroupAdoptionIncome
        {
            get => _totalGroupAdoptionIncome;
            set => SetProperty(ref _totalGroupAdoptionIncome, value);
        }

        public decimal TotalOtherIncome
        {
            get => _totalOtherIncome;
            set => SetProperty(ref _totalOtherIncome, value);
        }

        public decimal TotalVetCosts
        {
            get => _totalVetCosts;
            set => SetProperty(ref _totalVetCosts, value);
        }

        public decimal TotalVisitCost
        {
            get => _totalVisitCost;
            set => SetProperty(ref _totalVisitCost, value);
        }

        public decimal TotalWormingCosts
        {
            get => _totalWormingCosts;
            set => SetProperty(ref _totalWormingCosts, value);
        }

        public decimal TotalDeFleeingCosts
        {
            get => _totalDeFleeingCosts;
            set => SetProperty(ref _totalDeFleeingCosts, value);
        }

        public decimal TotalDentalCosts
        {
            get => _totalDentalCosts;
            set => SetProperty(ref _totalDentalCosts, value);
        }

        public decimal TotalSpayedNeuteringCosts
        {
            get => _totalSpayedNeuteringCosts;
            set => SetProperty(ref _totalSpayedNeuteringCosts, value);
        }

        public decimal TotalRabiesShotCosts
        {
            get => _totalRabiesShotCosts;
            set => SetProperty(ref _totalRabiesShotCosts, value);
        }

        public decimal TotalDistemperCosts
        {
            get => _totalDistemperCosts;
            set => SetProperty(ref _totalDistemperCosts, value);
        }

        public decimal TotalDAPPCosts
        {
            get => _totalDAPPCosts;
            set => SetProperty(ref _totalDAPPCosts, value);
        }

        public decimal TotalTripCosts
        {
            get => _totalTripCosts;
            set => SetProperty(ref _totalTripCosts, value);
        }

        public decimal TotalIntakeCosts
        {
            get => _totalIntakeCosts;
            set => SetProperty(ref _totalIntakeCosts, value);
        }

        public decimal TotalOtherExpenses
        {
            get => _totalOtherExpenses;
            set => SetProperty(ref _totalOtherExpenses, value);
        }

        public decimal TotalFuelExpenses
        {
            get => _totalFuelExpenses;
            set => SetProperty(ref _totalFuelExpenses, value);
        }

        public decimal TotalTollsExpenses
        {
            get => _totalTollsExpenses;
            set => SetProperty(ref _totalTollsExpenses, value);
        }

        public decimal TotalSuppliesExpenses
        {
            get => _totalSuppliesExpenses;
            set => SetProperty(ref _totalSuppliesExpenses, value);
        }

        public decimal TotalVetExpenses
        {
            get => _totalVetExpenses;
            set => SetProperty(ref _totalVetExpenses, value);
        }

        public decimal TotalOtherCategoryExpenses
        {
            get => _totalOtherCategoryExpenses;
            set => SetProperty(ref _totalOtherCategoryExpenses, value);
        }

        public decimal TotalMoneyOwed
        {
            get => _totalMoneyOwed;
            set => SetProperty(ref _totalMoneyOwed, value);
        }

        public int TotalPuppies
        {
            get => _totalPuppies;
            set => SetProperty(ref _totalPuppies, value);
        }

        public int PuppiesAvailable
        {
            get => _puppiesAvailable;
            set => SetProperty(ref _puppiesAvailable, value);
        }

        public int PuppiesAdopted
        {
            get => _puppiesAdopted;
            set => SetProperty(ref _puppiesAdopted, value);
        }

        public ObservableCollection<Animal> PuppiesAvailableForAdoption
        {
            get => _puppiesAvailableForAdoption;
            set => SetProperty(ref _puppiesAvailableForAdoption, value);
        }

        public ObservableCollection<Animal> PuppiesAlreadyAdopted
        {
            get => _puppiesAlreadyAdopted;
            set => SetProperty(ref _puppiesAlreadyAdopted, value);
        }

        public ObservableCollection<Animal> Animals
        {
            get => _animals;
            set => SetProperty(ref _animals, value);
        }

        public ObservableCollection<Person> People
        {
            get => _people;
            set => SetProperty(ref _people, value);
        }

        public ObservableCollection<Trip> Trips
        {
            get => _trips;
            set => SetProperty(ref _trips, value);
        }

        public ObservableCollection<VetVisit> VetVisits
        {
            get => _vetVisits;
            set => SetProperty(ref _vetVisits, value);
        }

        public ObservableCollection<Adoption> Adoptions
        {
            get => _adoptions;
            set => SetProperty(ref _adoptions, value);
        }

        public ObservableCollection<Intake> Intakes
        {
            get => _intakes;
            set => SetProperty(ref _intakes, value);
        }

        public ObservableCollection<Expense> Expenses
        {
            get => _expenses;
            set => SetProperty(ref _expenses, value);
        }

        public ObservableCollection<Income> Incomes
        {
            get => _incomes;
            set => SetProperty(ref _incomes, value);
        }

        public ObservableCollection<MoneyOwed> MoneyOwed
        {
            get => _moneyOwed;
            set => SetProperty(ref _moneyOwed, value);
        }

        public MainViewModel()
        {
            _dbService = new DatabaseService();
        }

        public async Task LoadDataAsync()
        {
            try
            {
                LoggingService.LogInfo("Starting data load");
                StatusMessage = "Loading data...";

                var animals = await _dbService.GetAnimalsAsync();
                Animals = new ObservableCollection<Animal>(animals);

                var people = await _dbService.GetPeopleAsync();
                People = new ObservableCollection<Person>(people);

                var trips = await _dbService.GetTripsAsync();
                Trips = new ObservableCollection<Trip>(trips);

                var vetVisits = await _dbService.GetVetVisitsAsync();
                VetVisits = new ObservableCollection<VetVisit>(vetVisits);

                var adoptions = await _dbService.GetAdoptionsAsync();
                Adoptions = new ObservableCollection<Adoption>(adoptions);

                var intakes = await _dbService.GetIntakesAsync();
                Intakes = new ObservableCollection<Intake>(intakes);

                var expenses = await _dbService.GetExpensesAsync();
                Expenses = new ObservableCollection<Expense>(expenses);

                var incomes = await _dbService.GetIncomeAsync();
                Incomes = new ObservableCollection<Income>(incomes);

                var moneyOwed = await _dbService.GetMoneyOwedAsync();
                MoneyOwed = new ObservableCollection<MoneyOwed>(moneyOwed);

                // Update dashboard stats
                AnimalsInCare = animals.Count(a => a.Status == "In Care");
                AnimalsReadyForAdoption = animals.Count(a => a.Status == "Ready");
                AnimalsAdopted = animals.Count(a => a.Status == "Adopted");

                var now = DateTime.Now;
                var (totalExp, totalInc, balance) = await _dbService.GetMonthlyFinancialSummaryAsync(now.Month, now.Year);
                MonthlyProfit = balance;
                TotalIncome = totalInc;
                TotalExpenses = totalExp;

                var yearlyIncome = incomes.Where(i => i.Date.Year == now.Year).Select(i => i.Amount).DefaultIfEmpty(0).Sum();
                var yearlyOtherExpenses = expenses.Where(e => e.Date.Year == now.Year).Select(e => e.Amount).DefaultIfEmpty(0).Sum();
                var yearlyVetExpenses = vetVisits.Where(v => v.Date.Year == now.Year).Select(v => v.TotalCost).DefaultIfEmpty(0).Sum();
                YearlyIncome = yearlyIncome;
                YearlyExpenses = yearlyOtherExpenses + yearlyVetExpenses;
                YearlyProfit = YearlyIncome - YearlyExpenses;

                // Calculate detailed income breakdown
                var monthlyAdoptions = adoptions.Where(a => a.Date.Month == now.Month && a.Date.Year == now.Year).ToList();
                var monthlyAdoptionFees = monthlyAdoptions.Where(a => a.PaidFee.HasValue).Select(a => a.PaidFee!.Value).DefaultIfEmpty(0).Sum();
                TotalAdoptionFees = monthlyAdoptionFees;
                TotalAdoptionAgreedFees = monthlyAdoptions.Where(a => a.AgreedFee.HasValue).Select(a => a.AgreedFee!.Value).DefaultIfEmpty(0).Sum();
                
                // Calculate unpaid adoption fees (Agreed - Paid)
                var monthlyUnpaidFees = monthlyAdoptions.Where(a => a.AgreedFee.HasValue)
                    .Select(a => (a.AgreedFee ?? 0) - (a.PaidFee ?? 0)).DefaultIfEmpty(0).Sum();
                TotalUnpaidAdoptionFees = monthlyUnpaidFees;
                
                // Break down income by type
                var monthlyIncomes = incomes.Where(i => i.Date.Month == now.Month && i.Date.Year == now.Year).ToList();
                TotalDonations = monthlyIncomes.Where(i => string.Equals(i.Type, "Donation", StringComparison.OrdinalIgnoreCase)).Select(i => i.Amount).DefaultIfEmpty(0).Sum();
                TotalGroupAdoptionIncome = monthlyIncomes.Where(i => string.Equals(i.Type, "Group Adoption", StringComparison.OrdinalIgnoreCase)).Select(i => i.Amount).DefaultIfEmpty(0).Sum();
                TotalIncomeAdoptionFee = monthlyIncomes.Where(i => string.Equals(i.Type, "Adoption Fee", StringComparison.OrdinalIgnoreCase)).Select(i => i.Amount).DefaultIfEmpty(0).Sum();
                TotalSingleAdoptionIncome = monthlyIncomes.Where(i => string.Equals(i.Type, "Single Adoption", StringComparison.OrdinalIgnoreCase)).Select(i => i.Amount).DefaultIfEmpty(0).Sum();
                TotalGrantIncome = monthlyIncomes.Where(i => string.Equals(i.Type, "Grant", StringComparison.OrdinalIgnoreCase)).Select(i => i.Amount).DefaultIfEmpty(0).Sum();
                TotalSponsorshipIncome = monthlyIncomes.Where(i => string.Equals(i.Type, "Sponsorship", StringComparison.OrdinalIgnoreCase)).Select(i => i.Amount).DefaultIfEmpty(0).Sum();
                TotalFuelIncome = monthlyIncomes.Where(i => string.Equals(i.Type, "Fuel", StringComparison.OrdinalIgnoreCase)).Select(i => i.Amount).DefaultIfEmpty(0).Sum();
                TotalCleaningIncome = monthlyIncomes.Where(i => string.Equals(i.Type, "Cleaning", StringComparison.OrdinalIgnoreCase)).Select(i => i.Amount).DefaultIfEmpty(0).Sum();
                TotalOtherIncome = monthlyIncomes.Where(i => string.Equals(i.Type, "Other", StringComparison.OrdinalIgnoreCase)).Select(i => i.Amount).DefaultIfEmpty(0).Sum();
                
                // Calculate detailed expense breakdown
                // Vet costs - main cost
                TotalVetCosts = vetVisits.Where(v => v.Date.Month == now.Month && v.Date.Year == now.Year).Select(v => v.TotalCost).DefaultIfEmpty(0).Sum();
                
                // Total Visit Cost - sum of all TotalCost fields from vet visits
                TotalVisitCost = TotalVetCosts;
                
                // Vet subcosts - individual treatments
                TotalWormingCosts = vetVisits.Where(v => v.Date.Month == now.Month && v.Date.Year == now.Year && v.WormingCost.HasValue).Select(v => v.WormingCost!.Value).DefaultIfEmpty(0).Sum();
                TotalDeFleeingCosts = vetVisits.Where(v => v.Date.Month == now.Month && v.Date.Year == now.Year && v.DeFleeingCost.HasValue).Select(v => v.DeFleeingCost!.Value).DefaultIfEmpty(0).Sum();
                TotalDentalCosts = vetVisits.Where(v => v.Date.Month == now.Month && v.Date.Year == now.Year && v.DentalCost.HasValue).Select(v => v.DentalCost!.Value).DefaultIfEmpty(0).Sum();
                TotalSpayedNeuteringCosts = vetVisits.Where(v => v.Date.Month == now.Month && v.Date.Year == now.Year && v.SpayedNeuteringCost.HasValue).Select(v => v.SpayedNeuteringCost!.Value).DefaultIfEmpty(0).Sum();
                TotalRabiesShotCosts = vetVisits.Where(v => v.Date.Month == now.Month && v.Date.Year == now.Year && v.RabiesShotCost.HasValue).Select(v => v.RabiesShotCost!.Value).DefaultIfEmpty(0).Sum();
                TotalDistemperCosts = vetVisits.Where(v => v.Date.Month == now.Month && v.Date.Year == now.Year && v.DistemperCost.HasValue).Select(v => v.DistemperCost!.Value).DefaultIfEmpty(0).Sum();
                TotalDAPPCosts = vetVisits.Where(v => v.Date.Month == now.Month && v.Date.Year == now.Year && v.DAPPCost.HasValue).Select(v => v.DAPPCost!.Value).DefaultIfEmpty(0).Sum();
                
                // Other expenses
                var monthlyTripCosts = trips.Where(t => t.Date.Month == now.Month && t.Date.Year == now.Year && t.FuelCost.HasValue).Select(t => t.FuelCost!.Value).DefaultIfEmpty(0).Sum();
                TotalTripCosts = monthlyTripCosts;
                var monthlyIntakes = intakes.Where(i => i.Date.Month == now.Month && i.Date.Year == now.Year).ToList();
                TotalIntakeCosts = monthlyIntakes.Select(i => i.TotalCost).DefaultIfEmpty(0).Sum();
                TotalIntakePuppies = monthlyIntakes.Select(i => i.PuppyCount).DefaultIfEmpty(0).Sum();
                AverageIntakeCostPerPuppy = TotalIntakePuppies > 0 ? TotalIntakeCosts / TotalIntakePuppies : 0;
                AverageIntakeCostPerLitter = monthlyIntakes.Count > 0 ? TotalIntakeCosts / monthlyIntakes.Count : 0;
                
                // Break down Expenses by category
                var monthlyExpenses = expenses.Where(e => e.Date.Month == now.Month && e.Date.Year == now.Year).ToList();
                TotalFuelExpenses = monthlyExpenses.Where(e => string.Equals(e.Category, "Fuel", StringComparison.OrdinalIgnoreCase)).Select(e => e.Amount).DefaultIfEmpty(0).Sum();
                TotalTollsExpenses = monthlyExpenses.Where(e => string.Equals(e.Category, "Tolls", StringComparison.OrdinalIgnoreCase)).Select(e => e.Amount).DefaultIfEmpty(0).Sum();
                TotalFoodExpenses = monthlyExpenses.Where(e => string.Equals(e.Category, "Food", StringComparison.OrdinalIgnoreCase)).Select(e => e.Amount).DefaultIfEmpty(0).Sum();
                TotalLaundryExpenses = monthlyExpenses.Where(e => string.Equals(e.Category, "Laundry", StringComparison.OrdinalIgnoreCase)).Select(e => e.Amount).DefaultIfEmpty(0).Sum();
                TotalSuppliesExpenses = monthlyExpenses.Where(e => string.Equals(e.Category, "Supplies", StringComparison.OrdinalIgnoreCase)).Select(e => e.Amount).DefaultIfEmpty(0).Sum();
                TotalVeterinaryExpenses = monthlyExpenses.Where(e => string.Equals(e.Category, "Vet", StringComparison.OrdinalIgnoreCase) || string.Equals(e.Category, "Veterinary", StringComparison.OrdinalIgnoreCase)).Select(e => e.Amount).DefaultIfEmpty(0).Sum();
                TotalVetExpenses = TotalVeterinaryExpenses;
                TotalOtherCategoryExpenses = monthlyExpenses.Where(e => string.Equals(e.Category, "Other", StringComparison.OrdinalIgnoreCase)).Select(e => e.Amount).DefaultIfEmpty(0).Sum();
                TotalOtherExpenses = monthlyExpenses.Select(e => e.Amount).DefaultIfEmpty(0).Sum();
                
                TotalMoneyOwedAmount = moneyOwed.Select(m => m.AmountOwed).DefaultIfEmpty(0).Sum();
                TotalMoneyPaid = moneyOwed.Select(m => m.AmountPaid).DefaultIfEmpty(0).Sum();
                TotalMoneyOwed = moneyOwed.Where(m => !m.IsFullyPaid).Select(m => m.TotalOwed).DefaultIfEmpty(0).Sum();

                // Puppies page shows ALL animals categorized by adoption status only
                var allPuppies = animals.ToList(); // All animals
                
                TotalPuppies = allPuppies.Count;
                
                var availablePuppies = allPuppies.Where(a => a.Status == "Ready").ToList();
                var adoptedPuppies = allPuppies.Where(a => a.Status == "Adopted").ToList();
                
                PuppiesAvailable = availablePuppies.Count;
                PuppiesAdopted = adoptedPuppies.Count;
                
                PuppiesAvailableForAdoption = new ObservableCollection<Animal>(availablePuppies);
                PuppiesAlreadyAdopted = new ObservableCollection<Animal>(adoptedPuppies);

                LoggingService.LogInfo($"Data loaded successfully: {animals.Count} animals, {people.Count} people");
                StatusMessage = "Ready";
            }
            catch (Exception ex)
            {
                LoggingService.LogError("Failed to load data", ex);
                StatusMessage = $"Error: {ex.Message}";
                throw;
            }
        }

        public async void AddAnimal(Animal animal)
        {
            try
            {
                StatusMessage = "Adding animal...";
                var newAnimal = await _dbService.AddAnimalAsync(animal);
                Animals.Add(newAnimal);
                await RefreshDashboardStatistics();
                StatusMessage = "Animal added successfully";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error adding animal: {ex.Message}";
            }
        }

        public async void AddPerson(Person person)
        {
            try
            {
                StatusMessage = "Adding person...";
                var newPerson = await _dbService.AddPersonAsync(person);
                People.Add(newPerson);
                StatusMessage = "Person added successfully";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error adding person: {ex.Message}";
            }
        }

        public async void UpdateAnimal(Animal animal)
        {
            try
            {
                StatusMessage = "Updating animal...";
                await _dbService.UpdateAnimalAsync(animal);
                // Update in collection
                var index = Animals.IndexOf(Animals.FirstOrDefault(a => a.Id == animal.Id)!);
                if (index >= 0)
                {
                    Animals[index] = animal;
                }
                await RefreshDashboardStatistics();
                StatusMessage = "Animal updated successfully";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error updating animal: {ex.Message}";
            }
        }

        public async void UpdatePerson(Person person)
        {
            try
            {
                StatusMessage = "Updating person...";
                await _dbService.UpdatePersonAsync(person);
                var index = People.IndexOf(People.FirstOrDefault(p => p.Id == person.Id)!);
                if (index >= 0)
                {
                    People[index] = person;
                }
                StatusMessage = "Person updated successfully";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error updating person: {ex.Message}";
            }
        }

        public async void UpdateTrip(Trip trip)
        {
            try
            {
                StatusMessage = "Updating trip...";
                await _dbService.UpdateTripAsync(trip);
                var index = Trips.IndexOf(Trips.FirstOrDefault(t => t.Id == trip.Id)!);
                if (index >= 0)
                {
                    Trips[index] = trip;
                }
                StatusMessage = "Trip updated successfully";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error updating trip: {ex.Message}";
            }
        }

        public async void UpdateVetVisit(VetVisit visit)
        {
            try
            {
                StatusMessage = "Updating vet visit...";
                await _dbService.UpdateVetVisitAsync(visit);
                var index = VetVisits.IndexOf(VetVisits.FirstOrDefault(v => v.Id == visit.Id)!);
                if (index >= 0)
                {
                    VetVisits[index] = visit;
                }
                StatusMessage = "Vet visit updated successfully";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error updating vet visit: {ex.Message}";
            }
        }

        public async void UpdateAdoption(Adoption adoption)
        {
            try
            {
                StatusMessage = "Updating adoption...";
                await _dbService.UpdateAdoptionAsync(adoption);
                var index = Adoptions.IndexOf(Adoptions.FirstOrDefault(a => a.Id == adoption.Id)!);
                if (index >= 0)
                {
                    Adoptions[index] = adoption;
                }
                StatusMessage = "Adoption updated successfully";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error updating adoption: {ex.Message}";
            }
        }

        public async void UpdateIntake(Intake intake)
        {
            try
            {
                StatusMessage = "Updating intake...";
                await _dbService.UpdateIntakeAsync(intake);
                var index = Intakes.IndexOf(Intakes.FirstOrDefault(i => i.Id == intake.Id)!);
                if (index >= 0)
                {
                    Intakes[index] = intake;
                }
                StatusMessage = "Intake updated successfully";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error updating intake: {ex.Message}";
            }
        }

        public async void UpdateExpense(Expense expense)
        {
            try
            {
                StatusMessage = "Updating expense...";
                await _dbService.UpdateExpenseAsync(expense);
                var index = Expenses.IndexOf(Expenses.FirstOrDefault(e => e.Id == expense.Id)!);
                if (index >= 0)
                {
                    Expenses[index] = expense;
                }
                StatusMessage = "Expense updated successfully";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error updating expense: {ex.Message}";
            }
        }

        public async void UpdateIncome(Income income)
        {
            try
            {
                StatusMessage = "Updating income...";
                await _dbService.UpdateIncomeAsync(income);
                var index = Incomes.IndexOf(Incomes.FirstOrDefault(i => i.Id == income.Id)!);
                if (index >= 0)
                {
                    Incomes[index] = income;
                }
                StatusMessage = "Income updated successfully";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error updating income: {ex.Message}";
            }
        }

        public async void AddTrip(Trip trip)
        {
            try
            {
                StatusMessage = "Adding trip...";
                var newTrip = await _dbService.AddTripAsync(trip);
                Trips.Insert(0, newTrip);
                StatusMessage = "Trip added successfully";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error adding trip: {ex.Message}";
            }
        }

        public async void AddVetVisit(VetVisit visit)
        {
            try
            {
                StatusMessage = "Adding vet visit...";
                var newVisit = await _dbService.AddVetVisitAsync(visit);
                VetVisits.Insert(0, newVisit);
                await RefreshDashboardStatistics();
                StatusMessage = "Vet visit added successfully";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error adding vet visit: {ex.Message}";
            }
        }

        public async void AddAdoption(Adoption adoption)
        {
            try
            {
                StatusMessage = "Adding adoption...";
                var newAdoption = await _dbService.AddAdoptionAsync(adoption);
                Adoptions.Insert(0, newAdoption);
                await RefreshDashboardStatistics();
                StatusMessage = "Adoption recorded successfully";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error recording adoption: {ex.Message}";
            }
        }

        public async void AddIntake(Intake intake)
        {
            try
            {
                StatusMessage = "Adding puppy intake...";
                var newIntake = await _dbService.AddIntakeAsync(intake);
                Intakes.Insert(0, newIntake);
                StatusMessage = "Puppy intake recorded successfully";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error recording intake: {ex.Message}";
            }
        }

        public async void AddExpense(Expense expense)
        {
            try
            {
                StatusMessage = "Adding expense...";
                var newExpense = await _dbService.AddExpenseAsync(expense);
                Expenses.Insert(0, newExpense);
                await RefreshDashboardStatistics();
                StatusMessage = "Expense added successfully";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error adding expense: {ex.Message}";
            }
        }

        public async void AddIncome(Income income)
        {
            try
            {
                StatusMessage = "Adding income...";
                var newIncome = await _dbService.AddIncomeAsync(income);
                Incomes.Insert(0, newIncome);
                await RefreshDashboardStatistics();
                StatusMessage = "Income recorded successfully";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error recording income: {ex.Message}";
            }
        }

        public async void AddMoneyOwed(MoneyOwed moneyOwed)
        {
            try
            {
                StatusMessage = "Adding money owed...";
                var newMoneyOwed = await _dbService.AddMoneyOwedAsync(moneyOwed);
                MoneyOwed.Insert(0, newMoneyOwed);
                StatusMessage = "Money owed recorded successfully";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error recording money owed: {ex.Message}";
            }
        }

        public async void UpdateMoneyOwed(MoneyOwed moneyOwed)
        {
            try
            {
                StatusMessage = "Updating money owed...";
                await _dbService.UpdateMoneyOwedAsync(moneyOwed);
                var index = MoneyOwed.IndexOf(MoneyOwed.FirstOrDefault(m => m.Id == moneyOwed.Id)!);
                if (index >= 0)
                {
                    MoneyOwed[index] = moneyOwed;
                }
                StatusMessage = "Money owed updated successfully";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error updating money owed: {ex.Message}";
            }
        }

        public async Task RefreshDashboardStatistics()
        {
            try
            {
                var animals = Animals.ToList();
                AnimalsInCare = animals.Count(a => a.Status == "In Care");
                AnimalsReadyForAdoption = animals.Count(a => a.Status == "Ready");
                AnimalsAdopted = animals.Count(a => a.Status == "Adopted");

                var now = DateTime.Now;
                var (totalExp, totalInc, balance) = await _dbService.GetMonthlyFinancialSummaryAsync(now.Month, now.Year);
                MonthlyProfit = balance;
                TotalIncome = totalInc;
                TotalExpenses = totalExp;

                var yearlyIncome = Incomes.Where(i => i.Date.Year == now.Year).Select(i => i.Amount).DefaultIfEmpty(0).Sum();
                var yearlyOtherExpenses = Expenses.Where(e => e.Date.Year == now.Year).Select(e => e.Amount).DefaultIfEmpty(0).Sum();
                var yearlyVetExpenses = VetVisits.Where(v => v.Date.Year == now.Year).Select(v => v.TotalCost).DefaultIfEmpty(0).Sum();
                YearlyIncome = yearlyIncome;
                YearlyExpenses = yearlyOtherExpenses + yearlyVetExpenses;
                YearlyProfit = YearlyIncome - YearlyExpenses;

                // Calculate detailed financial breakdown (match reports)
                var monthlyAdoptions = Adoptions.Where(a => a.Date.Month == now.Month && a.Date.Year == now.Year).ToList();
                var monthlyAdoptionFees = monthlyAdoptions.Where(a => a.PaidFee.HasValue).Select(a => a.PaidFee!.Value).DefaultIfEmpty(0).Sum();
                TotalAdoptionFees = monthlyAdoptionFees;
                TotalAdoptionAgreedFees = monthlyAdoptions.Where(a => a.AgreedFee.HasValue).Select(a => a.AgreedFee!.Value).DefaultIfEmpty(0).Sum();

                var monthlyUnpaidFees = monthlyAdoptions.Where(a => a.AgreedFee.HasValue)
                    .Select(a => (a.AgreedFee ?? 0) - (a.PaidFee ?? 0)).DefaultIfEmpty(0).Sum();
                TotalUnpaidAdoptionFees = monthlyUnpaidFees;

                var monthlyIncomes = Incomes.Where(i => i.Date.Month == now.Month && i.Date.Year == now.Year).ToList();
                TotalDonations = monthlyIncomes.Where(i => string.Equals(i.Type, "Donation", StringComparison.OrdinalIgnoreCase)).Select(i => i.Amount).DefaultIfEmpty(0).Sum();
                TotalGroupAdoptionIncome = monthlyIncomes.Where(i => string.Equals(i.Type, "Group Adoption", StringComparison.OrdinalIgnoreCase)).Select(i => i.Amount).DefaultIfEmpty(0).Sum();
                TotalIncomeAdoptionFee = monthlyIncomes.Where(i => string.Equals(i.Type, "Adoption Fee", StringComparison.OrdinalIgnoreCase)).Select(i => i.Amount).DefaultIfEmpty(0).Sum();
                TotalSingleAdoptionIncome = monthlyIncomes.Where(i => string.Equals(i.Type, "Single Adoption", StringComparison.OrdinalIgnoreCase)).Select(i => i.Amount).DefaultIfEmpty(0).Sum();
                TotalGrantIncome = monthlyIncomes.Where(i => string.Equals(i.Type, "Grant", StringComparison.OrdinalIgnoreCase)).Select(i => i.Amount).DefaultIfEmpty(0).Sum();
                TotalSponsorshipIncome = monthlyIncomes.Where(i => string.Equals(i.Type, "Sponsorship", StringComparison.OrdinalIgnoreCase)).Select(i => i.Amount).DefaultIfEmpty(0).Sum();
                TotalFuelIncome = monthlyIncomes.Where(i => string.Equals(i.Type, "Fuel", StringComparison.OrdinalIgnoreCase)).Select(i => i.Amount).DefaultIfEmpty(0).Sum();
                TotalCleaningIncome = monthlyIncomes.Where(i => string.Equals(i.Type, "Cleaning", StringComparison.OrdinalIgnoreCase)).Select(i => i.Amount).DefaultIfEmpty(0).Sum();
                TotalOtherIncome = monthlyIncomes.Where(i => string.Equals(i.Type, "Other", StringComparison.OrdinalIgnoreCase)).Select(i => i.Amount).DefaultIfEmpty(0).Sum();

                TotalVetCosts = VetVisits.Where(v => v.Date.Month == now.Month && v.Date.Year == now.Year).Select(v => v.TotalCost).DefaultIfEmpty(0).Sum();
                TotalVisitCost = TotalVetCosts;
                TotalWormingCosts = VetVisits.Where(v => v.Date.Month == now.Month && v.Date.Year == now.Year && v.WormingCost.HasValue).Select(v => v.WormingCost!.Value).DefaultIfEmpty(0).Sum();
                TotalDeFleeingCosts = VetVisits.Where(v => v.Date.Month == now.Month && v.Date.Year == now.Year && v.DeFleeingCost.HasValue).Select(v => v.DeFleeingCost!.Value).DefaultIfEmpty(0).Sum();
                TotalDentalCosts = VetVisits.Where(v => v.Date.Month == now.Month && v.Date.Year == now.Year && v.DentalCost.HasValue).Select(v => v.DentalCost!.Value).DefaultIfEmpty(0).Sum();
                TotalSpayedNeuteringCosts = VetVisits.Where(v => v.Date.Month == now.Month && v.Date.Year == now.Year && v.SpayedNeuteringCost.HasValue).Select(v => v.SpayedNeuteringCost!.Value).DefaultIfEmpty(0).Sum();
                TotalRabiesShotCosts = VetVisits.Where(v => v.Date.Month == now.Month && v.Date.Year == now.Year && v.RabiesShotCost.HasValue).Select(v => v.RabiesShotCost!.Value).DefaultIfEmpty(0).Sum();
                TotalDistemperCosts = VetVisits.Where(v => v.Date.Month == now.Month && v.Date.Year == now.Year && v.DistemperCost.HasValue).Select(v => v.DistemperCost!.Value).DefaultIfEmpty(0).Sum();
                TotalDAPPCosts = VetVisits.Where(v => v.Date.Month == now.Month && v.Date.Year == now.Year && v.DAPPCost.HasValue).Select(v => v.DAPPCost!.Value).DefaultIfEmpty(0).Sum();

                var monthlyTripCosts = Trips.Where(t => t.Date.Month == now.Month && t.Date.Year == now.Year && t.FuelCost.HasValue).Select(t => t.FuelCost!.Value).DefaultIfEmpty(0).Sum();
                TotalTripCosts = monthlyTripCosts;
                var monthlyIntakes = Intakes.Where(i => i.Date.Month == now.Month && i.Date.Year == now.Year).ToList();
                TotalIntakeCosts = monthlyIntakes.Select(i => i.TotalCost).DefaultIfEmpty(0).Sum();
                TotalIntakePuppies = monthlyIntakes.Select(i => i.PuppyCount).DefaultIfEmpty(0).Sum();
                AverageIntakeCostPerPuppy = TotalIntakePuppies > 0 ? TotalIntakeCosts / TotalIntakePuppies : 0;
                AverageIntakeCostPerLitter = monthlyIntakes.Count > 0 ? TotalIntakeCosts / monthlyIntakes.Count : 0;

                var monthlyExpenses = Expenses.Where(e => e.Date.Month == now.Month && e.Date.Year == now.Year).ToList();
                TotalFuelExpenses = monthlyExpenses.Where(e => string.Equals(e.Category, "Fuel", StringComparison.OrdinalIgnoreCase)).Select(e => e.Amount).DefaultIfEmpty(0).Sum();
                TotalTollsExpenses = monthlyExpenses.Where(e => string.Equals(e.Category, "Tolls", StringComparison.OrdinalIgnoreCase)).Select(e => e.Amount).DefaultIfEmpty(0).Sum();
                TotalFoodExpenses = monthlyExpenses.Where(e => string.Equals(e.Category, "Food", StringComparison.OrdinalIgnoreCase)).Select(e => e.Amount).DefaultIfEmpty(0).Sum();
                TotalLaundryExpenses = monthlyExpenses.Where(e => string.Equals(e.Category, "Laundry", StringComparison.OrdinalIgnoreCase)).Select(e => e.Amount).DefaultIfEmpty(0).Sum();
                TotalSuppliesExpenses = monthlyExpenses.Where(e => string.Equals(e.Category, "Supplies", StringComparison.OrdinalIgnoreCase)).Select(e => e.Amount).DefaultIfEmpty(0).Sum();
                TotalVeterinaryExpenses = monthlyExpenses.Where(e => string.Equals(e.Category, "Vet", StringComparison.OrdinalIgnoreCase) || string.Equals(e.Category, "Veterinary", StringComparison.OrdinalIgnoreCase)).Select(e => e.Amount).DefaultIfEmpty(0).Sum();
                TotalVetExpenses = TotalVeterinaryExpenses;
                TotalOtherCategoryExpenses = monthlyExpenses.Where(e => string.Equals(e.Category, "Other", StringComparison.OrdinalIgnoreCase)).Select(e => e.Amount).DefaultIfEmpty(0).Sum();
                TotalOtherExpenses = monthlyExpenses.Select(e => e.Amount).DefaultIfEmpty(0).Sum();

                TotalMoneyOwedAmount = MoneyOwed.Select(m => m.AmountOwed).DefaultIfEmpty(0).Sum();
                TotalMoneyPaid = MoneyOwed.Select(m => m.AmountPaid).DefaultIfEmpty(0).Sum();
                TotalMoneyOwed = MoneyOwed.Where(m => !m.IsFullyPaid).Select(m => m.TotalOwed).DefaultIfEmpty(0).Sum();

                // Puppies page shows ALL animals categorized by adoption status only
                var allPuppies = animals.ToList(); // All animals
                
                var availablePuppies = allPuppies.Where(a => a.Status == "Ready").ToList();
                var adoptedPuppies = allPuppies.Where(a => a.Status == "Adopted").ToList();
                
                TotalPuppies = allPuppies.Count;
                PuppiesAvailable = availablePuppies.Count;
                PuppiesAdopted = adoptedPuppies.Count;
                
                // Update puppy collections for DataGrids
                PuppiesAvailableForAdoption = new ObservableCollection<Animal>(availablePuppies);
                PuppiesAlreadyAdopted = new ObservableCollection<Animal>(adoptedPuppies);
            }
            catch (Exception ex)
            {
                LoggingService.LogError("Failed to refresh dashboard statistics", ex);
            }
        }

        public void CloseConnections()
        {
            try
            {
                _dbService?.Dispose();
            }
            catch (Exception ex)
            {
                LoggingService.LogError("Error closing database connections", ex);
            }
        }
    }
}
