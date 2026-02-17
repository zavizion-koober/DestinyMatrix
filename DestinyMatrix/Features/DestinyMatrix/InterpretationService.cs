using System.Text.Json;
using DestinyMatrix.Data; // Убедись, что путь к AppDbContext верный
using Microsoft.Extensions.DependencyInjection;

namespace DestinyMatrix.Features.DestinyMatrix;

public interface IInterpretationService
{
    string GetDescription(int arcanaId, string zoneKey);
    ArcanaInfo? GetFullInfo(int arcanaId);
}

public class InterpretationService : IInterpretationService
{
    private readonly IServiceScopeFactory _scopeFactory;

    public InterpretationService(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    public ArcanaInfo? GetFullInfo(int arcanaId)
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var entity = context.Arcanas.Find(arcanaId);
        if (entity == null) return null;

        return new ArcanaInfo(
            entity.Id,
            entity.Name,
            entity.Archetype,
            entity.Energy,
            JsonSerializer.Deserialize<Dictionary<string, string>>(entity.ZonesJson) ?? new()
        );
    }

    public string GetDescription(int arcanaId, string zoneKey)
    {
        var info = GetFullInfo(arcanaId);
        
        if (info?.Zones != null && info.Zones.TryGetValue(zoneKey, out var text))
        {
            return text;
        }

        return "ამ ზონის აღწერა მალე დაემატება...";
    }
}