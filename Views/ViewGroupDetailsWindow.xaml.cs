using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
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
    public partial class ViewGroupDetailsWindow : Window
    {
        private DatabaseService _dbService;
        private string _groupName;
        private List<Animal> _allMembers;
        private List<Animal> _filteredMembers;
        private PuppyGroup? _currentGroup;

        public ViewGroupDetailsWindow(string groupName)
        {
            InitializeComponent();
            _dbService = new DatabaseService();
            _groupName = groupName;
            _allMembers = new List<Animal>();
            _filteredMembers = new List<Animal>();
            
            LoadGroupData();
        }

        private void LoadGroupData()
        {
            try
            {
                GroupNameTitle.Text = $"Group: {_groupName}";
                
                // Load or create group record
                _currentGroup = _dbService.GetPuppyGroup(_groupName);
                if (_currentGroup == null)
                {
                    _currentGroup = new PuppyGroup
                    {
                        GroupName = _groupName,
                        DateCreated = DateTime.Today
                    };
                    _dbService.AddPuppyGroup(_currentGroup);
                }
                
                // Load group image if exists
                LoadGroupImage();
                
                // Load all animals in this group
                _allMembers = _dbService.GetAnimals()
                    .Where(a => a.GroupName == _groupName)
                    .OrderBy(a => a.Name)
                    .ToList();

                _filteredMembers = new List<Animal>(_allMembers);
                
                TotalMembersText.Text = _allMembers.Count.ToString();
                
                // Try to get date created from first animal's intake date as estimate
                if (_allMembers.Count > 0)
                {
                    var earliestIntake = _allMembers.Min(a => a.IntakeDate);
                    DateCreatedText.Text = earliestIntake.ToString("yyyy-MM-dd");
                }
                else
                {
                    DateCreatedText.Text = "No members";
                }

                MembersDataGrid.ItemsSource = _filteredMembers;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading group data: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadGroupImage()
        {
            try
            {
                if (_currentGroup != null && !string.IsNullOrEmpty(_currentGroup.ImagePath))
                {
                    string fullPath = Path.Combine(PathManager.GetGroupImagesDirectory(), _currentGroup.ImagePath);
                    
                    if (File.Exists(fullPath))
                    {
                        var bitmap = new BitmapImage();
                        bitmap.BeginInit();
                        bitmap.CacheOption = BitmapCacheOption.OnLoad;
                        bitmap.UriSource = new Uri(fullPath);
                        bitmap.EndInit();
                        
                        GroupImageDisplay.Source = bitmap;
                        GroupImageDisplay.Visibility = Visibility.Visible;
                        NoImageText.Visibility = Visibility.Collapsed;
                        RemoveImageButton.IsEnabled = true;
                        ImagePathText.Text = $"Image: {_currentGroup.ImagePath}";
                    }
                    else
                    {
                        // Image file doesn't exist, reset the path
                        _currentGroup.ImagePath = null;
                        _dbService.UpdatePuppyGroup(_currentGroup);
                        ShowNoImage();
                    }
                }
                else
                {
                    ShowNoImage();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading group image: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                ShowNoImage();
            }
        }

        private void ShowNoImage()
        {
            GroupImageDisplay.Source = null;
            GroupImageDisplay.Visibility = Visibility.Collapsed;
            NoImageText.Visibility = Visibility.Visible;
            RemoveImageButton.IsEnabled = false;
            ImagePathText.Text = "";
        }

        private void SearchBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            string searchText = SearchBox.Text?.ToLower() ?? "";
            
            if (string.IsNullOrWhiteSpace(searchText))
            {
                _filteredMembers = new List<Animal>(_allMembers);
            }
            else
            {
                _filteredMembers = _allMembers
                    .Where(a => 
                        (a.Name?.ToLower().Contains(searchText) ?? false) ||
                        (a.Breed?.ToLower().Contains(searchText) ?? false) ||
                        (a.Sex?.ToLower().Contains(searchText) ?? false) ||
                        (a.Status?.ToLower().Contains(searchText) ?? false))
                    .ToList();
            }
            
            MembersDataGrid.ItemsSource = null;
            MembersDataGrid.ItemsSource = _filteredMembers;
        }

        private void MembersDataGrid_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (MembersDataGrid.SelectedItem is Animal selectedAnimal)
            {
                try
                {
                    var editWindow = new AddAnimalWindow();
                    editWindow.LoadAnimal(selectedAnimal);
                    
                    if (editWindow.ShowDialog() == true && editWindow.ResultAnimal != null)
                    {
                        _dbService.UpdateAnimal(editWindow.ResultAnimal);
                        LoadGroupData(); // Refresh the data
                        MessageBox.Show($"Animal '{editWindow.ResultAnimal.Name}' updated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                        
                        // Refresh the main window
                        if (Application.Current.MainWindow is MainWindow mainWindow)
                        {
                            var viewModel = mainWindow.DataContext as ViewModels.MainViewModel;
                            if (viewModel != null)
                            {
                                _ = viewModel.LoadDataAsync();
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error opening animal details: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void AddNewAnimal_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var addWindow = new AddAnimalWindow();
                addWindow.SetGroupName(_groupName, locked: true);
                
                if (addWindow.ShowDialog() == true && addWindow.ResultAnimal != null)
                {
                    _dbService.AddAnimal(addWindow.ResultAnimal);
                    LoadGroupData(); // Refresh the data
                    MessageBox.Show($"Animal '{addWindow.ResultAnimal.Name}' added to group '{_groupName}' successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    
                    // Refresh the main window
                    if (Application.Current.MainWindow is MainWindow mainWindow)
                    {
                        var viewModel = mainWindow.DataContext as ViewModels.MainViewModel;
                        if (viewModel != null)
                        {
                            _ = viewModel.LoadDataAsync();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error adding new animal: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AddExisting_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var selectWindow = new SelectPuppiesWindow();
                selectWindow.Title = $"Add Existing Animals to '{_groupName}'";
                
                if (selectWindow.ShowDialog() == true)
                {
                    // Get the selected puppy IDs
                    var selectedIds = selectWindow.SelectedPuppyIds;
                    
                    if (selectedIds != null && selectedIds.Count > 0)
                    {
                        int addedCount = 0;
                        foreach (var animalId in selectedIds)
                        {
                            var animal = _dbService.GetAnimal(animalId);
                            if (animal != null)
                            {
                                // Update the animal's group name
                                animal.GroupName = _groupName;
                                _dbService.UpdateAnimal(animal);
                                addedCount++;
                            }
                        }
                        
                        LoadGroupData(); // Refresh the data
                        MessageBox.Show($"{addedCount} animal(s) added to group '{_groupName}' successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                        
                        // Refresh the main window
                        if (Application.Current.MainWindow is MainWindow mainWindow)
                        {
                            var viewModel = mainWindow.DataContext as ViewModels.MainViewModel;
                            if (viewModel != null)
                            {
                                _ = viewModel.LoadDataAsync();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error adding existing animals: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void UploadImage_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var openFileDialog = new OpenFileDialog
                {
                    Title = "Select Group Photo",
                    Filter = "Image Files (*.jpg;*.jpeg;*.png;*.bmp;*.gif)|*.jpg;*.jpeg;*.png;*.bmp;*.gif|All Files (*.*)|*.*",
                    FilterIndex = 1
                };

                if (openFileDialog.ShowDialog() == true)
                {
                    string sourceFilePath = openFileDialog.FileName;
                    string fileName = $"{_groupName}_{DateTime.Now:yyyyMMddHHmmss}{Path.GetExtension(sourceFilePath)}";
                    string targetDirectory = PathManager.GetGroupImagesDirectory();
                    string targetFilePath = Path.Combine(targetDirectory, fileName);

                    // Ensure directory exists
                    if (!Directory.Exists(targetDirectory))
                    {
                        Directory.CreateDirectory(targetDirectory);
                    }

                    // Copy file to group images directory
                    File.Copy(sourceFilePath, targetFilePath, true);

                    // Update group record
                    if (_currentGroup != null)
                    {
                        // Delete old image if exists
                        if (!string.IsNullOrEmpty(_currentGroup.ImagePath))
                        {
                            string oldFilePath = Path.Combine(targetDirectory, _currentGroup.ImagePath);
                            if (File.Exists(oldFilePath))
                            {
                                try { File.Delete(oldFilePath); } catch { }
                            }
                        }

                        _currentGroup.ImagePath = fileName;
                        _dbService.UpdatePuppyGroup(_currentGroup);
                        LoadGroupImage();
                        MessageBox.Show("Group photo uploaded successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error uploading image: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RemoveImage_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var result = MessageBox.Show("Are you sure you want to remove the group photo?", "Confirm Remove", MessageBoxButton.YesNo, MessageBoxImage.Question);
                
                if (result == MessageBoxResult.Yes && _currentGroup != null && !string.IsNullOrEmpty(_currentGroup.ImagePath))
                {
                    string filePath = Path.Combine(PathManager.GetGroupImagesDirectory(), _currentGroup.ImagePath);
                    
                    // Delete file if exists
                    if (File.Exists(filePath))
                    {
                        try { File.Delete(filePath); } catch { }
                    }

                    // Update group record
                    _currentGroup.ImagePath = null;
                    _dbService.UpdatePuppyGroup(_currentGroup);
                    ShowNoImage();
                    MessageBox.Show("Group photo removed successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error removing image: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExportToFile_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var saveFileDialog = new SaveFileDialog
                {
                    Title = "Export Group Information",
                    Filter = "Word Documents (*.docx)|*.docx|All Files (*.*)|*.*",
                    FileName = $"{_groupName}_Group_Info_{DateTime.Now:yyyyMMdd_HHmmss}.docx",
                    DefaultExt = "docx"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    CreateWordDocument(saveFileDialog.FileName);
                    MessageBox.Show($"Group information exported successfully!\n\nFile saved to:\n{saveFileDialog.FileName}", 
                        "Export Complete", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error exporting group information: {ex.Message}", "Export Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
                titleRun.AppendChild(new Text($"GROUP INFORMATION - {_groupName}"));
                RunProperties titleProps = titleRun.InsertAt(new RunProperties(), 0);
                titleProps.AppendChild(new Bold());
                titleProps.AppendChild(new FontSize() { Val = "32" });
                titlePara.ParagraphProperties = new ParagraphProperties(new Justification() { Val = JustificationValues.Center });

                // Export Date
                AddParagraph(body, $"Export Date: {DateTime.Now:yyyy-MM-dd HH:mm:ss}", false, true);
                AddParagraph(body, "", false, false);

                // Add group photo if available
                if (_currentGroup != null && !string.IsNullOrEmpty(_currentGroup.ImagePath))
                {
                    try
                    {
                        string fullPhotoPath = Path.Combine(PathManager.GetGroupImagesDirectory(), _currentGroup.ImagePath);
                        if (File.Exists(fullPhotoPath))
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

                // Group Summary Section
                AddParagraph(body, "GROUP SUMMARY", true, false);
                AddParagraph(body, $"Group Name: {_groupName}", false, false);
                AddParagraph(body, $"Total Members: {_allMembers.Count}", false, false);
                AddParagraph(body, $"Date Created: {DateCreatedText.Text}", false, false);
                AddParagraph(body, "", false, false);

                // Group Members Section
                AddParagraph(body, "GROUP MEMBERS", true, false);
                AddParagraph(body, "", false, false);

                if (_allMembers.Count == 0)
                {
                    AddParagraph(body, "No members in this group.", false, true);
                }
                else
                {
                    int memberNumber = 1;
                    foreach (var animal in _allMembers)
                    {
                        // Member header
                        AddParagraph(body, $"Member #{memberNumber} - {animal.Name}", true, false);
                        AddParagraph(body, "", false, false);

                        // Add photo if available
                        if (!string.IsNullOrEmpty(animal.PhotoPath))
                        {
                            try
                            {
                                string fullPhotoPath = PathManager.ResolveAnimalPhotoPath(animal.PhotoPath);
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

                        // Basic Info
                        AddParagraph(body, "Basic Information:", false, true);
                        AddParagraph(body, $"  Breed: {animal.Breed ?? "N/A"}", false, false);
                        AddParagraph(body, $"  Sex: {animal.Sex ?? "N/A"}", false, false);
                        AddParagraph(body, $"  Status: {animal.Status ?? "N/A"}", false, false);
                        AddParagraph(body, "", false, false);

                        // Dates
                        AddParagraph(body, "Dates:", false, true);
                        AddParagraph(body, $"  Date of Birth: {(animal.DOB.HasValue ? animal.DOB.Value.ToString("yyyy-MM-dd") : "N/A")}", false, false);
                        AddParagraph(body, $"  Intake Date: {animal.IntakeDate:yyyy-MM-dd}", false, false);
                        AddParagraph(body, "", false, false);

                        // Physical Info
                        AddParagraph(body, "Physical Information:", false, true);
                        AddParagraph(body, $"  Collar Color: {animal.CollarColor ?? "N/A"}", false, false);
                        AddParagraph(body, $"  Weight: {(animal.Weight.HasValue ? $"{animal.Weight.Value} (pounds + ounces)" : "N/A")}", false, false);
                        AddParagraph(body, "", false, false);
                        AddParagraph(body, "═══════════════════════════════════════", false, false);
                        AddParagraph(body, "", false, false);

                        memberNumber++;
                    }
                }

                // Footer
                AddParagraph(body, $"End of Report - Total: {_allMembers.Count} Animals", false, true);

                mainPart.Document.Save();
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

        private void AddImageToElement(string relationshipId, Body body)
        {
            const long emusPerInch = 914400;
            const long maxWidth = (long)(2.5 * emusPerInch); // 2.5 inches wide
            const long maxHeight = (long)(2.5 * emusPerInch); // 2.5 inches tall

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
