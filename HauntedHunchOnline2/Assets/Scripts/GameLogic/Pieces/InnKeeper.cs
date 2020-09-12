using System;
using System.Collections.Generic;

/// <summary>
/// Is a psuedo piece. Refer to the manual.
/// </summary>
public class InnKeeper : Piece, Passivable
{
    public InnKeeper(int r, int c, PlayerType p) : base(r, c, p) { }

    public override List<PossibleMove> PossibleMoves(Square[,] table, int turn)
    {
        // Frozen check
        if (IsFrozen(table, Row, Column)) return null;

        // Initialize the linked list
        var possibleMoves = new List<PossibleMove>();

        for (int i = 0; i < 8; i++)
            // In bounds & (short range | in between is empty) && (empty square | opponent Lotus)
            if (Row + l[i, 0] <= nr && Row + l[i, 0] >= 1 && Column + l[i, 1] <= nc && Column + l[i, 1] >= 1 &&
                (i <= 3 || i >= 4 && table[Row + l[i, 0] / 2, Column + l[i, 1] / 2].Piece == null))
            {
                // Empty square
                if (table[Row + l[i, 0], Column + l[i, 1]].Piece == null)
                {
                    possibleMoves.Add(new PossibleMove(Row + l[i, 0], Column + l[i, 1], MoveType.Shift));
                }

                // Opponent lotus
                else if (table[Row + l[i, 0], Column + l[i, 1]].Piece is Lotus &&
                         table[Row + l[i, 0], Column + l[i, 1]].Piece.Revealed &&
                         table[Row + l[i, 0], Column + l[i, 1]].Piece.Player != Player &&
                         turn % 2 == 1)
                {
                    possibleMoves.Add(new PossibleMove(Row + l[i, 0], Column + l[i, 1], MoveType.Capture));
                }
            }

        return possibleMoves;
    }

    public override void Move(Square[,] table, int toRow, int toColumn, ref int turn)
    {
        ClearSquareStates(table, Row, Column, l);

        if (Math.Abs(Row + Column - toRow - toColumn) == 2)
            Revealed = true;

        if (IsHiddenlyFrozen(table, Row, Column)) return;

        turn += table[toRow, toColumn].Piece == null ? 1 : 2;

        table[toRow, toColumn].Piece = table[Row, Column].Piece;
        table[Row, Column].PseudoPiece = null;
        table[Row, Column].Piece = null;
        Row = toRow;
        Column = toColumn;
    }

    #region IClonable

    public override object Clone() => new InnKeeper(Row, Column, Player) { Revealed = Revealed };

    #endregion
}

