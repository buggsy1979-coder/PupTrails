using Microsoft.EntityFrameworkCore;
using PupTrailsV3.Data;
using PupTrailsV3.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace PupTrailsV3.Services
{
    public class DatabaseService
    {
        private readonly PupTrailDbContext _context;

        public DatabaseService()
        {
            PathManager.EnsureDirectoriesExist();
            LegacyDataMigrator.EnsureLatestCopy();
            _context = new PupTrailDbContext();
            InitializeDatabase();
        }

        public void InitializeDatabase()
        {
            try
            {
                _context.Database.Migrate();
            }
            catch
            {
                _context.Database.EnsureCreated();
            }
        }

        // ANIMALS
        public async Task<List<Animal>> GetAnimalsAsync()
        {
            return await _context.Animals
                .Where(a => !a.IsDeleted)
                .Include(a => a.VetVisits)
                .Include(a => a.Adoptions)
                .OrderBy(a => a.Name)
                .ToListAsync();
        }

        public async Task<Animal?> GetAnimalAsync(int id)
        {
            return await _context.Animals
                .Include(a => a.VetVisits).ThenInclude(v => v.Services)
                .Include(a => a.Adoptions)
                .Include(a => a.Expenses)
                .Include(a => a.Incomes)
                .FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task<Animal> AddAnimalAsync(Animal animal)
        {
            try
            {
                // Check for duplicate animals by Name and DOB
                if (!string.IsNullOrWhiteSpace(animal.Name) && animal.DOB.HasValue)
                {
                var duplicate = await _context.Animals
                    .Where(a => !a.IsDeleted && a.Name == animal.Name && a.DOB == animal.DOB)
                    .FirstOrDefaultAsync();
                    
                if (duplicate != null)
                {
                    throw new InvalidOperationException($"An animal named '{animal.Name}' with the same date of birth already exists. This may be a duplicate.");
                }
            }
            
            _context.Animals.Add(animal);
            await _context.SaveChangesAsync();
            LoggingService.LogInfo($"Added animal: {animal.Name} (ID: {animal.Id})");
            return animal;
            }
            catch (Exception ex)
            {
                LoggingService.LogError($"Failed to add animal: {animal?.Name}", ex);
                throw;
            }
        }

        public async Task<Animal> UpdateAnimalAsync(Animal animal)
        {
            animal.UpdatedAt = DateTime.Now;
            
            // Detach any existing tracked entity with the same Id
            var existingEntity = _context.Animals.Local.FirstOrDefault(a => a.Id == animal.Id);
            if (existingEntity != null)
            {
                _context.Entry(existingEntity).State = Microsoft.EntityFrameworkCore.EntityState.Detached;
            }
            
            _context.Animals.Update(animal);
            await _context.SaveChangesAsync();
            return animal;
        }

        // Synchronous wrapper methods for UI operations
        public List<Animal> GetAnimals()
        {
            return _context.Animals
                .Where(a => !a.IsDeleted)
                .Include(a => a.VetVisits)
                .Include(a => a.Adoptions)
                .OrderBy(a => a.Name)
                .ToList();
        }

        public Animal? GetAnimal(int id)
        {
            return _context.Animals
                .Include(a => a.VetVisits).ThenInclude(v => v.Services)
                .Include(a => a.Adoptions)
                .Include(a => a.Expenses)
                .Include(a => a.Incomes)
                .FirstOrDefault(a => a.Id == id);
        }

        public Animal AddAnimal(Animal animal)
        {
            _context.Animals.Add(animal);
            _context.SaveChanges();
            return animal;
        }

        public Animal UpdateAnimal(Animal animal)
        {
            animal.UpdatedAt = DateTime.Now;
            
            // Detach any existing tracked entity with the same Id
            var existingEntity = _context.Animals.Local.FirstOrDefault(a => a.Id == animal.Id);
            if (existingEntity != null)
            {
                _context.Entry(existingEntity).State = Microsoft.EntityFrameworkCore.EntityState.Detached;
            }
            
            _context.Animals.Update(animal);
            _context.SaveChanges();
            return animal;
        }

        public void DeleteAnimal(int id)
        {
            var animal = _context.Animals.Find(id);
            if (animal != null)
            {
                animal.IsDeleted = true;
                animal.UpdatedAt = DateTime.Now;
                _context.SaveChanges();
            }
        }

        public async Task DeleteAnimalAsync(int id)
        {
            var animal = await _context.Animals.FindAsync(id);
            if (animal != null)
            {
                animal.IsDeleted = true;
                animal.UpdatedAt = DateTime.Now;
                await _context.SaveChangesAsync();
            }
        }

        // PEOPLE
        public async Task<List<Person>> GetPeopleAsync()
        {
            return await _context.People.Where(p => !p.IsDeleted).OrderBy(p => p.Name).ToListAsync();
        }

        public async Task<Person?> GetPersonAsync(int id)
        {
            return await _context.People
                .Include(p => p.VetVisits)
                .Include(p => p.Adoptions)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<Person> AddPersonAsync(Person person)
        {
            _context.People.Add(person);
            await _context.SaveChangesAsync();
            return person;
        }

        public async Task<Person> UpdatePersonAsync(Person person)
        {
            // Detach any existing tracked entity with the same Id
            var existingEntity = _context.People.Local.FirstOrDefault(p => p.Id == person.Id);
            if (existingEntity != null)
            {
                _context.Entry(existingEntity).State = Microsoft.EntityFrameworkCore.EntityState.Detached;
            }
            
            _context.People.Update(person);
            await _context.SaveChangesAsync();
            return person;
        }

        public async Task DeletePersonAsync(int id)
        {
            var person = await _context.People.FindAsync(id);
            if (person != null)
            {
                person.IsDeleted = true;
                await _context.SaveChangesAsync();
            }
        }

        // Synchronous People methods for UI operations
        public List<Person> GetPeople()
        {
            return _context.People.Where(p => !p.IsDeleted).OrderBy(p => p.Name).ToList();
        }

        // TRIPS
        public async Task<List<Trip>> GetTripsAsync()
        {
            return await _context.Trips
                .Where(t => !t.IsDeleted)
                .Include(t => t.TripAnimals).ThenInclude(ta => ta.Animal)
                .OrderByDescending(t => t.Date)
                .ToListAsync();
        }

        public async Task<Trip?> GetTripAsync(int id)
        {
            return await _context.Trips
                .Include(t => t.TripAnimals).ThenInclude(ta => ta.Animal)
                .Include(t => t.Expenses)
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<Trip> AddTripAsync(Trip trip)
        {
            _context.Trips.Add(trip);
            await _context.SaveChangesAsync();
            return trip;
        }

        public async Task<Trip> UpdateTripAsync(Trip trip)
        {
            // Detach any existing tracked entity with the same Id
            var existingEntity = _context.Trips.Local.FirstOrDefault(t => t.Id == trip.Id);
            if (existingEntity != null)
            {
                _context.Entry(existingEntity).State = Microsoft.EntityFrameworkCore.EntityState.Detached;
            }
            
            _context.Trips.Update(trip);
            await _context.SaveChangesAsync();
            return trip;
        }

        public async Task DeleteTripAsync(int id)
        {
            var trip = await _context.Trips.FindAsync(id);
            if (trip != null)
            {
                trip.IsDeleted = true;
                await _context.SaveChangesAsync();
            }
        }

        // VET VISITS
        public async Task<List<VetVisit>> GetVetVisitsAsync()
        {
            return await _context.VetVisits
                .Where(v => !v.IsDeleted)
                .Include(v => v.Animal)
                .Include(v => v.Person)
                .Include(v => v.Services)
                .OrderByDescending(v => v.Date)
                .ToListAsync();
        }

        public async Task<VetVisit?> GetVetVisitAsync(int id)
        {
            return await _context.VetVisits
                .Include(v => v.Animal)
                .Include(v => v.Person)
                .Include(v => v.Services)
                .FirstOrDefaultAsync(v => v.Id == id);
        }

        public async Task<List<VetVisit>> GetVetVisitsByAnimalIdAsync(int animalId)
        {
            return await _context.VetVisits
                .Where(v => v.AnimalId == animalId)
                .OrderByDescending(v => v.Date)
                .ToListAsync();
        }

        public async Task<Animal?> GetAnimalByIdAsync(int id)
        {
            return await _context.Animals.FindAsync(id);
        }

        public async Task<VetVisit> AddVetVisitAsync(VetVisit visit)
        {
            _context.VetVisits.Add(visit);
            await _context.SaveChangesAsync();
            return visit;
        }

        public async Task<VetVisit> UpdateVetVisitAsync(VetVisit visit)
        {
            // Detach any existing tracked entity with the same Id
            var existingEntity = _context.VetVisits.Local.FirstOrDefault(v => v.Id == visit.Id);
            if (existingEntity != null)
            {
                _context.Entry(existingEntity).State = Microsoft.EntityFrameworkCore.EntityState.Detached;
            }
            
            _context.VetVisits.Update(visit);
            await _context.SaveChangesAsync();
            return visit;
        }

        // ADOPTIONS
        public async Task<List<Adoption>> GetAdoptionsAsync()
        {
            return await _context.Adoptions
                .Where(a => !a.IsDeleted)
                .Include(a => a.Animal)
                .Include(a => a.Person)
                .OrderByDescending(a => a.Date)
                .ToListAsync();
        }

        public async Task<Adoption?> GetAdoptionAsync(int id)
        {
            return await _context.Adoptions
                .Include(a => a.Animal)
                .Include(a => a.Person)
                .FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task<Adoption> AddAdoptionAsync(Adoption adoption)
        {
            // Validate that IDs are set
            if (adoption.AnimalId == 0)
                throw new InvalidOperationException("No animal selected for adoption. Please select a valid animal.");
            if (adoption.PersonId == 0)
                throw new InvalidOperationException("No adopter selected. Please select a valid adopter.");
            
            // Verify the IDs exist in the database
            var animalExists = await _context.Animals.AnyAsync(a => a.Id == adoption.AnimalId);
            if (!animalExists)
                throw new InvalidOperationException($"Selected animal not found in database.");
            
            var personExists = await _context.People.AnyAsync(p => p.Id == adoption.PersonId);
            if (!personExists)
                throw new InvalidOperationException($"Selected adopter not found in database.");
            
            _context.Adoptions.Add(adoption);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                File.AppendAllText("adoption_error.log", $"{DateTime.Now}: {ex}\nInner: {ex.InnerException}\n");
                throw;
            }
            return adoption;
        }

        public async Task<Adoption> UpdateAdoptionAsync(Adoption adoption)
        {
            // Detach any existing tracked entity with the same Id
            var existingEntity = _context.Adoptions.Local.FirstOrDefault(a => a.Id == adoption.Id);
            if (existingEntity != null)
            {
                _context.Entry(existingEntity).State = Microsoft.EntityFrameworkCore.EntityState.Detached;
            }
            
            _context.Adoptions.Update(adoption);
            await _context.SaveChangesAsync();
            return adoption;
        }

        // EXPENSES
        public async Task<List<Expense>> GetExpensesAsync()
        {
            return await _context.Expenses
                .Where(e => !e.IsDeleted)
                .Include(e => e.Trip)
                .Include(e => e.Animal)
                .OrderByDescending(e => e.Date)
                .ToListAsync();
        }

        public async Task<Expense> AddExpenseAsync(Expense expense)
        {
            _context.Expenses.Add(expense);
            await _context.SaveChangesAsync();
            return expense;
        }

        public async Task<Expense> UpdateExpenseAsync(Expense expense)
        {
            // Detach any existing tracked entity with the same Id
            var existingEntity = _context.Expenses.Local.FirstOrDefault(e => e.Id == expense.Id);
            if (existingEntity != null)
            {
                _context.Entry(existingEntity).State = Microsoft.EntityFrameworkCore.EntityState.Detached;
            }
            
            _context.Expenses.Update(expense);
            await _context.SaveChangesAsync();
            return expense;
        }

        public async Task DeleteExpenseAsync(int id)
        {
            var expense = await _context.Expenses.FindAsync(id);
            if (expense != null)
            {
                expense.IsDeleted = true;
                await _context.SaveChangesAsync();
            }
        }

        // INCOME
        public async Task<List<Income>> GetIncomeAsync()
        {
            return await _context.Incomes                .Where(i => !i.IsDeleted)                .Include(i => i.Person)
                .Include(i => i.Animal)
                .OrderByDescending(i => i.Date)
                .ToListAsync();
        }

        public async Task<Income> AddIncomeAsync(Income income)
        {
            _context.Incomes.Add(income);
            await _context.SaveChangesAsync();
            return income;
        }

        public async Task<Income> UpdateIncomeAsync(Income income)
        {
            // Detach any existing tracked entity with the same Id
            var existingEntity = _context.Incomes.Local.FirstOrDefault(i => i.Id == income.Id);
            if (existingEntity != null)
            {
                _context.Entry(existingEntity).State = Microsoft.EntityFrameworkCore.EntityState.Detached;
            }
            
            _context.Incomes.Update(income);
            await _context.SaveChangesAsync();
            return income;
        }

        public async Task DeleteIncomeAsync(int id)
        {
            var income = await _context.Incomes.FindAsync(id);
            if (income != null)
            {
                income.IsDeleted = true;
                await _context.SaveChangesAsync();
            }
        }

        // REPORTS
        public async Task<(decimal TotalExpenses, decimal TotalIncome, decimal Balance)> GetMonthlyFinancialSummaryAsync(int month, int year)
        {
            // Base expenses from Expenses table
            var otherExpenses = (await _context.Expenses
                .Where(e => e.Date.Month == month && e.Date.Year == year)
                .ToListAsync())
                .Sum(e => e.Amount);

            // Include veterinary visit costs (sum of TotalCost across vet visits)
            var vetVisitExpenses = (await _context.VetVisits
                .Where(v => v.Date.Month == month && v.Date.Year == year)
                .ToListAsync())
                .Sum(v => v.TotalCost);

            var totalExpenses = otherExpenses + vetVisitExpenses;

            var income = (await _context.Incomes
                .Where(i => i.Date.Month == month && i.Date.Year == year)
                .ToListAsync())
                .Sum(i => i.Amount);

            return (totalExpenses, income, income - totalExpenses);
        }

        public async Task<decimal> GetAnimalTotalCostAsync(int animalId)
        {
            var vetCosts = (await _context.VetVisits
                .Where(v => v.AnimalId == animalId)
                .ToListAsync())
                .Sum(v => v.TotalCost);

            var expenses = (await _context.Expenses
                .Where(e => e.AnimalId == animalId)
                .ToListAsync())
                .Sum(e => e.Amount);

            return vetCosts + expenses;
        }

        public async Task<decimal> GetAnimalTotalIncomeAsync(int animalId)
        {
            return (await _context.Incomes
                .Where(i => i.AnimalId == animalId)
                .ToListAsync())
                .Sum(i => i.Amount);
        }

        // INTAKES
        public async Task<List<Intake>> GetIntakesAsync()
        {
            return await _context.Intakes
                .OrderByDescending(i => i.Date)
                .ToListAsync();
        }

        public async Task<Intake?> GetIntakeAsync(int id)
        {
            return await _context.Intakes
                .FirstOrDefaultAsync(i => i.Id == id);
        }

        public async Task<Intake> AddIntakeAsync(Intake intake)
        {
            // Calculate total cost
            intake.TotalCost = intake.PuppyCount * intake.CostPerPuppy;
            _context.Intakes.Add(intake);
            await _context.SaveChangesAsync();
            return intake;
        }

        public async Task<Intake> UpdateIntakeAsync(Intake intake)
        {
            // Calculate total cost
            intake.TotalCost = intake.PuppyCount * intake.CostPerPuppy;
            intake.UpdatedAt = DateTime.Now;
            
            // Detach any existing tracked entity with the same Id
            var existingEntity = _context.Intakes.Local.FirstOrDefault(i => i.Id == intake.Id);
            if (existingEntity != null)
            {
                _context.Entry(existingEntity).State = Microsoft.EntityFrameworkCore.EntityState.Detached;
            }
            
            _context.Intakes.Update(intake);
            await _context.SaveChangesAsync();
            return intake;
        }

        public async Task DeleteIntakeAsync(int id)
        {
            var intake = await _context.Intakes.FindAsync(id);
            if (intake != null)
            {
                _context.Intakes.Remove(intake);
                await _context.SaveChangesAsync();
            }
        }

        // Synchronous Intake methods for UI operations
        public List<Intake> GetIntakes()
        {
            return _context.Intakes
                .OrderByDescending(i => i.Date)
                .ToList();
        }

        public Intake? GetIntake(int id)
        {
            return _context.Intakes
                .FirstOrDefault(i => i.Id == id);
        }

        public Intake AddIntake(Intake intake)
        {
            // Calculate total cost
            intake.TotalCost = intake.PuppyCount * intake.CostPerPuppy;
            _context.Intakes.Add(intake);
            _context.SaveChanges();
            return intake;
        }

        public Intake UpdateIntake(Intake intake)
        {
            // Calculate total cost
            intake.TotalCost = intake.PuppyCount * intake.CostPerPuppy;
            intake.UpdatedAt = DateTime.Now;
            
            // Detach any existing tracked entity with the same Id
            var existingEntity = _context.Intakes.Local.FirstOrDefault(i => i.Id == intake.Id);
            if (existingEntity != null)
            {
                _context.Entry(existingEntity).State = Microsoft.EntityFrameworkCore.EntityState.Detached;
            }
            
            _context.Intakes.Update(intake);
            _context.SaveChanges();
            return intake;
        }

        public void DeleteIntake(int id)
        {
            var intake = _context.Intakes.Find(id);
            if (intake != null)
            {
                _context.Intakes.Remove(intake);
                _context.SaveChanges();
            }
        }

        public async Task<Dictionary<string, decimal>> GetExpensesByCategory()
        {
            // Workaround for SQLite not supporting Sum on decimal: aggregate on client side
            var expenses = await _context.Expenses.ToListAsync();
            return expenses
                .GroupBy(e => e.Category)
                .ToDictionary(
                    g => g.Key,
                    g => g.Sum(e => e.Amount)
                );
        }

        public async Task Backup(string backupPath)
        {
            try
            {
                // Portable path: database is in PupTrails/data folder
                var baseDir = AppContext.BaseDirectory;
                var dbPath = Path.Combine(baseDir, "PupTrails", "data", "PupTrail.db");
                File.Copy(dbPath, backupPath, true);
            }
            catch (Exception ex)
            {
                throw new Exception($"Backup failed: {ex.Message}");
            }
        }

        // MONEY OWED
        public async Task<List<MoneyOwed>> GetMoneyOwedAsync()
        {
            return await _context.MoneyOwed
                .OrderByDescending(m => m.Date)
                .ToListAsync();
        }

        public async Task<MoneyOwed?> GetMoneyOwedByIdAsync(int id)
        {
            return await _context.MoneyOwed
                .FirstOrDefaultAsync(m => m.Id == id);
        }

        public async Task<MoneyOwed> AddMoneyOwedAsync(MoneyOwed moneyOwed)
        {
            _context.MoneyOwed.Add(moneyOwed);
            await _context.SaveChangesAsync();
            return moneyOwed;
        }

        public async Task<MoneyOwed> UpdateMoneyOwedAsync(MoneyOwed moneyOwed)
        {
            moneyOwed.UpdatedAt = DateTime.Now;
            
            // Detach any existing tracked entity with the same Id
            var existingEntity = _context.MoneyOwed.Local.FirstOrDefault(m => m.Id == moneyOwed.Id);
            if (existingEntity != null)
            {
                _context.Entry(existingEntity).State = Microsoft.EntityFrameworkCore.EntityState.Detached;
            }
            
            _context.MoneyOwed.Update(moneyOwed);
            await _context.SaveChangesAsync();
            return moneyOwed;
        }

        public async Task DeleteMoneyOwedAsync(int id)
        {
            var moneyOwed = await _context.MoneyOwed.FindAsync(id);
            if (moneyOwed != null)
            {
                _context.MoneyOwed.Remove(moneyOwed);
                await _context.SaveChangesAsync();
            }
        }

        // Synchronous MoneyOwed methods for UI operations
        public List<MoneyOwed> GetMoneyOwed()
        {
            return _context.MoneyOwed
                .OrderByDescending(m => m.Date)
                .ToList();
        }

        public MoneyOwed? GetMoneyOwedById(int id)
        {
            return _context.MoneyOwed
                .FirstOrDefault(m => m.Id == id);
        }

        public MoneyOwed AddMoneyOwed(MoneyOwed moneyOwed)
        {
            _context.MoneyOwed.Add(moneyOwed);
            _context.SaveChanges();
            return moneyOwed;
        }

        public MoneyOwed UpdateMoneyOwed(MoneyOwed moneyOwed)
        {
            moneyOwed.UpdatedAt = DateTime.Now;
            _context.MoneyOwed.Update(moneyOwed);
            _context.SaveChanges();
            return moneyOwed;
        }

        public void DeleteMoneyOwed(int id)
        {
            var moneyOwed = _context.MoneyOwed.Find(id);
            if (moneyOwed != null)
            {
                _context.MoneyOwed.Remove(moneyOwed);
                _context.SaveChanges();
            }
        }

        // PUPPY GROUPS
        public List<PuppyGroup> GetPuppyGroups()
        {
            return _context.PuppyGroups.OrderBy(g => g.GroupName).ToList();
        }

        public PuppyGroup? GetPuppyGroup(string groupName)
        {
            return _context.PuppyGroups.FirstOrDefault(g => g.GroupName == groupName);
        }

        public PuppyGroup AddPuppyGroup(PuppyGroup group)
        {
            _context.PuppyGroups.Add(group);
            _context.SaveChanges();
            return group;
        }

        public PuppyGroup UpdatePuppyGroup(PuppyGroup group)
        {
            group.UpdatedAt = DateTime.Now;
            
            // Detach any existing tracked entity with the same Id
            var existingEntity = _context.PuppyGroups.Local.FirstOrDefault(g => g.Id == group.Id);
            if (existingEntity != null)
            {
                _context.Entry(existingEntity).State = Microsoft.EntityFrameworkCore.EntityState.Detached;
            }
            
            _context.PuppyGroups.Update(group);
            _context.SaveChanges();
            return group;
        }

        public void DeletePuppyGroup(int id)
        {
            var group = _context.PuppyGroups.Find(id);
            if (group != null)
            {
                _context.PuppyGroups.Remove(group);
                _context.SaveChanges();
            }
        }

        public void Dispose()
        {
            _context?.Dispose();
        }
    }
}
