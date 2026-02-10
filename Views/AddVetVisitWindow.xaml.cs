using System;
using System.Linq;
using System.Windows;
using PupTrailsV3.Models;
using PupTrailsV3.Services;

namespace PupTrailsV3.Views
{
    public partial class AddVetVisitWindow : Window
    {
        public VetVisit? ResultVetVisit { get; private set; }
        private int? _vetVisitId = null;
        private bool _isLoadingVisit = false;

        public AddVetVisitWindow()
        {
            InitializeComponent();
            DateTextBox.Text = DateTime.Today.ToString("yyyy-MM-dd");
            LoadVeterinarianNames();
            LoadAnimalNames();
            UpdateTotalCost();
        }

        private void LoadAnimalNames()
        {
            try
            {
                using (var db = new Data.PupTrailDbContext())
                {
                    // Get all animals, not just recent ones
                    var animals = db.Animals
                        .Where(a => !a.IsDeleted)
                        .OrderBy(a => a.Name)
                        .ToList();

                    AnimalBox.ItemsSource = animals;
                }
            }
            catch (Exception)
            {
                // Silently fail if database is not accessible
            }
        }

        private void AnimalBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (_isLoadingVisit)
            {
                return;
            }

            if (AnimalBox.SelectedItem is Animal selectedAnimal)
            {
                LoadLatestVetVisitForAnimal(selectedAnimal.Id);
            }
        }

        private void LoadLatestVetVisitForAnimal(int animalId)
        {
            try
            {
                _isLoadingVisit = true;

                using (var db = new Data.PupTrailDbContext())
                {
                    var visit = db.VetVisits
                        .Where(v => v.AnimalId == animalId)
                        .OrderByDescending(v => v.Date)
                        .FirstOrDefault();

                    if (visit != null)
                    {
                        LoadVetVisit(visit);
                        LoadVaccinationHistoryForAnimal(db, animalId);
                    }
                    else
                    {
                        _vetVisitId = null;
                        ClearVaccinationFields();
                    }
                }
            }
            finally
            {
                _isLoadingVisit = false;
            }
        }

        private void LoadVaccinationHistoryForAnimal(Data.PupTrailDbContext db, int animalId)
        {
            var allVisits = db.VetVisits
                .Where(v => v.AnimalId == animalId)
                .OrderByDescending(v => v.Date)
                .ToList();

            var rabiesDate = allVisits.Select(v => v.RabiesShotDate).FirstOrDefault(d => d.HasValue);
            var rabiesCost = allVisits.Select(v => v.RabiesShotCost).FirstOrDefault(c => c.HasValue);
            var distemperDate = allVisits.Select(v => v.DistemperDate).FirstOrDefault(d => d.HasValue);
            var distemperCost = allVisits.Select(v => v.DistemperCost).FirstOrDefault(c => c.HasValue);
            var dappDate = allVisits.Select(v => v.DAPPDate).FirstOrDefault(d => d.HasValue);
            var dappCost = allVisits.Select(v => v.DAPPCost).FirstOrDefault(c => c.HasValue);

            if (string.IsNullOrWhiteSpace(RabiesShotDateTextBox.Text) && rabiesDate.HasValue)
            {
                RabiesShotDateTextBox.Text = rabiesDate.Value.ToString("yyyy-MM-dd");
            }
            if (string.IsNullOrWhiteSpace(RabiesShotCostBox.Text) && rabiesCost.HasValue)
            {
                RabiesShotCostBox.Text = rabiesCost.Value.ToString("F2");
            }

            if (string.IsNullOrWhiteSpace(DistemperDateTextBox.Text) && distemperDate.HasValue)
            {
                DistemperDateTextBox.Text = distemperDate.Value.ToString("yyyy-MM-dd");
            }
            if (string.IsNullOrWhiteSpace(DistemperCostBox.Text) && distemperCost.HasValue)
            {
                DistemperCostBox.Text = distemperCost.Value.ToString("F2");
            }

            if (string.IsNullOrWhiteSpace(DAPPDateTextBox.Text) && dappDate.HasValue)
            {
                DAPPDateTextBox.Text = dappDate.Value.ToString("yyyy-MM-dd");
            }
            if (string.IsNullOrWhiteSpace(DAPPCostBox.Text) && dappCost.HasValue)
            {
                DAPPCostBox.Text = dappCost.Value.ToString("F2");
            }

            UpdateVaccinationsGivenFromFields();
        }

        private void LoadVeterinarianNames()
        {
            try
            {
                using (var db = new Data.PupTrailDbContext())
                {
                    var people = db.People
                        .Where(p => !p.IsDeleted && (p.Type == "Vet" || p.Type == "Veterinarian"))
                        .OrderBy(p => p.Name)
                        .ToList();

                    VeterinarianBox.ItemsSource = people;
                }
            }
            catch (Exception)
            {
                // Silently fail if database is not accessible
            }
        }

        private void VaccinationType_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (VaccinationTypeBox.SelectedItem is System.Windows.Controls.ComboBoxItem selected)
            {
                var vaccinationType = selected.Content.ToString();
                if (string.IsNullOrWhiteSpace(vaccinationType))
                {
                    return;
                }

                switch (vaccinationType)
                {
                    case "Rabies shot":
                        RabiesShotDateTextBox.Focus();
                        break;
                    case "Distemper":
                        DistemperDateTextBox.Focus();
                        break;
                    case "DAPP":
                        DAPPDateTextBox.Focus();
                        break;
                }
            }
        }

        public void LoadVetVisit(VetVisit vetVisit)
        {
            _vetVisitId = vetVisit.Id;
            DateTextBox.Text = vetVisit.Date.ToString("yyyy-MM-dd");
            
            // Set selected items by ID instead of Text
            using (var db = new Data.PupTrailDbContext())
            {
                if (vetVisit.AnimalId > 0)
                {
                    var animal = db.Animals.FirstOrDefault(a => a.Id == vetVisit.AnimalId);
                    AnimalBox.SelectedItem = animal;
                }
                
                if (vetVisit.PersonId.HasValue && vetVisit.PersonId > 0)
                {
                    var person = db.People.FirstOrDefault(p => p.Id == vetVisit.PersonId);
                    VeterinarianBox.SelectedItem = person;
                }
            }
            
            CostBox.Text = vetVisit.TotalCost.ToString("F2");
            
            WormingDateTextBox.Text = vetVisit.WormingDate?.ToString("yyyy-MM-dd") ?? "";
            WormingCostBox.Text = vetVisit.WormingCost?.ToString("F2") ?? "";
            
            DeFleeingDateTextBox.Text = vetVisit.DeFleeingDate?.ToString("yyyy-MM-dd") ?? "";
            DeFleeingCostBox.Text = vetVisit.DeFleeingCost?.ToString("F2") ?? "";
            
            DentalDateTextBox.Text = vetVisit.DentalDate?.ToString("yyyy-MM-dd") ?? "";
            DentalCostBox.Text = vetVisit.DentalCost?.ToString("F2") ?? "";
            
            SpayedNeuteringDateTextBox.Text = vetVisit.SpayedNeuteringDate?.ToString("yyyy-MM-dd") ?? "";
            SpayedNeuteringCostBox.Text = vetVisit.SpayedNeuteringCost?.ToString("F2") ?? "";
            
            // Load vaccinations - only show ones that have been given (have dates or costs)
            var vaccinationsGiven = new List<string>();
            
            if (vetVisit.RabiesShotDate.HasValue || vetVisit.RabiesShotCost.HasValue)
            {
                vaccinationsGiven.Add("Rabies shot");
                RabiesShotDateTextBox.Text = vetVisit.RabiesShotDate?.ToString("yyyy-MM-dd") ?? "";
                RabiesShotCostBox.Text = vetVisit.RabiesShotCost?.ToString("F2") ?? "";
            }
            
            if (vetVisit.DistemperDate.HasValue || vetVisit.DistemperCost.HasValue)
            {
                vaccinationsGiven.Add("Distemper");
                DistemperDateTextBox.Text = vetVisit.DistemperDate?.ToString("yyyy-MM-dd") ?? "";
                DistemperCostBox.Text = vetVisit.DistemperCost?.ToString("F2") ?? "";
            }
            
            if (vetVisit.DAPPDate.HasValue || vetVisit.DAPPCost.HasValue)
            {
                vaccinationsGiven.Add("DAPP");
                DAPPDateTextBox.Text = vetVisit.DAPPDate?.ToString("yyyy-MM-dd") ?? "";
                DAPPCostBox.Text = vetVisit.DAPPCost?.ToString("F2") ?? "";
            }
            
            VaccinationsGivenTextBox.Text = vaccinationsGiven.Count > 0 ? string.Join(", ", vaccinationsGiven) : "";
            
            ReadyCheckBox.IsChecked = vetVisit.ReadyForAdoption;
            NotesTextBox.Text = vetVisit.Notes ?? "";
            UpdateTotalCost();
            UpdateVaccinationsGivenFromFields();
        }

        private void CostField_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            UpdateTotalCost();
        }

        private void VaccinationField_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            UpdateVaccinationsGivenFromFields();
            UpdateTotalCost();
        }

        private void UpdateVaccinationsGivenFromFields()
        {
            var vaccinations = new List<string>();

            if (ParseDate(RabiesShotDateTextBox.Text).HasValue || ParseCost(RabiesShotCostBox.Text) > 0)
            {
                vaccinations.Add("Rabies shot");
            }

            if (ParseDate(DistemperDateTextBox.Text).HasValue || ParseCost(DistemperCostBox.Text) > 0)
            {
                vaccinations.Add("Distemper");
            }

            if (ParseDate(DAPPDateTextBox.Text).HasValue || ParseCost(DAPPCostBox.Text) > 0)
            {
                vaccinations.Add("DAPP");
            }

            VaccinationsGivenTextBox.Text = vaccinations.Count > 0 ? string.Join(", ", vaccinations) : string.Empty;
        }

        private void ClearVaccinationFields()
        {
            VaccinationsGivenTextBox.Text = string.Empty;
            RabiesShotDateTextBox.Text = string.Empty;
            RabiesShotCostBox.Text = string.Empty;
            DistemperDateTextBox.Text = string.Empty;
            DistemperCostBox.Text = string.Empty;
            DAPPDateTextBox.Text = string.Empty;
            DAPPCostBox.Text = string.Empty;
            UpdateVaccinationsGivenFromFields();
            UpdateTotalCost();
        }

        private decimal ParseCost(string costText)
        {
            if (string.IsNullOrWhiteSpace(costText))
            {
                return 0m;
            }

            return decimal.TryParse(costText, out var value) ? value : 0m;
        }

        private decimal CalculateTotalCost()
        {
            return ParseCost(WormingCostBox.Text)
                + ParseCost(DeFleeingCostBox.Text)
                + ParseCost(DentalCostBox.Text)
                + ParseCost(SpayedNeuteringCostBox.Text)
                + ParseCost(RabiesShotCostBox.Text)
                + ParseCost(DistemperCostBox.Text)
                + ParseCost(DAPPCostBox.Text);
        }

        private void UpdateTotalCost()
        {
            var total = CalculateTotalCost();
            CostBox.Text = total.ToString("F2");
        }

        private DateTime? ParseDate(string dateText)
        {
            if (string.IsNullOrWhiteSpace(dateText))
                return null;
            
            if (DateTime.TryParse(dateText, out DateTime result))
                return result;
            
            string[] formats = { "yyyy-MM-dd", "MM/dd/yyyy", "dd/MM/yyyy", "M/d/yyyy", "d/M/yyyy" };
            foreach (var format in formats)
            {
                if (DateTime.TryParseExact(dateText.Trim(), format, null, System.Globalization.DateTimeStyles.None, out result))
                    return result;
            }
            
            return null;
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (AnimalBox.SelectedItem == null || VeterinarianBox.SelectedItem == null)
            {
                MessageBox.Show("Please select an animal and a veterinarian", "Validation Error");
                return;
            }

            var totalCost = CalculateTotalCost();

            var selectedDate = ParseDate(DateTextBox.Text);
            if (selectedDate == null)
            {
                MessageBox.Show("Please enter a valid date (YYYY-MM-dd format)", "Validation Error");
                return;
            }

            // Get the selected animal and person objects
            var selectedAnimal = (Animal)AnimalBox.SelectedItem;
            var selectedPerson = (Person)VeterinarianBox.SelectedItem;

            // Parse optional costs
            decimal? wormingCost = null;
            if (!string.IsNullOrWhiteSpace(WormingCostBox.Text) && decimal.TryParse(WormingCostBox.Text, out decimal wc))
            {
                wormingCost = wc;
            }

            decimal? defleeingCost = null;
            if (!string.IsNullOrWhiteSpace(DeFleeingCostBox.Text) && decimal.TryParse(DeFleeingCostBox.Text, out decimal dc))
            {
                defleeingCost = dc;
            }

            decimal? dentalCost = null;
            if (!string.IsNullOrWhiteSpace(DentalCostBox.Text) && decimal.TryParse(DentalCostBox.Text, out decimal dentc))
            {
                dentalCost = dentc;
            }

            decimal? spayedNeuteringCost = null;
            if (!string.IsNullOrWhiteSpace(SpayedNeuteringCostBox.Text) && decimal.TryParse(SpayedNeuteringCostBox.Text, out decimal snc))
            {
                spayedNeuteringCost = snc;
            }

            // Parse vaccination costs
            decimal? rabiesShotCost = null;
            if (!string.IsNullOrWhiteSpace(RabiesShotCostBox.Text) && decimal.TryParse(RabiesShotCostBox.Text, out decimal rsc))
            {
                rabiesShotCost = rsc;
            }

            decimal? distemperCost = null;
            if (!string.IsNullOrWhiteSpace(DistemperCostBox.Text) && decimal.TryParse(DistemperCostBox.Text, out decimal disc))
            {
                distemperCost = disc;
            }

            decimal? dappCost = null;
            if (!string.IsNullOrWhiteSpace(DAPPCostBox.Text) && decimal.TryParse(DAPPCostBox.Text, out decimal dapc))
            {
                dappCost = dapc;
            }

            ResultVetVisit = new VetVisit
            {
                Id = _vetVisitId ?? 0,
                AnimalId = selectedAnimal.Id,
                PersonId = selectedPerson.Id,
                Date = selectedDate.Value,
                TotalCost = totalCost,
                Notes = NotesTextBox.Text,
                ReadyForAdoption = ReadyCheckBox.IsChecked ?? false,
                WormingDate = ParseDate(WormingDateTextBox.Text),
                WormingCost = wormingCost,
                DeFleeingDate = ParseDate(DeFleeingDateTextBox.Text),
                DeFleeingCost = defleeingCost,
                DentalDate = ParseDate(DentalDateTextBox.Text),
                DentalCost = dentalCost,
                SpayedNeuteringDate = ParseDate(SpayedNeuteringDateTextBox.Text),
                SpayedNeuteringCost = spayedNeuteringCost,
                VaccinationsGiven = VaccinationsGivenTextBox.Text,
                RabiesShotDate = ParseDate(RabiesShotDateTextBox.Text),
                RabiesShotCost = rabiesShotCost,
                DistemperDate = ParseDate(DistemperDateTextBox.Text),
                DistemperCost = distemperCost,
                DAPPDate = ParseDate(DAPPDateTextBox.Text),
                DAPPCost = dappCost
            };

            DialogResult = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void Reset_Click(object sender, RoutedEventArgs e)
        {
            DateTextBox.Text = DateTime.Today.ToString("yyyy-MM-dd");
            AnimalBox.SelectedItem = null;
            VeterinarianBox.SelectedItem = null;
            CostBox.Text = string.Empty;
            WormingDateTextBox.Text = string.Empty;
            WormingCostBox.Text = string.Empty;
            DeFleeingDateTextBox.Text = string.Empty;
            DeFleeingCostBox.Text = string.Empty;
            DentalDateTextBox.Text = string.Empty;
            DentalCostBox.Text = string.Empty;
            SpayedNeuteringDateTextBox.Text = string.Empty;
            SpayedNeuteringCostBox.Text = string.Empty;
            VaccinationsGivenTextBox.Text = string.Empty;
            VaccinationTypeBox.SelectedIndex = -1;
            RabiesShotDateTextBox.Text = string.Empty;
            RabiesShotCostBox.Text = string.Empty;
            DistemperDateTextBox.Text = string.Empty;
            DistemperCostBox.Text = string.Empty;
            DAPPDateTextBox.Text = string.Empty;
            DAPPCostBox.Text = string.Empty;
            ReadyCheckBox.IsChecked = false;
            NotesTextBox.Text = string.Empty;
            UpdateTotalCost();
            UpdateVaccinationsGivenFromFields();
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            if (_vetVisitId == null)
            {
                MessageBox.Show("Cannot delete a record that hasn't been saved yet.", "Delete Error");
                return;
            }

            var result = MessageBox.Show("Are you sure you want to delete this vet visit record?", "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result == MessageBoxResult.Yes)
            {
                using (var db = new Data.PupTrailDbContext())
                {
                    var vetVisit = db.VetVisits.Find(_vetVisitId);
                    if (vetVisit != null)
                    {
                        db.VetVisits.Remove(vetVisit);
                        db.SaveChanges();
                        MessageBox.Show("Vet visit record deleted successfully.", "Success");
                        DialogResult = false;
                        Close();
                    }
                    else
                    {
                        MessageBox.Show("Record not found.", "Delete Error");
                    }
                }
            }
        }
    }
}
