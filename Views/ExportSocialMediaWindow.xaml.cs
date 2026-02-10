using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using PupTrailsV3.Models;
using PupTrailsV3.Services;

namespace PupTrailsV3.Views
{
    public partial class ExportSocialMediaWindow : Window
    {
        private readonly DatabaseService _dbService;
        private readonly SocialMediaExportService _exportService;
        private readonly string _platform;
        private List<Animal> _animals = new List<Animal>();
        private Animal? _selectedAnimal;

        public ExportSocialMediaWindow(DatabaseService dbService, string platform)
        {
            InitializeComponent();
            _dbService = dbService;
            _exportService = new SocialMediaExportService(dbService);
            _platform = "Facebook"; // Always Facebook now

            // Set title
            TitleText.Text = "Social media exports";
            Title = "Social media exports";

            // Set initial preview text
            PreviewTextBox.Text = "Loading...";
            CharCountText.Text = "Character Count: 0";

            // Add loaded event handler to ensure preview updates after full initialization
            Loaded += ExportSocialMediaWindow_Loaded;

            LoadAnimals();
        }

        private async void ExportSocialMediaWindow_Loaded(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine($"Window Loaded - Platform: {_platform}, Selected Animal: {_selectedAnimal?.Name ?? "null"}");
            
            // If no animal is selected, show initial message
            if (_selectedAnimal == null)
            {
                PreviewTextBox.Text = "Please select an animal from the dropdown above.";
                CharCountText.Text = "Character Count: 0";
            }
            else
            {
                // If an animal is already selected, ensure preview updates after window is fully loaded
                await LoadPhotoPreview();
                UpdatePreview();
            }
        }

        private async void LoadAnimals()
        {
            try
            {
                _animals = await _dbService.GetAnimalsAsync();
                LoggingService.LogInfo($"ExportSocialMediaWindow: loaded {_animals.Count} animals");
                AnimalComboBox.ItemsSource = _animals;
                
                if (_animals.Any())
                {
                    AnimalComboBox.SelectedIndex = 0;
                    _selectedAnimal = AnimalComboBox.SelectedItem as Animal;
                    LoggingService.LogInfo($"ExportSocialMediaWindow: auto-selected '{_selectedAnimal?.Name}'");
                    // Ensure preview renders immediately after animals load
                    await LoadPhotoPreview();
                    UpdatePreview();
                }
                else
                {
                    PreviewTextBox.Text = "No animals found in the database. Please add an animal first.";
                    CharCountText.Text = "Character Count: 0";
                    LoggingService.LogWarning("ExportSocialMediaWindow: no animals available to preview");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading animals: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                LoggingService.LogError("ExportSocialMediaWindow: error loading animals", ex);
            }
        }

        private async void AnimalComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _selectedAnimal = AnimalComboBox.SelectedItem as Animal;
            
            System.Diagnostics.Debug.WriteLine($"=== Animal Selection Changed ===");
            System.Diagnostics.Debug.WriteLine($"Selected Animal: {_selectedAnimal?.Name ?? "null"}");
            System.Diagnostics.Debug.WriteLine($"PhotoPath: {_selectedAnimal?.PhotoPath ?? "null"}");
            System.Diagnostics.Debug.WriteLine($"IncludePhotos: {IncludePhotosCheckBox?.IsChecked}, IncludeAnimalInfo: {IncludeAnimalInfoCheckBox?.IsChecked}, IncludeVetInfo: {IncludeVetInfoCheckBox?.IsChecked}");
            
            if (_selectedAnimal != null)
            {
                PreviewTextBox.Text = "Generating preview...";
                await LoadPhotoPreview();
                UpdatePreview();
            }
            else
            {
                PreviewTextBox.Text = "Please select an animal from the dropdown above.";
                CharCountText.Text = "Character Count: 0";
            }
        }

        private async System.Threading.Tasks.Task LoadPhotoPreview()
        {
            PhotosPreviewWrapPanel.Children.Clear();

            if (_selectedAnimal == null)
            {
                PhotosPreviewPanel.Visibility = Visibility.Collapsed;
                System.Diagnostics.Debug.WriteLine("LoadPhotoPreview: No animal selected");
                return;
            }

            System.Diagnostics.Debug.WriteLine($"=== LoadPhotoPreview ===");
            System.Diagnostics.Debug.WriteLine($"Animal: {_selectedAnimal.Name}");
            System.Diagnostics.Debug.WriteLine($"PhotoPath: {_selectedAnimal.PhotoPath ?? "NULL"}");
            System.Diagnostics.Debug.WriteLine($"PhotoPath IsNullOrEmpty: {string.IsNullOrEmpty(_selectedAnimal.PhotoPath)}");
            LoggingService.LogInfo($"ExportSocialMediaWindow: LoadPhotoPreview for '{_selectedAnimal.Name}', photo='{_selectedAnimal.PhotoPath ?? "NULL"}'");

            // Always show photo preview if photo exists, regardless of checkbox state
            if (!string.IsNullOrEmpty(_selectedAnimal.PhotoPath))
            {
                System.Diagnostics.Debug.WriteLine($"PhotoPath is not empty: '{_selectedAnimal.PhotoPath}'");
                
                // Resolve full photo path regardless of storage format
                string fullPhotoPath = PathManager.ResolveAnimalPhotoPath(_selectedAnimal.PhotoPath);
                System.Diagnostics.Debug.WriteLine($"Full photo path: '{fullPhotoPath}'");
                LoggingService.LogInfo($"ExportSocialMediaWindow: resolved photo path '{fullPhotoPath}'");
                
                var fileExists = File.Exists(fullPhotoPath);
                System.Diagnostics.Debug.WriteLine($"File.Exists: {fileExists}");
                System.Diagnostics.Debug.WriteLine($"Current working directory: {Directory.GetCurrentDirectory()}");
                LoggingService.LogInfo($"ExportSocialMediaWindow: photo exists={fileExists}");
                
                if (fileExists)
                {
                    System.Diagnostics.Debug.WriteLine($"Photo file exists at: {fullPhotoPath}");
                    try
                    {
                        var fileInfo = new System.IO.FileInfo(fullPhotoPath);
                        System.Diagnostics.Debug.WriteLine($"File size: {fileInfo.Length} bytes");
                        System.Diagnostics.Debug.WriteLine($"Full path: {fileInfo.FullName}");
                        
                        var image = new Image
                        {
                            Width = 150,
                            Height = 150,
                            Margin = new Thickness(0, 0, 8, 8),
                            Stretch = System.Windows.Media.Stretch.UniformToFill
                        };

                        var bitmap = new BitmapImage();
                        bitmap.BeginInit();
                        bitmap.UriSource = new Uri(fullPhotoPath, UriKind.Absolute);
                        bitmap.DecodePixelWidth = 150;
                        bitmap.CacheOption = BitmapCacheOption.OnLoad;
                        bitmap.EndInit();
                        
                        image.Source = bitmap;
                        PhotosPreviewWrapPanel.Children.Add(image);
                        
                        PhotosPreviewPanel.Visibility = Visibility.Visible;
                        
                        System.Diagnostics.Debug.WriteLine($"✓ Photo preview loaded successfully!");
                        LoggingService.LogInfo("ExportSocialMediaWindow: photo preview loaded successfully");
                    }
                    catch (Exception ex)
                    {
                        PhotosPreviewPanel.Visibility = Visibility.Collapsed;
                        System.Diagnostics.Debug.WriteLine($"✗ Error loading photo preview: {ex.Message}");
                        System.Diagnostics.Debug.WriteLine($"✗ Exception type: {ex.GetType().Name}");
                        System.Diagnostics.Debug.WriteLine($"✗ Stack Trace: {ex.StackTrace}");
                        LoggingService.LogError("ExportSocialMediaWindow: error loading photo preview", ex);
                    }
                }
                else
                {
                    PhotosPreviewPanel.Visibility = Visibility.Collapsed;
                    System.Diagnostics.Debug.WriteLine($"✗ Photo file DOES NOT exist at: {fullPhotoPath}");
                    LoggingService.LogWarning($"ExportSocialMediaWindow: photo file not found at '{fullPhotoPath}'");
                }
            }
            else
            {
                PhotosPreviewPanel.Visibility = Visibility.Collapsed;
                System.Diagnostics.Debug.WriteLine($"✗ PhotoPath is empty/null for {_selectedAnimal.Name}");
                LoggingService.LogWarning($"ExportSocialMediaWindow: empty photo path for '{_selectedAnimal.Name}'");
            }
        }

        private async void UpdatePreview()
        {
            System.Diagnostics.Debug.WriteLine($"UpdatePreview called - Animal: {_selectedAnimal?.Name ?? "null"}");
            
            if (_selectedAnimal == null)
            {
                PreviewTextBox.Text = "Please select an animal to see the preview.";
                CharCountText.Text = "Character Count: 0";
                System.Diagnostics.Debug.WriteLine("UpdatePreview: No animal selected");
                return;
            }

            try
            {
                System.Diagnostics.Debug.WriteLine($"Generating preview for {_selectedAnimal.Name} on {_platform}");
                
                var options = new ExportOptions
                {
                    IncludePhotos = IncludePhotosCheckBox?.IsChecked.GetValueOrDefault() ?? false,
                    
                    // Animal Info
                    IncludeName = (IncludeAnimalInfoCheckBox?.IsChecked.GetValueOrDefault() ?? false) && (IncludeNameCheckBox?.IsChecked.GetValueOrDefault() ?? false),
                    IncludeBreed = (IncludeAnimalInfoCheckBox?.IsChecked.GetValueOrDefault() ?? false) && (IncludeBreedCheckBox?.IsChecked.GetValueOrDefault() ?? false),
                    IncludeSex = (IncludeAnimalInfoCheckBox?.IsChecked.GetValueOrDefault() ?? false) && (IncludeSexCheckBox?.IsChecked.GetValueOrDefault() ?? false),
                    IncludeAge = (IncludeAnimalInfoCheckBox?.IsChecked.GetValueOrDefault() ?? false) && (IncludeAgeCheckBox?.IsChecked.GetValueOrDefault() ?? false),
                    IncludeStatus = (IncludeAnimalInfoCheckBox?.IsChecked.GetValueOrDefault() ?? false) && (IncludeStatusCheckBox?.IsChecked.GetValueOrDefault() ?? false),
                    IncludeWeight = (IncludeAnimalInfoCheckBox?.IsChecked.GetValueOrDefault() ?? false) && (IncludeWeightCheckBox?.IsChecked.GetValueOrDefault() ?? false),
                    IncludeCollarColor = (IncludeAnimalInfoCheckBox?.IsChecked.GetValueOrDefault() ?? false) && (IncludeCollarColorCheckBox?.IsChecked.GetValueOrDefault() ?? false),
                    IncludeIntakeDate = (IncludeAnimalInfoCheckBox?.IsChecked.GetValueOrDefault() ?? false) && (IncludeIntakeDateCheckBox?.IsChecked.GetValueOrDefault() ?? false),
                    IncludeNotes = (IncludeAnimalInfoCheckBox?.IsChecked.GetValueOrDefault() ?? false) && (IncludeNotesCheckBox?.IsChecked.GetValueOrDefault() ?? false),

                    // Vet Info
                    IncludeVetVisitDates = (IncludeVetInfoCheckBox?.IsChecked.GetValueOrDefault() ?? false) && (IncludeVetVisitDatesCheckBox?.IsChecked.GetValueOrDefault() ?? false),
                    IncludeVaccinations = (IncludeVetInfoCheckBox?.IsChecked.GetValueOrDefault() ?? false) && (IncludeVaccinationsCheckBox?.IsChecked.GetValueOrDefault() ?? false),
                    IncludeVaccinationDates = (IncludeVetInfoCheckBox?.IsChecked.GetValueOrDefault() ?? false) && (IncludeVaccinationDatesCheckBox?.IsChecked.GetValueOrDefault() ?? false),
                    IncludeSpayedNeutered = (IncludeVetInfoCheckBox?.IsChecked.GetValueOrDefault() ?? false) && (IncludeSpayedNeuteredCheckBox?.IsChecked.GetValueOrDefault() ?? false),
                    IncludeWorming = (IncludeVetInfoCheckBox?.IsChecked.GetValueOrDefault() ?? false) && (IncludeWormingCheckBox?.IsChecked.GetValueOrDefault() ?? false),
                    IncludeDeFleeing = (IncludeVetInfoCheckBox?.IsChecked.GetValueOrDefault() ?? false) && (IncludeDeFleeingCheckBox?.IsChecked.GetValueOrDefault() ?? false),
                    IncludeDental = (IncludeVetInfoCheckBox?.IsChecked.GetValueOrDefault() ?? false) && (IncludeDentalCheckBox?.IsChecked.GetValueOrDefault() ?? false),
                    IncludeVetNotes = (IncludeVetInfoCheckBox?.IsChecked.GetValueOrDefault() ?? false) && (IncludeVetNotesCheckBox?.IsChecked.GetValueOrDefault() ?? false)
                };

                // Log vet info checkbox states
                System.Diagnostics.Debug.WriteLine($"Vet Checkboxes - VetInfo: {IncludeVetInfoCheckBox?.IsChecked}, VetVisitDates: {IncludeVetVisitDatesCheckBox?.IsChecked}, Vaccinations: {IncludeVaccinationsCheckBox?.IsChecked}");
                System.Diagnostics.Debug.WriteLine($"Vet Options - IncludeVetVisitDates: {options.IncludeVetVisitDates}, IncludeVaccinations: {options.IncludeVaccinations}, IncludeSpayedNeutered: {options.IncludeSpayedNeutered}");

                var preview = await _exportService.GeneratePostTextAsync(_selectedAnimal.Id, _platform, options);
                
                System.Diagnostics.Debug.WriteLine($"Preview generated - Length: {preview?.Length ?? 0}");
                System.Diagnostics.Debug.WriteLine($"Preview first 200 chars: {(preview?.Length > 0 ? preview.Substring(0, Math.Min(200, preview.Length)) : "EMPTY")}");
                LoggingService.LogInfo($"ExportSocialMediaWindow: preview text length={preview?.Length ?? 0}");
                
                if (string.IsNullOrEmpty(preview))
                {
                    PreviewTextBox.Text = "No content selected. Please check the options above to include information in your post.";
                    CharCountText.Text = "Character Count: 0";
                    System.Diagnostics.Debug.WriteLine("Preview is empty");
                    LoggingService.LogWarning("ExportSocialMediaWindow: preview is empty after generation");
                }
                else
                {
                    // Append contact phone number to the preview content
                    var finalPreview = preview + "\n\nContact: +1 604-757-2742";
                    PreviewTextBox.Text = finalPreview;
                    CharCountText.Text = $"Character Count: {finalPreview.Length}";
                    System.Diagnostics.Debug.WriteLine($"Preview set successfully - {preview.Length} characters");
                    LoggingService.LogInfo("ExportSocialMediaWindow: preview set successfully");
                }
            }
            catch (Exception ex)
            {
                PreviewTextBox.Text = $"Error generating preview: {ex.Message}\n\nPlease check your selections and try again.";
                CharCountText.Text = "Character Count: 0";
                System.Diagnostics.Debug.WriteLine($"Preview Error: {ex}");
                System.Diagnostics.Debug.WriteLine($"Stack Trace: {ex.StackTrace}");
                LoggingService.LogError("ExportSocialMediaWindow: error generating preview", ex);
            }
        }

        private void AnimalInfoCheckBox_Changed(object sender, RoutedEventArgs e)
        {
            if (_selectedAnimal == null) return;
            
            bool isChecked = IncludeAnimalInfoCheckBox.IsChecked.GetValueOrDefault();
            IncludeNameCheckBox.IsEnabled = isChecked;
            IncludeBreedCheckBox.IsEnabled = isChecked;
            IncludeSexCheckBox.IsEnabled = isChecked;
            IncludeAgeCheckBox.IsEnabled = isChecked;
            IncludeStatusCheckBox.IsEnabled = isChecked;
            IncludeWeightCheckBox.IsEnabled = isChecked;
            IncludeCollarColorCheckBox.IsEnabled = isChecked;
            IncludeIntakeDateCheckBox.IsEnabled = isChecked;
            IncludeNotesCheckBox.IsEnabled = isChecked;
            UpdatePreview();
        }

        private void VetInfoCheckBox_Changed(object sender, RoutedEventArgs e)
        {
            if (_selectedAnimal == null) return;
            
            bool isChecked = IncludeVetInfoCheckBox.IsChecked.GetValueOrDefault();
            IncludeVetVisitDatesCheckBox.IsEnabled = isChecked;
            IncludeVaccinationsCheckBox.IsEnabled = isChecked;
            IncludeVaccinationDatesCheckBox.IsEnabled = isChecked;
            IncludeSpayedNeuteredCheckBox.IsEnabled = isChecked;
            IncludeWormingCheckBox.IsEnabled = isChecked;
            IncludeDeFleeingCheckBox.IsEnabled = isChecked;
            IncludeDentalCheckBox.IsEnabled = isChecked;
            IncludeVetNotesCheckBox.IsEnabled = isChecked;
            UpdatePreview();
        }

        private async void SectionCheckBox_Changed(object sender, RoutedEventArgs e)
        {
            if (_selectedAnimal == null) return;
            
            await LoadPhotoPreview();
            UpdatePreview();
        }

        private void FieldCheckBox_Changed(object sender, RoutedEventArgs e)
        {
            if (_selectedAnimal == null) return;
            
            UpdatePreview();
        }

        private void SelectAll_Click(object sender, RoutedEventArgs e)
        {
            IncludePhotosCheckBox.IsChecked = true;
            IncludeAnimalInfoCheckBox.IsChecked = true;
            IncludeVetInfoCheckBox.IsChecked = true;

            // Animal fields
            IncludeNameCheckBox.IsChecked = true;
            IncludeBreedCheckBox.IsChecked = true;
            IncludeSexCheckBox.IsChecked = true;
            IncludeAgeCheckBox.IsChecked = true;
            IncludeStatusCheckBox.IsChecked = true;
            IncludeWeightCheckBox.IsChecked = true;
            IncludeCollarColorCheckBox.IsChecked = true;
            IncludeIntakeDateCheckBox.IsChecked = true;
            IncludeNotesCheckBox.IsChecked = true;

            // Vet fields
            IncludeVetVisitDatesCheckBox.IsChecked = true;
            IncludeVaccinationsCheckBox.IsChecked = true;
            IncludeVaccinationDatesCheckBox.IsChecked = true;
            IncludeSpayedNeuteredCheckBox.IsChecked = true;
            IncludeWormingCheckBox.IsChecked = true;
            IncludeDeFleeingCheckBox.IsChecked = true;
            IncludeDentalCheckBox.IsChecked = true;
            IncludeVetNotesCheckBox.IsChecked = true;
        }

        private void DeselectAll_Click(object sender, RoutedEventArgs e)
        {
            IncludePhotosCheckBox.IsChecked = false;
            IncludeAnimalInfoCheckBox.IsChecked = false;
            IncludeVetInfoCheckBox.IsChecked = false;

            // Animal fields
            IncludeNameCheckBox.IsChecked = false;
            IncludeBreedCheckBox.IsChecked = false;
            IncludeSexCheckBox.IsChecked = false;
            IncludeAgeCheckBox.IsChecked = false;
            IncludeStatusCheckBox.IsChecked = false;
            IncludeWeightCheckBox.IsChecked = false;
            IncludeCollarColorCheckBox.IsChecked = false;
            IncludeIntakeDateCheckBox.IsChecked = false;
            IncludeNotesCheckBox.IsChecked = false;

            // Vet fields
            IncludeVetVisitDatesCheckBox.IsChecked = false;
            IncludeVaccinationsCheckBox.IsChecked = false;
            IncludeVaccinationDatesCheckBox.IsChecked = false;
            IncludeSpayedNeuteredCheckBox.IsChecked = false;
            IncludeWormingCheckBox.IsChecked = false;
            IncludeDeFleeingCheckBox.IsChecked = false;
            IncludeDentalCheckBox.IsChecked = false;
            IncludeVetNotesCheckBox.IsChecked = false;
        }

        private void CopyToClipboard_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(PreviewTextBox.Text))
            {
                MessageBox.Show("No content to copy. Please select an animal first.", "Nothing to Copy", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            try
            {
                Clipboard.SetText(PreviewTextBox.Text);
                MessageBox.Show("Post text copied to clipboard! You can now paste it into your social media platform.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error copying to clipboard: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SaveAsWord_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(PreviewTextBox.Text) || _selectedAnimal == null)
            {
                MessageBox.Show("No content to save. Please select an animal first.", "Nothing to Save", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            try
            {
                var saveDialog = new SaveFileDialog
                {
                    Filter = "Word Documents (*.docx)|*.docx|All Files (*.*)|*.*",
                    FileName = $"{_selectedAnimal.Name}_{_platform}_Post.docx",
                    DefaultExt = ".docx"
                };

                if (saveDialog.ShowDialog() == true)
                {
                    var options = GetCurrentExportOptions();
                    _exportService.CreateWordDocument(saveDialog.FileName, _selectedAnimal, PreviewTextBox.Text, options);
                    MessageBox.Show($"Word document saved successfully to:\n{saveDialog.FileName}\n\nThe document includes the formatted text and photo (if available).", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving Word document: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private ExportOptions GetCurrentExportOptions()
        {
            return new ExportOptions
            {
                IncludePhotos = IncludePhotosCheckBox.IsChecked.GetValueOrDefault(),
                IncludeName = IncludeAnimalInfoCheckBox.IsChecked.GetValueOrDefault() && IncludeNameCheckBox.IsChecked.GetValueOrDefault(),
                IncludeBreed = IncludeAnimalInfoCheckBox.IsChecked.GetValueOrDefault() && IncludeBreedCheckBox.IsChecked.GetValueOrDefault(),
                IncludeSex = IncludeAnimalInfoCheckBox.IsChecked.GetValueOrDefault() && IncludeSexCheckBox.IsChecked.GetValueOrDefault(),
                IncludeAge = IncludeAnimalInfoCheckBox.IsChecked.GetValueOrDefault() && IncludeAgeCheckBox.IsChecked.GetValueOrDefault(),
                IncludeStatus = IncludeAnimalInfoCheckBox.IsChecked.GetValueOrDefault() && IncludeStatusCheckBox.IsChecked.GetValueOrDefault(),
                IncludeWeight = IncludeAnimalInfoCheckBox.IsChecked.GetValueOrDefault() && IncludeWeightCheckBox.IsChecked.GetValueOrDefault(),
                IncludeCollarColor = IncludeAnimalInfoCheckBox.IsChecked.GetValueOrDefault() && IncludeCollarColorCheckBox.IsChecked.GetValueOrDefault(),
                IncludeIntakeDate = IncludeAnimalInfoCheckBox.IsChecked.GetValueOrDefault() && IncludeIntakeDateCheckBox.IsChecked.GetValueOrDefault(),
                IncludeNotes = IncludeAnimalInfoCheckBox.IsChecked.GetValueOrDefault() && IncludeNotesCheckBox.IsChecked.GetValueOrDefault(),
                IncludeVetVisitDates = IncludeVetInfoCheckBox.IsChecked.GetValueOrDefault() && IncludeVetVisitDatesCheckBox.IsChecked.GetValueOrDefault(),
                IncludeVaccinations = IncludeVetInfoCheckBox.IsChecked.GetValueOrDefault() && IncludeVaccinationsCheckBox.IsChecked.GetValueOrDefault(),
                IncludeVaccinationDates = IncludeVetInfoCheckBox.IsChecked.GetValueOrDefault() && IncludeVaccinationDatesCheckBox.IsChecked.GetValueOrDefault(),
                IncludeSpayedNeutered = IncludeVetInfoCheckBox.IsChecked.GetValueOrDefault() && IncludeSpayedNeuteredCheckBox.IsChecked.GetValueOrDefault(),
                IncludeWorming = IncludeVetInfoCheckBox.IsChecked.GetValueOrDefault() && IncludeWormingCheckBox.IsChecked.GetValueOrDefault(),
                IncludeDeFleeing = IncludeVetInfoCheckBox.IsChecked.GetValueOrDefault() && IncludeDeFleeingCheckBox.IsChecked.GetValueOrDefault(),
                IncludeDental = IncludeVetInfoCheckBox.IsChecked.GetValueOrDefault() && IncludeDentalCheckBox.IsChecked.GetValueOrDefault(),
                IncludeVetNotes = IncludeVetInfoCheckBox.IsChecked.GetValueOrDefault() && IncludeVetNotesCheckBox.IsChecked.GetValueOrDefault()
            };
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
