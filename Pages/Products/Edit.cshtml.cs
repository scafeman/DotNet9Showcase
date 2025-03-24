using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using DotNet9Showcase.Data;
using DotNet9Showcase.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using Azure.Storage.Blobs;
using System.Text.RegularExpressions;

namespace DotNet9Showcase.Pages.Products;

public class EditModel : PageModel
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _config;
    private readonly ILogger<EditModel> _logger;

    public EditModel(AppDbContext context, IConfiguration config, ILogger<EditModel> logger)
    {
        _context = context;
        _config = config;
        _logger = logger;
    }

    [BindProperty]
    public Product Product { get; set; } = new();

    [BindProperty]
    public IFormFile? UploadedImage { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        Product = await _context.Products.FindAsync(id) ?? new Product();
        if (Product == null)
            return NotFound();

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
            return Page();

        var existingProduct = await _context.Products.FindAsync(Product.Id);
        if (existingProduct == null)
            return NotFound();

        existingProduct.Name = Product.Name;
        existingProduct.Price = Product.Price;

        if (UploadedImage != null && UploadedImage.Length > 0)
        {
            var fileExt = Path.GetExtension(UploadedImage.FileName).ToLowerInvariant();
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };

            if (!allowedExtensions.Contains(fileExt))
            {
                ModelState.AddModelError("UploadedImage", "Only JPG, PNG, or GIF files are allowed.");
                return Page();
            }

            // 🔁 Delete old blob if exists
            if (!string.IsNullOrEmpty(existingProduct.ImagePath))
            {
                try
                {
                    var containerName = _config["AzureBlob:ContainerName"];
                    var connStr = _config["AzureBlob:ConnectionString"];
                    var oldBlobName = Path.GetFileName(new Uri(existingProduct.ImagePath).AbsolutePath);
                    var container = new BlobContainerClient(connStr, containerName);
                    await container.DeleteBlobIfExistsAsync(oldBlobName);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning("Failed to delete old image blob: {Message}", ex.Message);
                }
            }

            // 📤 Upload new image
            var blobFileName = $"{Guid.NewGuid()}{fileExt}";
            var tempPath = Path.Combine(Path.GetTempPath(), blobFileName);

            using var stream = UploadedImage.OpenReadStream();
            using var image = Image.Load(stream);

            image.Mutate(x => x.Resize(new ResizeOptions
            {
                Mode = ResizeMode.Max,
                Size = new Size(300, 300)
            }));

            await image.SaveAsync(tempPath);

            var blobContainer = new BlobContainerClient(
                _config["AzureBlob:ConnectionString"],
                _config["AzureBlob:ContainerName"]);

            await blobContainer.CreateIfNotExistsAsync();
            await blobContainer.UploadBlobAsync(blobFileName, System.IO.File.OpenRead(tempPath));

            existingProduct.ImagePath = $"{blobContainer.Uri}/{blobFileName}";
        }

        await _context.SaveChangesAsync();
        return RedirectToPage("Index");
    }
}
