using System;
using System.Linq;
using System.Windows;
using PupTrailsV3.Models;
using PupTrailsV3.Services;

namespace PupTrailsV3.Views
{
    public partial class AddAdoptionWindow : Window
    {
        public Adoption? ResultAdoption { get; private set; }
        private int? _adoptionId = null;
        private DatabaseService _dbService;

        public AddAdoptionWindow()
        {
            InitializeComponent();
            _dbService = new DatabaseService();
            DateTextBox.Text = DateTime.Today.ToString("yyyy-MM-dd");
            LoadData();
        }

        private void LoadData()
        {
            // Load animals for ComboBox
            var animals = _dbService.GetAnimals();
            AnimalBox.ItemsSource = animals;
            
            // Load adopters (people with Type = "Adopter")
            var adopters = _dbService.GetPeople().Where(p => p.Type == "Adopter").ToList();
            AdopterBox.ItemsSource = adopters;
        }

        public void LoadAdoption(Adoption adoption)
        {
            _adoptionId = adoption.Id;
            AnimalBox.SelectedItem = AnimalBox.Items.Cast<Animal>().FirstOrDefault(a => a.Id == adoption.AnimalId);
            
            // Load adopter from PersonId
            var person = _dbService.GetPeople().FirstOrDefault(p => p.Id == adoption.PersonId);
            if (person != null)
            {
                AdopterBox.SelectedItem = AdopterBox.Items.Cast<Person>().FirstOrDefault(p => p.Id == person.Id);
            }
            
            DateTextBox.Text = adoption.Date.ToString("yyyy-MM-dd");
            FeeBox.Text = adoption.AgreedFee?.ToString("F2") ?? "";
            PaidCheckBox.IsChecked = adoption.Paid;
            NotesBox.Text = adoption.Notes ?? "";
        }

        private DateTime? ParseDate(string dateText)
        {
            if (string.IsNullOrWhiteSpace(dateText))
                return null;
            
            if (DateTime.TryParse(dateText, out DateTime result))
                return result;
            
            // Try common formats
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
            if (AnimalBox.SelectedItem == null || AdopterBox.SelectedItem == null)
            {
                MessageBox.Show("Please select an animal and an adopter", "Validation Error");
                return;
            }

            if (!decimal.TryParse(FeeBox.Text, out decimal fee))
            {
                MessageBox.Show("Please enter a valid fee", "Validation Error");
                return;
            }

            var selectedDate = ParseDate(DateTextBox.Text);
            if (selectedDate == null)
            {
                MessageBox.Show("Please enter a valid date (YYYY-MM-DD format)", "Validation Error");
                return;
            }

            var selectedAnimal = (Animal)AnimalBox.SelectedItem;
            var selectedAdopter = (Person)AdopterBox.SelectedItem;

            ResultAdoption = new Adoption
            {
                Id = _adoptionId ?? 0,
                Date = selectedDate.Value,
                AgreedFee = fee,
                PaidFee = PaidCheckBox.IsChecked ?? false ? fee : 0,
                Paid = PaidCheckBox.IsChecked ?? false,
                Notes = NotesBox.Text,
                AnimalId = selectedAnimal.Id,
                PersonId = selectedAdopter.Id
            };
            try
            {
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving adoption: {ex.Message}", "Save Error");
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void Reset_Click(object sender, RoutedEventArgs e)
        {
            AnimalBox.SelectedItem = null;
            AdopterBox.SelectedItem = null;
            DateTextBox.Text = DateTime.Today.ToString("yyyy-MM-dd");
            FeeBox.Text = string.Empty;
            DepositBox.Text = string.Empty;
            DepositDateTextBox.Text = string.Empty;
            FullPaymentDateTextBox.Text = string.Empty;
            PaidCheckBox.IsChecked = false;
            NotesBox.Text = string.Empty;
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            if (_adoptionId == null)
            {
                MessageBox.Show("Cannot delete a record that hasn't been saved yet.", "Delete Error");
                return;
            }

            var result = MessageBox.Show("Are you sure you want to delete this adoption record?", "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result == MessageBoxResult.Yes)
            {
                using (var db = new Data.PupTrailDbContext())
                {
                    var adoption = db.Adoptions.Find(_adoptionId);
                    if (adoption != null)
                    {
                        db.Adoptions.Remove(adoption);
                        db.SaveChanges();
                        MessageBox.Show("Adoption record deleted successfully.", "Success");
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
