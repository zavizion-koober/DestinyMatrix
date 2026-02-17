using MediatR;
using DestinyApp.Common;
using System.Text;

namespace DestinyMatrix.Features.DestinyMatrix;

public static class CalculateMatrix
{
    public record Query(int Day, int Month, int Year) : IRequest<Response>;

    public record Response(
        BaseNodes Nodes,
        AncestorNodes Ancestors,
        DestinyTargets Targets,
        int Center,
        List<ChakraRow> HealthMap,
        FullInterpretation Interpretations,
        List<AgeArcana> YearlyForecast,
        List<MapPoint> VisualMap,
        string PersonalReport
    );

    public record BaseNodes(int Left, int Top, int Right, int Bottom);
    public record AncestorNodes(int TopLeft, int TopRight, int BottomRight, int BottomLeft);
    public record DestinyTargets(int Sky, int Earth, int Personal, int Social, int Spiritual);
    public record ChakraRow(string Name, int Physical, int Energy, int Total);
    public record AgeArcana(int Age, int Arcana);
    public record MapPoint(string Label, int Value, int X, int Y);

    public record FullInterpretation(
        ArcanaContent Resource,
        ArcanaContent Comfort,
        ArcanaContent Money,
        ArcanaContent Love,
        ArcanaContent KarmicTail,
        ArcanaContent FamilyKarma 
    );

    public record ArcanaContent(int Id, string Name, string Plus, string Minus, string ZoneText);

    public class Handler : IRequestHandler<Query, Response>
    {
        private readonly IInterpretationService _interpreter;

        public Handler(IInterpretationService interpreter)
        {
            _interpreter = interpreter;
        }

        public Task<Response> Handle(Query request, CancellationToken ct)
        {
            // 1. ძირითადი წერტილების მათემატიკა
            int left = ArcanaMath.Reduce(request.Day);
            int top = ArcanaMath.Reduce(request.Month);
            int right = ArcanaMath.Reduce(SumDigits(request.Year));
            int bottom = ArcanaMath.Reduce(left + top + right);
            int center = ArcanaMath.Reduce(left + top + right + bottom);

            // 2. საგვარეულო დიაგონალები
            int topLeft = ArcanaMath.Reduce(left + top);
            int topRight = ArcanaMath.Reduce(top + right);
            int bottomRight = ArcanaMath.Reduce(right + bottom);
            int bottomLeft = ArcanaMath.Reduce(bottom + left);

            // 3. დანიშნულებები
            int sky = ArcanaMath.Reduce(top + bottom);
            int earth = ArcanaMath.Reduce(left + right);
            int personal = ArcanaMath.Reduce(sky + earth);
            int maleLine = ArcanaMath.Reduce(topLeft + bottomRight);
            int femaleLine = ArcanaMath.Reduce(topRight + bottomLeft);
            int social = ArcanaMath.Reduce(maleLine + femaleLine);
            int spiritual = ArcanaMath.Reduce(personal + social);

            // 4. შუალედური წერტილები (ფული და სიყვარული)
            int v4 = ArcanaMath.Reduce(bottom + center); 
            int h4 = ArcanaMath.Reduce(right + center);  

            // 5. ვიზუალური რუკის წერტილები (UI-სთვის)
            var visualMap = new List<MapPoint>();
            foreach (var coord in MatrixMap.Coordinates)
            {
                int val = coord.Key switch {
                    "Top" => top, "Right" => right, "Bottom" => bottom, "Left" => left,
                    "Center" => center, "TopLeft" => topLeft, "TopRight" => topRight,
                    "BottomRight" => bottomRight, "BottomLeft" => bottomLeft,
                    "MoneyEntrance" => h4, "LoveEntrance" => v4,
                    _ => 0
                };
                visualMap.Add(new MapPoint(coord.Key, val, coord.Value.X, coord.Value.Y));
            }

            // 6. ჯანმრთელობის რუკა და წლიური პროგნოზი
            var healthMap = CalculateHealth(top, bottom, left, right, center);
            var yearlyForecast = CalculateYearlyForecast(
                new BaseNodes(left, top, right, bottom), 
                new AncestorNodes(topLeft, topRight, bottomRight, bottomLeft));

            // 7. ტექსტური ინტერპრეტაციების წამოღება
            var interpretations = new FullInterpretation(
                Resource: GetContent(left, "resource"),
                Comfort: GetContent(center, "comfort"),
                Money: GetContent(right, "money"),
                Love: GetContent(v4, "love"),
                KarmicTail: GetContent(bottom, "karmic_tail"),
                FamilyKarma: GetContent(maleLine, "parental_karma")
            );

            // 8. სრული პერსონალური რეპორტის გენერაცია
            string report = GeneratePersonalReport(
                new BaseNodes(left, top, right, bottom), 
                center, 
                interpretations, 
                yearlyForecast, 
                request.Year,
                earth
            );

            return Task.FromResult(new Response(
                new BaseNodes(left, top, right, bottom),
                new AncestorNodes(topLeft, topRight, bottomRight, bottomLeft),
                new DestinyTargets(sky, earth, personal, social, spiritual),
                center,
                healthMap,
                interpretations,
                yearlyForecast,
                visualMap,
                report
            ));
        }

        private string GeneratePersonalReport(BaseNodes nodes, int center, FullInterpretation interp, List<AgeArcana> forecast, int birthYear, int earth)
        {
            var sb = new StringBuilder();
            int currentAge = 2026 - birthYear;

            sb.AppendLine("# ბედის მატრიცის პერსონალური ანალიზი\n");

            sb.AppendLine("## 1. პიროვნების ბირთვი და რესურსები");
            sb.AppendLine($"**მატრიცის ცენტრი ({center} — {interp.Comfort.Name}):** {interp.Comfort.ZoneText}");
            sb.AppendLine($"**ნიჭის წერტილი ({nodes.Left} — {interp.Resource.Name}):** {interp.Resource.ZoneText}\n");

            sb.AppendLine("## 2. ფინანსები და პირადი ცხოვრება");
            sb.AppendLine($"**ფულის არხი ({nodes.Right} — {interp.Money.Name}):** {interp.Money.ZoneText}");
            sb.AppendLine($"**სიყვარულის არხი ({interp.Love.Id} — {interp.Love.Name}):** {interp.Love.ZoneText}\n");

            sb.AppendLine("## 3. საგვარეულო ამოცანები და კარმა");
            sb.AppendLine($"**საგვარეულო დავალება ({interp.FamilyKarma.Id} — {interp.FamilyKarma.Name}):** {interp.FamilyKarma.ZoneText}");
            sb.AppendLine($"**კარმული კუდი ({nodes.Bottom} — {interp.KarmicTail.Name}):** {interp.KarmicTail.ZoneText}\n");

            var currentYearArcana = forecast.FirstOrDefault(x => x.Age == currentAge)?.Arcana ?? 0;
            var ageInfo = _interpreter.GetFullInfo(currentYearArcana);
            
            sb.AppendLine("## 4. ასაკობრივი პროგნოზი");
            sb.AppendLine($"**ამჟამად (თქვენ ხართ {currentAge} წლის):** იმყოფებით {currentYearArcana} არკანის გავლენის ქვეშ ({ageInfo?.Name}).");
            sb.AppendLine($"*რეკომენდაცია:* {ageInfo?.Energy.Plus ?? "იმოქმედეთ თავდაჯერებულად!"}\n");

            sb.AppendLine("## რეზიუმე");
            sb.AppendLine($"თქვენი მიზნების დეფიციტი (Earth = {earth}) მიუთითებს იმაზე, რომ გჭირდებათ მეტი მიწაზე დგომა და მკაფიო გეგმები.");

            return sb.ToString();
        }

        private List<ChakraRow> CalculateHealth(int t, int b, int l, int r, int c)
        {
            return new List<ChakraRow>
            {
                new ChakraRow("საჰასრარა", t, r, ArcanaMath.Reduce(t + r)),
                new ChakraRow("ანაჰატა", c, c, ArcanaMath.Reduce(c + c)),
                new ChakraRow("მულადჰარა", b, l, ArcanaMath.Reduce(b + l))
            };
        }

        private List<AgeArcana> CalculateYearlyForecast(BaseNodes bn, AncestorNodes an)
        {
            var forecast = new List<AgeArcana>();
            var points = new Dictionary<int, int> {
                { 0, bn.Left }, { 10, an.TopLeft }, { 20, bn.Top }, { 30, an.TopRight },
                { 40, bn.Right }, { 50, an.BottomRight }, { 60, bn.Bottom }, { 70, an.BottomLeft }, { 80, bn.Left }
            };
            for (int age = 0; age <= 80; age++) {
                int start = (age / 10) * 10;
                int end = start == 80 ? 80 : start + 10;
                int val = age % 10 == 0 ? points[age] : ArcanaMath.Reduce(points[start] + points[end]);
                forecast.Add(new AgeArcana(age, val));
            }
            return forecast;
        }

        private ArcanaContent GetContent(int id, string zone)
        {
            var info = _interpreter.GetFullInfo(id);
            return new ArcanaContent(
                Id: id,
                Name: info?.Name ?? "Unknown",
                Plus: info?.Energy.Plus ?? "",
                Minus: info?.Energy.Minus ?? "",
                ZoneText: _interpreter.GetDescription(id, zone)
            );
        }

        private int SumDigits(int n) => n.ToString().Sum(c => c - '0');
    }
}