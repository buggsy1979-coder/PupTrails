using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using PupTrailsV3.Models;

namespace PupTrailsV3.Views
{
    public partial class AddIncomeWindow : Window
    {
        public Income? ResultIncome { get; private set; }
        private int? _incomeId = null;

        public AddIncomeWindow()
        {
            InitializeComponent();
            DateTextBox.Text = DateTime.Today.ToString("yyyy-MM-dd");
            LoadGroups();
        }

        private void LoadGroups()
        {
            using (var db = new Data.PupTrailDbContext())
            {
                var groups = db.Animals
                    .Where(a => !string.IsNullOrEmpty(a.GroupName))
                    .Select(a => a.GroupName)
                    .Distinct()
                    .OrderBy(g => g)
                    .ToList();

                GroupBox.ItemsSource = groups;
            }
        }

        private void TypeBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (TypeBox.SelectedItem is ComboBoxItem item && item.Content.ToString() == "Group Adoption")
            {
                GroupLabel.Visibility = Visibility.Visible;
                GroupBox.Visibility = Visibility.Visible;
            }
            else
            {
                GroupLabel.Visibility = Visibility.Collapsed;
                GroupBox.Visibility = Visibility.Collapsed;
            }
        }

        public void LoadIncome(Income income)
        {
            _incomeId = income.Id;
            DateTextBox.Text = income.Date.ToString("yyyy-MM-dd");
            
            // Set Type ComboBox
            if (TypeBox.Items.Count > 0)
            {
                foreach (ComboBoxItem item in TypeBox.Items)
                {
                    if (item.Content.ToString() == income.Type)
                    {
                        TypeBox.SelectedItem = item;
                        break;
                    }
                }
            }
            
            AmountBox.Text = income.Amount.ToString("F2");
            NotesBox.Text = income.Notes ?? "";
            GroupBox.Text = income.GroupName ?? "";
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
            if (TypeBox.SelectedItem == null)
            {
                MessageBox.Show("Please select an income type", "Validation Error");
                return;
            }

            if (!decimal.TryParse(AmountBox.Text, out decimal amount))
            {
                MessageBox.Show("Please enter a valid amount", "Validation Error");
                return;
            }

            var selectedDate = ParseDate(DateTextBox.Text);
            if (selectedDate == null)
            {
                MessageBox.Show("Please enter a valid date (YYYY-MM-DD format)", "Validation Error");
                return;
            }

            var type = (TypeBox.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "Other";

            ResultIncome = new Income
            {
                Id = _incomeId ?? 0,
                Date = selectedDate.Value,
                Type = type,
                Amount = amount,
                Notes = NotesBox.Text,
                GroupName = type == "Group Adoption" ? GroupBox.Text : null,
                Currency = "CAD"
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
            TypeBox.SelectedIndex = -1;
            AmountBox.Text = string.Empty;
            NotesBox.Text = string.Empty;
            GroupBox.Text = string.Empty;
            GroupLabel.Visibility = Visibility.Collapsed;
            GroupBox.Visibility = Visibility.Collapsed;
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            if (_incomeId == null)
            {
                MessageBox.Show("Cannot delete a record that hasn't been saved yet.", "Delete Error");
                return;
            }

            var result = MessageBox.Show("Are you sure you want to delete this income record?", "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result == MessageBoxResult.Yes)
            {
                using (var db = new Data.PupTrailDbContext())
                {
                    var income = db.Incomes.Find(_incomeId);
                    if (income != null)
                    {
                        db.Incomes.Remove(income);
                        db.SaveChanges();
                        MessageBox.Show("Income record deleted successfully.", "Success");
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
