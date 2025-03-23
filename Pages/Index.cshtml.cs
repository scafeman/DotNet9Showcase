using Microsoft.AspNetCore.Mvc.RazorPages;
using DotNet9Showcase.Data;
using DotNet9Showcase.Models;
using Microsoft.EntityFrameworkCore;

namespace DotNet9Showcase.Pages;

public class IndexModel : PageModel
{
    private readonly AppDbContext _context;
    public List<Product> TopProducts { get; set; } = new();

    public IndexModel(AppDbContext context)
    {
        _context = context;
    }

    public async Task OnGetAsync()
    {
        TopProducts = await _context.Products
            .OrderByDescending(p => p.CreatedAt)
            .Take(3)
            .ToListAsync();
    }
}
