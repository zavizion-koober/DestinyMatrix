using DestinyMatrix.Features.Numerology;

namespace DestinyMatrix.Features.Numerology;

public class GeometryService
{
    // მასშტაბის კოეფიციენტი R = 4 წელი 
    private const double R_COEFFICIENT = 4.0; 

    public GeometryResult CalculateParabolas(AlexandrovCalculationResult matrixData)
    {
        var result = new GeometryResult();
        var counts = matrixData.MatrixCounts; 

        //  იღებს ციფრების რაოდენობას
        int C(int digit) => counts.ContainsKey($"Digit_{digit}") ? counts[$"Digit_{digit}"] : 0;

        // 8 ხაზის განსაზღვრა 
        // a = პირველი უჯრა, b = მეორე, c = მესამე
        var definitions = new List<(string Id, string Name, int a, int b, int c)>
        {
            ("row1", "მიზანდასახულობა (სტრიქონი 1)", C(1), C(4), C(7)),
            ("row2", "ოჯახი (სტრიქონი 2)", C(2), C(5), C(8)),
            ("row3", "სტაბილურობა (სტრიქონი 3)", C(3), C(6), C(9)),
            
            ("col1", "თვითშეფასება (სვეტი 1)", C(1), C(2), C(3)),
            ("col2", "ყოფა/ფინანსები (სვეტი 2)", C(4), C(5), C(6)),
            ("col3", "ნიჭი (სვეტი 3)", C(7), C(8), C(9)),
            
            ("diag1", "ტემპერამენტი (დიაგონალი 3-5-7)", C(3), C(5), C(7)),
            ("diag2", "სულიერება (დიაგონალი 1-5-9)", C(1), C(5), C(9))
        };

        foreach (var def in definitions)
        {
            var line = new GraphLine
            {
                Id = def.Id,
                Name = def.Name,
                // ფორმულა ჩვენებისთვის
                Equation = $"Age = ({def.a}x² + {def.b}x + {def.c}) × 4",
                Points = GeneratePoints(def.a, def.b, def.c)
            };
            result.Lines.Add(line);
        }

        return result;
    }

    private List<GraphPoint> GeneratePoints(int a, int b, int c)
    {
        var points = new List<GraphPoint>();
        
        // ვაგენერირებთ X ღერძს 0-დან 3.5-მდე (აქტივობის ზონები)
        for (double x = 0; x <= 3.5; x += 0.1)
        {
            // პარაბოლის განტოლება
            double rawY = (a * x * x) + (b * x) + c;
            
            // გადაყვანა ასაკში (წლები)
            double age = rawY * R_COEFFICIENT;

            points.Add(new GraphPoint 
            { 
                X = Math.Round(x, 2), 
                AgeY = Math.Round(age, 1) 
            });
        }
        return points;
    }
}
