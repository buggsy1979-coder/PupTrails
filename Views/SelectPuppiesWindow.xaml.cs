using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using PupTrailsV3.Models;
using PupTrailsV3.Services;

namespace PupTrailsV3.Views
{
    public partial class SelectPuppiesWindow : Window
    {
        private DatabaseService? _dbService;
        public string? SelectedGroupName { get; private set; }
        public List<int> SelectedPuppyIds { get; private set; } = new List<int>();
        private List<PuppyItem> _allPuppies = new List<PuppyItem>();
        private ObservableCollection<PuppyItem> _displayPuppies = new ObservableCollection<PuppyItem>();

        public SelectPuppiesWindow()
        {
            try
            {
                InitializeComponent();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading window interface: {ex.Message}\n\nStack: {ex.StackTrace}", "UI Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                _dbService = new DatabaseService();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error connecting to database: {ex.Message}\n\nThe window will open but features may be limited.", "Database Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            // Load data after window is fully loaded
            this.Loaded += SelectPuppiesWindow_Loaded;
        }

        private void SelectPuppiesWindow_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                // Set today's date as default for group creation
                if (GroupDateCreatedBox != null)
                {
                    GroupDateCreatedBox.Text = DateTime.Today.ToString("yyyy-MM-dd");
                }
                
                LoadData();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading data: {ex.Message}\n\nYou can still use the window but some features may not work.", "Data Load Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void LoadData()
        {
            try
            {
                if (_dbService == null)
                {
                    // Silently return if no database service
                    return;
                }

                // Ensure controls exist before using them
                if (PuppiesListBox == null || ExistingGroupPuppiesBox == null || ExistingGroupsCombo == null)
                {
                    return;
                }

                // Load ALL animals to allow adding them to groups (even if they're already in a group)
                var animals = _dbService.GetAnimals()?.OrderBy(a => a.Name).ToList() ?? new List<Animal>();
                
                _allPuppies.Clear();
                foreach (var animal in animals)
                {
                    // Show group membership in the display text if applicable
                    string displayText = $"{animal.Name} ({animal.Breed}, {animal.Sex})";
                    if (!string.IsNullOrEmpty(animal.GroupName))
                    {
                        displayText += $" [Currently in: {animal.GroupName}]";
                    }
                    
                    _allPuppies.Add(new PuppyItem
                    {
                        Id = animal.Id,
                        DisplayText = displayText,
                        IsSelected = false
                    });
                }

                _displayPuppies.Clear();
                foreach (var puppy in _allPuppies)
                {
                    _displayPuppies.Add(puppy);
                }
                
                PuppiesListBox.ItemsSource = _displayPuppies;
                ExistingGroupPuppiesBox.ItemsSource = _displayPuppies;

                // Load existing group names for the Existing Groups ComboBox
                var existingGroups = _dbService.GetAnimals()
                    .Where(a => !string.IsNullOrEmpty(a.GroupName))
                    .Select(a => a.GroupName)
                    .Distinct()
                    .OrderBy(g => g)
                    .ToList();

                // Populate the Existing Groups ComboBox only
                ExistingGroupsCombo.ItemsSource = existingGroups;
                if (existingGroups.Count > 0)
                {
                    ExistingGroupsCombo.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading group data: {ex.Message}\n\nStack: {ex.StackTrace}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void GroupMode_Changed(object sender, RoutedEventArgs e)
        {
            // Null checks - event can fire during initialization before controls are ready
            if (NewGroupRadio == null || NewGroupSection == null || ExistingGroupSection == null || DeleteGroupSection == null || ExistingGroupsCombo == null)
            {
                return;
            }

            if (NewGroupRadio.IsChecked == true)
            {
                NewGroupSection.Visibility = Visibility.Visible;
                ExistingGroupSection.Visibility = Visibility.Collapsed;
                DeleteGroupSection.Visibility = Visibility.Collapsed;
                if (ManageGroupsSection != null) ManageGroupsSection.Visibility = Visibility.Collapsed;
            }
            else if (ManageGroupsRadio != null && ManageGroupsRadio.IsChecked == true)
            {
                NewGroupSection.Visibility = Visibility.Collapsed;
                ExistingGroupSection.Visibility = Visibility.Collapsed;
                DeleteGroupSection.Visibility = Visibility.Collapsed;
                if (ManageGroupsSection != null)
                {
                    ManageGroupsSection.Visibility = Visibility.Visible;
                    LoadGroupsForManagement();
                }
            }
            else if (DeleteGroupRadio.IsChecked == true)
            {
                NewGroupSection.Visibility = Visibility.Collapsed;
                ExistingGroupSection.Visibility = Visibility.Collapsed;
                DeleteGroupSection.Visibility = Visibility.Visible;
                if (ManageGroupsSection != null) ManageGroupsSection.Visibility = Visibility.Collapsed;
                LoadGroupsForDeletion();
            }
            else
            {
                // Check if there are any existing groups
                if (ExistingGroupsCombo.Items.Count == 0)
                {
                    MessageBox.Show("No existing groups found. Please create a new group first.", "No Groups", MessageBoxButton.OK, MessageBoxImage.Information);
                    NewGroupRadio.IsChecked = true;
                    return;
                }
                
                NewGroupSection.Visibility = Visibility.Collapsed;
                ExistingGroupSection.Visibility = Visibility.Visible;
                DeleteGroupSection.Visibility = Visibility.Collapsed;
                if (ManageGroupsSection != null) ManageGroupsSection.Visibility = Visibility.Collapsed;
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_dbService == null)
                {
                    MessageBox.Show("Database service is not available. Please restart the application.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (NewGroupRadio.IsChecked == true)
                {
                    // Create new group
                    if (string.IsNullOrWhiteSpace(NewGroupNameBox.Text))
                    {
                        MessageBox.Show("Please enter a group name", "Validation Error");
                        return;
                    }

                    var selectedPuppies = _displayPuppies.Where(p => p.IsSelected).ToList();
                    if (selectedPuppies.Count == 0)
                    {
                        MessageBox.Show("Please select at least one puppy", "Validation Error");
                        return;
                    }

                    string groupName = NewGroupNameBox.Text.Trim();

                    // Check if group already exists
                    var existingGroup = _dbService.GetAnimals()
                        .FirstOrDefault(a => a.GroupName == groupName);
                    if (existingGroup != null)
                    {
                        MessageBox.Show("A group with this name already exists", "Duplicate Group");
                        return;
                    }

                    // Update selected puppies with group name
                    foreach (var puppy in selectedPuppies)
                    {
                        var animal = _dbService.GetAnimals().FirstOrDefault(a => a.Id == puppy.Id);
                        if (animal != null)
                        {
                            animal.GroupName = groupName;
                            _dbService.UpdateAnimal(animal);
                        }
                    }

                    SelectedGroupName = groupName;
                    SelectedPuppyIds = selectedPuppies.Select(p => p.Id).ToList();
                    DialogResult = true;
                    Close();
                }
                else
                {
                    // Add to existing group
                    if (ExistingGroupsCombo.SelectedItem == null)
                    {
                        MessageBox.Show("Please select a group", "Validation Error");
                        return;
                    }

                    var selectedPuppies = _displayPuppies.Where(p => p.IsSelected).ToList();
                    if (selectedPuppies.Count == 0)
                    {
                        MessageBox.Show("Please select at least one puppy", "Validation Error");
                        return;
                    }

                    string? groupName = ExistingGroupsCombo.SelectedItem.ToString();
                    if (string.IsNullOrEmpty(groupName))
                    {
                        MessageBox.Show("Invalid group name", "Validation Error");
                        return;
                    }

                    // Update selected puppies with group name
                    foreach (var puppy in selectedPuppies)
                    {
                        var animal = _dbService.GetAnimals().FirstOrDefault(a => a.Id == puppy.Id);
                        if (animal != null)
                        {
                            animal.GroupName = groupName;
                            _dbService.UpdateAnimal(animal);
                        }
                    }

                    SelectedGroupName = groupName;
                    SelectedPuppyIds = selectedPuppies.Select(p => p.Id).ToList();
                    DialogResult = true;
                    Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving group: {ex.Message}", "Error");
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void AddNewAnimal_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var addAnimalWindow = new AddAnimalWindow();
                
                if (addAnimalWindow.ShowDialog() == true && addAnimalWindow.ResultAnimal != null)
                {
                    try
                    {
                        if (_dbService != null)
                        {
                            var animal = addAnimalWindow.ResultAnimal;
                            _dbService.AddAnimal(animal);
                            
                            MessageBox.Show($"Animal '{animal.Name}' added successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                            
                            // Reload data to show the new animal
                            LoadData();
                            
                            // Refresh the main window if it's the owner
                            if (Owner is MainWindow mainWindow)
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
                        MessageBox.Show($"Error adding animal: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening add animal window: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                "Are you sure you want to delete selected items?\n\nNote: This button is for demonstration. Please specify what should be deleted:\n- Selected animals?\n- Selected groups?\n- Current group?",
                "Delete Confirmation",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                MessageBox.Show("Delete functionality ready. Please clarify what should be deleted and I'll implement it.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void AddSelectedToNewGroup_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_dbService == null)
                {
                    MessageBox.Show("Database service is not available.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Validate group name
                string groupName = NewGroupNameBox.Text?.Trim() ?? "";
                if (string.IsNullOrWhiteSpace(groupName))
                {
                    MessageBox.Show("Please enter a group name.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Get selected puppies
                var selectedPuppies = _displayPuppies.Where(p => p.IsSelected).ToList();
                if (selectedPuppies.Count == 0)
                {
                    MessageBox.Show("Please select at least one puppy to add.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Add selected puppies to the group
                foreach (var puppy in selectedPuppies)
                {
                    var animal = _dbService.GetAnimals().FirstOrDefault(a => a.Id == puppy.Id);
                    if (animal != null)
                    {
                        animal.GroupName = groupName;
                        _dbService.UpdateAnimal(animal);
                    }
                }

                MessageBox.Show($"{selectedPuppies.Count} puppy/puppies added to group '{groupName}' successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                
                // Uncheck selected items
                foreach (var puppy in selectedPuppies)
                {
                    puppy.IsSelected = false;
                }
                
                // Refresh the list
                LoadData();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error adding selected puppies: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AddSelectedToExistingGroup_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_dbService == null)
                {
                    MessageBox.Show("Database service is not available.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Validate group selection
                if (ExistingGroupsCombo.SelectedItem == null)
                {
                    MessageBox.Show("Please select a group.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                string? groupName = ExistingGroupsCombo.SelectedItem.ToString();
                if (string.IsNullOrEmpty(groupName))
                {
                    MessageBox.Show("Invalid group name.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Get selected puppies
                var selectedPuppies = _displayPuppies.Where(p => p.IsSelected).ToList();
                if (selectedPuppies.Count == 0)
                {
                    MessageBox.Show("Please select at least one puppy to add.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Add selected puppies to the group
                foreach (var puppy in selectedPuppies)
                {
                    var animal = _dbService.GetAnimals().FirstOrDefault(a => a.Id == puppy.Id);
                    if (animal != null)
                    {
                        animal.GroupName = groupName;
                        _dbService.UpdateAnimal(animal);
                    }
                }

                MessageBox.Show($"{selectedPuppies.Count} puppy/puppies added to group '{groupName}' successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                
                // Uncheck selected items
                foreach (var puppy in selectedPuppies)
                {
                    puppy.IsSelected = false;
                }
                
                // Refresh the list and group members
                LoadData();
                LoadGroupMembers();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error adding selected puppies: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExistingGroup_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            // Null check - event can fire during initialization
            if (ExistingGroupsCombo == null || GroupMembersList == null || GroupMembersPanel == null)
            {
                return;
            }
            
            LoadGroupMembers();
        }

        private void LoadGroupMembers()
        {
            if (_dbService == null || GroupMembersPanel == null || GroupMembersList == null || ExistingGroupsCombo == null)
            {
                if (GroupMembersPanel != null)
                    GroupMembersPanel.Visibility = Visibility.Collapsed;
                return;
            }

            if (ExistingGroupsCombo.SelectedItem == null)
            {
                GroupMembersPanel.Visibility = Visibility.Collapsed;
                return;
            }

            string? groupName = ExistingGroupsCombo.SelectedItem.ToString();
            if (string.IsNullOrEmpty(groupName))
            {
                GroupMembersPanel.Visibility = Visibility.Collapsed;
                return;
            }

            try
            {
                var groupMembers = _dbService.GetAnimals()
                    .Where(a => a.GroupName == groupName)
                    .Select(a => $"{a.Name} ({a.Breed}, {a.Sex}, {a.Status})")
                    .ToList();

                if (groupMembers.Count > 0)
                {
                    GroupMembersList.ItemsSource = groupMembers;
                    GroupMembersPanel.Visibility = Visibility.Visible;
                }
                else
                {
                    GroupMembersPanel.Visibility = Visibility.Collapsed;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading group members: {ex.Message}", "Error");
                GroupMembersPanel.Visibility = Visibility.Collapsed;
            }
        }

        private void ViewGroup_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_dbService == null)
                {
                    MessageBox.Show("Database service is not available. Please restart the application.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (ExistingGroupsCombo.SelectedItem == null)
                {
                    MessageBox.Show("Please select a group first.", "No Group Selected", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                string? groupName = ExistingGroupsCombo.SelectedItem.ToString();
                if (string.IsNullOrEmpty(groupName))
                {
                    MessageBox.Show("Invalid group name.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var groupMembers = _dbService.GetAnimals()
                    .Where(a => a.GroupName == groupName)
                    .ToList();

                if (groupMembers.Count == 0)
                {
                    MessageBox.Show($"No animals found in group '{groupName}'.\n\nThis group doesn't exist yet or has no members.", "Empty Group", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                // Open the new themed window
                var detailsWindow = new ViewGroupDetailsWindow(groupName);
                detailsWindow.Owner = this;
                detailsWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error viewing group details: {ex.Message}\n\nPlease ensure the database is accessible.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AddNewAnimalToGroup_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_dbService == null)
                {
                    MessageBox.Show("Database service is not available. Please restart the application.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (ExistingGroupsCombo.SelectedItem == null)
                {
                    MessageBox.Show("Please select a group first.", "No Group Selected", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                string? groupName = ExistingGroupsCombo.SelectedItem.ToString();
                if (string.IsNullOrEmpty(groupName))
                {
                    MessageBox.Show("Invalid group name.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var addAnimalWindow = new AddAnimalWindow();
                addAnimalWindow.SetGroupName(groupName, locked: true);

                if (addAnimalWindow.ShowDialog() == true && addAnimalWindow.ResultAnimal != null)
                {
                    try
                    {
                        var animal = addAnimalWindow.ResultAnimal;
                        _dbService.AddAnimal(animal);
                        
                        MessageBox.Show($"Animal '{animal.Name}' added to group '{groupName}' successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                        
                        // Reload data to show the new animal in this window
                        LoadData();
                        LoadGroupMembers();
                        
                        // Refresh the main window if it's the owner
                        if (Owner is MainWindow mainWindow)
                        {
                            var viewModel = mainWindow.DataContext as ViewModels.MainViewModel;
                            if (viewModel != null)
                            {
                                _ = viewModel.LoadDataAsync();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error adding animal: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening add animal window: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadGroupsForDeletion()
        {
            try
            {
                if (_dbService == null || GroupsToDeleteListBox == null)
                {
                    return;
                }

                // Get all existing groups
                var existingGroups = _dbService.GetAnimals()
                    .Where(a => !string.IsNullOrEmpty(a.GroupName))
                    .Select(a => a.GroupName)
                    .Distinct()
                    .OrderBy(g => g)
                    .ToList();

                // Get animal counts for each group
                var groupCounts = _dbService.GetAnimals()
                    .Where(a => !string.IsNullOrEmpty(a.GroupName))
                    .GroupBy(a => a.GroupName!)
                    .ToDictionary(g => g.Key, g => g.Count());

                var groupItems = new ObservableCollection<PuppyItem>();
                foreach (var group in existingGroups)
                {
                    int count = group != null && groupCounts.ContainsKey(group) ? groupCounts[group] : 0;
                    groupItems.Add(new PuppyItem
                    {
                        Id = 0, // Not used for groups
                        DisplayText = $"{group} ({count} animals)",
                        IsSelected = false
                    });
                }

                GroupsToDeleteListBox.ItemsSource = groupItems;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading groups: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DeleteSelectedGroups_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_dbService == null || GroupsToDeleteListBox == null)
                {
                    MessageBox.Show("Database service is not available.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var items = GroupsToDeleteListBox.ItemsSource as ObservableCollection<PuppyItem>;
                if (items == null)
                {
                    return;
                }

                var selectedGroups = items.Where(p => p.IsSelected).ToList();
                if (selectedGroups.Count == 0)
                {
                    MessageBox.Show("Please select at least one group to delete.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Confirmation dialog
                string groupList = string.Join("\n• ", selectedGroups.Select(g => g.DisplayText));
                var result = MessageBox.Show(
                    $"Are you sure you want to delete the following groups?\n\n• {groupList}\n\nThis will remove the group name from all animals in these groups. The animals themselves will NOT be deleted.",
                    "Confirm Group Deletion",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result != MessageBoxResult.Yes)
                {
                    return;
                }

                // Delete the groups (remove group name from animals)
                int totalAnimalsUpdated = 0;
                foreach (var groupItem in selectedGroups)
                {
                    // Extract group name (remove the count part)
                    string? fullText = groupItem.DisplayText;
                    if (string.IsNullOrEmpty(fullText)) continue;

                    int indexOfParen = fullText.LastIndexOf('(');
                    string groupName = indexOfParen > 0 ? fullText.Substring(0, indexOfParen).Trim() : fullText;

                    // Find all animals in this group and remove the group name
                    var animalsInGroup = _dbService.GetAnimals()
                        .Where(a => a.GroupName == groupName)
                        .ToList();

                    foreach (var animal in animalsInGroup)
                    {
                        animal.GroupName = null;
                        _dbService.UpdateAnimal(animal);
                        totalAnimalsUpdated++;
                    }
                }

                MessageBox.Show($"Successfully deleted {selectedGroups.Count} group(s).\n{totalAnimalsUpdated} animal(s) updated.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                // Refresh the groups list
                LoadGroupsForDeletion();
                LoadData(); // Refresh main data too
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error deleting groups: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadGroupsForManagement()
        {
            try
            {
                if (_dbService == null || ManageGroupsListBox == null)
                {
                    return;
                }

                // Get all groups with their member counts
                var groupsWithCounts = _dbService.GetAnimals()
                    .Where(a => !string.IsNullOrEmpty(a.GroupName))
                    .GroupBy(a => a.GroupName)
                    .Select(g => new GroupManagementItem
                    {
                        GroupName = g.Key ?? "",
                        MemberCount = g.Count(),
                        MemberCountText = $"{g.Count()} member{(g.Count() == 1 ? "" : "s")}"
                    })
                    .OrderBy(g => g.GroupName)
                    .ToList();

                ManageGroupsListBox.ItemsSource = new ObservableCollection<GroupManagementItem>(groupsWithCounts);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading groups for management: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ManageGroup_DoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            try
            {
                // Check if the double-click was on a CheckBox - if so, ignore it
                if (e.OriginalSource is System.Windows.Controls.CheckBox || 
                    e.OriginalSource is System.Windows.Shapes.Path) // CheckBox internal elements
                {
                    return;
                }

                if (ManageGroupsListBox.SelectedItem is GroupManagementItem selectedGroup)
                {
                    OpenGroupDetailsWindow(selectedGroup.GroupName);
                    e.Handled = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening group details: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void GroupItem_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            // This handler helps with selection - find the data context
            if (sender is System.Windows.FrameworkElement element && element.DataContext is GroupManagementItem item)
            {
                // Select the item in the ListBox
                ManageGroupsListBox.SelectedItem = item;
                
                // If double-click, open details
                if (e.ClickCount == 2)
                {
                    OpenGroupDetailsWindow(item.GroupName);
                    e.Handled = true;
                }
            }
        }

        private void OpenGroupDetails_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (ManageGroupsListBox == null)
                {
                    return;
                }

                var items = ManageGroupsListBox.ItemsSource as ObservableCollection<GroupManagementItem>;
                if (items == null)
                {
                    return;
                }

                // First check for checked items (via checkbox)
                var checkedGroups = items.Where(g => g.IsSelected).ToList();
                if (checkedGroups.Count > 0)
                {
                    // Open the first checked group
                    OpenGroupDetailsWindow(checkedGroups[0].GroupName);
                    return;
                }

                // Fall back to selected item (via clicking)
                if (ManageGroupsListBox.SelectedItem is GroupManagementItem selectedGroup)
                {
                    OpenGroupDetailsWindow(selectedGroup.GroupName);
                }
                else
                {
                    MessageBox.Show("Please select or check a group to manage.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening group details: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DeleteGroupsFromManage_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_dbService == null || ManageGroupsListBox == null)
                {
                    MessageBox.Show("Database service is not available.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var items = ManageGroupsListBox.ItemsSource as ObservableCollection<GroupManagementItem>;
                if (items == null)
                {
                    return;
                }

                var selectedGroups = items.Where(g => g.IsSelected).ToList();
                if (selectedGroups.Count == 0)
                {
                    MessageBox.Show("Please select at least one group to delete.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Confirmation dialog
                string groupList = string.Join("\n• ", selectedGroups.Select(g => $"{g.GroupName} ({g.MemberCount} members)"));
                var result = MessageBox.Show(
                    $"Are you sure you want to delete the following groups?\n\n• {groupList}\n\nThis will remove the group name from all animals in these groups. The animals themselves will NOT be deleted.",
                    "Confirm Group Deletion",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result != MessageBoxResult.Yes)
                {
                    return;
                }

                // Delete the groups (remove group name from animals)
                int totalAnimalsUpdated = 0;
                foreach (var groupItem in selectedGroups)
                {
                    string groupName = groupItem.GroupName;

                    // Find all animals in this group and remove the group name
                    var animalsInGroup = _dbService.GetAnimals()
                        .Where(a => a.GroupName == groupName)
                        .ToList();

                    foreach (var animal in animalsInGroup)
                    {
                        animal.GroupName = null;
                        _dbService.UpdateAnimal(animal);
                        totalAnimalsUpdated++;
                    }
                }

                MessageBox.Show($"Successfully deleted {selectedGroups.Count} group(s).\n{totalAnimalsUpdated} animal(s) updated.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                // Refresh the groups list
                LoadGroupsForManagement();
                LoadData(); // Refresh main data too
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error deleting groups: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OpenGroupDetailsWindow(string groupName)
        {
            try
            {
                var groupDetailsWindow = new ViewGroupDetailsWindow(groupName);
                groupDetailsWindow.Owner = this;
                groupDetailsWindow.ShowDialog();
                
                // Refresh the list after closing the details window
                LoadGroupsForManagement();
                LoadData();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening group details window: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    // Helper class for displaying puppies in the ListBox
    public class PuppyItem : INotifyPropertyChanged
    {
        private bool _isSelected;

        public int Id { get; set; }
        public string? DisplayText { get; set; }
        
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    OnPropertyChanged(nameof(IsSelected));
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    // Helper class for displaying groups in management mode
    public class GroupManagementItem : INotifyPropertyChanged
    {
        private bool _isSelected;

        public string GroupName { get; set; } = "";
        public int MemberCount { get; set; }
        public string MemberCountText { get; set; } = "";
        
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    OnPropertyChanged(nameof(IsSelected));
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
