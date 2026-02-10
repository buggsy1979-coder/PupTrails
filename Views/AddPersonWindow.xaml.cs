using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using Microsoft.Win32;
using PupTrailsV3.Models;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;

namespace PupTrailsV3.Views
{
    public partial class AddPersonWindow : Window
    {
        public Person? ResultPerson { get; private set; }
        private int? _personId = null;

        public AddPersonWindow()
        {
            InitializeComponent();
        }

        public void LoadPerson(Person person)
        {
            _personId = person.Id;
            NameBox.Text = person.Name;
            
            // Set Type ComboBox
            if (TypeBox.Items.Count > 0)
            {
                foreach (System.Windows.Controls.ComboBoxItem item in TypeBox.Items)
                {
                    if (item.Content.ToString() == person.Type)
                    {
                        TypeBox.SelectedItem = item;
                        break;
                    }
                }
            }
            
            EmailBox.Text = person.Email;
            PhoneBox.Text = person.Phone;
            AddressBox.Text = person.Address;
            NotesBox.Text = person.Notes ?? "";
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(NameBox.Text))
            {
                MessageBox.Show("Please enter a name", "Validation Error");
                return;
            }

            ResultPerson = new Person
            {
                Id = _personId ?? 0,
                Name = NameBox.Text,
                Type = (TypeBox.SelectedItem as System.Windows.Controls.ComboBoxItem)?.Content.ToString() ?? "Contact",
                Email = EmailBox.Text,
                Phone = PhoneBox.Text,
                Address = AddressBox.Text,
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
            NameBox.Text = string.Empty;
            TypeBox.SelectedIndex = -1;
            EmailBox.Text = string.Empty;
            PhoneBox.Text = string.Empty;
            AddressBox.Text = string.Empty;
            NotesBox.Text = string.Empty;
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            if (_personId == null)
            {
                MessageBox.Show("Cannot delete a record that hasn't been saved yet.", "Delete Error");
                return;
            }

            var result = MessageBox.Show("Are you sure you want to delete this person record?", "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result == MessageBoxResult.Yes)
            {
                using (var db = new Data.PupTrailDbContext())
                {
                    var person = db.People.Find(_personId);
                    if (person != null)
                    {
                        db.People.Remove(person);
                        db.SaveChanges();
                        MessageBox.Show("Person record deleted successfully.", "Success");
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

        private void Export_Click(object sender, RoutedEventArgs e)
        {
            if (_personId == null)
            {
                MessageBox.Show("Please save the person record before exporting.", "Export Error");
                return;
            }

            using (var db = new Data.PupTrailDbContext())
            {
                var person = db.People.Find(_personId);
                if (person == null)
                {
                    MessageBox.Show("Person record not found.", "Export Error");
                    return;
                }

                // Get all adoptions for this person if they are an adopter
                var adoptions = new List<dynamic>();
                if (person.Type == "Adopter")
                {
                    adoptions = db.Adoptions
                        .Where(a => a.PersonId == _personId.Value && !a.IsDeleted)
                        .Select(a => new
                        {
                            a.Id,
                            a.Date,
                            AnimalName = a.Animal.Name,
                            a.AgreedFee,
                            a.PaidFee,
                            a.Paid,
                            a.Notes
                        })
                        .ToList<dynamic>();
                }

                // Save dialog
                var saveDialog = new SaveFileDialog
                {
                    Filter = "Word Document (*.docx)|*.docx|All Files (*.*)|*.*",
                    FileName = $"{person.Name}_PersonRecord_{DateTime.Now:yyyyMMdd}.docx",
                    DefaultExt = "docx"
                };

                if (saveDialog.ShowDialog() == true)
                {
                    try
                    {
                        CreateWordDocument(saveDialog.FileName, person, adoptions);
                        MessageBox.Show($"Export successful!\n\nFile saved to:\n{saveDialog.FileName}\n\nThis file can be opened in Microsoft Word.", "Export Complete", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error saving file: {ex.Message}", "Export Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void CreateWordDocument(string filePath, Person person, List<dynamic> adoptions)
        {
            using (WordprocessingDocument wordDoc = WordprocessingDocument.Create(filePath, WordprocessingDocumentType.Document))
            {
                MainDocumentPart mainPart = wordDoc.AddMainDocumentPart();
                mainPart.Document = new Document();
                Body body = mainPart.Document.AppendChild(new Body());

                // Title
                Paragraph titlePara = body.AppendChild(new Paragraph());
                Run titleRun = titlePara.AppendChild(new Run());
                titleRun.AppendChild(new Text("Person Information Report"));
                RunProperties titleProps = titleRun.InsertAt(new RunProperties(), 0);
                titleProps.AppendChild(new Bold());
                titleProps.AppendChild(new FontSize() { Val = "32" });
                
                // Empty line
                body.AppendChild(new Paragraph());

                // Person Information
                AddLabelValueParagraph(body, "Name:", person.Name);
                AddLabelValueParagraph(body, "Type:", person.Type);
                AddLabelValueParagraph(body, "Email:", person.Email ?? "N/A");
                AddLabelValueParagraph(body, "Phone:", person.Phone ?? "N/A");
                AddLabelValueParagraph(body, "Address:", person.Address ?? "N/A");
                
                // Adoption Records (if applicable)
                if (adoptions.Count > 0)
                {
                    body.AppendChild(new Paragraph());
                    
                    Paragraph adoptionTitlePara = body.AppendChild(new Paragraph());
                    Run adoptionTitleRun = adoptionTitlePara.AppendChild(new Run());
                    adoptionTitleRun.AppendChild(new Text("Adoption Records"));
                    RunProperties adoptionTitleProps = adoptionTitleRun.InsertAt(new RunProperties(), 0);
                    adoptionTitleProps.AppendChild(new Bold());
                    adoptionTitleProps.AppendChild(new FontSize() { Val = "28" });
                    
                    body.AppendChild(new Paragraph());
                    
                    foreach (var adoption in adoptions)
                    {
                        AddLabelValueParagraph(body, "Date:", adoption.Date.ToString("yyyy-MM-dd"));
                        AddLabelValueParagraph(body, "Animal Name:", adoption.AnimalName);
                        AddLabelValueParagraph(body, "Agreed Fee:", "$" + adoption.AgreedFee?.ToString("F2"));
                        AddLabelValueParagraph(body, "Paid Fee:", "$" + adoption.PaidFee?.ToString("F2"));
                        AddLabelValueParagraph(body, "Payment Complete:", adoption.Paid ? "Yes" : "No");
                        if (!string.IsNullOrWhiteSpace(adoption.Notes))
                        {
                            AddLabelValueParagraph(body, "Notes:", adoption.Notes);
                        }
                        body.AppendChild(new Paragraph());
                    }
                }

                mainPart.Document.Save();
            }
        }

        private void AddLabelValueParagraph(Body body, string label, string value)
        {
            Paragraph para = body.AppendChild(new Paragraph());
            
            // Add label (bold)
            Run labelRun = para.AppendChild(new Run());
            labelRun.AppendChild(new Text(label));
            RunProperties labelProps = labelRun.InsertAt(new RunProperties(), 0);
            labelProps.AppendChild(new Bold());
            
            // Add space
            para.AppendChild(new Run(new Text(" ")));
            
            // Add value (normal)
            Run valueRun = para.AppendChild(new Run());
            valueRun.AppendChild(new Text(value ?? ""));
        }
    }
}
