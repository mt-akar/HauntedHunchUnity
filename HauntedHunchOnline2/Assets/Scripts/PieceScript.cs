using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System;
using UnityEngine;

public class PieceScript : MonoBehaviour
{
    class PiecePosition
    {
        public Vector3 Position { get; set; }
        public Coordinate Coordinate { get; set; }
        public bool PlacementBoard { get; set; }

        public PiecePosition()
        {

        }

        public PiecePosition(Vector3 position, Coordinate coordinate, bool placementBoard)
        {
            Position = position;
            Coordinate = coordinate;
            PlacementBoard = placementBoard;
        }
    }

    public GameObject GameBoard;
    public GameObject PlacementBoard;

    #region Board Constants

    readonly int nr;
    readonly int nc;

    float squareWidth;
    float gap;

    float leftOfGameBoard;
    float rightOfGameBoard;
    float topOfGameBoard;
    float bottomOfGameBoard;

    float leftOfPlacementBoard;
    float rightOfPlacementBoard;
    float topOfPlacementBoard;
    float bottomOfPlacementBoard;

    #endregion

    // Variables that store information about piece and mouse at the beginning of the drag
    PiecePosition pieceOrigin;
    Vector3 prevMousePos;

    // Coordinate of the piece
    //Coordinate coordinate = new Coordinate(0, 0);

    void Awake()
    {
        #region Board Constants

        squareWidth = GameCoordinator.squareWidth;
        gap = GameCoordinator.gap;

        leftOfGameBoard = GameCoordinator.leftOfGameBoard;
        rightOfGameBoard = GameCoordinator.rightOfGameBoard;
        bottomOfGameBoard = GameCoordinator.bottomOfGameBoard;
        topOfGameBoard = GameCoordinator.topOfGameBoard;

        leftOfPlacementBoard = GameCoordinator.leftOfPlacementBoard;
        rightOfPlacementBoard = GameCoordinator.rightOfPlacementBoard;
        bottomOfPlacementBoard = GameCoordinator.bottomOfPlacementBoard;
        topOfPlacementBoard = GameCoordinator.topOfPlacementBoard;

        #endregion
    }

    #region Mouse Event Wrappers

    void OnMouseDown()
    {
        if (GameCoordinator.Instance.PlacementStage)
        {
            if (!GameCoordinator.Instance.Ready)
                OnMouseDownPlacement();
        }
        else
            OnMouseDownGame();
    }

    void OnMouseDrag()
    {
        if (pieceOrigin == null)
            return;

        // Get mouse position
        var mousePos = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0));
        mousePos.z = -1;

        // Find the offset since the last time this function is called and move the piece
        var offsetMousePos = mousePos - prevMousePos;
        transform.position += offsetMousePos;

        // Update
        prevMousePos = mousePos;
    }

    void OnMouseUp()
    {
        if (GameCoordinator.Instance.PlacementStage)
        {
            if (!GameCoordinator.Instance.Ready)
                OnMouseUpPlacement();
        }
        else
            OnMouseUpGame();
    }

    #endregion

    #region Placement Mouse Events

    void OnMouseDownPlacement()
    {
        // Update piece origin
        pieceOrigin = new PiecePosition();
        pieceOrigin.Position = transform.position;
        if (IsInsidePlacementBoard(transform.position))
        {
            pieceOrigin.Coordinate = CoordinateOfVector3Placement(transform.position);
            pieceOrigin.PlacementBoard = true;
        }
        else if (IsInsideGameBoard(transform.position))
        {
            pieceOrigin.Coordinate = CoordinateOfVector3Game(transform.position);

            // If the piece that we are trying to move is not our piece, return
            if (!GameCoordinator.Instance.IsPieceMine(pieceOrigin.Coordinate))
            {
                pieceOrigin = null;
                return;
            }

            pieceOrigin.PlacementBoard = false;

        }
        else
            Debug.Log("There is an error: Piece cannot be outside of both boards.");

        // Update prevMouse
        var mousePosition = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0));
        mousePosition.z = -1;
        transform.position = mousePosition;
        prevMousePos = mousePosition;
    }

    void OnMouseUpPlacement()
    {
        // If not dragging, return
        if (pieceOrigin == null)
            return;

        // Get mouse position on the screen
        var targetPosition = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0));
        targetPosition.z = -1;

        // Inside placement board
        if (IsInsidePlacementBoard(targetPosition))
        {
            // Target coordinate
            Coordinate newCoordinate = CoordinateOfVector3Placement(targetPosition);

            // If target square is not null and it is not the same square, move the piece
            if (GameCoordinator.Instance.IsPieceNull(newCoordinate, true) && (!newCoordinate.Equals(pieceOrigin.Coordinate) || !pieceOrigin.PlacementBoard))
            {
                // Raise event to notify other player and update their board. If move is from placement board to placement board, don't notify.
                if (!pieceOrigin.PlacementBoard)
                {
                    Debug.Log("Event is being raised. To placement board.");
                    // Zero event code is for placing a piece.
                    PhotonNetwork.RaiseEvent(PlayerNetworking.movePiecePlacementEventCode, new MoveInfoPlacement(false, true, pieceOrigin.Coordinate, newCoordinate, 1), new RaiseEventOptions { Receivers = ReceiverGroup.Others }, new SendOptions { Reliability = true });
                }

                // Snap
                SnapPlacementBoard(targetPosition);

                // Visual and logical
                GameCoordinator.Instance.MoveSelfPiecePlacement(pieceOrigin.Coordinate, pieceOrigin.PlacementBoard, newCoordinate, true);

                // Null the origin
                pieceOrigin = null;
            }
            else
            {
                ResetMovingPiece();
                return;
            }
        }
        // Inside game board
        else if (IsInsideGameBoard(targetPosition))
        {
            // Target coordinate
            Coordinate newCoordinate = CoordinateOfVector3Game(targetPosition);

            // If target square is not null and it is not the same square, move the piece
            if (GameCoordinator.Instance.IsPieceNull(newCoordinate, false) && (!newCoordinate.Equals(pieceOrigin.Coordinate) || pieceOrigin.PlacementBoard) && newCoordinate.Row <= 2)
            {
                // Raise event to notify other player and update their board.
                // Zero event code is for placing a piece.
                Debug.Log("Event is being raised. To game board.");
                PhotonNetwork.RaiseEvent(PlayerNetworking.movePiecePlacementEventCode, new MoveInfoPlacement(pieceOrigin.PlacementBoard, false, pieceOrigin.Coordinate, newCoordinate, 1), new RaiseEventOptions { Receivers = ReceiverGroup.Others }, new SendOptions { Reliability = true });

                // Snap
                SnapGameBoard(targetPosition);

                // Visual and logical
                GameCoordinator.Instance.MoveSelfPiecePlacement(pieceOrigin.Coordinate, pieceOrigin.PlacementBoard, newCoordinate, false);

                // Null the origin
                pieceOrigin = null;
            }
            else
            {
                ResetMovingPiece();
                return;
            }
        }
        // Outside both boards
        else
        {
            ResetMovingPiece();
            return;
        }

        Debug.Log("logic");
        Debug.Log(GameCoordinator.Instance.GetLogicCoordinator());
        Debug.Log("visual");
        Debug.Log(GameCoordinator.Instance);
    }

    #endregion

    #region Game Mouse Events

    void OnMouseDownGame()
    {
        // If for some reason pieceOrigin is not null
        if (pieceOrigin != null)
            throw new Exception("pieceOrigin has to be null");

        // Update piece origin
        pieceOrigin = new PiecePosition(transform.position, CoordinateOfVector3Game(transform.position), false);

        // If the piece that we are trying to move is not our piece, return
        if (!GameCoordinator.Instance.IsPieceMine(pieceOrigin.Coordinate))
        {
            pieceOrigin = null;
            return;
        }

        // Show possible moves on the board
        GameCoordinator.Instance.ShowPossibleMoves(pieceOrigin.Coordinate);

        // Update prevMouse
        var mousePosition = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0));
        mousePosition.z = -1;
        transform.position = mousePosition;
        prevMousePos = mousePosition;
    }

    void OnMouseUpGame()
    {
        // If pieceOrigin is null
        if (pieceOrigin == null)
            return;

        var destinationCoordinate = CoordinateOfVector3Game(transform.position);

        if (!GameCoordinator.Instance.Move(pieceOrigin.Coordinate, destinationCoordinate /* This might be turned into where the mouse is instead of where the piece is */))
        {
            ResetMovingPiece();
        }
    }

    #endregion


    #region Helpers

    void ResetMovingPiece()
    {
        transform.position = pieceOrigin.Position;
        pieceOrigin = null;
    }

    #endregion

    #region Analytic Helpers

    void SnapPlacementBoard(Vector3 mousePos)
    {
        transform.position = NearestSquareCenterPlacement(mousePos);
    }

    void SnapGameBoard(Vector3 mousePos)
    {
        transform.position = NearestSquareCenterGame(mousePos);
    }

    bool IsInsidePlacementBoard(Vector3 vec)
    {
        return vec.x >= leftOfPlacementBoard && vec.x <= rightOfPlacementBoard &&
            vec.y >= bottomOfPlacementBoard && vec.y <= topOfPlacementBoard;
        //return Physics2D.OverlapCircle(transform.position, 0.01f, GameBoardLayer) == null;
    }

    bool IsInsideGameBoard(Vector3 vec)
    {
        return vec.x >= leftOfGameBoard && vec.x <= rightOfGameBoard &&
            vec.y >= bottomOfGameBoard && vec.y <= topOfGameBoard;
        //return Physics2D.OverlapCircle(transform.position, 0.01f, GameBoardLayer) != null;
    }

    Vector3 NearestSquareCenterPlacement(Vector3 position)
    {
        position -= PlacementBoard.transform.position + (Vector3.left + Vector3.down) * squareWidth / 2;

        Vector3 result = new Vector3(
            Mathf.RoundToInt(position.x / squareWidth) * squareWidth,
            Mathf.RoundToInt(position.y / squareWidth) * squareWidth,
            -1);

        result += PlacementBoard.transform.position + (Vector3.left + Vector3.down) * squareWidth / 2;

        return result;
    }

    Vector3 NearestSquareCenterGame(Vector3 position)
    {
        position -= GameBoard.transform.position + Vector3.left * squareWidth / 2;

        Vector3 result = new Vector3(
            Mathf.RoundToInt(position.x / squareWidth) * squareWidth,
            Mathf.RoundToInt(position.y / squareWidth) * squareWidth,
            -1);

        result += GameBoard.transform.position + Vector3.left * squareWidth / 2;

        return result;
    }

    Coordinate CoordinateOfVector3Placement(Vector3 vec)
    {
        Vector3 leftBottomOfPlacementBoard = new Vector3(leftOfPlacementBoard, bottomOfPlacementBoard, 0);
        vec -= leftBottomOfPlacementBoard;

        int row = Mathf.CeilToInt(vec.y / squareWidth);
        int column = Mathf.CeilToInt(vec.x / squareWidth);

        return new Coordinate(row, column);
    }

    Coordinate CoordinateOfVector3Game(Vector3 vec)
    {
        Vector3 leftBottomOfGameBoard = new Vector3(leftOfGameBoard, bottomOfGameBoard, 0);
        vec -= leftBottomOfGameBoard;

        int row = Mathf.CeilToInt(vec.y / squareWidth);
        int column = Mathf.CeilToInt(vec.x / squareWidth);

        return new Coordinate(row, column);
    }

    #endregion
}
