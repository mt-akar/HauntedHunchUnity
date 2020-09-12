
using System.Collections.Generic;
/// <summary>
/// Moves to adjacent squares. Can suicide, removing all adjacent pieces.
/// </summary>
public class Boomer : Piece
{
    public Boomer(int r, int c, PlayerType p) : base(r, c, p) { }

    public override List<PossibleMove> PossibleMoves(Square[,] table, int turn)
    {
        // Frozen check
        if (IsFrozen(table, Row, Column)) return null;

        // Initialize the linked list
        var possibleMoves = new List<PossibleMove>();

        // Suicide
        possibleMoves.Add(new PossibleMove(Row, Column, MoveType.AbilityUno));

        for (int i = 0; i < 4; i++)
            // In bounds & (empty square | psuedo piece)
            if (Row + e[i, 0] <= nr && Row + e[i, 0] >= 1 && Column + e[i, 1] <= nc && Column + e[i, 1] >= 1 && (table[Row + e[i, 0], Column + e[i, 1]].Piece == null ||
                table[Row + e[i, 0], Column + e[i, 1]].Piece == table[Row + e[i, 0], Column + e[i, 1]].PseudoPiece))
            {
                possibleMoves.Add(new PossibleMove(Row + e[i, 0], Column + e[i, 1], MoveType.Shift));
            }

        return possibleMoves;
    }

    public override GameBoardUpdate Move(Square[,] table, int toRow, int toColumn, ref int turn)
    {
        ClearSquareStates(table, Row, Column, e);

        if (IsHiddenlyFrozen(table, Row, Column)) return null;

        turn++;

        table[toRow, toColumn].Piece = table[Row, Column].Piece;
        table[Row, Column].Piece = null;
        Row = toRow;
        Column = toColumn;

        return new GameBoardUpdate(singleShift: new Shift(from: new Coordinate(Row, Column), to: new Coordinate(toRow, toColumn)));
    }

    // Suicide
    public override void AbilityUno(Square[,] table, ref int turn)
    {
        ClearSquareStates(table, Row, Column, e);

        if (IsHiddenlyFrozen(table, Row, Column))
        {
            Revealed = true;
            return;
        }

        turn++;

        for (int i = 0; i < 4; i++)
            // In bounds & not null
            if (Row + e[i, 0] <= nr && Row + e[i, 0] >= 1 && Column + e[i, 1] <= nc && Column + e[i, 1] >= 1 && table[Row + e[i, 0], Column + e[i, 1]].Piece != null)
            {
                table[Row + e[i, 0], Column + e[i, 1]].PseudoPiece = null;
                table[Row + e[i, 0], Column + e[i, 1]].Piece = null;
            }
        table[Row, Column].PseudoPiece = null;
        table[Row, Column].Piece = null;
    }

    #region IClonable

    public override object Clone() => new Boomer(Row, Column, Player) { Revealed = Revealed };

    #endregion
}
