
using System.Collections.Generic;
/// <summary>
/// Moves to adjacent squares, freezes adjacent opponenet pieces. Doesn't get frozen.
/// </summary>
public class Freezer : Piece
{
    public Freezer(int r, int c, PlayerType p) : base(r, c, p) { }

    public override List<PossibleMove> PossibleMoves(Square[,] table, int turn)
    {
        // A Freezer can be frozen(as a state) but doesn't get effected by it.

        // Initialize the linked list
        var possibleMoves = new List<PossibleMove>();

        for (int i = 0; i < 4; i++)
            // In bounds & (empty square | pseudo piece)
            if (Row + e[i, 0] <= nr && Row + e[i, 0] >= 1 && Column + e[i, 1] <= nc && Column + e[i, 1] >= 1 && (table[Row + e[i, 0], Column + e[i, 1]].Piece == null ||
                table[Row + e[i, 0], Column + e[i, 1]].Piece == table[Row + e[i, 0], Column + e[i, 1]].PseudoPiece))
            {
                possibleMoves.Add(new PossibleMove(Row + e[i, 0], Column + e[i, 1], MoveType.Shift));
            }

        return possibleMoves;
    }

    public override void Move(Square[,] table, int toRow, int toColumn, ref int turn)
    {
        ClearSquareStates(table, Row, Column, e);

        turn++;

        table[toRow, toColumn].Piece = table[Row, Column].Piece;
        table[Row, Column].Piece = null;
        Row = toRow;
        Column = toColumn;

        // Check for a opponent mind controller around it
        anotherMindController:
        for (int i = 0; i < 4; i++)
            if (Row + e[i, 0] <= nr && Row + e[i, 0] >= 1 && Column + e[i, 1] <= nc && Column + e[i, 1] >= 1 &&
                table[Row + e[i, 0], Column + e[i, 1]].Piece != null &&
                table[Row + e[i, 0], Column + e[i, 1]].Piece is MindController &&
                table[Row + e[i, 0], Column + e[i, 1]].Piece.Player != table[Row, Column].Piece.Player)
            {
                table[Row, Column].Piece.Player ^= PlayerType.Opponent;
                table[Row, Column].Piece.Revealed = true;
                table[Row, Column].SetImageAccordingToPiece();
                table[Row + e[i, 0], Column + e[i, 1]].Piece = null;
                goto anotherMindController; // Check for another mind controller, possibly initially friendly
            }
    }

    #region IClonable

    public override object Clone() => new Freezer(Row, Column, Player) { Revealed = Revealed };

    #endregion
}
