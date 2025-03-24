using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using DotNet9Showcase.Models;
using DotNet9Showcase.Data;
using Microsoft.AspNetCore.Http;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace DotNet9Showcase.Pages.Products
{
    public class CreateModel : PageModel
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _config;

        [BindProperty]
        public Product Product { get; set; } = new();

        [BindProperty]
        public IFormFile? UploadedImage { get; set; }

        public CreateModel(AppDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        public void OnGet() { }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            if (UploadedImage != null && UploadedImage.Length > 0)
            {
                var connectionString = _config["AzureBlob:ConnectionString"];
                var containerName = _config["AzureBlob:ContainerName"];
                var containerUrl = $"https://mscafescusstoraccount.blob.core.windows.net/{containerName}";

                var blobServiceClient = new BlobServiceClient(connectionString);
                var containerClient = blobServiceClient.GetBlobContainerClient(containerName);
                await containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);

                var fileExt = Path.GetExtension(UploadedImage.FileName);
                var blobName = $"{Guid.NewGuid()}{fileExt}";
                var blobClient = containerClient.GetBlobClient(blobName);

                using var inputStream = UploadedImage.OpenReadStream();
                using var image = Image.Load(inputStream);

                image.Mutate(x => x.Resize(new ResizeOptions
                {
                    Mode = ResizeMode.Max,
                    Size = new Size(400, 400)
                }));

                using var outputStream = new MemoryStream();
                await image.SaveAsJpegAsync(outputStream);
                outputStream.Position = 0;

                await blobClient.UploadAsync(outputStream, overwrite: true);

                // Save the full public URL
                Product.ImagePath = $"{containerUrl}/{blobName}";
            }

            Product.CreatedAt = DateTime.UtcNow;
            _context.Products.Add(Product);
            await _context.SaveChangesAsync();
            return RedirectToPage("Index");
        }
    }
}
