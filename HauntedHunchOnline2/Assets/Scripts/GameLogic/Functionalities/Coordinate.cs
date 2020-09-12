
public class Coordinate
{
    public int Row { get; set; }
    public int Column { get; set; }

    public Coordinate(int row, int column)
    {
        Row = row;
        Column = column;
    }

    public override bool Equals(object obj)
    {
        if (!(obj is Coordinate))
            return false;
        return Row == ((Coordinate)obj).Row && Column == ((Coordinate)obj).Column;
    }

    public override int GetHashCode()
    {
        var hashCode = 240067226;
        hashCode = hashCode * -1521134295 + Row.GetHashCode();
        hashCode = hashCode * -1521134295 + Column.GetHashCode();
        return hashCode;
    }

    public override string ToString()
    {
        return "(" + Row + ", " + Column + ")";
    }
}

