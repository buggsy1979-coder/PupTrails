using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using PupTrailsV3.Models;
using A = DocumentFormat.OpenXml.Drawing;
using DW = DocumentFormat.OpenXml.Drawing.Wordprocessing;
using PIC = DocumentFormat.OpenXml.Drawing.Pictures;

namespace PupTrailsV3.Services
{
    public class SocialMediaExportService
    {
        private readonly DatabaseService _dbService;

        public SocialMediaExportService(DatabaseService dbService)
        {
            _dbService = dbService;
        }

        public async Task<string> GeneratePostTextAsync(int animalId, string platform, ExportOptions options)
        {
            var animal = await _dbService.GetAnimalByIdAsync(animalId);
            if (animal == null) return string.Empty;

            var vetVisits = await _dbService.GetVetVisitsByAnimalIdAsync(animalId) ?? new List<VetVisit>();
            
            System.Diagnostics.Debug.WriteLine($"=== GeneratePostTextAsync ===");
            System.Diagnostics.Debug.WriteLine($"Animal: {animal.Name} (ID: {animal.Id})");
            System.Diagnostics.Debug.WriteLine($"Vet Visits Count: {vetVisits.Count}");
            
            if (vetVisits.Any())
            {
                foreach (var visit in vetVisits)
                {
                    System.Diagnostics.Debug.WriteLine($"  Visit Date: {visit.Date:yyyy-MM-dd}, Vaccinations: {visit.VaccinationsGiven ?? "none"}, SpayedNeutered: {visit.SpayedNeuteringDate?.ToString("yyyy-MM-dd") ?? "no"}");
                }
            }
            
            System.Diagnostics.Debug.WriteLine($"Options - IncludeVetVisitDates: {options.IncludeVetVisitDates}, IncludeVaccinations: {options.IncludeVaccinations}");
            System.Diagnostics.Debug.WriteLine($"Options - IncludeSpayedNeutered: {options.IncludeSpayedNeutered}, IncludeWorming: {options.IncludeWorming}");
            System.Diagnostics.Debug.WriteLine($"Options - IncludeDeFleeing: {options.IncludeDeFleeing}, IncludeDental: {options.IncludeDental}");

            return GenerateFacebookPost(animal, vetVisits, options);
        }

        private string GenerateFacebookPost(Animal animal, List<VetVisit> vetVisits, ExportOptions options)
        {
            var sb = new StringBuilder();

            // Header
            if (options.IncludeName || !AnyInfoSelected(options))
            {
                sb.AppendLine($"🐾 Meet {animal.Name} - Looking for a Forever Home! 🐾");
                sb.AppendLine();
            }

            // Basic Information Section
            if (options.IncludeName || options.IncludeBreed || options.IncludeSex || 
                options.IncludeAge || options.IncludeStatus || options.IncludeWeight || !AnyInfoSelected(options))
            {
                sb.AppendLine("📋 About " + (options.IncludeName ? animal.Name : "This Pet") + ":");
                
                if ((options.IncludeBreed || !AnyInfoSelected(options)) && !string.IsNullOrEmpty(animal.Breed))
                    sb.AppendLine($"• Breed: {animal.Breed}");
                
                if ((options.IncludeAge || !AnyInfoSelected(options)) && animal.DOB.HasValue)
                {
                    var age = CalculateAge(animal.DOB.Value);
                    sb.AppendLine($"• Age: {age}");
                }
                
                if ((options.IncludeSex || !AnyInfoSelected(options)) && !string.IsNullOrEmpty(animal.Sex))
                    sb.AppendLine($"• Sex: {animal.Sex}");
                
                if ((options.IncludeWeight || !AnyInfoSelected(options)) && animal.Weight.HasValue)
                    sb.AppendLine($"• Weight: {animal.Weight} lbs");
                
                if ((options.IncludeStatus || !AnyInfoSelected(options)) && !string.IsNullOrEmpty(animal.Status))
                    sb.AppendLine($"• Status: {animal.Status}");

                if (options.IncludeCollarColor && !string.IsNullOrEmpty(animal.CollarColor))
                    sb.AppendLine($"• Collar Color: {animal.CollarColor}");

                if (options.IncludeIntakeDate)
                    sb.AppendLine($"• In our care since: {animal.IntakeDate:MMMM yyyy}");

                sb.AppendLine();
            }

            // Vet Information
            AddVetInformation(sb, animal, vetVisits, options, true);

            // Notes
            if (options.IncludeNotes && !string.IsNullOrEmpty(animal.Notes))
            {
                sb.AppendLine($"💝 About {(options.IncludeName ? animal.Name : "This Pet")}:");
                sb.AppendLine(animal.Notes);
                sb.AppendLine();
            }

            // Footer
            sb.AppendLine($"📞 Contact us today to meet {(options.IncludeName ? animal.Name : "this wonderful pet")}!");
            sb.AppendLine("❤️ #AdoptDontShop #RescuePet #AdoptionReady");

            return sb.ToString();
        }

        private bool AnyInfoSelected(ExportOptions options)
        {
            return options.IncludeName || options.IncludeBreed || options.IncludeSex || options.IncludeAge || options.IncludeStatus || options.IncludeWeight || options.IncludeCollarColor || options.IncludeIntakeDate || options.IncludeNotes || options.IncludeVetVisitDates || options.IncludeVaccinations || options.IncludeVaccinationDates || options.IncludeSpayedNeutered || options.IncludeWorming || options.IncludeDeFleeing || options.IncludeDental || options.IncludeVetNotes;
        }


        private void AddVetInformation(StringBuilder sb, Animal animal, List<VetVisit> vetVisits, ExportOptions options, bool detailed)
        {
            System.Diagnostics.Debug.WriteLine($"AddVetInformation - Vet Visits Count: {vetVisits?.Count ?? 0}");
            
            // Check if any vet options are requested
            var hasAnyVetOption = options.IncludeVetVisitDates || options.IncludeVaccinations || 
                                 options.IncludeSpayedNeutered || options.IncludeWorming ||
                                 options.IncludeDeFleeing || options.IncludeDental || options.IncludeVetNotes;
            
            if (!hasAnyVetOption)
            {
                System.Diagnostics.Debug.WriteLine("No vet options enabled - returning");
                return;
            }
            
            // If no vet visits exist, don't show anything
            if (vetVisits == null || !vetVisits.Any())
            {
                System.Diagnostics.Debug.WriteLine("No vet visits found - not displaying vet section");
                return;
            }

            sb.AppendLine("🏥 Health Information:");

            var isSpayedNeutered = vetVisits.Any(v => v.SpayedNeuteringDate.HasValue);
            System.Diagnostics.Debug.WriteLine($"Is Spayed/Neutered: {isSpayedNeutered}");
            
            if (options.IncludeSpayedNeutered && isSpayedNeutered)
            {
                var spayedVisit = vetVisits.FirstOrDefault(v => v.SpayedNeuteringDate.HasValue);
                if (spayedVisit?.SpayedNeuteringDate != null)
                    sb.AppendLine($"✓ Spayed/Neutered ({spayedVisit.SpayedNeuteringDate.Value:MMMM yyyy})");
            }

            var vaccinations = new List<string>();
            var vaccinationDates = new Dictionary<string, DateTime>();

            foreach (var visit in vetVisits.OrderByDescending(v => v.Date))
            {
                if (options.IncludeVaccinations && !string.IsNullOrEmpty(visit.VaccinationsGiven))
                {
                    var vacs = visit.VaccinationsGiven.Split(',').Select(v => v.Trim()).Where(v => !string.IsNullOrEmpty(v));
                    foreach (var vac in vacs)
                    {
                        if (!vaccinations.Contains(vac))
                        {
                            vaccinations.Add(vac);
                            if (options.IncludeVaccinationDates)
                            {
                                if (vac.Contains("Rabies") && visit.RabiesShotDate.HasValue)
                                    vaccinationDates[vac] = visit.RabiesShotDate.Value;
                                else if (vac.Contains("Distemper") && visit.DistemperDate.HasValue)
                                    vaccinationDates[vac] = visit.DistemperDate.Value;
                                else if (vac.Contains("DAPP") && visit.DAPPDate.HasValue)
                                    vaccinationDates[vac] = visit.DAPPDate.Value;
                            }
                        }
                    }
                }
            }

            if (vaccinations.Any())
            {
                if (options.IncludeVaccinationDates && vaccinationDates.Any())
                {
                    sb.AppendLine("✓ Vaccinations:");
                    foreach (var vac in vaccinations)
                    {
                        if (vaccinationDates.ContainsKey(vac))
                            sb.AppendLine($"  • {vac} ({vaccinationDates[vac]:MM/yyyy})");
                        else
                            sb.AppendLine($"  • {vac}");
                    }
                }
                else
                {
                    sb.AppendLine($"✓ Vaccinations: {string.Join(", ", vaccinations)}");
                }
            }

            // Worming Treatment
            if (options.IncludeWorming)
            {
                var wormingVisits = vetVisits.Where(v => v.WormingDate.HasValue).OrderByDescending(v => v.WormingDate).ToList();
                if (wormingVisits.Any())
                {
                    var lastWorming = wormingVisits.First();
                    if (lastWorming.WormingDate.HasValue)
                        sb.AppendLine($"✓ Worming Treatment: {lastWorming.WormingDate.Value:MMMM d, yyyy}");
                }
            }

            // De-fleeing Treatment
            if (options.IncludeDeFleeing)
            {
                var defleeingVisits = vetVisits.Where(v => v.DeFleeingDate.HasValue).OrderByDescending(v => v.DeFleeingDate).ToList();
                if (defleeingVisits.Any())
                {
                    var lastDefleeing = defleeingVisits.First();
                    if (lastDefleeing.DeFleeingDate.HasValue)
                        sb.AppendLine($"✓ Flea Treatment: {lastDefleeing.DeFleeingDate.Value:MMMM d, yyyy}");
                }
            }

            // Dental Treatment
            if (options.IncludeDental)
            {
                var dentalVisits = vetVisits.Where(v => v.DentalDate.HasValue).OrderByDescending(v => v.DentalDate).ToList();
                if (dentalVisits.Any())
                {
                    var lastDental = dentalVisits.First();
                    if (lastDental.DentalDate.HasValue)
                        sb.AppendLine($"✓ Dental Treatment: {lastDental.DentalDate.Value:MMMM d, yyyy}");
                }
            }

            if (options.IncludeVetVisitDates && vetVisits.Any())
            {
                var lastVisit = vetVisits.OrderByDescending(v => v.Date).First();
                sb.AppendLine($"✓ Last Vet Visit: {lastVisit.Date:MMMM d, yyyy}");
            }

            if (options.IncludeVetNotes)
            {
                foreach (var visit in vetVisits.OrderByDescending(v => v.Date).Take(1))
                {
                    if (!string.IsNullOrEmpty(visit.Notes))
                    {
                        sb.AppendLine($"✓ Vet Notes: {visit.Notes}");
                    }
                }
            }

            sb.AppendLine();
        }

        private string CalculateAge(DateTime dob)
        {
            var today = DateTime.Today;
            var age = today.Year - dob.Year;
            if (dob.Date > today.AddYears(-age)) age--;

            if (age == 0)
            {
                var months = ((today.Year - dob.Year) * 12) + today.Month - dob.Month;
                if (today.Day < dob.Day) months--;
                
                if (months <= 0)
                    return "Under 1 month old";
                else if (months == 1)
                    return "1 month old";
                else
                    return $"{months} months old";
            }
            else if (age == 1)
                return "1 year old";
            else
                return $"{age} years old";
        }

        public async Task ExportCompletePackageAsync(int animalId, string platform, string zipPath, ExportOptions options)
        {
            var animal = await _dbService.GetAnimalByIdAsync(animalId);
            if (animal == null) throw new Exception("Animal not found");

            var tempDir = Path.Combine(Path.GetTempPath(), $"PupTrails_Export_{Guid.NewGuid()}");
            Directory.CreateDirectory(tempDir);

            try
            {
                var facebookText = await GeneratePostTextAsync(animalId, "Facebook", options);
                var instagramText = await GeneratePostTextAsync(animalId, "Instagram", options);
                var petfinderText = await GeneratePostTextAsync(animalId, "Petfinder", options);

                File.WriteAllText(Path.Combine(tempDir, $"{animal.Name}_Facebook_Post.txt"), facebookText);
                File.WriteAllText(Path.Combine(tempDir, $"{animal.Name}_Instagram_Post.txt"), instagramText);
                File.WriteAllText(Path.Combine(tempDir, $"{animal.Name}_Petfinder_Post.txt"), petfinderText);

                string resolvedPhotoPath = string.Empty;
                if (!string.IsNullOrEmpty(animal.PhotoPath))
                {
                    resolvedPhotoPath = PathManager.ResolveAnimalPhotoPath(animal.PhotoPath!);
                }
                if (options.IncludePhotos && !string.IsNullOrEmpty(resolvedPhotoPath) && File.Exists(resolvedPhotoPath))
                {
                    var photosDir = Path.Combine(tempDir, "Photos");
                    Directory.CreateDirectory(photosDir);

                    var originalDir = Path.Combine(photosDir, "Original");
                    Directory.CreateDirectory(originalDir);
                    var ext = Path.GetExtension(resolvedPhotoPath);
                    File.Copy(resolvedPhotoPath, Path.Combine(originalDir, $"{animal.Name}_Original{ext}"));

                    ImageResizer.CreateResizedPhotos(resolvedPhotoPath, photosDir, animal.Name);
                }

                if (File.Exists(zipPath))
                    File.Delete(zipPath);

                ZipFile.CreateFromDirectory(tempDir, zipPath);
            }
            finally
            {
                if (Directory.Exists(tempDir))
                {
                    try { Directory.Delete(tempDir, true); } catch { }
                }
            }
        }

        public void CreateWordDocument(string filePath, Animal animal, string postText, ExportOptions options)
        {
            System.Diagnostics.Debug.WriteLine($"=== CreateWordDocument ===");
            System.Diagnostics.Debug.WriteLine($"File path: {filePath}");
            System.Diagnostics.Debug.WriteLine($"Animal: {animal.Name}");
            System.Diagnostics.Debug.WriteLine($"Animal PhotoPath: {animal.PhotoPath ?? "NULL"}");
            System.Diagnostics.Debug.WriteLine($"IncludePhotos option: {options.IncludePhotos}");
            
            using (WordprocessingDocument wordDoc = WordprocessingDocument.Create(filePath, WordprocessingDocumentType.Document))
            {
                MainDocumentPart mainPart = wordDoc.AddMainDocumentPart();
                mainPart.Document = new Document();
                Body body = new Body();

                // Add title
                var titleParagraph = new Paragraph();
                var titleRun = new Run();
                var titleRunProperties = new RunProperties();
                titleRunProperties.AppendChild(new Bold());
                titleRunProperties.AppendChild(new FontSize { Val = "32" });
                titleRun.AppendChild(titleRunProperties);
                titleRun.AppendChild(new Text($"{animal.Name} - Social Media Post"));
                titleParagraph.AppendChild(titleRun);
                body.AppendChild(titleParagraph);

                // Add spacing
                body.AppendChild(new Paragraph());

                // Add photo if available
                if (options.IncludePhotos && !string.IsNullOrEmpty(animal.PhotoPath))
                {
                    string fullPhotoPath = PathManager.ResolveAnimalPhotoPath(animal.PhotoPath!);
                    System.Diagnostics.Debug.WriteLine($"Full photo path: {fullPhotoPath}");
                    System.Diagnostics.Debug.WriteLine($"Checking if photo exists: {fullPhotoPath}");
                    
                    if (File.Exists(fullPhotoPath))
                    {
                        System.Diagnostics.Debug.WriteLine($"✓ Photo file exists, adding to document");
                        AddImageToBody(mainPart, body, fullPhotoPath);
                        body.AppendChild(new Paragraph());
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"✗ Photo file NOT found at: {fullPhotoPath}");
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"Photos not included: IncludePhotos={options.IncludePhotos}, PhotoPath={!string.IsNullOrEmpty(animal.PhotoPath)}");
                }

                // Add post text (split by lines to preserve formatting)
                var lines = postText.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
                foreach (var line in lines)
                {
                    var paragraph = new Paragraph();
                    var run = new Run();
                    run.AppendChild(new Text(line));
                    paragraph.AppendChild(run);
                    body.AppendChild(paragraph);
                }

                mainPart.Document.AppendChild(body);
                mainPart.Document.Save();
            }
        }

        public async Task ExportCompletePackageToFolderAsync(int animalId, string folderPath, ExportOptions options)
        {
            var animal = await _dbService.GetAnimalByIdAsync(animalId);
            if (animal == null) throw new Exception("Animal not found");

            // Create main export folder
            if (Directory.Exists(folderPath))
            {
                Directory.Delete(folderPath, true);
            }
            Directory.CreateDirectory(folderPath);

            // Generate and save Word document for Facebook
            var facebookText = await GeneratePostTextAsync(animalId, "Facebook", options);
            CreateWordDocument(Path.Combine(folderPath, "Facebook.docx"), animal, facebookText, options);

            // Copy photos if requested
            if (options.IncludePhotos && !string.IsNullOrEmpty(animal.PhotoPath))
            {
                string fullPhotoPath = PathManager.ResolveAnimalPhotoPath(animal.PhotoPath!);
                
                if (File.Exists(fullPhotoPath))
                {
                    var photosDir = Path.Combine(folderPath, "Photos");
                    Directory.CreateDirectory(photosDir);

                    // Copy original
                    var originalDir = Path.Combine(photosDir, "Original");
                    Directory.CreateDirectory(originalDir);
                    var ext = Path.GetExtension(fullPhotoPath);
                    File.Copy(fullPhotoPath, Path.Combine(originalDir, $"{animal.Name}_Original{ext}"));

                    // Create resized versions
                    ImageResizer.CreateResizedPhotos(fullPhotoPath, photosDir, animal.Name);
                }
            }
        }

        private void AddImageToBody(MainDocumentPart mainPart, Body body, string imagePath)
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

                using (FileStream stream = new FileStream(imagePath, FileMode.Open, FileAccess.Read))
                {
                    imagePart.FeedData(stream);
                }

                AddImageToElement(mainPart.GetIdOfPart(imagePart), body);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error adding image to document: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Image path: {imagePath}");
            }
        }

        private void AddImageToElement(string relationshipId, Body body)
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
