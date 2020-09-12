
using System.Collections.Generic;
/// <summary>
/// Basic piece. Moves to adjacent squares, captures adjacent opponent pieces.
/// </summary>
public class Guard : Piece
{
    public Guard(int r, int c, PlayerType p) : base(r, c, p) { }

    public override List<PossibleMove> PossibleMoves(Square[,] table, int turn)
    {
        // Frozen check
        if (IsFrozen(table, Row, Column)) return null;

        // Initialize the linked list
        var possibleMoves = new List<PossibleMove>();

        for (int i = 0; i < 4; i++)
            // In bounds & (empty square | pseudo piece | opponent piece)
            if (Row + e[i, 0] <= nr && Row + e[i, 0] >= 1 && Column + e[i, 1] <= nc && Column + e[i, 1] >= 1)
            {
                // opponenet piece
                if (table[Row + e[i, 0], Column + e[i, 1]].Piece.Player != Player && turn % 2 == 1)
                {
                    possibleMoves.Add(new PossibleMove(Row + e[i, 0], Column + e[i, 1], MoveType.Capture));

                }
                // empty square | psuedo piece
                else if (table[Row + e[i, 0], Column + e[i, 1]].Piece == null ||
                table[Row + e[i, 0], Column + e[i, 1]].Piece == table[Row + e[i, 0], Column + e[i, 1]].PseudoPiece)
                {
                    possibleMoves.Add(new PossibleMove(Row + e[i, 0], Column + e[i, 1], MoveType.Shift));
                }
            }

        return possibleMoves;
    }

    public override void Move(Square[,] table, int toRow, int toColumn, ref int turn)
    {
        ClearSquareStates(table, Row, Column, e);

        if (IsHiddenlyFrozen(table, Row, Column)) return;

        // Move
        if (table[toRow, toColumn].Piece == null || table[toRow, toColumn].Piece == table[toRow, toColumn].PseudoPiece)
        {
            turn++;
        }

        // Capture
        else
        {
            Revealed = true;

            if (table[toRow, toColumn].Piece is MindController)
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

    public override object Clone() => new Guard(Row, Column, Player) { Revealed = Revealed };

    #endregion
}
