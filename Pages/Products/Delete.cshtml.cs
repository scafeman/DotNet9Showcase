using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using DotNet9Showcase.Data;
using DotNet9Showcase.Models;

namespace DotNet9Showcase.Pages.Products;

public class DeleteModel : PageModel
{
    private readonly AppDbContext _context;

    [BindProperty]
    public Product Product { get; set; } = new();

    public DeleteModel(AppDbContext context) => _context = context;

    public async Task<IActionResult> OnGetAsync(int id)
    {
        Product = await _context.Products.FindAsync(id) ?? new Product();
        if (Product == null)
            return NotFound();

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null)
            return NotFound();

        if (!string.IsNullOrEmpty(product.ImagePath))
        {
            var imagePath = Path.Combine("wwwroot", product.ImagePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
            if (System.IO.File.Exists(imagePath))
            {
                System.IO.File.Delete(imagePath);
            }
        }

        _context.Products.Remove(product);
        await _context.SaveChangesAsync();
        return RedirectToPage("Index");
    }
}
