public class Shift
{
    public Coordinate From { get; set; }
    public Coordinate To { get; set; }

    public Shift(Coordinate from, Coordinate to)
    {
        From = from;
        To = to;
    }
}
