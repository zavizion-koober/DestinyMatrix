namespace DestinyMatrix.Features.DestinyMatrix;

public static class MatrixMap
{
    // კოორდინატები 200x200 ზომის ტილოზე (Canvas)
    public static readonly Dictionary<string, (int X, int Y)> Coordinates = new()
    {
        // პიროვნული კვადრატი (პირდაპირი მართკუთხედი)
        { "Top", (100, 10) },         // თვე (0 წელი) - სულიერი რესურსი
        { "Right", (190, 100) },       // წელი (20 წელი) - მატერიალური რესურსი
        { "Bottom", (100, 190) },      // კარმა (40 წელი) - წარსული ვალები
        { "Left", (10, 100) },         // დღე (60 წელი) - ფიზიკური რესურსი
        { "Center", (100, 100) },      // კომფორტის ზონა - სულის ცენტრი

        // საგვარეულო კვადრატი (დიაგონალური მართკუთხედი)
        { "TopLeft", (36, 36) },       // მამის ხაზი (ზედა)
        { "TopRight", (164, 36) },     // დედის ხაზი (ზედა)
        { "BottomRight", (164, 164) }, // დედის ხაზი (ქვედა)
        { "BottomLeft", (36, 164) },   // მამის ხაზი (ქვედა)

        // ფულისა და სიყვარულის არხების წერტილები
        { "MoneyEntrance", (145, 100) }, // ფულის არხის შესასვლელი
        { "LoveEntrance", (100, 145) },  // სიყვარულის არხის შესასვლელი
        { "FinancePoint", (164, 132) },  // წმინდა ფულის წერტილი
        { "LovePoint", (132, 164) }      // წმინდა სიყვარულის წერტილი
    };

    // მეთოდი ხაზების დასახატად (SVG-სთვის)
    public static string GetLinePath(string from, string to)
    {
        if (Coordinates.TryGetValue(from, out var start) && Coordinates.TryGetValue(to, out var end))
        {
            return $"M {start.X} {start.Y} L {end.X} {end.Y}";
        }
        return string.Empty;
    }
}