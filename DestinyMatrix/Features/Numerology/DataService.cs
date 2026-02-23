using System.Text.Json;
using DestinyMatrix.Features.Numerology;

namespace DestinyMatrix.Features.Numerology;

public class DataService
{
    private readonly NumerologyDataContent _registry;

    public DataService(IHostEnvironment env)
    {
        string filePath = Path.Combine(env.ContentRootPath, "Data", "matrix_data.json");
        
        if (!File.Exists(filePath)) 
        {
            _registry = new NumerologyDataContent();
            return;
        }

        try 
        {
            string jsonString = File.ReadAllText(filePath);
            _registry = JsonSerializer.Deserialize<NumerologyDataContent>(jsonString, new JsonSerializerOptions 
            { 
                PropertyNameCaseInsensitive = true 
            }) ?? new NumerologyDataContent();
        }
        catch 
        {
            _registry = new NumerologyDataContent();
        }
    }

  

    private string CalculateNameCompatibility(int nameNum, int destinyNum)
    {
        if (nameNum == destinyNum) 
            return "იდეალური თანხვედრა (რეზონანსი). სახელი სრულად ეხმარება ბედისწერის რეალიზაციას.";
        
        if (Math.Abs(nameNum - destinyNum) % 2 == 0)
            return "ჰარმონიული კავშირი. სახელი და ბედისწერა ერთმანეთს ავსებენ.";
            
        return "ენერგიების სხვაობა. სახელი ადამიანს აძლევს დამატებით თვისებებს, რომლებიც ყოველთვის არ ემთხვევა მის თანდაყოლილ ბუნებას.";
    }

    public object GetDigitInfo(int digit, int count)
    {
        string dKey = digit.ToString();
        string cKey = count.ToString();
        string title = $"{digit}-იანი";
        string desc = "ინფორმაცია არ მოიძებნა";

        if (_registry.Digits.TryGetValue(dKey, out var content))
        {
            title = $"{content.Title} ({digit}-იანების რაოდენობა: {count})";
            if (content.Values.TryGetValue(cKey, out var val)) desc = val;
            else if (content.Values.Count > 0) desc = content.Values.Last().Value;
        }
        return new { digit, count, title, description = desc };
    }

    public string GetLineDescription(string lineName, int count)
    {
        if (_registry.Lines.TryGetValue(lineName.ToLower(), out var vals))
        {
            if (vals.TryGetValue(count.ToString(), out var txt)) return txt;
            if (vals.Count > 0) return vals.Last().Value;
        }
        return "ინფორმაცია არ მოიძებნა";
    }

    public PsychotypeContent GetPsychotypeInfo(int typeId)
    {
        return _registry.Psychotypes.TryGetValue(typeId.ToString(), out var p) 
            ? p : new PsychotypeContent { Title = "უცნობი", Description = "" };
    }

    public string GetDestinyInfo(string type, int number)
    {
        if (_registry.Destiny.TryGetValue(type, out var dict))
        {
            if (dict.TryGetValue(number.ToString(), out var text)) return text;
            int sum = number;
            while (sum > 9 && sum != 11 && sum != 22) sum = sum.ToString().Sum(c => c - '0');
            if (dict.TryGetValue(sum.ToString(), out var textSum)) return textSum;
        }
        return "ინფორმაცია არ მოიძებნა";
    }
    
    public TransitionContent? GetTransitionText(string key) => 
        _registry.Transitions.TryGetValue(key, out var t) ? t : null;

    public string GetGraphDescription(int value) => 
        _registry.Graph.TryGetValue(value.ToString(), out var desc) ? desc : "";

    public string GetPersonalYearDescription(int yearNum) =>
        _registry.PersonalYear.TryGetValue(yearNum.ToString(), out var desc) ? desc : "";
    
    public string GetHealthDescription(string elementKey, string status)
    {
        if (_registry.HealthDescriptions.TryGetValue(elementKey, out var content))
        {
            if (status.Contains("Weak")) return content.Weak;
            if (status.Contains("Excess")) return content.Excess;
            return content.Norm;
        }
        return "";
    }
    
    public string GetFleshlyDescription(int count)
    {
        string key = count >= 6 ? "6" : count.ToString();
        if (_registry.LoveSexuality.FleshlyDiagonal.TryGetValue(key, out var desc)) return desc;
        if (count >= 5 && _registry.LoveSexuality.FleshlyDiagonal.TryGetValue("5", out var desc5)) return desc5;
        return "ინფორმაცია არ მოიძებნა";
    }

    public string GetLoveComparisonDescription(int fleshly, int spiritual)
    {
        string key = "harmony"; 
        if (fleshly > spiritual) key = "fleshly_dominates"; 
        else if (spiritual > fleshly) key = "spiritual_dominates"; 
        
        if (_registry.LoveSexuality.Comparison.TryGetValue(key, out var desc)) return desc;
        return "";
    }

    public string GetFleshlyCompatibility(int manCount, int womanCount)
    {
        if (_registry.Compatibility?.Fleshly == null) return "ინფორმაცია არ მოიძებნა";
        var dict = _registry.Compatibility.Fleshly;
        int diff = Math.Abs(manCount - womanCount);
        if (manCount <= 1 && womanCount >= 3) return GetVal(dict, "danger_man_weak");
        if (womanCount == 5 && manCount != 5) return GetVal(dict, "woman_max");
        if (diff == 0) return GetVal(dict, "perfect");
        if (diff == 1) return GetVal(dict, "good");
        return GetVal(dict, "hard");
    }

    public string GetCharacterCompatibility(int count1, int count2)
    {
        if (_registry.Compatibility?.Character == null) return "ინფორმაცია არ მოიძებნა";
        var dict = _registry.Compatibility.Character;
        if (count1 == 3 || count2 == 3) return GetVal(dict, "golden_mean");
        if (count1 >= 4 && count2 >= 4) return GetVal(dict, "war_of_leaders");
        if (count1 <= 2 && count2 <= 2) return GetVal(dict, "passive_pair");
        return GetVal(dict, "ideal_leader_follower");
    }

    public string GetEnergyCompatibility(int count1, int count2)
    {
        if (_registry.Compatibility?.Energy == null) return "ინფორმაცია არ მოიძებნა";
        var dict = _registry.Compatibility.Energy;
        if (count1 == 2 || count2 == 2) return GetVal(dict, "universal_22");
        if (count1 <= 1 && count2 <= 1) return GetVal(dict, "repulsion_weak");     
        if (count1 >= 3 && count2 >= 3) return GetVal(dict, "repulsion_strong"); 
        return GetVal(dict, "ideal_donor_vampire");
    }

    public string GetFamilyLineCompatibility(int count1, int count2)
    {
        if (_registry.Compatibility?.Family == null) return "ინფორმაცია არ მოიძებნა";
        var dict = _registry.Compatibility.Family;
        if (count1 >= 4 && count2 >= 4) return GetVal(dict, "ideal_strong_family");
        if (count1 <= 2 && count2 <= 2) return GetVal(dict, "weak_family_desire");
        return GetVal(dict, "one_pulls_family");
    }

    public string GetLifeLineCompatibility(int manCount, int womanCount)
    {
        if (_registry.Compatibility?.Stability == null) return "ინფორმაცია არ მოიძებნა";
        var dict = _registry.Compatibility.Stability;
        if (womanCount >= 4 && manCount <= 2) return GetVal(dict, "woman_provider_risk");
        if (manCount >= womanCount) return GetVal(dict, "man_provider_norm");
        return GetVal(dict, "average_life_stability");
    }

    public string GetHabitsCompatibility(int count1, int count2)
    {
        if (_registry.Compatibility?.Habits == null) return "ინფორმაცია არ მოიძებნა";
        var dict = _registry.Compatibility.Habits;
        if (count1 == count2) return GetVal(dict, "match");
        bool p1High = count1 >= 3;
        bool p2High = count2 >= 3;
        bool p1Low = count1 <= 2;
        bool p2Low = count2 <= 2;
        if (p1High && p2High) return GetVal(dict, "both_conservative");
        if (p1Low && p2Low) return GetVal(dict, "both_revolutionary");
        return GetVal(dict, "revolutionary_conservative");
    }

    public string GetKarmaAnalysis(int parentBalance, int childBalance, int child6)
    {
        if (_registry.Karma?.General == null) return "ინფორმაცია არ მოიძებნა";
        var dict = _registry.Karma.General;
        if (childBalance < 0)
        {
            if (childBalance <= -2) return GetVal(dict, "critical_burden");
            return GetVal(dict, "karmic_debt");
        }
        if (childBalance < parentBalance && child6 > 0)
        {
            return GetVal(dict, "lineage_weakening");
        }
        return GetVal(dict, "clean_karma");
    }

    public string GetGeometryIntersection(string line1Id, string line2Id)
    {
        if (_registry.Geometry?.Intersections == null) 
            return "გადაკვეთა: ორი სფეროს ურთიერთქმედება.";
        var keys = new List<string> { line1Id, line2Id };
        keys.Sort();
        string key = $"{keys[0]}_{keys[1]}";
        if (_registry.Geometry.Intersections.TryGetValue(key, out var text)) return text;
        return _registry.Geometry.Intersections.TryGetValue("generic_intersection", out var gen) 
            ? gen : "მნიშვნელოვანი ცხოვრებისეული გზაჯვარედინი.";
    }

    private string GetVal(Dictionary<string, string> dict, string key) => 
        dict.TryGetValue(key, out var val) ? val : "ინფორმაცია არ მოიძებნა";
}
