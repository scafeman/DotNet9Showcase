using DotNet9Showcase.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var connectionString = Environment.GetEnvironmentVariable("AZURE_BLOB_CONNECTION_STRING");

builder.Configuration.AddEnvironmentVariables(); // ensure env vars are included
var config = builder.Configuration;

// Define the SQLite DB path (App_Data inside the root directory)
var dbPath = Path.Combine(builder.Environment.ContentRootPath, "App_Data", "DotNet9Showcase.db");

// Register services
builder.Services.AddRazorPages();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite($"Data Source={dbPath}"));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<AzureBlobHelper>();

var app = builder.Build();

// Ensure DB and folder exist
var dataFolder = Path.Combine(app.Environment.ContentRootPath, "App_Data");
Directory.CreateDirectory(dataFolder);

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated(); // Use Migrations in production
}

// Middleware
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseStaticFiles();
app.UseRouting();

app.UseSwagger();
app.UseSwaggerUI();

app.MapRazorPages();

// Minimal API
app.MapGet("/api/products", async (AppDbContext db) =>
    await db.Products.ToListAsync());

app.Run();
