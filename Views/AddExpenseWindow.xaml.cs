using System;
using System.Windows;
using System.Windows.Controls;
using PupTrailsV3.Models;

namespace PupTrailsV3.Views
{
    public partial class AddExpenseWindow : Window
    {
        public Expense? ResultExpense { get; private set; }
        private int? _expenseId = null;

        public AddExpenseWindow()
        {
            InitializeComponent();
            DateTextBox.Text = DateTime.Today.ToString("yyyy-MM-dd");
        }

        public void LoadExpense(Expense expense)
        {
            _expenseId = expense.Id;
            DateTextBox.Text = expense.Date.ToString("yyyy-MM-dd");
            
            // Set Category ComboBox
            if (CategoryBox.Items.Count > 0)
            {
                foreach (ComboBoxItem item in CategoryBox.Items)
                {
                    if (item.Content.ToString() == expense.Category)
                    {
                        CategoryBox.SelectedItem = item;
                        break;
                    }
                }
            }
            
            AmountBox.Text = expense.Amount.ToString("F2");
            NotesBox.Text = expense.Notes;
            
            // Load trip-related fields if this expense has associated trip data
            // Parse from Notes if stored there, or leave blank
            // Note: Trip data might be in Notes field as we're merging functionality
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
            if (CategoryBox.SelectedItem == null)
            {
                MessageBox.Show("Please select a category", "Validation Error");
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

            var category = (CategoryBox.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "Other";
            
            // Build notes with trip information if provided
            var notes = NotesBox.Text ?? "";
            var tripDetails = new System.Text.StringBuilder();
            
            if (!string.IsNullOrWhiteSpace(PurposeBox.Text))
                tripDetails.AppendLine($"Purpose: {PurposeBox.Text}");
            if (!string.IsNullOrWhiteSpace(FromBox.Text))
                tripDetails.AppendLine($"From: {FromBox.Text}");
            if (!string.IsNullOrWhiteSpace(ToBox.Text))
                tripDetails.AppendLine($"To: {ToBox.Text}");
            if (!string.IsNullOrWhiteSpace(DistanceBox.Text))
                tripDetails.AppendLine($"Distance: {DistanceBox.Text} km");
            
            if (tripDetails.Length > 0)
            {
                if (!string.IsNullOrWhiteSpace(notes))
                    notes += "\n\n" + tripDetails.ToString();
                else
                    notes = tripDetails.ToString();
            }

            ResultExpense = new Expense
            {
                Id = _expenseId ?? 0,
                Date = selectedDate.Value,
                Category = category,
                Amount = amount,
                Notes = notes,
                Currency = "CAD",
                ReceiptPath = ReceiptPathBox.Text
            };

            DialogResult = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void BrowseReceipt_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog();
            dialog.Filter = "All Files|*.*|PDF Files|*.pdf|Images|*.png;*.jpg;*.jpeg";
            if (dialog.ShowDialog() == true)
            {
                ReceiptPathBox.Text = dialog.FileName;
            }
        }

        private void Reset_Click(object sender, RoutedEventArgs e)
        {
            DateTextBox.Text = DateTime.Today.ToString("yyyy-MM-dd");
            CategoryBox.SelectedIndex = -1;
            PurposeBox.Text = string.Empty;
            FromBox.Text = string.Empty;
            ToBox.Text = string.Empty;
            DistanceBox.Text = string.Empty;
            AmountBox.Text = string.Empty;
            NotesBox.Text = string.Empty;
            ReceiptPathBox.Text = string.Empty;
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            if (_expenseId == null)
            {
                MessageBox.Show("Cannot delete a record that hasn't been saved yet.", "Delete Error");
                return;
            }

            var result = MessageBox.Show("Are you sure you want to delete this expense record?", "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result == MessageBoxResult.Yes)
            {
                using (var db = new Data.PupTrailDbContext())
                {
                    var expense = db.Expenses.Find(_expenseId);
                    if (expense != null)
                    {
                        db.Expenses.Remove(expense);
                        db.SaveChanges();
                        MessageBox.Show("Expense record deleted successfully.", "Success");
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
