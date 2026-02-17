namespace DestinyMatrix.Features.DestinyMatrix;

public record ArcanaDb(List<ArcanaInfo> Arcana);

public record ArcanaInfo(
    int Id,
    string Name,
    string Archetype,
    EnergyState Energy, 
    Dictionary<string, string> Zones
);

public record EnergyState(string Plus, string Minus);