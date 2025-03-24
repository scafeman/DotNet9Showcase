using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using DotNet9Showcase.Data;
using DotNet9Showcase.Models;
using Azure.Storage.Blobs;

namespace DotNet9Showcase.Pages.Products;

public class DeleteModel : PageModel
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _config;
    private readonly ILogger<DeleteModel> _logger;

    [BindProperty]
    public Product? Product { get; set; }

    public DeleteModel(AppDbContext context, IConfiguration config, ILogger<DeleteModel> logger)
    {
        _context = context;
        _config = config;
        _logger = logger;
    }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        Product = await _context.Products.FindAsync(id);
        if (Product == null)
            return NotFound();

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null)
            return NotFound();

        // Delete image from Azure Blob Storage
        if (!string.IsNullOrEmpty(product.ImagePath))
        {
            try
            {
                var connectionString = _config["AzureBlob:ConnectionString"];
                var containerName = _config["AzureBlob:ContainerName"];
                var blobClient = new BlobContainerClient(connectionString, containerName);
                var blobName = Path.GetFileName(new Uri(product.ImagePath).AbsolutePath);

                await blobClient.DeleteBlobIfExistsAsync(blobName);
                _logger.LogInformation("Deleted blob: {Blob}", blobName);
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Error deleting blob: {Message}", ex.Message);
            }
        }

        _context.Products.Remove(product);
        await _context.SaveChangesAsync();
        return RedirectToPage("Index");
    }
}
