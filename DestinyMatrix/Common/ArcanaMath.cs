namespace DestinyApp.Common;

public static class ArcanaMath
{
    public static int Reduce(int number)
    {
        while (number > 22)
        {
            number = number.ToString().Sum(c => c - '0');
        }
        return number == 0 ? 22 : number;
    }
}