using Microsoft.AspNetCore.Mvc.RazorPages;
using DotNet9Showcase.Models;
using DotNet9Showcase.Data;
using Microsoft.EntityFrameworkCore;

namespace DotNet9Showcase.Pages.Products;

public class IndexModel : PageModel
{
    private readonly AppDbContext _context;
    public List<Product> Products { get; set; } = new();

    public IndexModel(AppDbContext context) => _context = context;

    public async Task OnGetAsync()
    {
        Products = await _context.Products.ToListAsync();
    }
}
