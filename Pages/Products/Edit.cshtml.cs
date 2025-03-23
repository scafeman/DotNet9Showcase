using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using DotNet9Showcase.Data;
using DotNet9Showcase.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace DotNet9Showcase.Pages.Products;

public class EditModel : PageModel
{
    private readonly AppDbContext _context;
    private readonly ILogger<EditModel> _logger;

    public EditModel(AppDbContext context, ILogger<EditModel> logger)
    {
        _context = context;
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

            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "products");
            Directory.CreateDirectory(uploadsFolder);

            var fileName = $"{Guid.NewGuid()}{fileExt}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            using var stream = UploadedImage.OpenReadStream();
            using var image = Image.Load(stream);

            image.Mutate(x => x.Resize(new ResizeOptions
            {
                Mode = ResizeMode.Max,
                Size = new Size(300, 300)
            }));

            await image.SaveAsync(filePath);

            // Optional: delete old image
            if (!string.IsNullOrEmpty(existingProduct.ImagePath))
            {
                var oldPath = Path.Combine("wwwroot", existingProduct.ImagePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
                if (System.IO.File.Exists(oldPath))
                {
                    System.IO.File.Delete(oldPath);
                }
            }

            existingProduct.ImagePath = $"/images/products/{fileName}";
        }

        await _context.SaveChangesAsync();
        return RedirectToPage("Index");
    }
}
