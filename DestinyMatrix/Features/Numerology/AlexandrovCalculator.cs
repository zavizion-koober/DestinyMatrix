using DestinyMatrix.Features.Numerology;

namespace DestinyMatrix.Features.Numerology;

public class AlexandrovCalculator
{
    // 1. საბაზისო გამოთვლა (თარიღის დაშლა და დამატებითი ციფრები)
    public (int[] mainRow, int[] additionalRow) CalculateNumbers(DateTime birthDate)
    {
        string dateStrFull = birthDate.ToString("ddMMyyyy");
        string dateStrNoZeros = dateStrFull.Replace("0", "");
        
        int[] mainRow = dateStrNoZeros.Select(c => int.Parse(c.ToString())).ToArray();

        int firstNum = dateStrFull.Select(c => int.Parse(c.ToString())).Sum();
        int secondNum = SumDigits(firstNum);

        int day = birthDate.Day;
        int firstDigitOfDate = (day < 10) ? day : int.Parse(day.ToString().Substring(0, 1));
        
        int thirdNum = firstNum - (firstDigitOfDate * 2);
        int fourthNum = SumDigits(Math.Abs(thirdNum));

        return (mainRow, new[] { firstNum, secondNum, thirdNum, fourthNum });
    }

    private int SumDigits(int n)
    {
        return Math.Abs(n).ToString().Select(c => int.Parse(c.ToString())).Sum();
    }

    // 2. გადასვლების (ტრანსფორმაციების) ლოგიკა
    public List<TransitionResult> GetTransitions(Dictionary<string, int> matrix)
    {
        var transitions = new List<TransitionResult>();
        int Get(string key) => matrix.ContainsKey(key) ? matrix[key] : 0;

        int c1 = Get("Digit_1");
        int c2 = Get("Digit_2");
        int c4 = Get("Digit_4");
        int c6 = Get("Digit_6");
        int c8 = Get("Digit_8");

        if (c1 >= 3) transitions.Add(new TransitionResult { Name = "11_to_8", IsPossible = true });
        if (c8 >= 1) transitions.Add(new TransitionResult { Name = "8_to_11", IsPossible = true });
        if (c6 >= 1 && (c2 >= 2 || c4 >= 1)) transitions.Add(new TransitionResult { Name = "6_to_7", IsPossible = true });
        if (c2 >= 2) transitions.Add(new TransitionResult { Name = "22_to_4", IsPossible = true });
        if (c4 >= 1) transitions.Add(new TransitionResult { Name = "4_to_22", IsPossible = true });

        return transitions;
    }

    // 3. გრაფიკის გამოთვლა (ცხოვრების ძალების გრაფიკი)
    public List<int> CalculateGraph(DateTime date)
    {
        long dayMonth = date.Day * 100 + date.Month; 
        long year = date.Year;
        long result = dayMonth * year; 
        return result.ToString().Select(c => int.Parse(c.ToString())).ToList();
    }

    // 4. პერსონალური წლის გამოთვლა
    public int CalculatePersonalYear(DateTime birthDate)
    {
        int currentYear = DateTime.Now.Year;
        int sum = birthDate.Day + birthDate.Month + currentYear;
        while (sum > 9)
        {
            sum = sum.ToString().Select(c => int.Parse(c.ToString())).Sum();
        }
        return sum;
    }

    // 5. U-Sin ჯანმრთელობის გამოთვლა (ხუთი ელემენტის სისტემა)
    private Dictionary<string, USinElement> CalculateUSin(List<int> allDigitsWithZeros)
    {
        int Count(int digit) => allDigitsWithZeros.Count(x => x == digit);

        var usin = new Dictionary<string, USinElement>();

        usin.Add("Wood", CreateElement("Wood", "ღვიძლი", 8, Count(8), "ნაღვლის ბუშტი", 0, Count(0)));
        usin.Add("Fire", CreateElement("Fire", "გული", 7, Count(7), "წვრილი ნაწლავი", 4, Count(4)));
        usin.Add("Earth", CreateElement("Earth", "ელენთა", 5, Count(5), "კუჭი", 3, Count(3)));
        usin.Add("Metal", CreateElement("Metal", "ფილტვები", 9, Count(9), "მსხვილი ნაწლავი", 2, Count(2)));
        usin.Add("Water", CreateElement("Water", "თირკმელები", 6, Count(6), "შარდის ბუშტი", 1, Count(1)));

        return usin;
    }

    private USinElement CreateElement(string keyName, string yinName, int yinDigit, int yinCount, string yangName, int yangDigit, int yangCount)
    {
        int total = yinCount + yangCount;
        string status = "Norm";
        
        if (total <= 1) status = "Weak";
        else if (total >= 3) status = "Excess";
        
        return new USinElement
        {
            ElementName = keyName,
            YinName = yinName, YinDigit = yinDigit, YinCount = yinCount,
            YangName = yangName, YangDigit = yangDigit, YangCount = yangCount,
            Status = status,
            Description = "" 
        };
    }
    
    // 6. მთავარი მეთოდი (სრული გაანგარიშება)
    public AlexandrovCalculationResult CalculateFullMatrix(DateTime date)
    {
        var (mainRow, additionalRow) = CalculateNumbers(date);

        // U-Sin-ისთვის ნულების შენარჩუნება
        var digitsWithZeros = new List<int>();
        string fullDateStr = date.ToString("ddMMyyyy"); 
        foreach (char c in fullDateStr) digitsWithZeros.Add(int.Parse(c.ToString())); 
        
        foreach (var num in additionalRow)
        {
            string s = Math.Abs(num).ToString();
            digitsWithZeros.AddRange(s.Select(c => int.Parse(c.ToString())));
        }

        // მატრიცისთვის ნულების წაშლა
        var digitsForMatrix = digitsWithZeros.Where(d => d > 0).ToList();
        var matrixCounts = new Dictionary<string, int>();
        for (int i = 1; i <= 9; i++) matrixCounts[$"Digit_{i}"] = digitsForMatrix.Count(d => d == i);

        int GetCnt(int d) => matrixCounts.ContainsKey($"Digit_{d}") ? matrixCounts[$"Digit_{d}"] : 0;
        
        var lines = new MatrixLines
        {
            Row1 = GetCnt(1) + GetCnt(4) + GetCnt(7),
            Row2 = GetCnt(2) + GetCnt(5) + GetCnt(8),
            Row3 = GetCnt(3) + GetCnt(6) + GetCnt(9),
            Col1 = GetCnt(1) + GetCnt(2) + GetCnt(3),
            Col2 = GetCnt(4) + GetCnt(5) + GetCnt(6),
            Col3 = GetCnt(7) + GetCnt(8) + GetCnt(9),
            Diag1 = GetCnt(3) + GetCnt(5) + GetCnt(7),
            Diag2 = GetCnt(1) + GetCnt(5) + GetCnt(9)
        };
        
        int psychotypeId = 3;
        if (GetCnt(1) > GetCnt(2)) psychotypeId = 1;
        else if (GetCnt(2) > GetCnt(1)) psychotypeId = 2;

        int fleshlySum = GetCnt(3) + GetCnt(5) + GetCnt(7);
        int spiritualSum = GetCnt(1) + GetCnt(5) + GetCnt(9);

        var loveResult = new LoveSexualityResult
        {
            FleshlyCount = fleshlySum,
            SpiritualCount = spiritualSum,
            FleshlyDescription = "",
            ComparisonDescription = ""
        };

        return new AlexandrovCalculationResult
        {
            BirthDate = date.ToString("dd.MM.yyyy"),
            MainNumbers = mainRow.ToList(),
            AdditionalNumbers = additionalRow.ToList(),
            MatrixCounts = matrixCounts,
            Lines = lines,
            Transitions = GetTransitions(matrixCounts),
            PsychotypeId = psychotypeId,
            GraphPoints = CalculateGraph(date),
            PersonalYearNumber = CalculatePersonalYear(date),
            USinHealth = CalculateUSin(digitsWithZeros),
            LoveSexuality = loveResult 
        };
    }

    // --- ქორწინების სტაბილურობის გამოთვლა ---
    public FamilyStabilityResult CalculateStability(AlexandrovCalculationResult man, AlexandrovCalculationResult woman)
    {
        // ყოფითი სტაბილურობა (БС) = FleshlyDiag * Row3 * Row2
        long CalcBS(AlexandrovCalculationResult p) 
        {
            long diagFlesh = p.LoveSexuality.FleshlyCount == 0 ? 1 : p.LoveSexuality.FleshlyCount; 
            long row3 = p.Lines.Row3 == 0 ? 1 : p.Lines.Row3;
            long row2 = p.Lines.Row2 == 0 ? 1 : p.Lines.Row2;
            return diagFlesh * row3 * row2;
        }

        long bsMan = CalcBS(man);
        long bsWoman = CalcBS(woman);
        long householdStabilityFamily = bsMan * bsWoman; 

        // სულიერი სტაბილურობა (ДС) = SpiritualDiag * Col1 * Row1
        long CalcDS(AlexandrovCalculationResult p)
        {
            long diagSpirit = p.LoveSexuality.SpiritualCount == 0 ? 1 : p.LoveSexuality.SpiritualCount;
            long col1 = p.Lines.Col1 == 0 ? 1 : p.Lines.Col1;
            long row1 = p.Lines.Row1 == 0 ? 1 : p.Lines.Row1;
            return diagSpirit * col1 * row1;
        }

        long dsMan = CalcDS(man);
        long dsWoman = CalcDS(woman);
        long spiritualStabilityFamily = dsMan * dsWoman; 

        // საერთო სტაბილურობა (ОСС) = БСС + ДСС
        long totalStability = householdStabilityFamily + spiritualStabilityFamily;

        return new FamilyStabilityResult
        {
            HouseholdStability = householdStabilityFamily,
            SpiritualStability = spiritualStabilityFamily,
            TotalStability = totalStability,
            Description = $"საერთო სტაბილურობის მაჩვენებელია {totalStability}. " +
                          $"რაც უფრო მაღალია ეს რიცხვი, მით უფრო დიდხანს შეუძლია წყვილს შეინარჩუნოს კავშირი კრიზისების დროს."
        };
    }
}
