using System.Text.Json;
using DestinyMatrix.Features.DestinyMatrix;
using MediatR;
using System.Text.Json.Serialization;
using DestinyMatrix.Data;
using Microsoft.EntityFrameworkCore;



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
builder.Services.AddSingleton<IInterpretationService, InterpretationService>();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=destiny.db"));

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

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var env = scope.ServiceProvider.GetRequiredService<IHostEnvironment>();

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
                Console.WriteLine("-->ბაზა შეივსო ბლიაძ");
            }
        }
    }
}

app.Run();