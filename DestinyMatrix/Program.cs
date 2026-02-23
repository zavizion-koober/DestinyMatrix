using System.Text.Json;
using DestinyMatrix.Features.DestinyMatrix;
using MediatR;
using System.Text.Json.Serialization;
using DestinyMatrix.Data;
using Microsoft.EntityFrameworkCore;
using DestinyMatrix.Features.Numerology; // Added namespace

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

builder.Services.AddSingleton<IInterpretationService, InterpretationService>();
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

builder.Services.ConfigureHttpJsonOptions(options => {
    options.SerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Database context
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=destiny.db"));

// Register Numerology Services
builder.Services.AddSingleton<AlexandrovCalculator>();
builder.Services.AddSingleton<DataService>();
builder.Services.AddSingleton<GeometryService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");
app.UseHttpsRedirection();

app.MapGet("/api/debug/db-status", async (AppDbContext context) =>
{
    var count = await context.Arcanas.CountAsync();
    return Results.Ok(new 
    { 
        TotalArcanas = count, 
        Status = count == 22 ? "Ready" : "Empty/Partial" 
    });
});

app.MapGet("/api/matrix/calculate", async (int day, int month, int year, IMediator mediator) =>
    {
        var query = new CalculateMatrix.Query(day, month, year);
        var result = await mediator.Send(query);
        return Results.Ok(result);
    })
    .WithName("CalculateMatrix")
    .WithOpenApi();

app.MapGet("/api/numerology/calculate", async (int day, int month, int year, IMediator mediator) =>
    {
        var query = new CalculateNumerology.Query(day, month, year);
        var result = await mediator.Send(query);
        return Results.Ok(result);
    })
    .WithName("CalculateNumerology")
    .WithOpenApi();

// Compatibility Analysis (2 dates: male + female)
app.MapGet("/api/numerology/compatibility", (
    int manDay, int manMonth, int manYear,
    int womanDay, int womanMonth, int womanYear,
    AlexandrovCalculator calculator, DataService dataService) =>
{
    var manDate = new DateTime(manYear, manMonth, manDay);
    var womanDate = new DateTime(womanYear, womanMonth, womanDay);
    
    var manMatrix = calculator.CalculateFullMatrix(manDate);
    var womanMatrix = calculator.CalculateFullMatrix(womanDate);
    var stability = calculator.CalculateStability(manMatrix, womanMatrix);
    
    // Get full matrices for both partners for display
    var manQuery = new CalculateNumerology.Query(manDay, manMonth, manYear);
    var womanQuery = new CalculateNumerology.Query(womanDay, womanMonth, womanYear);
    
    return Results.Ok(new
    {
        man = new { birthDate = manDate.ToString("dd.MM.yyyy"), loveSexuality = manMatrix.LoveSexuality, psychotypeId = manMatrix.PsychotypeId },
        woman = new { birthDate = womanDate.ToString("dd.MM.yyyy"), loveSexuality = womanMatrix.LoveSexuality, psychotypeId = womanMatrix.PsychotypeId },
        stability,
        manLines = manMatrix.Lines,
        womanLines = womanMatrix.Lines
    });
})
.WithName("CalculateCompatibility")
.WithOpenApi();

// Child Karma Analysis (3 dates: father + mother + child)
app.MapGet("/api/numerology/child-analysis", (
    int fatherDay, int fatherMonth, int fatherYear,
    int motherDay, int motherMonth, int motherYear,
    int childDay, int childMonth, int childYear,
    AlexandrovCalculator calculator) =>
{
    var fatherMatrix = calculator.CalculateFullMatrix(new DateTime(fatherYear, fatherMonth, fatherDay));
    var motherMatrix = calculator.CalculateFullMatrix(new DateTime(motherYear, motherMonth, motherDay));
    var childMatrix = calculator.CalculateFullMatrix(new DateTime(childYear, childMonth, childDay));
    
    int GetCount(Dictionary<string, int> counts, int digit) => counts.ContainsKey($"Digit_{digit}") ? counts[$"Digit_{digit}"] : 0;
    
    var fatherKarma = new { date = fatherMatrix.BirthDate, count6 = GetCount(fatherMatrix.MatrixCounts, 6), count7 = GetCount(fatherMatrix.MatrixCounts, 7), count8 = GetCount(fatherMatrix.MatrixCounts, 8) };
    var motherKarma = new { date = motherMatrix.BirthDate, count6 = GetCount(motherMatrix.MatrixCounts, 6), count7 = GetCount(motherMatrix.MatrixCounts, 7), count8 = GetCount(motherMatrix.MatrixCounts, 8) };
    var childKarma = new { date = childMatrix.BirthDate, count6 = GetCount(childMatrix.MatrixCounts, 6), count7 = GetCount(childMatrix.MatrixCounts, 7), count8 = GetCount(childMatrix.MatrixCounts, 8) };
    
    int fatherBalance = (fatherKarma.count7 + fatherKarma.count8) - fatherKarma.count6;
    int motherBalance = (motherKarma.count7 + motherKarma.count8) - motherKarma.count6;
    int childBalance = (childKarma.count7 + childKarma.count8) - childKarma.count6;
    int parentsTotalBalance = fatherBalance + motherBalance;
    
    return Results.Ok(new
    {
        father = new { fatherKarma.date, fatherKarma.count6, fatherKarma.count7, fatherKarma.count8, balance = fatherBalance },
        mother = new { motherKarma.date, motherKarma.count6, motherKarma.count7, motherKarma.count8, balance = motherBalance },
        child = new { childKarma.date, childKarma.count6, childKarma.count7, childKarma.count8, balance = childBalance },
        parentsTotalBalance,
        verdict = parentsTotalBalance >= 0 ? "Positive karmic inheritance" : "Karmic debt to resolve"
    });
})
.WithName("CalculateChildAnalysis")
.WithOpenApi();

// Life Geometry (parabola calculations from 1 date)
app.MapGet("/api/numerology/geometry", (
    int day, int month, int year,
    AlexandrovCalculator calculator, GeometryService geometryService) =>
{
    var matrixData = calculator.CalculateFullMatrix(new DateTime(year, month, day));
    var geometry = geometryService.CalculateParabolas(matrixData);
    return Results.Ok(geometry);
})
.WithName("CalculateGeometry")
.WithOpenApi();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var env = scope.ServiceProvider.GetRequiredService<IHostEnvironment>();

    // Ensure database is created
    context.Database.EnsureCreated();

    if (!context.Arcanas.Any())
    {
        var filePath = Path.Combine(env.ContentRootPath,"Data","arcana_db.json");
        if (File.Exists(filePath))
        {
            var json = File.ReadAllText(filePath);
            var data = JsonSerializer.Deserialize<ArcanaDb>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (data?.Arcana != null)
            {
                var entities = data.Arcana.Select(a => new ArcanaEntity
                {
                    Id = a.Id,
                    Name = a.Name,
                    Archetype = a.Archetype,
                    Energy = a.Energy,
                    ZonesJson = JsonSerializer.Serialize(a.Zones) 
                });

                context.Arcanas.AddRange(entities);
                context.SaveChanges();
                Console.WriteLine("--> Database seeded");
            }
        }
    }
}

app.Run();