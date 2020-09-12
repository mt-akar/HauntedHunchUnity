using System;
using System.Collections.Generic;

/// <summary>
/// The checkers piece which is able to move backwards and can't capture more than 1 piece at once.
/// TODO: Double capture jump
/// </summary>
public class Jumper : Piece
{
    public Jumper(int r, int c, PlayerType p) : base(r, c, p) { }

    public override List<PossibleMove> PossibleMoves(Square[,] table, int turn)
    {
        // Frozen check
        if (IsFrozen(table, Row, Column)) return null;

        // Initialize the linked list
        var possibleMoves = new List<PossibleMove>();

        for (int i = 0; i < 8; i++)
            // In bounds
            if (Row + l[i, 0] <= nr && Row + l[i, 0] >= 1 && Column + l[i, 1] <= nc && Column + l[i, 1] >= 1 &&

                // destination is empty square
                (table[Row + l[i, 0], Column + l[i, 1]].Piece == null ||

                // destination is psuedo piece
                table[Row + l[i, 0], Column + l[i, 1]].Piece is InnKeeper))
            {
                // Shift
                if (i <= 3)
                {
                    possibleMoves.Add(new PossibleMove(Row + l[i, 0], Column + l[i, 1], MoveType.Shift));
                }

                // Jump
                else if (i >= 4 &&
                         table[Row + l[i, 0] / 2, Column + l[i, 1] / 2].Piece != null &&
                         table[Row + l[i, 0] / 2, Column + l[i, 1] / 2].Piece.Player != Player &&
                         !(table[Row + l[i, 0] / 2, Column + l[i, 1] / 2].Piece is InnKeeper) &&
                         turn % 2 == 1)
                {
                    possibleMoves.Add(new PossibleMove(Row + l[i, 0], Column + l[i, 1], MoveType.Jump));
                }
            }

        return possibleMoves;
    }

    public override void Move(Square[,] table, int toRow, int toColumn, ref int turn)
    {
        ClearSquareStates(table, Row, Column, l);

        if (IsHiddenlyFrozen(table, Row, Column)) return;

        // Move
        if (Math.Abs(Row + Column - toRow - toColumn) == 1)
        {
            turn++;
        }

        // Capture
        else
        {
            Revealed = true;
            table[(Row + toRow) / 2, (Column + toColumn) / 2].Piece = null;

            if (table[(Row + toRow) / 2, (Column + toColumn) / 2].Piece is MindController)
            {
                turn += 4;
                Player = 1 - Player;
            }
            else
            {
                turn += 2;
            }
        }


        table[toRow, toColumn].Piece = table[Row, Column].Piece;
        table[Row, Column].Piece = null;
        Row = toRow;
        Column = toColumn;
    }

    #region IClonable

    public override object Clone() => new Jumper(Row, Column, Player) { Revealed = Revealed };

    #endregion
}

