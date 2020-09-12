using System;
using System.Collections.Generic;

public class GameLogicCoordinator
{
    #region Variables and Properties

    const int nr = 7; // Number of rows
    const int nc = 6; // Number of columns

    Square[,] table = new Square[nr + 1, nc + 1]; // Table is 7x5. Zero indexes are ignored for a better understanding of the coordinates, will always stay null.
    Square[,] placementTable = new Square[3, nc + 1];

    Square[,,] history = new Square[1000, nr + 1, nc + 1]; // Game history, for undo
    public Square selectedSquare; // Current moving piece
    Square interacterSquare; // Interacting piece in moves where there is more than one piece involved
    public int turn = 0; // 4k+3 & 4k are white's turns, 4k+1 & 4k+2 are black's turns.
    public bool gameEnded;
    public bool turnConstraintsEnabled = true;

    #endregion

    #region Constructor

    public GameLogicCoordinator()
    {
        for (int i = 1; i <= nr; i++)
            for (int j = 1; j <= nc; j++)
                table[i, j] = new Square(i, j);

        for (int i = 1; i <= 2; i++)
            for (int j = 1; j <= nc; j++)
                placementTable[i, j] = new Square(i, j, new Guard(i, j, PlayerType.Self));

        //UpdateHistory();
    }

    #endregion

    #region Actions

    /// <summary>
    /// Activated when clicked on a square.
    /// Does different things depending on the input and the state of the variables table, turn, cur and interactor.
    /// </summary>
    /// <param name="sen"> Square that is just clicked </param>
    public void Action(Square sen)
    {
        if (gameEnded) return;

        // If we are at an in-between move of an ability with interactor
        if (interacterSquare != null)
        {
            if (sen.State == SquareState.AbilityWithInteracterable)
            {
                selectedSquare.Piece.AbilityWithInteracterStageTwo(table, interacterSquare, sen, ref turn);
                UpdateHistory();
                UpdatePits();
            }
            interacterSquare = null;
            selectedSquare = null;
            return;
        }

        tunnel1: // Used when selecting a friendly piece to move while already had been selected a friendly piece to move

        if (selectedSquare == null) // If no piece is chosen yet
        {
            // If a valid piece is chosen, paint the possible moves for preview.
            if (sen.Piece != null && (!turnConstraintsEnabled ||
                sen.Piece.Player == PlayerType.Self && (turn % 4 == 0 || turn % 4 == 3) || sen.Piece.Player == PlayerType.Opponent && (turn % 4 == 1 || turn % 4 == 2)))
            {
                sen.Piece.PossibleMoves(table, turn);
                selectedSquare = sen;
            }
            // If a non-valid square is chosen, do nothing
            else
            {
                // Explicitly kick other (cur == null) cases, don't merge 2 if statements.
            }
        }
        else // (selected != null)
        {
            // Ability Uno
            if (sen.State == SquareState.AbilityUnoable)
            {
                selectedSquare.Piece.AbilityUno(table, ref turn);
                UpdateHistory();

                UpdatePits();
                selectedSquare = null;
            }

            // Ability With Interacter
            else if (sen.State == SquareState.AbilityWithInteracterable)
            {
                interacterSquare = selectedSquare.Piece.AbilityWithInteracterStageOne(table, sen);

                // If the piece is hiddenly frozen, ability with interacter stage one returns null. Then cur should also be null.
                if (interacterSquare == null)
                    selectedSquare = null;

                // Don't check for gameEnded
                return;
            }

            else if (selectedSquare == sen)
            {
                // Do nothing
            }

            // Move
            else if (sen.State == SquareState.Moveable)
            {
                selectedSquare.Piece.Move(table, sen.Row, sen.Column, ref turn);
                UpdateHistory();

                UpdatePits();
                selectedSquare = null;
            }

            // If another friendly piece is chosen
            else if (selectedSquare != sen && sen.Piece != null && selectedSquare.Piece.Player == sen.Piece.Player)
            {
                selectedSquare = null;
                Repaint();
                goto tunnel1; // Behave as if the cur was null and go back to the top of the LMDown method
            }

            // Unvalid square chosen
            else
            {
                selectedSquare = null;
                Repaint();
            }
        }

        // Update gameEnded
        UpdateGameEnded();
    }

    public void Deselect()
    {
        interacterSquare = null;
        selectedSquare = null;
        Repaint();
    }

    public void Select(Square sen)
    {
        if (selectedSquare != null || interacterSquare != null)
            throw new Exception("selected piece and interacted piece has to be null");

        if (gameEnded)
            return;

        // If a valid piece is chosen, paint the possible moves for preview.
        if (sen.Piece != null && (!turnConstraintsEnabled ||
            sen.Piece.Player == PlayerType.Self && (turn % 4 == 0 || turn % 4 == 3) || sen.Piece.Player == PlayerType.Opponent && (turn % 4 == 1 || turn % 4 == 2)))
        {
            sen.Piece.PossibleMoves(table, turn);
            selectedSquare = sen;
        }
        // If a non-valid square is chosen, do nothing
        else
        {
            // Explicitly kick other (cur == null) cases, don't merge 2 if statements.
        }
    }

    public void Activate(Square sen)
    {
        // If we are at an in-between move of an ability with interactor
        if (interacterSquare != null)
        {
            if (sen.State == SquareState.AbilityWithInteracterable)
            {
                selectedSquare.Piece.AbilityWithInteracterStageTwo(table, interacterSquare, sen, ref turn);
                UpdateHistory();
                UpdatePits();
            }
            interacterSquare = null;
            selectedSquare = null;
            return;
        }
        // Ability Uno
        if (sen.State == SquareState.AbilityUnoable)
        {
            selectedSquare.Piece.AbilityUno(table, ref turn);
            UpdateHistory();

            UpdatePits();
            selectedSquare = null;
        }

        // Ability With Interacter
        else if (sen.State == SquareState.AbilityWithInteracterable)
        {
            interacterSquare = selectedSquare.Piece.AbilityWithInteracterStageOne(table, sen);

            // If the piece is hiddenly frozen, ability with interacter stage one returns null. Then cur should also be null.
            if (interacterSquare == null)
                selectedSquare = null;

            // Don't check for gameEnded
            return;
        }

        // None Move
        else if (sen.State == SquareState.Moveable && selectedSquare != sen)
        {
            selectedSquare.Piece.Move(table, sen.Row, sen.Column, ref turn);
            UpdateHistory();

            UpdatePits();
            selectedSquare = null;
        }

        // If another friendly piece is chosen
        else if (selectedSquare != sen && sen.Piece != null && selectedSquare.Piece.Player == sen.Piece.Player)
        {
            selectedSquare = null;
            Repaint();
            Activate(sen); // Behave as if the cur was null and go back to the top of the LMDown method
        }

        // Unvalid square chosen
        else
        {
            selectedSquare = null;
            Repaint();
        }

        // Update gameEnded
        UpdateGameEnded();
    }

    #endregion

    #region Tool methods

    /// <summary>
    /// To use UpdatePits in this class without a parameter
    /// </summary>
    void UpdatePits()
    {
        UpdatePits(table);
    }

    /// <summary>
    /// Remove the pieces that are on the pits and that have no adjacent friendly piece from the game.
    /// </summary>
    public static void UpdatePits(Square[,] tabl)
    {
        int[,] pits = { { 3, 2 }, { 3, 5 }, { 5, 2 }, { 5, 5 } };
        for (int i = 0; i < 4; i++)
        {
            if (tabl[pits[i, 0], pits[i, 1]].Piece != null &&
               (tabl[pits[i, 0] + 1, pits[i, 1]].Piece == null || tabl[pits[i, 0] + 1, pits[i, 1]].Piece.Player != tabl[pits[i, 0], pits[i, 1]].Piece.Player) &&
               (tabl[pits[i, 0], pits[i, 1] + 1].Piece == null || tabl[pits[i, 0], pits[i, 1] + 1].Piece.Player != tabl[pits[i, 0], pits[i, 1]].Piece.Player) &&
               (tabl[pits[i, 0] - 1, pits[i, 1]].Piece == null || tabl[pits[i, 0] - 1, pits[i, 1]].Piece.Player != tabl[pits[i, 0], pits[i, 1]].Piece.Player) &&
               (tabl[pits[i, 0], pits[i, 1] - 1].Piece == null || tabl[pits[i, 0], pits[i, 1] - 1].Piece.Player != tabl[pits[i, 0], pits[i, 1]].Piece.Player) &&
               (tabl[pits[i, 0] + 1, pits[i, 1]].PseudoPiece == null || tabl[pits[i, 0] + 1, pits[i, 1]].PseudoPiece.Player != tabl[pits[i, 0], pits[i, 1]].Piece.Player) &&
               (tabl[pits[i, 0], pits[i, 1] + 1].PseudoPiece == null || tabl[pits[i, 0], pits[i, 1] + 1].PseudoPiece.Player != tabl[pits[i, 0], pits[i, 1]].Piece.Player) &&
               (tabl[pits[i, 0] - 1, pits[i, 1]].PseudoPiece == null || tabl[pits[i, 0] - 1, pits[i, 1]].PseudoPiece.Player != tabl[pits[i, 0], pits[i, 1]].Piece.Player) &&
               (tabl[pits[i, 0], pits[i, 1] - 1].PseudoPiece == null || tabl[pits[i, 0], pits[i, 1] - 1].PseudoPiece.Player != tabl[pits[i, 0], pits[i, 1]].Piece.Player))
            {
                tabl[pits[i, 0], pits[i, 1]].Piece = null;
            }
            // If the piece is Pseudo. (For PseudoPiece concept, refer to Square.cs or the game manual)
            if (tabl[pits[i, 0], pits[i, 1]].PseudoPiece != null &&
               (tabl[pits[i, 0] + 1, pits[i, 1]].Piece == null || tabl[pits[i, 0] + 1, pits[i, 1]].Piece.Player != tabl[pits[i, 0], pits[i, 1]].PseudoPiece.Player) &&
               (tabl[pits[i, 0], pits[i, 1] + 1].Piece == null || tabl[pits[i, 0], pits[i, 1] + 1].Piece.Player != tabl[pits[i, 0], pits[i, 1]].PseudoPiece.Player) &&
               (tabl[pits[i, 0] - 1, pits[i, 1]].Piece == null || tabl[pits[i, 0] - 1, pits[i, 1]].Piece.Player != tabl[pits[i, 0], pits[i, 1]].PseudoPiece.Player) &&
               (tabl[pits[i, 0], pits[i, 1] - 1].Piece == null || tabl[pits[i, 0], pits[i, 1] - 1].Piece.Player != tabl[pits[i, 0], pits[i, 1]].PseudoPiece.Player) &&
               (tabl[pits[i, 0] + 1, pits[i, 1]].PseudoPiece == null || tabl[pits[i, 0] + 1, pits[i, 1]].PseudoPiece.Player != tabl[pits[i, 0], pits[i, 1]].Piece.Player) &&
               (tabl[pits[i, 0], pits[i, 1] + 1].PseudoPiece == null || tabl[pits[i, 0], pits[i, 1] + 1].PseudoPiece.Player != tabl[pits[i, 0], pits[i, 1]].Piece.Player) &&
               (tabl[pits[i, 0] - 1, pits[i, 1]].PseudoPiece == null || tabl[pits[i, 0] - 1, pits[i, 1]].PseudoPiece.Player != tabl[pits[i, 0], pits[i, 1]].Piece.Player) &&
               (tabl[pits[i, 0], pits[i, 1] - 1].PseudoPiece == null || tabl[pits[i, 0], pits[i, 1] - 1].PseudoPiece.Player != tabl[pits[i, 0], pits[i, 1]].Piece.Player))
            {
                // There isn't another piece on the pseudo piece
                if (tabl[pits[i, 0], pits[i, 1]].Piece == tabl[pits[i, 0], pits[i, 1]].PseudoPiece)
                {
                    tabl[pits[i, 0], pits[i, 1]].PseudoPiece = null;
                    tabl[pits[i, 0], pits[i, 1]].Piece = null;
                }
                // There is another piece on the pseudo piece
                else
                {
                    tabl[pits[i, 0], pits[i, 1]].PseudoPiece = null;
                }
            }
        }
    }

    /// <summary>
    /// Record the game to have an undo button
    /// </summary>
    void UpdateHistory()
    {
        for (int i = 1; i <= nr; i++)
            for (int j = 1; j <= nc; j++)
                history[turn, i, j] = (Square)table[i, j].Clone();
    }

    /// <summary>
    /// Repaint the board to default colors
    /// </summary>
    void Repaint()
    {
        for (int i = 1; i <= nr; i++)
            for (int j = 1; j <= nc; j++)
                table[i, j].State = SquareState.None;
    }

    void UpdateGameEnded()
    {
        bool whiteLotusIsOnBoard = false;
        bool blackLotusIsOnBoard = false;
        for (int i = 1; i <= nr; i++)
            for (int j = 1; j <= nc; j++)
            {
                if (table[i, j].Piece == null || !(table[i, j].Piece is Lotus)) continue;

                // If either lotus is on the board
                if (table[i, j].Piece.Player == PlayerType.Self)
                    whiteLotusIsOnBoard = true;
                else
                    blackLotusIsOnBoard = true;

                // If a lotus reaches the last row for either player, game ends.
                if (table[i, j].Piece.Player == PlayerType.Self && i == nr || table[i, j].Piece.Player == PlayerType.Opponent && i == 1)
                    gameEnded = true;
            }
        // If any lotus is removed, game ends.
        if (!whiteLotusIsOnBoard || !blackLotusIsOnBoard)
            gameEnded = true;
    }

    #endregion

    #region Public Methods

    public bool IsPieceMine(Coordinate coord)
    {
        return table[coord.Row, coord.Column].Piece.Player == PlayerType.Self;
    }

    public void MoveSelfPiecePlacement(Coordinate from, bool fromPlacementBoard, Coordinate to, bool toPlacementBoard)
    {
        // If one of the coordinates are not appropirate for this action
        if (fromPlacementBoard && placementTable[from.Row, from.Column].Piece == null || !fromPlacementBoard && table[from.Row, from.Column].Piece == null)
            throw new Exception($"Piece you are trying to move is null: ({from.Column}, {from.Row})");
        if (toPlacementBoard && placementTable[to.Row, to.Column].Piece != null || !toPlacementBoard && table[to.Row, to.Column].Piece != null)
            throw new Exception($"Square you are trying to place piece on is already occupied: ({to.Column}, {to.Row})");

        // This implementation can be simplified by defining a new reference to either placement table or table
        Piece temp;

        if (fromPlacementBoard)
        {
            temp = placementTable[from.Row, from.Column].Piece;
            placementTable[from.Row, from.Column].Piece = null;
        }
        else
        {
            temp = table[from.Row, from.Column].Piece;
            table[from.Row, from.Column].Piece = null;
        }

        if (toPlacementBoard)
            placementTable[to.Row, to.Column].Piece = temp;
        else
            table[to.Row, to.Column].Piece = temp;
    }

    public void PlaceOpponentPieceToGameBoard(Coordinate to, byte pieceType)
    {
        if (table[to.Row, to.Column].Piece != null)
        {
            throw new Exception($"Square you are trying to place piece on is already occupied: ({to.Column}, {to.Row})");
        }

        table[to.Row, to.Column].Piece = new Guard(to.Row, to.Column, PlayerType.Opponent); // change. enumerate the piece type
    }

    public void MoveOpponentPiecePlacement(Coordinate from, Coordinate to)
    {
        // If one of the coordinates are not appropirate for this action
        if (table[from.Row, from.Column].Piece == null)
            throw new Exception($"Piece you are trying to move is null: ({from.Column}, {from.Row})");
        if (table[to.Row, to.Column].Piece != null)
            throw new Exception($"Square you are trying to place piece on is already occupied: ({to.Column}, {to.Row})");

        // This implementation can be simplified by defining a new reference to either placement table or table
        Piece temp = table[from.Row, from.Column].Piece;
        table[from.Row, from.Column].Piece = null;
        table[to.Row, to.Column].Piece = temp;
    }

    public void RemoveOpponentPieceFromGameBoard(Coordinate coord)
    {
        // If the given coordinate is null
        if (coord == null)
            throw new Exception("Coordinate given should not be null");

        // If the piece in the given coordinate is null
        if (table[coord.Row, coord.Column].Piece == null)
            throw new Exception($"Piece you are trying to remove is null: ({coord.Column}, {coord.Row})");

        table[coord.Row, coord.Column].Piece = null;
    }

    public void IncrementTurnBy2()
    {
        turn += 2;
    }

    public bool IsMyTurn(bool isWhite)
    {
        return isWhite ^ (turn % 4 == 1 || turn % 4 == 2);
    }

    public List<PossibleMove> GetPossibleMoves(Coordinate coord)
    {
        // If the given coordinate is null
        if (coord == null)
            throw new Exception("Coordinate given should not be null");

        // selected or interacter isn't null
        if (selectedSquare != null || interacterSquare != null)
            throw new Exception("Selected piece and interacted piece has to be null");

        // coord is null
        if (table[coord.Row, coord.Column].Piece == null)
            throw new Exception("The piece you are asking the possible moves of is null");

        // coord is null
        if (table[coord.Row, coord.Column].Piece.Player == PlayerType.Opponent)
            throw new Exception("The piece you are asking the possible moves of is not yours");

        // If game has ended
        if (gameEnded) return null;

        // If turn is not yours
        if (turn % 4 != 0 && turn % 4 != 3) // This might be an exception instead
            return null;

        // If a valid piece is chosen, paint the possible moves for preview.
        selectedSquare = table[coord.Row, coord.Column];
        return table[coord.Row, coord.Column].Piece.PossibleMoves(table, turn);
    }

    public GameBoardUpdate Move(Coordinate from, Coordinate to, MoveType moveType)
    {
        GameBoardUpdate update = new GameBoardUpdate(); // change, no initializetion needed

        // Ability Uno
        if (moveType == MoveType.AbilityUno)
        {
            table[from.Row, from.Column].Piece.AbilityUno(table, ref turn); // update =
            UpdateHistory();

            UpdatePits();
        }

        // Ability With Interacter
        /*
        else if (moveType == MoveType.AbilityWithInteracter)
        {
            interacterSquare = selectedSquare.Piece.AbilityWithInteracterStageOne(table, sen);

            // If the piece is hiddenly frozen, ability with interacter stage one returns null. Then cur should also be null.
            if (interacterSquare == null)
                selectedSquare = null;

            // Don't check for gameEnded
            return;
        }*/

        // Move
        else if (moveType == MoveType.Shift || moveType == MoveType.Capture || moveType == MoveType.Jump)
        {
            update = table[from.Row, from.Column].Piece.Move(table, to.Row, to.Column, ref turn);
            UpdateHistory();

            UpdatePits();
        }
        
        else
        {
            throw new Exception("I dont think this can happen but you mey never know."); // change
        }

        return update;
    }

    #endregion

    #region ToString

    public override string ToString()
    {
        var ret = "";

        for (int i = nr; i >= 1; i--)
        {
            for (int j = 1; j <= nc; j++)
                ret += (table[i, j].Piece != null ? table[i, j].Piece.ToString() : "null") + " ";
            ret += "\n";
        }

        ret += "\n";

        for (int i = 2; i >= 1; i--)
        {
            for (int j = 1; j <= nc; j++)
                ret += (placementTable[i, j].Piece != null ? placementTable[i, j].Piece.ToString() : "null") + " ";
            ret += "\n";
        }

        return ret;
    }

    #endregion
}
