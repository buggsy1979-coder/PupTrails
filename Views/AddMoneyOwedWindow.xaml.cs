using System;
using System.Linq;
using System.Windows;
using PupTrailsV3.Models;

namespace PupTrailsV3.Views
{
    public partial class AddMoneyOwedWindow : Window
    {
        public MoneyOwed? ResultMoneyOwed { get; private set; }
        private int? _moneyOwedId = null;

        public AddMoneyOwedWindow()
        {
            InitializeComponent();
            DateTextBox.Text = DateTime.Today.ToString("yyyy-MM-dd");
        }

        public void LoadMoneyOwed(MoneyOwed moneyOwed)
        {
            _moneyOwedId = moneyOwed.Id;
            DateTextBox.Text = moneyOwed.Date.ToString("yyyy-MM-dd");
            DebtorBox.Text = moneyOwed.Debtor ?? "";
            ReasonBox.Text = moneyOwed.Reason ?? "";
            AmountOwedBox.Text = moneyOwed.AmountOwed.ToString("F2");
            AmountPaidBox.Text = moneyOwed.AmountPaid.ToString("F2");
            DatePaidTextBox.Text = moneyOwed.DatePaid?.ToString("yyyy-MM-dd") ?? "";
            NotesBox.Text = moneyOwed.Notes ?? "";
            CalculateTotalOwed();
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

        private void AmountChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            CalculateTotalOwed();
        }

        private void CalculateTotalOwed()
        {
            if (decimal.TryParse(AmountOwedBox.Text, out decimal amountOwed) &&
                decimal.TryParse(AmountPaidBox.Text, out decimal amountPaid))
            {
                decimal totalOwed = amountOwed - amountPaid;
                TotalOwedBox.Text = totalOwed.ToString("F2");
                
                // Change color based on whether fully paid
                if (totalOwed <= 0)
                {
                    TotalOwedBox.Foreground = (System.Windows.Media.Brush)FindResource("AccentGreen");
                }
                else
                {
                    TotalOwedBox.Foreground = (System.Windows.Media.Brush)FindResource("AccentRed");
                }
            }
            else
            {
                TotalOwedBox.Text = "0.00";
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(DebtorBox.Text))
            {
                MessageBox.Show("Please enter a debtor name", "Validation Error");
                return;
            }

            if (!decimal.TryParse(AmountOwedBox.Text, out decimal amountOwed))
            {
                MessageBox.Show("Please enter a valid amount owed", "Validation Error");
                return;
            }

            if (!decimal.TryParse(AmountPaidBox.Text, out decimal amountPaid))
            {
                amountPaid = 0;
            }

            var selectedDate = ParseDate(DateTextBox.Text);
            if (selectedDate == null)
            {
                MessageBox.Show("Please enter a valid date (YYYY-MM-DD format)", "Validation Error");
                return;
            }

            DateTime? datePaid = null;
            if (!string.IsNullOrWhiteSpace(DatePaidTextBox.Text))
            {
                datePaid = ParseDate(DatePaidTextBox.Text);
                if (datePaid == null)
                {
                    MessageBox.Show("Please enter a valid date paid (YYYY-MM-DD format)", "Validation Error");
                    return;
                }
            }

            ResultMoneyOwed = new MoneyOwed
            {
                Id = _moneyOwedId ?? 0,
                Date = selectedDate.Value,
                Debtor = DebtorBox.Text,
                Reason = ReasonBox.Text,
                AmountOwed = amountOwed,
                AmountPaid = amountPaid,
                DatePaid = datePaid,
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
            DebtorBox.Text = string.Empty;
            ReasonBox.Text = string.Empty;
            AmountOwedBox.Text = string.Empty;
            AmountPaidBox.Text = "0.00";
            DatePaidTextBox.Text = string.Empty;
            NotesBox.Text = string.Empty;
            TotalOwedBox.Text = "0.00";
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            if (_moneyOwedId == null)
            {
                MessageBox.Show("Cannot delete a record that hasn't been saved yet.", "Delete Error");
                return;
            }

            var result = MessageBox.Show("Are you sure you want to delete this money owed record?", "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result == MessageBoxResult.Yes)
            {
                using (var db = new Data.PupTrailDbContext())
                {
                    var moneyOwed = db.MoneyOwed.Find(_moneyOwedId);
                    if (moneyOwed != null)
                    {
                        db.MoneyOwed.Remove(moneyOwed);
                        db.SaveChanges();
                        MessageBox.Show("Money owed record deleted successfully.", "Success");
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
