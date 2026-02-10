using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using PupTrailsV3.Data;
using PupTrailsV3.Models;
using PupTrailsV3.Views;
using Microsoft.EntityFrameworkCore;

namespace PupTrailsV3.Views
{
    public partial class AdvancedSearchWindow : Window
    {
        public AdvancedSearchWindow()
        {
            InitializeComponent();
            SearchBox.Focus();
        }

        private void SearchBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Search_Click(sender, e);
            }
        }

        private void Search_Click(object sender, RoutedEventArgs e)
        {
            var searchQuery = SearchBox.Text?.Trim().ToLower();
            if (string.IsNullOrWhiteSpace(searchQuery))
            {
                MessageBox.Show("Please enter a search query.", "Search", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var results = new List<SearchResult>();

            using (var db = new PupTrailDbContext())
            {
                // Search Animals
                if (SearchAnimalsCheck.IsChecked == true)
                {
                    var animals = db.Animals
                        .Where(a => a.Name.ToLower().Contains(searchQuery) ||
                                   (a.GroupName != null && a.GroupName.ToLower().Contains(searchQuery)) ||
                                   (a.Breed != null && a.Breed.ToLower().Contains(searchQuery)))
                        .ToList();

                    foreach (var animal in animals)
                    {
                        results.Add(new SearchResult
                        {
                            Type = "Animal",
                            DisplayName = animal.Name,
                            AdditionalInfo = $"Breed: {animal.Breed ?? "Unknown"}, Status: {animal.Status}",
                            Date = animal.IntakeDate,
                            Details = $"Group: {animal.GroupName ?? "None"}",
                            EntityId = animal.Id,
                            Entity = animal
                        });
                    }
                }

                // Search People
                if (SearchPeopleCheck.IsChecked == true)
                {
                    var people = db.People
                        .Where(p => p.Name.ToLower().Contains(searchQuery) ||
                                   (p.Email != null && p.Email.ToLower().Contains(searchQuery)) ||
                                   (p.Phone != null && p.Phone.ToLower().Contains(searchQuery)))
                        .ToList();

                    foreach (var person in people)
                    {
                        results.Add(new SearchResult
                        {
                            Type = "Person",
                            DisplayName = person.Name,
                            AdditionalInfo = $"Type: {person.Type}",
                            Date = person.CreatedAt,
                            Details = $"Email: {person.Email ?? "N/A"}, Phone: {person.Phone ?? "N/A"}",
                            EntityId = person.Id,
                            Entity = person
                        });
                    }
                }

                // Search Trips
                if (SearchTripsCheck.IsChecked == true)
                {
                    var trips = db.Trips
                        .Where(t => t.Purpose.ToLower().Contains(searchQuery) ||
                                   (t.StartLocation != null && t.StartLocation.ToLower().Contains(searchQuery)) ||
                                   (t.EndLocation != null && t.EndLocation.ToLower().Contains(searchQuery)))
                        .ToList();

                    foreach (var trip in trips)
                    {
                        results.Add(new SearchResult
                        {
                            Type = "Trip",
                            DisplayName = trip.Purpose,
                            AdditionalInfo = $"From: {trip.StartLocation} To: {trip.EndLocation}",
                            Date = trip.Date,
                            Details = $"Distance: {trip.DistanceKm}km, Cost: ${trip.FuelCost:F2}",
                            EntityId = trip.Id,
                            Entity = trip
                        });
                    }
                }

                // Search Vet Visits
                if (SearchVetVisitsCheck.IsChecked == true)
                {
                    var vetVisits = db.VetVisits
                        .Include(v => v.Animal)
                        .Include(v => v.Person)
                        .Where(v => (v.Animal != null && v.Animal.Name.ToLower().Contains(searchQuery)) ||
                                   (v.Person != null && v.Person.Name.ToLower().Contains(searchQuery)) ||
                                   (v.Notes != null && v.Notes.ToLower().Contains(searchQuery)))
                        .ToList();

                    foreach (var visit in vetVisits)
                    {
                        // Build rich details string to surface extra vet visit information
                        var detailsParts = new List<string>
                        {
                            $"Cost: ${visit.TotalCost:F2}",
                            $"Ready: {visit.ReadyForAdoption}"
                        };

                        if (!string.IsNullOrWhiteSpace(visit.VaccinationsGiven))
                        {
                            detailsParts.Add($"Vaccinations: {visit.VaccinationsGiven}");
                        }

                        if (visit.RabiesShotDate.HasValue || visit.RabiesShotCost.HasValue)
                        {
                            detailsParts.Add($"Rabies: {visit.RabiesShotDate?.ToString("yyyy-MM-dd") ?? "n/a"} (${(visit.RabiesShotCost ?? 0):F2})");
                        }
                        if (visit.DistemperDate.HasValue || visit.DistemperCost.HasValue)
                        {
                            detailsParts.Add($"Distemper: {visit.DistemperDate?.ToString("yyyy-MM-dd") ?? "n/a"} (${(visit.DistemperCost ?? 0):F2})");
                        }
                        if (visit.DAPPDate.HasValue || visit.DAPPCost.HasValue)
                        {
                            detailsParts.Add($"DAPP: {visit.DAPPDate?.ToString("yyyy-MM-dd") ?? "n/a"} (${(visit.DAPPCost ?? 0):F2})");
                        }
                        if (visit.WormingDate.HasValue || visit.WormingCost.HasValue)
                        {
                            detailsParts.Add($"Worming: {visit.WormingDate?.ToString("yyyy-MM-dd") ?? "n/a"} (${(visit.WormingCost ?? 0):F2})");
                        }
                        if (visit.DeFleeingDate.HasValue || visit.DeFleeingCost.HasValue)
                        {
                            detailsParts.Add($"De-fleeing: {visit.DeFleeingDate?.ToString("yyyy-MM-dd") ?? "n/a"} (${(visit.DeFleeingCost ?? 0):F2})");
                        }
                        if (visit.DentalDate.HasValue || visit.DentalCost.HasValue)
                        {
                            detailsParts.Add($"Dental: {visit.DentalDate?.ToString("yyyy-MM-dd") ?? "n/a"} (${(visit.DentalCost ?? 0):F2})");
                        }
                        if (visit.SpayedNeuteringDate.HasValue || visit.SpayedNeuteringCost.HasValue)
                        {
                            detailsParts.Add($"Spayed/Neutering: {visit.SpayedNeuteringDate?.ToString("yyyy-MM-dd") ?? "n/a"} (${(visit.SpayedNeuteringCost ?? 0):F2})");
                        }

                        var details = string.Join("; ", detailsParts);

                        results.Add(new SearchResult
                        {
                            Type = "Vet Visit",
                            DisplayName = visit.Animal?.Name ?? "Unknown Animal",
                            AdditionalInfo = $"Vet: {visit.Person?.Name ?? "Unknown"}",
                            Date = visit.Date,
                            Details = details,
                            EntityId = visit.Id,
                            Entity = visit
                        });
                    }
                }

                // Search Adoptions
                if (SearchAdoptionsCheck.IsChecked == true)
                {
                    var adoptions = db.Adoptions
                        .Include(a => a.Animal)
                        .Include(a => a.Person)
                        .Where(a => (a.Animal != null && a.Animal.Name.ToLower().Contains(searchQuery)) ||
                                   (a.Person != null && a.Person.Name.ToLower().Contains(searchQuery)))
                        .ToList();

                    foreach (var adoption in adoptions)
                    {
                        results.Add(new SearchResult
                        {
                            Type = "Adoption",
                            DisplayName = adoption.Animal?.Name ?? "Unknown Animal",
                            AdditionalInfo = $"Adopter: {adoption.Person?.Name ?? "Unknown"}",
                            Date = adoption.Date,
                            Details = $"Fee: ${adoption.AgreedFee:F2}, Paid: {adoption.Paid}",
                            EntityId = adoption.Id,
                            Entity = adoption
                        });
                    }
                }

                // Search Intakes
                if (SearchIntakesCheck.IsChecked == true)
                {
                    var intakes = db.Intakes
                        .Where(i => (i.Location != null && i.Location.ToLower().Contains(searchQuery)) ||
                                   (i.Notes != null && i.Notes.ToLower().Contains(searchQuery)))
                        .ToList();

                    foreach (var intake in intakes)
                    {
                        results.Add(new SearchResult
                        {
                            Type = "Intake",
                            DisplayName = $"{intake.PuppyCount} Puppies",
                            AdditionalInfo = $"Location: {intake.Location ?? "Unknown"}",
                            Date = intake.Date,
                            Details = $"Cost per puppy: ${intake.CostPerPuppy:F2}, Total: ${intake.TotalCost:F2}",
                            EntityId = intake.Id,
                            Entity = intake
                        });
                    }
                }

                // Search Expenses
                if (SearchExpensesCheck.IsChecked == true)
                {
                    var expenses = db.Expenses
                        .Include(e => e.Animal)
                        .Include(e => e.Trip)
                        .Where(e => (e.Category != null && e.Category.ToLower().Contains(searchQuery)) ||
                                   (e.Notes != null && e.Notes.ToLower().Contains(searchQuery)) ||
                                   (e.Trip != null && e.Trip.Purpose != null && e.Trip.Purpose.ToLower().Contains(searchQuery)) ||
                                   (e.Animal != null && e.Animal.Name.ToLower().Contains(searchQuery)))
                        .ToList();

                    foreach (var expense in expenses)
                    {
                        results.Add(new SearchResult
                        {
                            Type = "Expense",
                            DisplayName = expense.Category,
                            AdditionalInfo = $"Amount: ${expense.Amount:F2}",
                            Date = expense.Date,
                            Details = $"Notes: {expense.Notes ?? "N/A"}",
                            EntityId = expense.Id,
                            Entity = expense
                        });
                    }
                }

                // Search Income
                if (SearchIncomeCheck.IsChecked == true)
                {
                    var incomes = db.Incomes
                        .Include(i => i.Person)
                        .Include(i => i.Animal)
                        .Where(i => (i.Type != null && i.Type.ToLower().Contains(searchQuery)) ||
                                   (i.Notes != null && i.Notes.ToLower().Contains(searchQuery)) ||
                                   (i.GroupName != null && i.GroupName.ToLower().Contains(searchQuery)) ||
                                   (i.Person != null && i.Person.Name.ToLower().Contains(searchQuery)) ||
                                   (i.Animal != null && i.Animal.Name.ToLower().Contains(searchQuery)))
                        .ToList();

                    foreach (var income in incomes)
                    {
                        results.Add(new SearchResult
                        {
                            Type = "Income",
                            DisplayName = income.Type,
                            AdditionalInfo = $"Amount: ${income.Amount:F2}",
                            Date = income.Date,
                            Details = $"From: {income.Person?.Name ?? income.GroupName ?? "N/A"}",
                            EntityId = income.Id,
                            Entity = income
                        });
                    }
                }

                // Search Puppy Groups
                if (SearchPuppyGroupsCheck.IsChecked == true)
                {
                    var groups = db.PuppyGroups
                        .Where(g => (g.GroupName != null && g.GroupName.ToLower().Contains(searchQuery)) ||
                                   (g.Notes != null && g.Notes.ToLower().Contains(searchQuery)))
                        .ToList();

                    foreach (var group in groups)
                    {
                        var createdDateText = group.DateCreated.HasValue
                            ? group.DateCreated.Value.ToString("yyyy-MM-dd")
                            : "n/a";

                        results.Add(new SearchResult
                        {
                            Type = "Puppy Group",
                            DisplayName = group.GroupName,
                            AdditionalInfo = $"Created: {createdDateText}",
                            Date = group.DateCreated ?? DateTime.MinValue,
                            Details = $"Notes: {group.Notes ?? "N/A"}",
                            EntityId = group.Id,
                            Entity = group
                        });
                    }
                }

                // Search Money Owed
                if (SearchMoneyOwedCheck.IsChecked == true)
                {
                    var moneyOwed = db.MoneyOwed
                        .Where(m => (m.Debtor != null && m.Debtor.ToLower().Contains(searchQuery)) ||
                                   (m.Reason != null && m.Reason.ToLower().Contains(searchQuery)))
                        .ToList();

                    foreach (var money in moneyOwed)
                    {
                        results.Add(new SearchResult
                        {
                            Type = "Money Owed",
                            DisplayName = money.Debtor ?? "Unknown",
                            AdditionalInfo = money.Reason ?? "No reason",
                            Date = money.Date,
                            Details = $"Owed: ${money.AmountOwed:F2}, Paid: ${money.AmountPaid:F2}, Remaining: ${money.TotalOwed:F2}",
                            EntityId = money.Id,
                            Entity = money
                        });
                    }
                }
            }

            // Update UI
            ResultsDataGrid.ItemsSource = results;
            ResultsCountText.Text = $"Found {results.Count} result(s) for \"{SearchBox.Text}\"";
        }

        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            SearchBox.Clear();
            ResultsDataGrid.ItemsSource = null;
            ResultsCountText.Text = "No search performed yet";
            SearchBox.Focus();
        }

        private void Result_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (ResultsDataGrid.SelectedItem is SearchResult result)
            {
                OpenRecord(result);
            }
        }

        private void OpenRecord(SearchResult result)
        {
            try
            {
                switch (result.Type)
                {
                    case "Animal":
                        if (result.Entity is Animal animal)
                        {
                            var animalDialog = new AddAnimalWindow { Owner = this.Owner };
                            animalDialog.LoadAnimal(animal);
                            animalDialog.ShowDialog();
                        }
                        break;

                    case "Person":
                        if (result.Entity is Person person)
                        {
                            var personDialog = new AddPersonWindow { Owner = this.Owner };
                            personDialog.LoadPerson(person);
                            personDialog.ShowDialog();
                        }
                        break;

                    case "Trip":
                        if (result.Entity is Trip trip)
                        {
                            var tripDialog = new AddTripWindow { Owner = this.Owner };
                            tripDialog.LoadTrip(trip);
                            tripDialog.ShowDialog();
                        }
                        break;

                    case "Vet Visit":
                        if (result.Entity is VetVisit vetVisit)
                        {
                            var vetDialog = new AddVetVisitWindow { Owner = this.Owner };
                            vetDialog.LoadVetVisit(vetVisit);
                            vetDialog.ShowDialog();
                        }
                        break;

                    case "Adoption":
                        if (result.Entity is Adoption adoption)
                        {
                            var adoptionDialog = new AddAdoptionWindow { Owner = this.Owner };
                            adoptionDialog.LoadAdoption(adoption);
                            adoptionDialog.ShowDialog();
                        }
                        break;

                    case "Intake":
                        if (result.Entity is Intake intake)
                        {
                            var intakeDialog = new AddIntakeWindow { Owner = this.Owner };
                            intakeDialog.LoadIntake(intake);
                            intakeDialog.ShowDialog();
                        }
                        break;

                    case "Money Owed":
                        if (result.Entity is MoneyOwed moneyOwed)
                        {
                            var moneyDialog = new AddMoneyOwedWindow { Owner = this.Owner };
                            moneyDialog.LoadMoneyOwed(moneyOwed);
                            moneyDialog.ShowDialog();
                        }
                        break;

                    case "Expense":
                        if (result.Entity is Expense expense)
                        {
                            var expenseDialog = new AddExpenseWindow { Owner = this.Owner };
                            expenseDialog.LoadExpense(expense);
                            expenseDialog.ShowDialog();
                        }
                        break;

                    case "Income":
                        if (result.Entity is Income income)
                        {
                            var incomeDialog = new AddIncomeWindow { Owner = this.Owner };
                            incomeDialog.LoadIncome(income);
                            incomeDialog.ShowDialog();
                        }
                        break;

                    case "Puppy Group":
                        if (result.Entity is PuppyGroup group)
                        {
                            var groupDialog = new ViewGroupDetailsWindow(group.GroupName) { Owner = this.Owner };
                            groupDialog.ShowDialog();
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening record: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }

    public class SearchResult
    {
        public string Type { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string AdditionalInfo { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public string Details { get; set; } = string.Empty;
        public int EntityId { get; set; }
        public object? Entity { get; set; }
    }
}
