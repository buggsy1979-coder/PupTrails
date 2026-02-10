using System;
using System.Windows;
using PupTrailsV3.Models;

namespace PupTrailsV3.Views
{
    public partial class AddTripWindow : Window
    {
        public Trip? ResultTrip { get; private set; }
        private int? _tripId = null;

        public AddTripWindow()
        {
            InitializeComponent();
            DateTextBox.Text = DateTime.Today.ToString("yyyy-MM-dd");
        }

        public void LoadTrip(Trip trip)
        {
            _tripId = trip.Id;
            DateTextBox.Text = trip.Date.ToString("yyyy-MM-dd");
            PurposeBox.Text = trip.Purpose;
            FromBox.Text = trip.StartLocation;
            ToBox.Text = trip.EndLocation;
            DistanceBox.Text = trip.DistanceKm?.ToString() ?? "";
            FuelCostBox.Text = trip.FuelCost?.ToString("F2") ?? "";
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
            if (string.IsNullOrWhiteSpace(PurposeBox.Text) || string.IsNullOrWhiteSpace(FromBox.Text) || string.IsNullOrWhiteSpace(ToBox.Text))
            {
                MessageBox.Show("Please fill in all required fields", "Validation Error");
                return;
            }

            if (!double.TryParse(DistanceBox.Text, out double distance))
            {
                MessageBox.Show("Please enter a valid distance", "Validation Error");
                return;
            }

            if (!decimal.TryParse(FuelCostBox.Text, out decimal fuelCost))
            {
                MessageBox.Show("Please enter a valid fuel cost", "Validation Error");
                return;
            }

            var selectedDate = ParseDate(DateTextBox.Text);
            if (selectedDate == null)
            {
                MessageBox.Show("Please enter a valid date (YYYY-MM-DD format)", "Validation Error");
                return;
            }

            ResultTrip = new Trip
            {
                Id = _tripId ?? 0,
                Date = selectedDate.Value,
                Purpose = PurposeBox.Text,
                StartLocation = FromBox.Text,
                EndLocation = ToBox.Text,
                DistanceKm = (decimal)distance,
                FuelCost = fuelCost
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
            PurposeBox.Text = string.Empty;
            FromBox.Text = string.Empty;
            ToBox.Text = string.Empty;
            DistanceBox.Text = string.Empty;
            FuelCostBox.Text = string.Empty;
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            if (_tripId == null)
            {
                MessageBox.Show("Cannot delete a record that hasn't been saved yet.", "Delete Error");
                return;
            }

            var result = MessageBox.Show("Are you sure you want to delete this trip record?", "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result == MessageBoxResult.Yes)
            {
                using (var db = new Data.PupTrailDbContext())
                {
                    var trip = db.Trips.Find(_tripId);
                    if (trip != null)
                    {
                        db.Trips.Remove(trip);
                        db.SaveChanges();
                        MessageBox.Show("Trip record deleted successfully.", "Success");
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
