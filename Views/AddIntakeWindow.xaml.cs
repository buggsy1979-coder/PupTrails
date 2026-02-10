using System;
using System.Windows;
using PupTrailsV3.Models;

namespace PupTrailsV3.Views
{
    public partial class AddIntakeWindow : Window
    {
        public Intake? ResultIntake { get; private set; }
        private int? _intakeId = null;

        public AddIntakeWindow()
        {
            InitializeComponent();
            DateTextBox.Text = DateTime.Today.ToString("yyyy-MM-dd");
        }

        public void LoadIntake(Intake intake)
        {
            _intakeId = intake.Id;
            DateTextBox.Text = intake.Date.ToString("yyyy-MM-dd");
            PuppyCountBox.Text = intake.PuppyCount.ToString();
            LocationBox.Text = intake.Location;
            CostPerLitterBox.Text = intake.CostPerLitter?.ToString("F2") ?? "";
            CostPerPuppyBox.Text = intake.CostPerPuppy.ToString("F2");
            TotalCostBox.Text = intake.TotalCost.ToString("F2");
            NotesBox.Text = intake.Notes ?? "";
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

        private void PuppyCount_Changed(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            CalculateCosts();
        }

        private void CostPerLitter_Changed(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            CalculateCosts();
        }

        private void CostPerPuppy_Changed(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            // Only used when Cost Per Litter is not filled
            CalculateCosts();
        }

        private void CalculateCosts()
        {
            if (!int.TryParse(PuppyCountBox.Text, out int puppyCount) || puppyCount == 0)
            {
                CostPerPuppyBox.Text = "0.00";
                TotalCostBox.Text = "0.00";
                return;
            }

            // Priority 1: If Cost Per Litter is filled, calculate Cost Per Puppy and Total Cost
            if (decimal.TryParse(CostPerLitterBox.Text, out decimal costPerLitter) && costPerLitter > 0)
            {
                decimal costPerPuppy = costPerLitter / puppyCount;
                CostPerPuppyBox.Text = costPerPuppy.ToString("F2");
                TotalCostBox.Text = costPerLitter.ToString("F2");
            }
            // Priority 2: If only Cost Per Puppy is filled, calculate Total Cost
            else if (decimal.TryParse(CostPerPuppyBox.Text, out decimal costPerPuppyManual) && costPerPuppyManual > 0)
            {
                decimal totalCost = puppyCount * costPerPuppyManual;
                TotalCostBox.Text = totalCost.ToString("F2");
            }
            else
            {
                CostPerPuppyBox.Text = "0.00";
                TotalCostBox.Text = "0.00";
            }
        }

        private void CalculateTotalCost()
        {
            CalculateCosts(); // Use the new unified calculation method
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(PuppyCountBox.Text, out int puppyCount))
            {
                MessageBox.Show("Please enter a valid puppy count", "Validation Error");
                return;
            }

            if (!decimal.TryParse(CostPerPuppyBox.Text, out decimal costPerPuppy))
            {
                MessageBox.Show("Please enter a valid cost per puppy", "Validation Error");
                return;
            }

            var selectedDate = ParseDate(DateTextBox.Text);
            if (selectedDate == null)
            {
                MessageBox.Show("Please enter a valid date (YYYY-MM-DD format)", "Validation Error");
                return;
            }

            decimal? costPerLitter = null;
            if (decimal.TryParse(CostPerLitterBox.Text, out decimal litter))
            {
                costPerLitter = litter;
            }

            ResultIntake = new Intake
            {
                Id = _intakeId ?? 0,
                Date = selectedDate.Value,
                PuppyCount = puppyCount,
                Location = LocationBox.Text,
                CostPerLitter = costPerLitter,
                CostPerPuppy = costPerPuppy,
                TotalCost = puppyCount * costPerPuppy,
                Notes = NotesBox.Text
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
            PuppyCountBox.Text = string.Empty;
            LocationBox.Text = string.Empty;
            CostPerLitterBox.Text = string.Empty;
            CostPerPuppyBox.Text = string.Empty;
            TotalCostBox.Text = "0.00";
            NotesBox.Text = string.Empty;
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            if (_intakeId == null)
            {
                MessageBox.Show("Cannot delete a record that hasn't been saved yet.", "Delete Error");
                return;
            }

            var result = MessageBox.Show("Are you sure you want to delete this intake record?", "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result == MessageBoxResult.Yes)
            {
                using (var db = new Data.PupTrailDbContext())
                {
                    var intake = db.Intakes.Find(_intakeId);
                    if (intake != null)
                    {
                        db.Intakes.Remove(intake);
                        db.SaveChanges();
                        MessageBox.Show("Intake record deleted successfully.", "Success");
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
