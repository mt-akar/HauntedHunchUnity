public class PossibleMove
{
    public int Row { get; }
    public int Column { get; }
    public MoveType MoveType { get; }

    public PossibleMove(int row, int column, MoveType moveType)
    {
        Row = row;
        Column = column;
        MoveType = moveType;
    }
}
