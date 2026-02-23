using MediatR;
using DestinyMatrix.Data; 

namespace DestinyMatrix.Features.Numerology;

public static class CalculateNumerology
{
    public record Query(int Day, int Month, int Year) : IRequest<NumerologyMatrixResponse>;

    public class Handler : IRequestHandler<Query, NumerologyMatrixResponse>
    {
        private readonly AlexandrovCalculator _calculator;
        private readonly DataService _dataService;

        public Handler(AlexandrovCalculator calculator, DataService dataService)
        {
            _calculator = calculator;
            _dataService = dataService;
        }

        public Task<NumerologyMatrixResponse> Handle(Query request, CancellationToken cancellationToken)
        {
            var birthDate = new DateTime(request.Year, request.Month, request.Day);
            var mathResult = _calculator.CalculateFullMatrix(birthDate);

            foreach (var transition in mathResult.Transitions)
            {
                var textData = _dataService.GetTransitionText(transition.Name);
                if (textData != null)
                {
                    transition.Name = textData.Name; 
                    transition.Description = textData.Description;
                    transition.Condition = textData.Condition;
                    transition.Effect = textData.Effect;
                }
            }

            foreach (var kvp in mathResult.USinHealth)
            {
                string elementKey = kvp.Key;
                var elementData = kvp.Value;
                elementData.Description = _dataService.GetHealthDescription(elementKey, elementData.Status);
            }

            var love = mathResult.LoveSexuality;
            love.FleshlyDescription = _dataService.GetFleshlyDescription(love.FleshlyCount);
            love.ComparisonDescription = _dataService.GetLoveComparisonDescription(love.FleshlyCount, love.SpiritualCount);

            var response = new NumerologyMatrixResponse
            {
                BirthDate = mathResult.BirthDate,
                MainNumbers = mathResult.MainNumbers,
                AdditionalNumbers = mathResult.AdditionalNumbers,
                Transitions = mathResult.Transitions,
                USinHealth = mathResult.USinHealth,
                LoveSexuality = mathResult.LoveSexuality,
                Matrix = new Dictionary<string, object>(),
                Lines = new Dictionary<string, object>(),
                Destiny = new Dictionary<string, string>(),
                Graph = new List<object>(),
                PersonalYear = new object()
            };

            foreach (var kvp in mathResult.MatrixCounts)
            {
                int digit = int.Parse(kvp.Key.Split('_')[1]);
                response.Matrix.Add(kvp.Key, _dataService.GetDigitInfo(digit, kvp.Value));
            }

            void AddLine(string key, int val) => 
                response.Lines.Add(key, new { count = val, description = _dataService.GetLineDescription(key, val) });

            AddLine("row1", mathResult.Lines.Row1); AddLine("row2", mathResult.Lines.Row2); AddLine("row3", mathResult.Lines.Row3);
            AddLine("col1", mathResult.Lines.Col1); AddLine("col2", mathResult.Lines.Col2); AddLine("col3", mathResult.Lines.Col3);
            AddLine("diag1", mathResult.Lines.Diag1); AddLine("diag2", mathResult.Lines.Diag2);

            response.Psychotype = _dataService.GetPsychotypeInfo(mathResult.PsychotypeId);

            if (mathResult.AdditionalNumbers.Count >= 4)
            {
                response.Destiny = new Dictionary<string, string>
                {
                    { "goal", _dataService.GetDestinyInfo("goal", mathResult.AdditionalNumbers[1]) },
                    { "base", _dataService.GetDestinyInfo("base", mathResult.AdditionalNumbers[3]) }
                };
            }

            int startYear = birthDate.Year;
            for (int i = 0; i < mathResult.GraphPoints.Count; i++)
            {
                int val = mathResult.GraphPoints[i];
                response.Graph.Add(new { year = startYear + (i * 12), value = val, description = _dataService.GetGraphDescription(val) });
            }

            int pYearNum = mathResult.PersonalYearNumber;
            response.PersonalYear = new { year = DateTime.Now.Year, number = pYearNum, description = _dataService.GetPersonalYearDescription(pYearNum) };

            return Task.FromResult(response);
        }
    }
}
