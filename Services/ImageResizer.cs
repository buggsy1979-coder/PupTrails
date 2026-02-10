using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Runtime.Versioning;

namespace PupTrailsV3.Services
{
    [SupportedOSPlatform("windows")]
    public static class ImageResizer
    {
        public static void CreateResizedPhotos(string sourcePath, string outputDir, string animalName)
        {
            try
            {
                using var originalImage = Image.FromFile(sourcePath);
                var ext = Path.GetExtension(sourcePath);

                var instagramDir = Path.Combine(outputDir, "Instagram");
                Directory.CreateDirectory(instagramDir);
                SaveResizedImage(originalImage, Path.Combine(instagramDir, $"{animalName}_Instagram{ext}"), 1080, 1080);

                var facebookDir = Path.Combine(outputDir, "Facebook");
                Directory.CreateDirectory(facebookDir);
                SaveResizedImage(originalImage, Path.Combine(facebookDir, $"{animalName}_Facebook{ext}"), 1200, 630);

                var petfinderDir = Path.Combine(outputDir, "Petfinder");
                Directory.CreateDirectory(petfinderDir);
                SaveResizedImage(originalImage, Path.Combine(petfinderDir, $"{animalName}_Petfinder{ext}"), 1024, 768);
            }
            catch
            {
                // If resizing fails, skip it
            }
        }

        private static void SaveResizedImage(Image original, string outputPath, int maxWidth, int maxHeight)
        {
            int newWidth, newHeight;
            double ratioX = (double)maxWidth / original.Width;
            double ratioY = (double)maxHeight / original.Height;
            double ratio = Math.Min(ratioX, ratioY);

            newWidth = (int)(original.Width * ratio);
            newHeight = (int)(original.Height * ratio);

            using var newImage = new Bitmap(newWidth, newHeight);
            using var graphics = Graphics.FromImage(newImage);
            
            graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
            graphics.DrawImage(original, 0, 0, newWidth, newHeight);

            newImage.Save(outputPath);
        }
    }
}
