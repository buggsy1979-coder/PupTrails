using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using PupTrailsV3.Data;
using PupTrailsV3.Models;
using A = DocumentFormat.OpenXml.Drawing;
using DW = DocumentFormat.OpenXml.Drawing.Wordprocessing;
using PIC = DocumentFormat.OpenXml.Drawing.Pictures;

namespace PupTrailsV3.Services
{
    public static class MasterExportService
    {
        /// <summary>
        /// Exports all PupTrail data to a comprehensive Word document
        /// </summary>
        public static async Task<string> ExportAllDataAsync(string outputPath)
        {
            using var context = new PupTrailDbContext();
            var exportTimestamp = DateTime.Now;
            
            // Create Word document
            using var wordDoc = WordprocessingDocument.Create(outputPath, WordprocessingDocumentType.Document);
            var mainPart = wordDoc.AddMainDocumentPart();
            mainPart.Document = new Document();
            var body = mainPart.Document.AppendChild(new Body());
            
            // Add title
            AddTitle(body, "PupTrail Master Export");
            AddParagraph(body, $"Export Date: {exportTimestamp:MMMM dd, yyyy HH:mm:ss}");
            AddParagraph(body, "");
            
            // Export each section
            await ExportAnimals(context, body, mainPart);
            await ExportPeople(context, body);
            await ExportVetVisits(context, body);
            await ExportAdoptions(context, body);
            await ExportExpenses(context, body);
            await ExportIncome(context, body);
            await ExportIntakes(context, body);
            await ExportMoneyOwed(context, body);
            await ExportPuppyGroups(context, body);
            
            AddExportFooter(mainPart, exportTimestamp);
            mainPart.Document.Save();
            
            return outputPath;
        }

        private static void AddTitle(Body body, string text)
        {
            var para = body.AppendChild(new Paragraph());
            var run = para.AppendChild(new Run());
            var runProps = run.AppendChild(new RunProperties());
            runProps.AppendChild(new Bold());
            runProps.AppendChild(new FontSize() { Val = "32" });
            run.AppendChild(new Text(text));
        }

        private static void AddHeading(Body body, string text)
        {
            var para = body.AppendChild(new Paragraph());
            var run = para.AppendChild(new Run());
            var runProps = run.AppendChild(new RunProperties());
            runProps.AppendChild(new Bold());
            runProps.AppendChild(new FontSize() { Val = "24" });
            run.AppendChild(new Text(text));
            body.AppendChild(new Paragraph()); // Add spacing
        }

        private static void AddParagraph(Body body, string text)
        {
            var para = body.AppendChild(new Paragraph());
            var run = para.AppendChild(new Run());
            run.AppendChild(new Text(text));
        }

        private static void AddBoldParagraph(Body body, string text)
        {
            var para = body.AppendChild(new Paragraph());
            var run = para.AppendChild(new Run());
            var runProps = run.AppendChild(new RunProperties());
            runProps.AppendChild(new Bold());
            run.AppendChild(new Text(text));
        }

        private static void AddExportFooter(MainDocumentPart mainPart, DateTime exportTimestamp)
        {
            var footerPart = mainPart.AddNewPart<FooterPart>();
            var footer = new Footer();

            var para = new Paragraph();
            var run = new Run();
            run.AppendChild(new Text($"PupTrail Master Export - Created {exportTimestamp:yyyy-MM-dd HH:mm:ss}"));
            para.AppendChild(run);
            footer.AppendChild(para);

            footerPart.Footer = footer;

            var documentBody = mainPart.Document.Body ?? mainPart.Document.AppendChild(new Body());
            var sectionProps = documentBody.Elements<SectionProperties>().FirstOrDefault();
            if (sectionProps == null)
            {
                sectionProps = new SectionProperties();
                documentBody.Append(sectionProps);
            }

            sectionProps.RemoveAllChildren<FooterReference>();
            sectionProps.AppendChild(new FooterReference
            {
                Id = mainPart.GetIdOfPart(footerPart),
                Type = HeaderFooterValues.Default
            });
        }

        private static async Task ExportAnimals(PupTrailDbContext context, Body body, MainDocumentPart mainPart)
        {
            AddHeading(body, "Animals");
            var animals = await context.Animals
                .Where(a => !a.IsDeleted)
                .OrderBy(a => a.Name)
                .ToListAsync();
            
            AddParagraph(body, $"Total Animals: {animals.Count}");
            AddParagraph(body, "");
            
            foreach (var animal in animals)
            {
                AddBoldParagraph(body, $"Name: {animal.Name}");
                if (!string.IsNullOrEmpty(animal.TempName))
                    AddParagraph(body, $"  Temp Name: {animal.TempName}");
                AddParagraph(body, $"  Breed: {animal.Breed ?? "N/A"}");
                AddParagraph(body, $"  Sex: {animal.Sex ?? "N/A"}");
                AddParagraph(body, $"  Colour: {animal.Colour ?? "N/A"}");
                AddParagraph(body, $"  Collar Color: {animal.CollarColor ?? "N/A"}");
                if (animal.Weight.HasValue)
                    AddParagraph(body, $"  Weight: {animal.Weight:F2} lbs");
                if (animal.DOB.HasValue)
                    AddParagraph(body, $"  Date of Birth: {animal.DOB:MMMM dd, yyyy}");
                AddParagraph(body, $"  Intake Date: {animal.IntakeDate:MMMM dd, yyyy}");
                AddParagraph(body, $"  Status: {animal.Status}");
                AddParagraph(body, $"  Origin Location: {animal.OriginLocation ?? "N/A"}");
                AddParagraph(body, $"  Origin Country: {animal.OriginCountry}");
                if (!string.IsNullOrEmpty(animal.Microchip))
                    AddParagraph(body, $"  Microchip: {animal.Microchip}");
                if (!string.IsNullOrEmpty(animal.GroupName))
                    AddParagraph(body, $"  Group Name: {animal.GroupName}");
                if (!string.IsNullOrEmpty(animal.Notes))
                    AddParagraph(body, $"  Notes: {animal.Notes}");
                
                // Add photo if available
                if (!string.IsNullOrEmpty(animal.PhotoPath))
                {
                    string fullPhotoPath = PathManager.ResolveAnimalPhotoPath(animal.PhotoPath);
                    if (!string.IsNullOrEmpty(fullPhotoPath) && File.Exists(fullPhotoPath))
                    {
                        AddImageToBody(mainPart, body, fullPhotoPath);
                    }
                }
                
                AddParagraph(body, "");
            }
        }

        private static async Task ExportPeople(PupTrailDbContext context, Body body)
        {
            AddHeading(body, "People");
            var people = await context.People
                .Where(p => !p.IsDeleted)
                .OrderBy(p => p.Name)
                .ToListAsync();
            
            AddParagraph(body, $"Total People: {people.Count}");
            AddParagraph(body, "");
            
            foreach (var person in people)
            {
                AddBoldParagraph(body, $"Name: {person.Name}");
                AddParagraph(body, $"  Type: {person.Type}");
                if (!string.IsNullOrEmpty(person.Email))
                    AddParagraph(body, $"  Email: {person.Email}");
                if (!string.IsNullOrEmpty(person.Phone))
                    AddParagraph(body, $"  Phone: {person.Phone}");
                if (!string.IsNullOrEmpty(person.Address))
                    AddParagraph(body, $"  Address: {person.Address}");
                if (!string.IsNullOrEmpty(person.Notes))
                    AddParagraph(body, $"  Notes: {person.Notes}");
                AddParagraph(body, "");
            }
        }

        private static async Task ExportVetVisits(PupTrailDbContext context, Body body)
        {
            AddHeading(body, "Veterinary Visits");
            var vetVisits = await context.VetVisits
                .Include(v => v.Animal)
                .Include(v => v.Person)
                .Include(v => v.Services)
                .Where(v => !v.IsDeleted)
                .OrderByDescending(v => v.Date)
                .ToListAsync();
            
            AddParagraph(body, $"Total Vet Visits: {vetVisits.Count}");
            AddParagraph(body, "");
            
            foreach (var visit in vetVisits)
            {
                AddBoldParagraph(body, $"Visit Date: {visit.Date:MMMM dd, yyyy}");
                AddParagraph(body, $"  Animal: {visit.Animal?.Name ?? "N/A"}");
                AddParagraph(body, $"  Veterinarian: {visit.Person?.Name ?? "N/A"}");
                AddParagraph(body, $"  Total Cost: ${visit.TotalCost:F2}");
                AddParagraph(body, $"  Ready for Adoption: {(visit.ReadyForAdoption ? "Yes" : "No")}");
                
                if (visit.WormingDate.HasValue)
                    AddParagraph(body, $"  Worming: {visit.WormingDate:MMMM dd, yyyy} - ${visit.WormingCost ?? 0:F2}");
                if (visit.DeFleeingDate.HasValue)
                    AddParagraph(body, $"  Flea Treatment: {visit.DeFleeingDate:MMMM dd, yyyy} - ${visit.DeFleeingCost ?? 0:F2}");
                if (visit.DentalDate.HasValue)
                    AddParagraph(body, $"  Dental: {visit.DentalDate:MMMM dd, yyyy} - ${visit.DentalCost ?? 0:F2}");
                if (visit.SpayedNeuteringDate.HasValue)
                    AddParagraph(body, $"  Spay/Neuter: {visit.SpayedNeuteringDate:MMMM dd, yyyy} - ${visit.SpayedNeuteringCost ?? 0:F2}");
                
                if (visit.Services.Any())
                {
                    AddParagraph(body, "  Services:");
                    foreach (var service in visit.Services)
                    {
                        AddParagraph(body, $"    - {service.ServiceName}: ${service.Cost:F2}");
                    }
                }
                
                if (!string.IsNullOrEmpty(visit.Notes))
                    AddParagraph(body, $"  Notes: {visit.Notes}");
                if (!string.IsNullOrEmpty(visit.InvoicePath) && File.Exists(visit.InvoicePath))
                    AddParagraph(body, $"  Invoice: {visit.InvoicePath}");
                AddParagraph(body, "");
            }
        }

        private static async Task ExportAdoptions(PupTrailDbContext context, Body body)
        {
            AddHeading(body, "Adoptions");
            var adoptions = await context.Adoptions
                .Include(a => a.Animal)
                .Include(a => a.Person)
                .Where(a => !a.IsDeleted)
                .OrderByDescending(a => a.Date)
                .ToListAsync();
            
            AddParagraph(body, $"Total Adoptions: {adoptions.Count}");
            AddParagraph(body, "");
            
            foreach (var adoption in adoptions)
            {
                AddBoldParagraph(body, $"Adoption Date: {adoption.Date:MMMM dd, yyyy}");
                AddParagraph(body, $"  Animal: {adoption.Animal?.Name ?? "N/A"}");
                AddParagraph(body, $"  Adopter: {adoption.Person?.Name ?? "N/A"}");
                if (adoption.AgreedFee.HasValue)
                    AddParagraph(body, $"  Agreed Fee: ${adoption.AgreedFee:F2}");
                if (adoption.PaidFee.HasValue)
                    AddParagraph(body, $"  Paid Fee: ${adoption.PaidFee:F2}");
                AddParagraph(body, $"  Payment Status: {(adoption.Paid ? "Paid" : "Unpaid")}");
                if (!string.IsNullOrEmpty(adoption.ContractPath) && File.Exists(adoption.ContractPath))
                    AddParagraph(body, $"  Contract: {adoption.ContractPath}");
                if (!string.IsNullOrEmpty(adoption.Notes))
                    AddParagraph(body, $"  Notes: {adoption.Notes}");
                AddParagraph(body, "");
            }
        }

        private static async Task ExportExpenses(PupTrailDbContext context, Body body)
        {
            AddHeading(body, "Expenses");
            var expenses = await context.Expenses
                .Include(e => e.Animal)
                .Include(e => e.Trip)
                .Where(e => !e.IsDeleted)
                .OrderByDescending(e => e.Date)
                .ToListAsync();
            
            AddParagraph(body, $"Total Expenses: {expenses.Count}");
            AddParagraph(body, $"Total Amount: ${expenses.Sum(e => e.Amount):F2}");
            AddParagraph(body, "");
            
            foreach (var expense in expenses)
            {
                AddBoldParagraph(body, $"Date: {expense.Date:MMMM dd, yyyy}");
                AddParagraph(body, $"  Category: {expense.Category}");
                AddParagraph(body, $"  Amount: ${expense.Amount:F2} {expense.Currency}");
                if (expense.Animal != null)
                    AddParagraph(body, $"  Related Animal: {expense.Animal.Name}");
                if (expense.Trip != null)
                    AddParagraph(body, $"  Related Trip: {expense.Trip.Purpose}");
                if (!string.IsNullOrEmpty(expense.Notes))
                    AddParagraph(body, $"  Notes: {expense.Notes}");
                if (!string.IsNullOrEmpty(expense.ReceiptPath) && File.Exists(expense.ReceiptPath))
                    AddParagraph(body, $"  Receipt: {expense.ReceiptPath}");
                AddParagraph(body, "");
            }
        }

        private static async Task ExportIncome(PupTrailDbContext context, Body body)
        {
            AddHeading(body, "Income");
            var incomes = await context.Incomes
                .Include(i => i.Animal)
                .Include(i => i.Person)
                .Where(i => !i.IsDeleted)
                .OrderByDescending(i => i.Date)
                .ToListAsync();
            
            AddParagraph(body, $"Total Income Entries: {incomes.Count}");
            AddParagraph(body, $"Total Amount: ${incomes.Sum(i => i.Amount):F2}");
            AddParagraph(body, "");
            
            foreach (var income in incomes)
            {
                AddBoldParagraph(body, $"Date: {income.Date:MMMM dd, yyyy}");
                AddParagraph(body, $"  Type: {income.Type}");
                AddParagraph(body, $"  Amount: ${income.Amount:F2} {income.Currency}");
                if (income.Person != null)
                    AddParagraph(body, $"  From: {income.Person.Name}");
                if (income.Animal != null)
                    AddParagraph(body, $"  Related Animal: {income.Animal.Name}");
                if (!string.IsNullOrEmpty(income.GroupName))
                    AddParagraph(body, $"  Group Name: {income.GroupName}");
                if (!string.IsNullOrEmpty(income.Notes))
                    AddParagraph(body, $"  Notes: {income.Notes}");
                AddParagraph(body, "");
            }
        }
        private static async Task ExportIntakes(PupTrailDbContext context, Body body)
        {
            AddHeading(body, "Intake Records");
            var intakes = await context.Intakes
                .OrderByDescending(i => i.Date)
                .ToListAsync();
            
            AddParagraph(body, $"Total Intake Records: {intakes.Count}");
            AddParagraph(body, "");
            
            foreach (var intake in intakes)
            {
                AddBoldParagraph(body, $"Date: {intake.Date:MMMM dd, yyyy}");
                AddParagraph(body, $"  Puppy Count: {intake.PuppyCount}");
                if (!string.IsNullOrEmpty(intake.Location))
                    AddParagraph(body, $"  Location: {intake.Location}");
                if (intake.CostPerLitter.HasValue)
                    AddParagraph(body, $"  Cost Per Litter: ${intake.CostPerLitter:F2}");
                AddParagraph(body, $"  Cost Per Puppy: ${intake.CostPerPuppy:F2}");
                AddParagraph(body, $"  Total Cost: ${intake.TotalCost:F2}");
                if (!string.IsNullOrEmpty(intake.Notes))
                    AddParagraph(body, $"  Notes: {intake.Notes}");
                AddParagraph(body, "");
            }
        }

        private static async Task ExportMoneyOwed(PupTrailDbContext context, Body body)
        {
            AddHeading(body, "Money Owed Records");
            var moneyOwed = await context.MoneyOwed
                .OrderByDescending(m => m.Date)
                .ToListAsync();
            
            AddParagraph(body, $"Total Money Owed Records: {moneyOwed.Count}");
            AddParagraph(body, $"Total Outstanding: ${moneyOwed.Sum(m => m.TotalOwed):F2}");
            AddParagraph(body, "");
            
            foreach (var record in moneyOwed)
            {
                AddBoldParagraph(body, $"Date: {record.Date:MMMM dd, yyyy}");
                if (!string.IsNullOrEmpty(record.Debtor))
                    AddParagraph(body, $"  Debtor: {record.Debtor}");
                AddParagraph(body, $"  Amount Owed: ${record.AmountOwed:F2}");
                AddParagraph(body, $"  Amount Paid: ${record.AmountPaid:F2}");
                AddParagraph(body, $"  Total Owed: ${record.TotalOwed:F2}");
                AddParagraph(body, $"  Status: {(record.IsFullyPaid ? "Fully Paid" : "Outstanding")}");
                if (record.DatePaid.HasValue)
                    AddParagraph(body, $"  Date Paid: {record.DatePaid:MMMM dd, yyyy}");
                if (!string.IsNullOrEmpty(record.Reason))
                    AddParagraph(body, $"  Reason: {record.Reason}");
                if (!string.IsNullOrEmpty(record.Notes))
                    AddParagraph(body, $"  Notes: {record.Notes}");
                AddParagraph(body, "");
            }
        }

        private static async Task ExportPuppyGroups(PupTrailDbContext context, Body body)
        {
            AddHeading(body, "Puppy Groups");
            var groups = await context.PuppyGroups
                .OrderBy(g => g.GroupName)
                .ToListAsync();
            
            AddParagraph(body, $"Total Puppy Groups: {groups.Count}");
            AddParagraph(body, "");
            
            foreach (var group in groups)
            {
                AddBoldParagraph(body, $"Group Name: {group.GroupName}");
                if (group.DateCreated.HasValue)
                    AddParagraph(body, $"  Date Created: {group.DateCreated:MMMM dd, yyyy}");
                if (!string.IsNullOrEmpty(group.ImagePath) && File.Exists(group.ImagePath))
                    AddParagraph(body, $"  Image: {group.ImagePath}");
                if (!string.IsNullOrEmpty(group.Notes))
                    AddParagraph(body, $"  Notes: {group.Notes}");
                AddParagraph(body, "");
            }
        }

        private static void AddImageToBody(MainDocumentPart mainPart, Body body, string imagePath)
        {
            try
            {
                // Determine image format based on file extension
                var ext = Path.GetExtension(imagePath).ToLower();
                
                ImagePart? imagePart = null;
                
                if (ext == ".png")
                    imagePart = mainPart.AddImagePart(ImagePartType.Png);
                else if (ext == ".gif")
                    imagePart = mainPart.AddImagePart(ImagePartType.Gif);
                else if (ext == ".bmp")
                    imagePart = mainPart.AddImagePart(ImagePartType.Bmp);
                else
                    imagePart = mainPart.AddImagePart(ImagePartType.Jpeg); // Default to JPEG

                if (imagePart != null)
                {
                    using (FileStream stream = new FileStream(imagePath, FileMode.Open, FileAccess.Read))
                    {
                        imagePart.FeedData(stream);
                    }

                    AddImageToElement(mainPart.GetIdOfPart(imagePart), body);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error adding image to document: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Image path: {imagePath}");
            }
        }

        private static void AddImageToElement(string relationshipId, Body body)
        {
            // Define the reference of the image
            var element =
                new Drawing(
                    new DW.Inline(
                        new DW.Extent() { Cx = 5000000L, Cy = 3750000L }, // Image size (width and height in EMUs)
                        new DW.EffectExtent()
                        {
                            LeftEdge = 0L,
                            TopEdge = 0L,
                            RightEdge = 0L,
                            BottomEdge = 0L
                        },
                        new DW.DocProperties()
                        {
                            Id = (UInt32Value)1U,
                            Name = "Picture 1"
                        },
                        new DW.NonVisualGraphicFrameDrawingProperties(
                            new A.GraphicFrameLocks() { NoChangeAspect = true }),
                        new A.Graphic(
                            new A.GraphicData(
                                new PIC.Picture(
                                    new PIC.NonVisualPictureProperties(
                                        new PIC.NonVisualDrawingProperties()
                                        {
                                            Id = (UInt32Value)0U,
                                            Name = "Animal Photo"
                                        },
                                        new PIC.NonVisualPictureDrawingProperties()),
                                    new PIC.BlipFill(
                                        new A.Blip(
                                            new A.BlipExtensionList(
                                                new A.BlipExtension()
                                                {
                                                    Uri = "{28A0092B-C50C-407E-A947-70E740481C1C}"
                                                })
                                        )
                                        {
                                            Embed = relationshipId,
                                            CompressionState = A.BlipCompressionValues.Print
                                        },
                                        new A.Stretch(
                                            new A.FillRectangle())),
                                    new PIC.ShapeProperties(
                                        new A.Transform2D(
                                            new A.Offset() { X = 0L, Y = 0L },
                                            new A.Extents() { Cx = 5000000L, Cy = 3750000L }),
                                        new A.PresetGeometry(
                                            new A.AdjustValueList()
                                        )
                                        { Preset = A.ShapeTypeValues.Rectangle }))
                            )
                            { Uri = "http://schemas.openxmlformats.org/drawingml/2006/picture" })
                    )
                    {
                        DistanceFromTop = (UInt32Value)0U,
                        DistanceFromBottom = (UInt32Value)0U,
                        DistanceFromLeft = (UInt32Value)0U,
                        DistanceFromRight = (UInt32Value)0U
                    });

            body.AppendChild(new Paragraph(new Run(element)));
        }
    }
}