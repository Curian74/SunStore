namespace SunStoreAPI.Services
{
    public class ImageHandlerService
    {
        private readonly IWebHostEnvironment _environment;

        public ImageHandlerService(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        public async Task<string> HandleImageUpload(IFormFile img, string target)
        {
            string imagePath = null!;
            if (img != null)
            {
                string wwwRootFolder = _environment.WebRootPath;
                if (!Directory.Exists(wwwRootFolder))
                {
                    Directory.CreateDirectory(wwwRootFolder);
                }

                // Store the image inside target.
                // Example: "ProductImg".
                string folder = Path.Combine(_environment.WebRootPath, $"{target}");
                Directory.CreateDirectory(folder);
                string fileName = Guid.NewGuid().ToString() + "_" + img.FileName;
                string filePath = Path.Combine(folder, fileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await img.CopyToAsync(fileStream);
                }

                // Store the relative path to the image.
                imagePath = $"https://localhost:7270/{target}/{fileName}";
            }

            return imagePath ?? "";
        }

        public bool IsImageFile(IFormFile file)
        {
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp" };
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            return allowedExtensions.Contains(extension);
        }
    }
}
