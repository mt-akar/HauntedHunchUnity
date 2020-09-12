using System;
using System.Collections.Generic;

/// <summary>
/// Moves to adjacent squares. Can push and pull other opponent pieces.
/// </summary>
public class Courier : Piece
{
    public Courier(int r, int c, PlayerType p) : base(r, c, p) { }

    public override List<PossibleMove> PossibleMoves(Square[,] table, int turn)
    {
        // Frozen check
        if (IsFrozen(table, Row, Column)) return null;

        // Initialize the linked list
        var possibleMoves = new List<PossibleMove>();

        for (int i = 0; i < 4; i++)
            // In bounds & (empty square | psuedo piece)
            if (Row + e[i, 0] <= nr && Row + e[i, 0] >= 1 && Column + e[i, 1] <= nc && Column + e[i, 1] >= 1 && (table[Row + e[i, 0], Column + e[i, 1]].Piece == null ||
                table[Row + e[i, 0], Column + e[i, 1]].Piece == table[Row + e[i, 0], Column + e[i, 1]].PseudoPiece))
            {
                possibleMoves.Add(new PossibleMove(Row + e[i, 0], Column + e[i, 1], MoveType.Shift));
            }

        // pull and push range { interacter_row, interacter_column, to_row, toColumn }
        int[,] b = {
                { 1, 0, 1, -1 }, { 1, 0, 2, 0 }, { 1, 0, 1, 1 }, { 0, 1, 1, 1 }, { 0, 1, 0, 2 }, { 0, 1, -1, 1 },
                { -1, 0, -1, 1 }, { -1, 0, -2, 0 }, { -1, 0, -1, -1 },  { 0, -1, -1, -1 }, { 0, -1, 0, -2 }, { 0, -1, 1, -1 },
                { 1, 0, 0, 1 }, { 1, 0, -1, 0 }, { 1, 0, 0, -1 }, { 0, 1, -1, 0 }, { 0, 1, 0, -1 }, { 0, 1, 1, 0 },
                { -1, 0, 0, -1 }, { -1, 0, 1, 0 }, { -1, 0, 0, 1 }, { 0, -1, 1, 0 }, { 0, -1, 0, 1 }, { 0, -1, -1, 0 } };

        for (int i = 0; i < 24; i++)
            // In bounds & interacter is opponenet piece, not psuedo & to is null or psuedo
            if (Row + b[i, 0] <= nr && Row + b[i, 0] >= 1 && Column + b[i, 1] <= nc && Column + b[i, 1] >= 1 &&
                Row + b[i, 2] <= nr && Row + b[i, 2] >= 1 && Column + b[i, 3] <= nc && Column + b[i, 3] >= 1 &&
                table[Row + b[i, 0], Column + b[i, 1]].Piece != null && table[Row + b[i, 0], Column + b[i, 1]].Piece.Player != Player &&
                table[Row + b[i, 0], Column + b[i, 1]].Piece != table[Row + b[i, 0], Column + b[i, 1]].PseudoPiece &&
                (table[Row + b[i, 2], Column + b[i, 3]].Piece == null || table[Row + b[i, 2], Column + b[i, 3]].Piece == table[Row + b[i, 2], Column + b[i, 3]].PseudoPiece))
            {
                possibleMoves.Add(new PossibleMove(Row + b[i, 0], Column + b[i, 1], MoveType.AbilityWithInteracter));
            }

        return possibleMoves;
    }

    public override void Move(Square[,] table, int toRow, int toColumn, ref int turn)
    {
        ClearSquareStates(table, Row, Column, e);

        if (IsHiddenlyFrozen(table, Row, Column)) return;

        turn++;

        table[toRow, toColumn].Piece = table[Row, Column].Piece;
        table[Row, Column].Piece = null;
        Row = toRow;
        Column = toColumn;
    }

    // Intermediate step. Paint second step squares. Returns interacter's square.
    public override Square AbilityWithInteracterStageOne(Square[,] table, Square sen)
    {
        table[Row, Column].State = SquareState.None;
        sen.State = SquareState.None;

        if (IsHiddenlyFrozen(table, Row, Column))
        {
            ClearSquareStates(table, Row, Column, e);
            Revealed = true;
            return null;
        }

        for (int i = 0; i < 4; i++)
        {
            // For pull, in bounds & (empty square | psuedo piece)
            if (Row + e[i, 0] <= nr && Row + e[i, 0] >= 1 && Column + e[i, 1] <= nc && Column + e[i, 1] >= 1 &&
                (table[Row + e[i, 0], Column + e[i, 1]].Piece == null || table[Row + e[i, 0], Column + e[i, 1]].Piece == table[Row + e[i, 0], Column + e[i, 1]].PseudoPiece))
            {
                table[Row + e[i, 0], Column + e[i, 1]].State = SquareState.AbilityWithInteracterable;
            }
            // For push, in bounds & (empty square | psuedo piece)
            if (sen.Row + e[i, 0] <= nr && sen.Row + e[i, 0] >= 1 && sen.Column + e[i, 1] <= nc && sen.Column + e[i, 1] >= 1 &&
                (table[sen.Row + e[i, 0], sen.Column + e[i, 1]].Piece == null || table[sen.Row + e[i, 0], sen.Column + e[i, 1]].Piece == table[sen.Row + e[i, 0], sen.Column + e[i, 1]].PseudoPiece))
            {
                table[sen.Row + e[i, 0], sen.Column + e[i, 1]].State = SquareState.AbilityWithInteracterable;
            }
        }
        return sen;
    }

    // Do the actual pulling or pushing.
    public override void AbilityWithInteracterStageTwo(Square[,] table, Square interacter, Square sen, ref int turn)
    {
        ClearSquareStates(table, Row, Column, e);
        ClearSquareStates(table, interacter.Row, interacter.Column, e);

        turn++;

        if (interacter.Piece is MindController)
        {
            turn += (turn % 2 == 1) ? 2 : 3;

            Player = 1 - Player;

            if (Math.Abs(sen.Row - Row) + Math.Abs(sen.Column - Column) == 2) // Push
            {
                interacter.Piece = table[Row, Column].Piece;
                table[Row, Column].Piece = null;

                interacter.Piece.Row = interacter.Row;
                interacter.Piece.Column = interacter.Column;
            }
            else // Pull
            {
                sen.Piece = table[Row, Column].Piece;
                table[Row, Column].Piece = null;
                interacter.Piece = null;

                sen.Piece.Row = sen.Row;
                sen.Piece.Column = sen.Column;
            }
        }
        else
        {
            if (Math.Abs(sen.Row - Row) + Math.Abs(sen.Column - Column) == 2) // Push
            {
                sen.Piece = interacter.Piece;
                interacter.Piece = table[Row, Column].Piece;
                table[Row, Column].Piece = null;

                sen.Piece.Row = sen.Row;
                sen.Piece.Column = sen.Column;
                interacter.Piece.Row = interacter.Row;
                interacter.Piece.Column = interacter.Column;
            }
            else // Pull
            {
                sen.Piece = table[Row, Column].Piece;
                table[Row, Column].Piece = interacter.Piece;
                interacter.Piece = null;

                /* If we swap the order of following line couples, since "Row" is the row of courier which is now sen.Piece, table[row, column] is not interacter's new place.
                 * Instead it is cur's new place so interacter's coordinates will not be updated. */
                table[Row, Column].Piece.Row = Row;
                table[Row, Column].Piece.Column = Column;
                sen.Piece.Row = sen.Row;
                sen.Piece.Column = sen.Column;
            }
        }
    }

    #region IClonable

    public override object Clone() => new Courier(Row, Column, Player) { Revealed = Revealed };

    #endregion
}
