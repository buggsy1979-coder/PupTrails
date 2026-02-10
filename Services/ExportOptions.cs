namespace PupTrailsV3.Services
{
    public class ExportOptions
    {
        // Photos
        public bool IncludePhotos { get; set; }

        // Basic Animal Information
        public bool IncludeName { get; set; }
        public bool IncludeBreed { get; set; }
        public bool IncludeSex { get; set; }
        public bool IncludeAge { get; set; }
        public bool IncludeStatus { get; set; }
        public bool IncludeWeight { get; set; }
        public bool IncludeCollarColor { get; set; }
        public bool IncludeIntakeDate { get; set; }
        public bool IncludeNotes { get; set; }

        // Vet Information
        public bool IncludeVetVisitDates { get; set; }
        public bool IncludeVaccinations { get; set; }
        public bool IncludeVaccinationDates { get; set; }
        public bool IncludeSpayedNeutered { get; set; }
        public bool IncludeWorming { get; set; }
        public bool IncludeDeFleeing { get; set; }
        public bool IncludeDental { get; set; }
        public bool IncludeVetNotes { get; set; }
    }
}
