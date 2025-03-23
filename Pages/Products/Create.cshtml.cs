using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using DotNet9Showcase.Models;
using DotNet9Showcase.Data;
using Microsoft.AspNetCore.Http;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace DotNet9Showcase.Pages.Products
{
    public class CreateModel : PageModel
    {
        private readonly AppDbContext _context;
        private readonly ILogger<CreateModel> _logger;

        [BindProperty]
        public Product Product { get; set; } = new();

        [BindProperty]
        public IFormFile? UploadedImage { get; set; }

        public CreateModel(AppDbContext context, ILogger<CreateModel> logger)
        {
            _context = context;
            _logger = logger;
        }

        public void OnGet() { }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            try
            {
                if (UploadedImage != null && UploadedImage.Length > 0)
                {
                    var fileExt = Path.GetExtension(UploadedImage.FileName).ToLowerInvariant();
                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };

                    if (!allowedExtensions.Contains(fileExt))
                    {
                        ModelState.AddModelError("UploadedImage", "Only JPG, PNG, or GIF files are allowed.");
                        return Page();
                    }

                    var rootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "products");
                    _logger.LogInformation("Creating folder: {path}", rootPath);
                    Directory.CreateDirectory(rootPath); // Ensures parent folders

                    var fileName = $"{Guid.NewGuid()}{fileExt}";
                    var filePath = Path.Combine(rootPath, fileName);

                    using var stream = UploadedImage.OpenReadStream();
                    using var image = Image.Load(stream);

                    image.Mutate(x => x.Resize(new ResizeOptions
                    {
                        Mode = ResizeMode.Max,
                        Size = new Size(300, 300)
                    }));

                    await image.SaveAsync(filePath);
                    _logger.LogInformation("Saved image to: {filePath}", filePath);

                    // Save the web path
                    Product.ImagePath = $"/images/products/{fileName}";
                }
                else
                {
                    _logger.LogWarning("No image uploaded.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Image upload failed.");
                ModelState.AddModelError("UploadedImage", $"Image upload failed: {ex.Message}");
                return Page();
            }

            Product.CreatedAt = DateTime.UtcNow;
            _context.Products.Add(Product);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Product created successfully!";
            TempData["SuccessMessage"] = "Product updated successfully!";
            TempData["SuccessMessage"] = "Product deleted.";
            return RedirectToPage("Index");
        }
    }
}
