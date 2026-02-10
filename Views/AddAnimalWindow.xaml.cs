using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using PupTrailsV3.Models;
using PupTrailsV3.Services;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using A = DocumentFormat.OpenXml.Drawing;
using DW = DocumentFormat.OpenXml.Drawing.Wordprocessing;
using PIC = DocumentFormat.OpenXml.Drawing.Pictures;

namespace PupTrailsV3.Views
{
    public partial class AddAnimalWindow : Window
    {
        public Animal? ResultAnimal { get; private set; }
        private int? _animalId = null;
        private string? _photoPath = null;
        private string? _selectedGroupName = null;

        public AddAnimalWindow()
        {
            InitializeComponent();
            IntakeDateTextBox.Text = DateTime.Today.ToString("yyyy-MM-dd");
        }

        public void SetGroupName(string groupName, bool locked = false)
        {
            _selectedGroupName = groupName;
            // Note: Group name is now managed via the Select/Manage Groups button
            // If locked, the user should not be able to change it via that button
        }

        public void LoadAnimal(Animal animal)
        {
            _animalId = animal.Id;
            NameBox.Text = animal.Name;
            BreedBox.Text = animal.Breed;
            
            // Set Sex ComboBox
            if (SexBox.Items.Count > 0)
            {
                foreach (System.Windows.Controls.ComboBoxItem item in SexBox.Items)
                {
                    if (item.Content.ToString() == animal.Sex)
                    {
                        SexBox.SelectedItem = item;
                        break;
                    }
                }
            }
            
            // Set Status ComboBox
            if (StatusBox.Items.Count > 0)
            {
                foreach (System.Windows.Controls.ComboBoxItem item in StatusBox.Items)
                {
                    if (item.Content.ToString() == animal.Status)
                    {
                        StatusBox.SelectedItem = item;
                        break;
                    }
                }
            }
            
            IntakeDateTextBox.Text = animal.IntakeDate.ToString("yyyy-MM-dd");
            DOBTextBox.Text = animal.DOB?.ToString("yyyy-MM-dd") ?? "";
            CollarColorBox.Text = animal.CollarColor ?? "";
            WeightBox.Text = animal.Weight?.ToString() ?? "";
            NotesTextBox.Text = animal.Notes ?? "";
            
            // Set group name
            _selectedGroupName = animal.GroupName;
            
            // Load photo if exists
            _photoPath = animal.PhotoPath;
            LoadAnimalPhoto();
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
            if (string.IsNullOrWhiteSpace(NameBox.Text))
            {
                MessageBox.Show("Please enter an animal name", "Validation Error");
                return;
            }

            var intakeDate = ParseDate(IntakeDateTextBox.Text);
            if (intakeDate == null)
            {
                MessageBox.Show("Please enter a valid intake date (YYYY-MM-DD format)", "Validation Error");
                return;
            }

            // Parse weight
            decimal? weight = null;
            if (!string.IsNullOrWhiteSpace(WeightBox.Text))
            {
                if (decimal.TryParse(WeightBox.Text, out decimal parsedWeight))
                {
                    weight = parsedWeight;
                }
                else
                {
                    MessageBox.Show("Please enter a valid weight (numbers only)", "Validation Error");
                    return;
                }
            }

            ResultAnimal = new Animal
            {
                Id = _animalId ?? 0,
                Name = NameBox.Text,
                Breed = BreedBox.Text ?? "Unknown",
                Sex = (SexBox.SelectedItem as System.Windows.Controls.ComboBoxItem)?.Content.ToString() ?? "Unknown",
                Status = (StatusBox.SelectedItem as System.Windows.Controls.ComboBoxItem)?.Content.ToString() ?? "Planned",
                IntakeDate = intakeDate.Value,
                DOB = ParseDate(DOBTextBox.Text),
                CollarColor = CollarColorBox.Text,
                Weight = weight,
                GroupName = _selectedGroupName,
                PhotoPath = _photoPath,
                Notes = NotesTextBox.Text
            };

            DialogResult = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void SelectGroup_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var selectWindow = new SelectPuppiesWindow();
                if (selectWindow.ShowDialog() == true)
                {
                    _selectedGroupName = selectWindow.SelectedGroupName;
                    MessageBox.Show($"Group selected: {_selectedGroupName ?? "None"}", "Group Selection", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening group management window: {ex.Message}\n\nPlease ensure the database is accessible.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Reset_Click(object sender, RoutedEventArgs e)
        {
            NameBox.Text = string.Empty;
            BreedBox.Text = string.Empty;
            SexBox.SelectedIndex = -1;
            StatusBox.SelectedIndex = -1;
            IntakeDateTextBox.Text = DateTime.Today.ToString("yyyy-MM-dd");
            DOBTextBox.Text = string.Empty;
            CollarColorBox.Text = string.Empty;
            WeightBox.Text = string.Empty;
            _selectedGroupName = null;
            _photoPath = null;
            ShowNoPhoto();
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            if (_animalId == null)
            {
                MessageBox.Show("Cannot delete a record that hasn't been saved yet.", "Delete Error");
                return;
            }

            var result = MessageBox.Show("Are you sure you want to delete this animal record? This will also delete all associated records (vet visits, adoptions, etc.).", "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result == MessageBoxResult.Yes)
            {
                using (var db = new Data.PupTrailDbContext())
                {
                    var animal = db.Animals.Find(_animalId);
                    if (animal != null)
                    {
                        db.Animals.Remove(animal);
                        db.SaveChanges();
                        MessageBox.Show("Animal record deleted successfully.", "Success");
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

        private void ViewGroupMembers_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string groupName = _selectedGroupName?.Trim() ?? "";
                
                if (string.IsNullOrWhiteSpace(groupName))
                {
                    MessageBox.Show("Please enter or select a group name first.\n\nYou can type a group name or click 'Select/Manage' to choose an existing group.", "No Group Name", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                using (var db = new Data.PupTrailDbContext())
                {
                    // First check if any animals exist at all
                    var totalAnimals = db.Animals.Count();
                    if (totalAnimals == 0)
                    {
                        MessageBox.Show("No animals have been added to the system yet.\n\nPlease add some animals first before creating groups.", "No Animals Found", MessageBoxButton.OK, MessageBoxImage.Information);
                        return;
                    }

                    var groupMembers = db.Animals
                        .Where(a => a.GroupName == groupName)
                        .OrderBy(a => a.Name)
                        .ToList();

                    if (groupMembers.Count == 0)
                    {
                        // Check if this group name exists but with different spacing/casing
                        var similarGroups = db.Animals
                            .Where(a => !string.IsNullOrEmpty(a.GroupName))
                            .Select(a => a.GroupName)
                            .Distinct()
                            .ToList();

                        if (similarGroups.Count == 0)
                        {
                            MessageBox.Show($"No animals found in group '{groupName}'.\n\nNo groups have been created yet. Use 'Select/Manage' to create a new group.", "Empty Group", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        else
                        {
                            string availableGroups = string.Join("\n• ", similarGroups);
                            MessageBox.Show($"No animals found in group '{groupName}'.\n\nAvailable groups:\n• {availableGroups}\n\nMake sure the group name matches exactly.", "Group Not Found", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        return;
                    }

                    // Build detailed group information display
                    string memberDetails = "═══════════════════════════════════════════════\n";
                    memberDetails += $"  GROUP: {groupName}\n";
                    memberDetails += $"  Total Members: {groupMembers.Count}\n";
                    memberDetails += "═══════════════════════════════════════════════\n\n";

                    foreach (var animal in groupMembers)
                    {
                        memberDetails += $"🐾 {animal.Name}\n";
                        memberDetails += $"   Breed: {animal.Breed}\n";
                        memberDetails += $"   Sex: {animal.Sex} | Status: {animal.Status}\n";
                        memberDetails += $"   Intake Date: {animal.IntakeDate:yyyy-MM-dd}\n";
                        if (animal.DOB.HasValue)
                            memberDetails += $"   Date of Birth: {animal.DOB.Value:yyyy-MM-dd}\n";
                        memberDetails += "\n";
                    }

                    MessageBox.Show(memberDetails, $"Group Details - {groupName}", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error viewing group members: {ex.Message}\n\nDetails: {ex.InnerException?.Message}\n\nPlease ensure the database is accessible.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadAnimalPhoto()
        {
            try
            {
                if (!string.IsNullOrEmpty(_photoPath))
                {
                    string fullPath = PathManager.ResolveAnimalPhotoPath(_photoPath);
                    
                    if (!string.IsNullOrEmpty(fullPath) && File.Exists(fullPath))
                    {
                        var bitmap = new BitmapImage();
                        bitmap.BeginInit();
                        bitmap.CacheOption = BitmapCacheOption.OnLoad;
                        bitmap.UriSource = new Uri(fullPath);
                        bitmap.EndInit();
                        
                        AnimalPhotoDisplay.Source = bitmap;
                        AnimalPhotoDisplay.Visibility = Visibility.Visible;
                        NoPhotoText.Visibility = Visibility.Collapsed;
                        RemovePhotoButton.IsEnabled = true;
                        PhotoPathText.Text = $"Photo: {_photoPath}";
                    }
                    else
                    {
                        // Photo file doesn't exist, reset
                        _photoPath = null;
                        ShowNoPhoto();
                    }
                }
                else
                {
                    ShowNoPhoto();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading photo: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                ShowNoPhoto();
            }
        }

        private void ShowNoPhoto()
        {
            AnimalPhotoDisplay.Source = null;
            AnimalPhotoDisplay.Visibility = Visibility.Collapsed;
            NoPhotoText.Visibility = Visibility.Visible;
            RemovePhotoButton.IsEnabled = false;
            PhotoPathText.Text = "";
        }

        private void UploadPhoto_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var openFileDialog = new OpenFileDialog
                {
                    Title = "Select Animal Photo",
                    Filter = "Image Files (*.jpg;*.jpeg;*.png;*.bmp;*.gif)|*.jpg;*.jpeg;*.png;*.bmp;*.gif|All Files (*.*)|*.*",
                    FilterIndex = 1
                };

                if (openFileDialog.ShowDialog() == true)
                {
                    string sourceFilePath = openFileDialog.FileName;
                    string animalName = string.IsNullOrWhiteSpace(NameBox.Text) ? "Animal" : NameBox.Text.Trim();
                    string fileName = $"{animalName}_{DateTime.Now:yyyyMMddHHmmss}{Path.GetExtension(sourceFilePath)}";
                    string targetDirectory = PathManager.GetAnimalPhotosDirectory();
                    string targetFilePath = Path.Combine(targetDirectory, fileName);

                    // Ensure directory exists
                    if (!Directory.Exists(targetDirectory))
                    {
                        Directory.CreateDirectory(targetDirectory);
                    }

                    // Delete old photo if exists
                    if (!string.IsNullOrEmpty(_photoPath))
                    {
                        string oldFilePath = PathManager.ResolveAnimalPhotoPath(_photoPath);
                        if (!string.IsNullOrEmpty(oldFilePath) && File.Exists(oldFilePath))
                        {
                            try { File.Delete(oldFilePath); } catch { }
                        }
                    }

                    // Copy file to animal photos directory
                    File.Copy(sourceFilePath, targetFilePath, true);

                    // Update photo path
                    _photoPath = fileName;
                    LoadAnimalPhoto();
                    MessageBox.Show("Photo uploaded successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error uploading photo: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RemovePhoto_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var result = MessageBox.Show("Are you sure you want to remove this photo?", "Confirm Remove", MessageBoxButton.YesNo, MessageBoxImage.Question);
                
                if (result == MessageBoxResult.Yes && !string.IsNullOrEmpty(_photoPath))
                {
                    string filePath = PathManager.ResolveAnimalPhotoPath(_photoPath);
                    
                    // Delete file if exists
                    if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath))
                    {
                        try { File.Delete(filePath); } catch { }
                    }

                    // Update photo path
                    _photoPath = null;
                    ShowNoPhoto();
                    MessageBox.Show("Photo removed successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error removing photo: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExportToFile_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var saveFileDialog = new SaveFileDialog
                {
                    Title = "Export Animal Information",
                    Filter = "Word Documents (*.docx)|*.docx|All Files (*.*)|*.*",
                    FileName = $"{NameBox.Text?.Trim() ?? "Animal"}_Info_{DateTime.Now:yyyyMMdd_HHmmss}.docx",
                    DefaultExt = "docx"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    CreateWordDocument(saveFileDialog.FileName);
                    MessageBox.Show($"Animal information exported successfully!\n\nFile saved to:\n{saveFileDialog.FileName}", 
                        "Export Complete", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error exporting animal information: {ex.Message}\n\n{ex.StackTrace}", "Export Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CreateWordDocument(string filePath)
        {
            using (WordprocessingDocument wordDoc = WordprocessingDocument.Create(filePath, WordprocessingDocumentType.Document))
            {
                MainDocumentPart mainPart = wordDoc.AddMainDocumentPart();
                mainPart.Document = new Document();
                Body body = mainPart.Document.AppendChild(new Body());

                // Add header image (Annie's Little Paws Rescue logo)
                try
                {
                    string headerImagePath = PathManager.GetExportHeaderImagePath();
                    if (File.Exists(headerImagePath))
                    {
                        AddHeaderImageToBody(wordDoc, body, headerImagePath);
                        AddParagraph(body, "", false, false);
                    }
                }
                catch
                {
                    // If header image fails, continue without it
                }

                // Title
                Paragraph titlePara = body.AppendChild(new Paragraph());
                Run titleRun = titlePara.AppendChild(new Run());
                titleRun.AppendChild(new Text("ANIMAL INFORMATION REPORT"));
                RunProperties titleProps = titleRun.InsertAt(new RunProperties(), 0);
                titleProps.AppendChild(new Bold());
                titleProps.AppendChild(new FontSize() { Val = "32" });
                titlePara.ParagraphProperties = new ParagraphProperties(new Justification() { Val = JustificationValues.Center });

                // Export Date
                AddParagraph(body, $"Export Date: {DateTime.Now:yyyy-MM-dd HH:mm:ss}", false, true);
                AddParagraph(body, "", false, false);

                // Add photo if available
                if (!string.IsNullOrEmpty(_photoPath))
                {
                    try
                    {
                        string fullPhotoPath = PathManager.ResolveAnimalPhotoPath(_photoPath);
                        if (!string.IsNullOrEmpty(fullPhotoPath) && File.Exists(fullPhotoPath))
                        {
                            AddImageToBody(wordDoc, body, fullPhotoPath);
                            AddParagraph(body, "", false, false);
                        }
                    }
                    catch
                    {
                        // If image fails, continue without it
                    }
                }

                // Basic Info Section
                AddParagraph(body, "BASIC INFORMATION", true, false);
                AddParagraph(body, $"Name: {NameBox.Text ?? "N/A"}", false, false);
                AddParagraph(body, $"Breed: {BreedBox.Text ?? "N/A"}", false, false);
                AddParagraph(body, $"Sex: {(SexBox.SelectedItem as System.Windows.Controls.ComboBoxItem)?.Content?.ToString() ?? "N/A"}", false, false);
                AddParagraph(body, $"Status: {(StatusBox.SelectedItem as System.Windows.Controls.ComboBoxItem)?.Content?.ToString() ?? "N/A"}", false, false);
                AddParagraph(body, "", false, false);

                // Dates Section
                AddParagraph(body, "DATES", true, false);
                AddParagraph(body, $"Date of Birth: {DOBTextBox.Text ?? "N/A"}", false, false);
                AddParagraph(body, $"Intake Date: {IntakeDateTextBox.Text ?? "N/A"}", false, false);
                AddParagraph(body, "", false, false);

                // Identification Section
                AddParagraph(body, "IDENTIFICATION", true, false);
                AddParagraph(body, $"Collar Color: {CollarColorBox.Text ?? "N/A"}", false, false);
                AddParagraph(body, "", false, false);

                // Physical Info Section
                AddParagraph(body, "PHYSICAL INFORMATION", true, false);
                AddParagraph(body, $"Weight: {(string.IsNullOrWhiteSpace(WeightBox.Text) ? "N/A" : WeightBox.Text + " (pounds + ounces)")}", false, false);
                AddParagraph(body, "", false, false);

                // Group Info Section
                AddParagraph(body, "GROUP INFORMATION", true, false);
                AddParagraph(body, $"Group Name: {_selectedGroupName ?? "N/A"}", false, false);
                AddParagraph(body, "", false, false);

                // Vet Visit History Section
                AddVetVisitHistory(body);

                mainPart.Document.Save();
            }
        }

        private void AddVetVisitHistory(Body body)
        {
            try
            {
                // Only fetch vet visits if we have an animal ID (editing existing animal)
                if (_animalId.HasValue)
                {
                    using (var db = new Data.PupTrailDbContext())
                    {
                        var vetVisits = db.VetVisits
                            .Where(v => v.AnimalId == _animalId.Value && !v.IsDeleted)
                            .OrderByDescending(v => v.Date)
                            .ToList();

                        if (vetVisits.Any())
                        {
                            AddParagraph(body, "VET VISIT HISTORY", true, false);
                            AddParagraph(body, $"Total Visits: {vetVisits.Count}", false, false);
                            AddParagraph(body, "", false, false);

                            int visitNumber = 1;
                            foreach (var visit in vetVisits)
                            {
                                AddParagraph(body, $"Visit #{visitNumber} - {visit.Date:yyyy-MM-dd}", true, false);
                                
                                // Load the veterinarian name if available
                                string vetName = "N/A";
                                if (visit.PersonId.HasValue)
                                {
                                    var vet = db.People.FirstOrDefault(p => p.Id == visit.PersonId.Value);
                                    if (vet != null)
                                    {
                                        vetName = vet.Name;
                                    }
                                }
                                
                                AddParagraph(body, $"Vet: {vetName} | Cost: ${visit.TotalCost:F2} | Ready: {(visit.ReadyForAdoption ? "Yes" : "No")}", false, false);
                                
                                // Add specific treatments in compact format
                                var treatments = new List<string>();
                                if (visit.WormingDate.HasValue)
                                    treatments.Add($"Worm: {visit.WormingDate.Value:MM/dd} ${visit.WormingCost ?? 0:F2}");
                                if (visit.DeFleeingDate.HasValue)
                                    treatments.Add($"Flea: {visit.DeFleeingDate.Value:MM/dd} ${visit.DeFleeingCost ?? 0:F2}");
                                if (visit.DentalDate.HasValue)
                                    treatments.Add($"Dental: {visit.DentalDate.Value:MM/dd} ${visit.DentalCost ?? 0:F2}");
                                if (visit.SpayedNeuteringDate.HasValue)
                                    treatments.Add($"Spay/Neuter: {visit.SpayedNeuteringDate.Value:MM/dd} ${visit.SpayedNeuteringCost ?? 0:F2}");
                                
                                if (treatments.Any())
                                {
                                    AddParagraph(body, string.Join(" | ", treatments), false, false);
                                }
                                
                                if (!string.IsNullOrWhiteSpace(visit.Notes))
                                {
                                    AddParagraph(body, $"Notes: {visit.Notes}", false, false);
                                }
                                
                                AddParagraph(body, "─────────────────", false, false);
                                visitNumber++;
                            }
                        }
                        else
                        {
                            AddParagraph(body, "VET VISIT HISTORY", true, false);
                            AddParagraph(body, "No vet visits recorded.", false, false);
                            AddParagraph(body, "", false, false);
                        }
                    }
                }
                else
                {
                    // For new animals not yet saved
                    AddParagraph(body, "VET VISIT HISTORY", true, false);
                    AddParagraph(body, "No vet visits recorded (animal not yet saved to database).", false, false);
                    AddParagraph(body, "", false, false);
                }
            }
            catch (Exception ex)
            {
                // If there's an error fetching vet visits, just add an error message
                AddParagraph(body, "VET VISIT HISTORY", true, false);
                AddParagraph(body, $"Error loading vet visit history: {ex.Message}", false, false);
                AddParagraph(body, "", false, false);
            }
        }

        private void AddParagraph(Body body, string text, bool isBold, bool isItalic)
        {
            Paragraph para = body.AppendChild(new Paragraph());
            Run run = para.AppendChild(new Run());
            run.AppendChild(new Text(text));
            
            if (isBold || isItalic)
            {
                RunProperties runProps = run.InsertAt(new RunProperties(), 0);
                if (isBold) runProps.AppendChild(new Bold());
                if (isItalic) runProps.AppendChild(new Italic());
                if (isBold) runProps.AppendChild(new FontSize() { Val = "24" });
            }
        }

        private void AddImageToBody(WordprocessingDocument wordDoc, Body body, string imagePath)
        {
            MainDocumentPart? mainPart = wordDoc.MainDocumentPart;
            if (mainPart == null) return;
            
            ImagePart imagePart = mainPart.AddImagePart(ImagePartType.Jpeg);

            using (FileStream stream = new FileStream(imagePath, FileMode.Open))
            {
                imagePart.FeedData(stream);
            }

            AddImageToElement(mainPart.GetIdOfPart(imagePart), body);
        }

        private void AddHeaderImageToBody(WordprocessingDocument wordDoc, Body body, string imagePath)
        {
            MainDocumentPart? mainPart = wordDoc.MainDocumentPart;
            if (mainPart == null) return;
            
            ImagePart imagePart = mainPart.AddImagePart(ImagePartType.Png);

            using (FileStream stream = new FileStream(imagePath, FileMode.Open))
            {
                imagePart.FeedData(stream);
            }

            AddHeaderImageToElement(mainPart.GetIdOfPart(imagePart), body);
        }

        private void AddHeaderImageToElement(string relationshipId, Body body)
        {
            const long emusPerInch = 914400;
            const long maxWidth = (long)(6.5 * emusPerInch); // Full page width
            const long maxHeight = (long)(1.5 * emusPerInch); // Header height

            var element = new Drawing(
                new DW.Inline(
                    new DW.Extent() { Cx = maxWidth, Cy = maxHeight },
                    new DW.EffectExtent() { LeftEdge = 0L, TopEdge = 0L, RightEdge = 0L, BottomEdge = 0L },
                    new DW.DocProperties() { Id = (UInt32Value)1U, Name = "Header Logo" },
                    new DW.NonVisualGraphicFrameDrawingProperties(
                        new A.GraphicFrameLocks() { NoChangeAspect = true }),
                    new A.Graphic(
                        new A.GraphicData(
                            new PIC.Picture(
                                new PIC.NonVisualPictureProperties(
                                    new PIC.NonVisualDrawingProperties() { Id = (UInt32Value)0U, Name = "HeaderLogo.png" },
                                    new PIC.NonVisualPictureDrawingProperties()),
                                new PIC.BlipFill(
                                    new A.Blip() { Embed = relationshipId },
                                    new A.Stretch(new A.FillRectangle())),
                                new PIC.ShapeProperties(
                                    new A.Transform2D(
                                        new A.Offset() { X = 0L, Y = 0L },
                                        new A.Extents() { Cx = maxWidth, Cy = maxHeight }),
                                    new A.PresetGeometry(new A.AdjustValueList()) { Preset = A.ShapeTypeValues.Rectangle }))                        ) { Uri = "http://schemas.openxmlformats.org/drawingml/2006/picture" })
                ) { DistanceFromTop = (UInt32Value)0U, DistanceFromBottom = (UInt32Value)0U, DistanceFromLeft = (UInt32Value)0U, DistanceFromRight = (UInt32Value)0U });

            Paragraph para = body.AppendChild(new Paragraph());
            para.ParagraphProperties = new ParagraphProperties(new Justification() { Val = JustificationValues.Center });
            Run run = para.AppendChild(new Run());
            run.AppendChild(element);
        }

        private void AddImageToElement(string relationshipId, Body body)
        {
            const long emusPerInch = 914400;
            const long maxWidth = (long)(3.0 * emusPerInch); // 3 inches wide
            const long maxHeight = (long)(3.0 * emusPerInch); // 3 inches tall

            var element = new Drawing(
                new DW.Inline(
                    new DW.Extent() { Cx = maxWidth, Cy = maxHeight },
                    new DW.EffectExtent() { LeftEdge = 0L, TopEdge = 0L, RightEdge = 0L, BottomEdge = 0L },
                    new DW.DocProperties() { Id = (UInt32Value)1U, Name = "Animal Photo" },
                    new DW.NonVisualGraphicFrameDrawingProperties(
                        new A.GraphicFrameLocks() { NoChangeAspect = true }),
                    new A.Graphic(
                        new A.GraphicData(
                            new PIC.Picture(
                                new PIC.NonVisualPictureProperties(
                                    new PIC.NonVisualDrawingProperties() { Id = (UInt32Value)0U, Name = "AnimalPhoto.jpg" },
                                    new PIC.NonVisualPictureDrawingProperties()),
                                new PIC.BlipFill(
                                    new A.Blip() { Embed = relationshipId },
                                    new A.Stretch(new A.FillRectangle())),
                                new PIC.ShapeProperties(
                                    new A.Transform2D(
                                        new A.Offset() { X = 0L, Y = 0L },
                                        new A.Extents() { Cx = maxWidth, Cy = maxHeight }),
                                    new A.PresetGeometry(new A.AdjustValueList()) { Preset = A.ShapeTypeValues.Rectangle }))
                        ) { Uri = "http://schemas.openxmlformats.org/drawingml/2006/picture" })
                ) { DistanceFromTop = (UInt32Value)0U, DistanceFromBottom = (UInt32Value)0U, DistanceFromLeft = (UInt32Value)0U, DistanceFromRight = (UInt32Value)0U });

            Paragraph para = body.AppendChild(new Paragraph());
            para.ParagraphProperties = new ParagraphProperties(new Justification() { Val = JustificationValues.Center });
            Run run = para.AppendChild(new Run());
            run.AppendChild(element);
        }
    }
}
