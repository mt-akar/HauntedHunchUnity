using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections.Generic;
using UnityEngine;

public class GameCoordinator : MonoBehaviour
{
    #region Singleton

    public static GameCoordinator Instance;

    #endregion

    // Reference to the game board objects
    public GameObject GameBoard;
    public GameObject PlacementBoard;

    // Grid dimensions
    public static int nr = 7; // Number of rows
    public static int nc = 6; // Number of columns

    #region Board Positional Constants

    public static float squareWidth = 1.6f;
    public static float gap;

    public static float leftOfGameBoard;
    public static float rightOfGameBoard;
    public static float topOfGameBoard;
    public static float bottomOfGameBoard;

    public static float leftOfPlacementBoard;
    public static float rightOfPlacementBoard;
    public static float topOfPlacementBoard;
    public static float bottomOfPlacementBoard;

    #endregion

    // Logic coordinator instance
    GameLogicCoordinator gameLogicCoordinator = new GameLogicCoordinator();

    // Prefabs and sprites
    public GameObject PiecePrefab;
    public GameObject possibleMovePrefab;
    public Sprite unrevealedPieceSprite;
    public Sprite[] pieceSprites;

    // Piece colors
    static Color opponentUnrevealedPieceColor = new Color(0.82f, 0.345f, 0.345f);
    static Color opponentRevealedPieceColor = new Color(1f, 0.42f, 0.42f);
    static Color selfUnrevealedPieceColor = new Color(0.255f, 0.725f, 0.698f);
    static Color selfRevealedPieceColor = new Color(0.31f, 0.886f, 0.851f);

    // Piece game objects
    GameObject[,] table = new GameObject[nr + 1, nc + 1];
    GameObject[,] placementTable = new GameObject[3, nc + 1];

    // Game stages
    public bool Ready = false;
    bool opponentReady = false;
    public bool PlacementStage { get; private set; } = true;

    // Possible move related data
    List<PossibleMove> possibleMoves;
    GameObject[] possibleMoveIndicators;

    void Awake()
    {
        #region Board Constants

        gap = squareWidth / 4;

        leftOfGameBoard = GameBoard.transform.position.x - squareWidth * nc / 2;
        rightOfGameBoard = GameBoard.transform.position.x + squareWidth * nc / 2;
        bottomOfGameBoard = GameBoard.transform.position.y - squareWidth * nr / 2;
        topOfGameBoard = GameBoard.transform.position.y + squareWidth * nr / 2;

        leftOfPlacementBoard = PlacementBoard.transform.position.x - squareWidth * nc / 2;
        rightOfPlacementBoard = PlacementBoard.transform.position.x + squareWidth * nc / 2;
        bottomOfPlacementBoard = PlacementBoard.transform.position.y - squareWidth;
        topOfPlacementBoard = PlacementBoard.transform.position.y + squareWidth;

        #endregion
    }

    // Start is called before the first frame update
    void Start()
    {
        Instance = this;

        for (int i = 2; i >= 1; i--)
        {
            for (int j = 1; j <= nc; j++)
            {
                var newPiece = Instantiate(PiecePrefab, PlacementBoard.transform.position + new Vector3(squareWidth * (j - 3.5f), squareWidth * (i - 1.5f), -1), Quaternion.identity);
                newPiece.GetComponent<SpriteRenderer>().sprite = pieceSprites[0];
                newPiece.GetComponent<SpriteRenderer>().material.color = selfUnrevealedPieceColor;
                placementTable[i, j] = newPiece;
            }
        }
    }

    #region Public Methods

    public bool IsPieceNull(Coordinate coord, bool isPlacementTable)
    {
        var tab = isPlacementTable ? placementTable : table;
        return tab[coord.Row, coord.Column] == null;
    }

    public bool IsPieceMine(Coordinate coord)
    {
        return gameLogicCoordinator.IsPieceMine(coord);
    }

    public void MoveSelfPiecePlacement(Coordinate from, bool fromPlacementBoard, Coordinate to, bool toPlacementBoard)
    {
        // If one of the coordinates are not appropirate for this action
        if (fromPlacementBoard && placementTable[from.Row, from.Column] == null || !fromPlacementBoard && table[from.Row, from.Column] == null)
            throw new Exception($"Piece you are trying to move is null: ({from.Column}, {from.Row})");
        if (toPlacementBoard && placementTable[to.Row, to.Column] != null || !toPlacementBoard && table[to.Row, to.Column] != null)
            throw new Exception($"Square you are trying to place piece on is already occupied: ({to.Column}, {to.Row})");

        // This implementation can be simplified by defining a new reference to either placement table or table
        GameObject temp;

        if (fromPlacementBoard)
        {
            temp = placementTable[from.Row, from.Column];
            placementTable[from.Row, from.Column] = null;
        }
        else
        {
            temp = table[from.Row, from.Column];
            table[from.Row, from.Column] = null;
        }

        if (toPlacementBoard)
            placementTable[to.Row, to.Column] = temp;
        else
            table[to.Row, to.Column] = temp;

        // For logical part
        gameLogicCoordinator.MoveSelfPiecePlacement(from, fromPlacementBoard, to, toPlacementBoard);
    }

    public void PlaceOpponentPieceToGameBoard(Coordinate to, byte pieceType)
    {
        if (table[to.Row, to.Column] != null)
        {
            throw new Exception($"Square you are trying to place piece on is already occupied: ({to.Column}, {to.Row})");
        }

        Debug.Log("game coordinator, place piece");
        var opponentPiece = Instantiate(PiecePrefab, Vector3OfCoordinateGame(to), Quaternion.identity);
        opponentPiece.GetComponent<SpriteRenderer>().sprite = unrevealedPieceSprite;
        opponentPiece.GetComponent<SpriteRenderer>().material.color = opponentUnrevealedPieceColor;
        table[to.Row, to.Column] = opponentPiece;

        // For logical part
        gameLogicCoordinator.PlaceOpponentPieceToGameBoard(to, pieceType);
    }

    public void MoveOpponentPieceInGameBoardPlacement(Coordinate from, Coordinate to)
    {
        // If one of the coordinates are not appropirate for this action
        if (table[from.Row, from.Column] == null)
            throw new Exception($"Piece you are trying to move is null: ({from.Column}, {from.Row})");
        if (table[to.Row, to.Column] != null)
            throw new Exception($"Square you are trying to place piece on is already occupied: ({to.Column}, {to.Row})");

        // Visually
        table[from.Row, from.Column].transform.position = Vector3OfCoordinateGame(to);

        // Logically
        table[to.Row, to.Column] = table[from.Row, from.Column];
        table[from.Row, from.Column] = null;

        // For logical part
        gameLogicCoordinator.MoveOpponentPiecePlacement(from, to);
    }

    public void RemoveOpponentPieceFromGameBoard(Coordinate coord)
    {
        if (table[coord.Row, coord.Column] == null)
            throw new Exception($"Piece you are trying to remove is null: ({coord.Column}, {coord.Row})");

        Destroy(table[coord.Row, coord.Column]);
        table[coord.Row, coord.Column] = null;

        // For logical part
        gameLogicCoordinator.RemoveOpponentPieceFromGameBoard(coord);
    }

    public void ReadyUp() // change, add a visual indicator for have you readied up or not and if you are not ready wheather you can ready up or not.
    {
        // If you are ready, return
        if (Ready)
        {
            Ready = false;
            return;
        }

        // Check if all the pieces are placed on the game board
        for (int i = 2; i >= 1; i--)
            for (int j = 1; j <= nc; j++)
                if (placementTable[i, j] != null)
                    return;

        // Ready up
        Ready = true;
        PhotonNetwork.RaiseEvent(PlayerNetworking.readyEventCode, null, new RaiseEventOptions { Receivers = ReceiverGroup.Others }, new SendOptions { Reliability = true });

        // If opponent is ready before you, end placement stage
        if (opponentReady)
        {
            // If you are not the master client, you go second. This achieved by increasing the turn by 2 since every player has 2 moves each turn.
            if (!PhotonNetwork.IsMasterClient)
                gameLogicCoordinator.IncrementTurnBy2();

            PlacementStage = false;
        }
    }

    public void OpponentReady()
    {
        // Opponent readied up
        opponentReady = true;

        // If you are ready before your opponenet, end placement stage
        if (Ready)
        {
            // If you are not the master client, you go second. This achieved by increasing the turn by 2 since every player has 2 moves each turn.
            if (!PhotonNetwork.IsMasterClient)
                gameLogicCoordinator.IncrementTurnBy2();

            PlacementStage = false;
        }
    }

    public GameLogicCoordinator GetLogicCoordinator() // change, delete
    {
        return gameLogicCoordinator;
    }

    public void ShowPossibleMoves(Coordinate coord)
    {
        if (PlacementStage)
            throw new Exception("You should not call this method (ShowPossibleMoves) in placement stage");

        if (table[coord.Row, coord.Column] == null)
            throw new Exception($"Piece you are trying to get possible moves of is null: ({coord.Column}, {coord.Row})");

        // If this is not your turn, return
        if (gameLogicCoordinator.IsMyTurn(PhotonNetwork.IsMasterClient))
        {
            Debug.Log("Not your turn!");
            return;
        }

        // Define possible moves array
        possibleMoves = gameLogicCoordinator.GetPossibleMoves(coord);

        // If there is no move to show, return
        if (possibleMoves == null || possibleMoves.Count == 0)
            return;

        // Instantiate possible move indicators array
        possibleMoveIndicators = new GameObject[possibleMoves.Count];

        // Instantiate the possible move indicators
        for (int i = 0; i < possibleMoveIndicators.Length; i++)
            possibleMoveIndicators[i] = Instantiate(possibleMovePrefab, Vector3OfCoordinateGame(coord), Quaternion.identity);
    }

    public bool Move(Coordinate from, Coordinate to)
    {
        // If from coordinate is null
        if (from == null)
            throw new Exception("From coordinate given cannot be null");

        // Get rid of all the indicators
        for (int i = 0; i < possibleMoveIndicators.Length; i++)
            Destroy(possibleMoveIndicators[i]);
        possibleMoveIndicators = null;

        // Check if the destination is one of the possible moves
        for (int i = 0; i < possibleMoves.Count; i++)
            if (possibleMoves[i].Row == to.Row && possibleMoves[i].Column == to.Column)
            {
                Debug.Log("Move is aloud. Sending move signal to logic coordinator.");
                GameBoardUpdate update = gameLogicCoordinator.Move(from, to, possibleMoves[i].MoveType);
                possibleMoves = null;
                PhotonNetwork.RaiseEvent(PlayerNetworking.movePieceGameEventCode, new MoveInfoGame(possibleMoves[i].MoveType, from, to), new RaiseEventOptions { Receivers = ReceiverGroup.Others }, new SendOptions { Reliability = true });

                // proccess update

                return true;
            }

        possibleMoves = null;
        return false;
    }

    public void OpponentMove(MoveInfoGame moveInfo)
    {
        // If the given coordinate is null
        if (moveInfo == null)
            throw new Exception("moveInfo should not be null");

        Debug.Log("Opponenet move is in game coordinator. Sending move signal to logic coordinator.");
        gameLogicCoordinator.Move(moveInfo.From, moveInfo.To, moveInfo.MoveType);
    }

    #endregion

    #region Analytic Helpers

    Vector3 Vector3OfCoordinateGame(Coordinate coor)
    {
        var vec = new Vector3(coor.Column * squareWidth, coor.Row * squareWidth, -1);
        Vector3 leftBottomOfGameBoard = new Vector3(leftOfGameBoard - squareWidth / 2, bottomOfGameBoard - squareWidth / 2, 0);

        return vec + leftBottomOfGameBoard;
    }

    #endregion

    #region ToString

    public override string ToString()
    {
        var ret = "";

        for (int i = nr; i >= 1; i--)
        {
            for (int j = 1; j <= nc; j++)
                ret += (table[i, j] != null ? "piece" : "null") + " ";
            ret += "\n";
        }

        ret += "\n";

        for (int i = 2; i >= 1; i--)
        {
            for (int j = 1; j <= nc; j++)
                ret += (placementTable[i, j] != null ? "piece" : "null") + " ";
            ret += "\n";
        }

        return ret;
    }

    #endregion
}