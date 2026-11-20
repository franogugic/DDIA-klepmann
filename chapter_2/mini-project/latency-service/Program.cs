using Microsoft.EntityFrameworkCore;
using MiniProjekt.Data;
using MiniProjekt.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    dbContext.Database.EnsureCreated();
    if (!dbContext.Products.Any())
    {
        var rand = new Random();
        for (int i = 0; i < 5000; i++)
        {
            var newProduct = new Product
            {
                Id = i + 1,
                Name = $"Product {i}",
                Price = rand.Next(1, 100)
            };
            dbContext.Products.Add(newProduct);
        }
        await dbContext.SaveChangesAsync();
    }
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}



app.MapGet("/product/{id}",async (int id, AppDbContext db) =>
{
    try
    {
        var numb = Random.Shared.Next(1, 101);
        if (numb <= builder.Configuration.GetValue<int>("SlowRequestPercentage"))
        {
            await Task.Delay(builder.Configuration.GetValue<int>("SlowRequestDelayMs"));
        }
    
        var product = await db.Products.FindAsync(id);
        return product == null ? Results.NotFound() : Results.Ok(product);
    }
    finally
    {
    }

});

app.Run();


