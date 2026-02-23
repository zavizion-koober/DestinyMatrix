using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace DestinyMatrix.Features.Numerology;

// 1. DATA MODELS (JSON მონაცემთა ბაზა)

public class NumerologyDataContent
{
    public Dictionary<string, DigitContent> Digits { get; set; } = new();
    public Dictionary<string, Dictionary<string, string>> Lines { get; set; } = new();
    
    [JsonPropertyName("psychotypes")]
    public Dictionary<string, PsychotypeContent> Psychotypes { get; set; } = new();
    
    [JsonPropertyName("destiny")]
    public Dictionary<string, Dictionary<string, string>> Destiny { get; set; } = new();
    
    [JsonPropertyName("transitions")]
    public Dictionary<string, TransitionContent> Transitions { get; set; } = new();

    [JsonPropertyName("personal_year")] 
    public Dictionary<string, string> PersonalYear { get; set; } = new();

    [JsonPropertyName("graph")]
    public Dictionary<string, string> Graph { get; set; } = new();

    [JsonPropertyName("health_descriptions")]
    public Dictionary<string, HealthDescriptionContent> HealthDescriptions { get; set; } = new();

    [JsonPropertyName("love_sexuality")]
    public LoveSexualityContent LoveSexuality { get; set; } = new();

    [JsonPropertyName("compatibility")]
    public CompatibilityContent Compatibility { get; set; } = new();

    [JsonPropertyName("karma")]
    public KarmaContent Karma { get; set; } = new();

    // გეომეტრიის ტექსტები 
    [JsonPropertyName("geometry")]
    public GeometryContent Geometry { get; set; } = new();
    
    public Dictionary<string, NameContent> Names { get; set; } = new();
}

public class NameContent
{
    public string Title { get; set; } = string.Empty;
    public Dictionary<string, string> Values { get; set; } = new();
}

public class GeometryContent
{
    [JsonPropertyName("titles")]
    public Dictionary<string, string> Titles { get; set; } = new();

    [JsonPropertyName("general_description")]
    public string GeneralDescription { get; set; } = string.Empty;

    [JsonPropertyName("intersections")]
    public Dictionary<string, string> Intersections { get; set; } = new();

    [JsonPropertyName("graph_trends")]
    public Dictionary<string, string> Trends { get; set; } = new();
}

public class KarmaContent
{
    [JsonPropertyName("general")]
    public Dictionary<string, string> General { get; set; } = new();
}

public class CompatibilityContent
{
    [JsonPropertyName("fleshly")]
    public Dictionary<string, string> Fleshly { get; set; } = new();
    [JsonPropertyName("character")]
    public Dictionary<string, string> Character { get; set; } = new();
    [JsonPropertyName("energy")]
    public Dictionary<string, string> Energy { get; set; } = new();
    [JsonPropertyName("family")]
    public Dictionary<string, string> Family { get; set; } = new();
    [JsonPropertyName("stability")]
    public Dictionary<string, string> Stability { get; set; } = new();
    [JsonPropertyName("habits")]
    public Dictionary<string, string> Habits { get; set; } = new();
}

public class LoveSexualityContent
{
    [JsonPropertyName("fleshly_diagonal")]
    public Dictionary<string, string> FleshlyDiagonal { get; set; } = new(); 
    [JsonPropertyName("comparison")]
    public Dictionary<string, string> Comparison { get; set; } = new(); 
}

public class HealthDescriptionContent
{
    public string Title { get; set; } = string.Empty;
    public string Organs { get; set; } = string.Empty;
    public string Weak { get; set; } = string.Empty;
    public string Norm { get; set; } = string.Empty;
    public string Excess { get; set; } = string.Empty;
}

public class DigitContent
{
    public string Title { get; set; } = string.Empty;
    public Dictionary<string, string> Values { get; set; } = new();
}

public class PsychotypeContent
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

public class TransitionContent
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty; 
    public string Condition { get; set; } = string.Empty; 
    public string Effect { get; set; } = string.Empty;   
}

// 2. API RESPONSE MODELS (შედეგები)

// გეომეტრიული ანალიზი (პარაბოლები)
public class GeometryResult
{
    public List<GraphLine> Lines { get; set; } = new();
    public List<IntersectionPoint> Intersections { get; set; } = new(); 
}

public class GraphLine
{
    public string Id { get; set; } = string.Empty; 
    public string Name { get; set; } = string.Empty; 
    public string Equation { get; set; } = string.Empty;
    public List<GraphPoint> Points { get; set; } = new(); 
    
    // საკონტროლო წერტილები 
    public GraphMilestones Milestones { get; set; } = new();
}

public class GraphMilestones
{
    public double AgeAtStart { get; set; }   // x=0.5
    public double AgeAtMax { get; set; }     // x=1.0
    public double AgeAtLimit { get; set; }   // x=1.5
    public double AgeAtEnd { get; set; }     // x=1.625
}

public class GraphPoint
{
    public double X { get; set; } 
    public double AgeY { get; set; } 
}

public class IntersectionPoint
{
    public string Line1 { get; set; } = string.Empty;
    public string Line2 { get; set; } = string.Empty;
    public double Age { get; set; } 
    public string Description { get; set; } = string.Empty;
}

// კარმული ანალიზი
public class ChildKarmaResult
{
    public KarmaStats Father { get; set; } = new();
    public KarmaStats Mother { get; set; } = new();
    public KarmaStats Child { get; set; } = new();
    public int ParentsTotalBalance { get; set; } 
    public ComparisonResult Verdict { get; set; } = new();
}

public class KarmaStats
{
    public string Date { get; set; } = string.Empty;
    public int Count6 { get; set; } 
    public int Count7 { get; set; } 
    public int Count8 { get; set; } 
    public int Balance => (Count7 + Count8) - Count6;
}

// ოჯახის სტაბილურობა
public class FamilyStabilityResult
{
    public long HouseholdStability { get; set; } // БС
    public long SpiritualStability { get; set; } // ДС
    public long TotalStability { get; set; }     // ОСС
    public string Description { get; set; } = string.Empty;
}

// სრული თავსებადობა
public class FullCompatibilityResult
{
    public PartnerInfo Man { get; set; } = new();
    public PartnerInfo Woman { get; set; } = new();
    
    public ComparisonResult Fleshly { get; set; } = new();   
    public ComparisonResult Character { get; set; } = new(); 
    public ComparisonResult Energy { get; set; } = new();    
    public ComparisonResult Family { get; set; } = new();    
    public ComparisonResult Life { get; set; } = new();      
    public ComparisonResult Habits { get; set; } = new();    
    public FamilyStabilityResult StabilityCalculation { get; set; } = new();
}

public class PartnerInfo
{
    public string Date { get; set; } = string.Empty;
    public int FleshlyCount { get; set; } 
    public int CharacterCount { get; set; } 
    public int EnergyCount { get; set; } 
    public int FamilyCount { get; set; }  
    public int LifeCount { get; set; }    
    public int HabitsCount { get; set; }  
}

public class ComparisonResult
{
    public string Status { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

// ძირითადი მატრიცა
public class NumerologyMatrixResponse
{
    public string BirthDate { get; set; } = string.Empty;
    public List<int> MainNumbers { get; set; } = new();
    public List<int> AdditionalNumbers { get; set; } = new();
    public Dictionary<string, object> Matrix { get; set; } = new();
    public Dictionary<string, object> Lines { get; set; } = new();
    public PsychotypeContent Psychotype { get; set; } = new();
    public Dictionary<string, string> Destiny { get; set; } = new();
    public List<object> Graph { get; set; } = new();
    public object PersonalYear { get; set; } = new();
    public List<TransitionResult> Transitions { get; set; } = new();
    public Dictionary<string, USinElement> USinHealth { get; set; } = new();
    public LoveSexualityResult LoveSexuality { get; set; } = new();
    
    
}

public class USinElement
{
    public string ElementName { get; set; } = string.Empty;
    public string YinName { get; set; } = string.Empty;
    public int YinDigit { get; set; }
    public int YinCount { get; set; }
    public string YangName { get; set; } = string.Empty;
    public int YangDigit { get; set; }
    public int YangCount { get; set; }
    public int TotalCount => YinCount + YangCount;
    public string Status { get; set; } = "Norm"; 
    public string Description { get; set; } = string.Empty; 
}

public class LoveSexualityResult
{
    public int FleshlyCount { get; set; }
    public int SpiritualCount { get; set; }
    public string FleshlyDescription { get; set; } = string.Empty;
    public string ComparisonDescription { get; set; } = string.Empty;
}

public class MatrixLines
{
    public int Row1 { get; set; } 
    public int Row2 { get; set; } 
    public int Row3 { get; set; }
    public int Col1 { get; set; } 
    public int Col2 { get; set; } 
    public int Col3 { get; set; }
    public int Diag1 { get; set; } 
    public int Diag2 { get; set; }
}

public class AlexandrovCalculationResult
{
    public string BirthDate { get; set; } = string.Empty;
    public List<int> MainNumbers { get; set; } = new();
    public List<int> AdditionalNumbers { get; set; } = new();
    public Dictionary<string, int> MatrixCounts { get; set; } = new();
    public MatrixLines Lines { get; set; } = new();
    public List<TransitionResult> Transitions { get; set; } = new();
    public int PsychotypeId { get; set; }
    public List<int> GraphPoints { get; set; } = new();
    public int PersonalYearNumber { get; set; }
    public Dictionary<string, USinElement> USinHealth { get; set; } = new();
    public LoveSexualityResult LoveSexuality { get; set; } = new();
    
    // დამატებითი ციფრების ანალიზი 
    public AdditionalNumbersAnalysis AdditionalNumbersAnalysis { get; set; } = new();
}

public class AdditionalNumbersAnalysis
{
    public bool HasGoalMatch { get; set; }
    public string Description { get; set; } = string.Empty;
}

public class TransitionResult
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Condition { get; set; } = string.Empty;
    public string Effect { get; set; } = string.Empty;
    public bool IsPossible { get; set; }
}
