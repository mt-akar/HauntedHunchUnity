using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System;
using UnityEngine;

public class PlayerNetworking : MonoBehaviour, IOnEventCallback
{
    // Reference to the game board objects
    public GameObject GameBoard;
    public GameObject PlacementBoard;

    const int moveInfoPlacement_typeCode = 0;

    public static byte movePiecePlacementEventCode = 0;
    public static byte readyEventCode = 1;
    public static byte movePieceGameEventCode = 2;

    bool eventExecuting = false;

    // public Text DebugTextBox;

    private void Awake()
    {
        PhotonPeer.RegisterType(typeof(MoveInfoPlacement), moveInfoPlacement_typeCode, MoveInfoPlacement.Serialize, MoveInfoPlacement.Deserialize);
    }

    public void OnEnable()
    {
        PhotonNetwork.AddCallbackTarget(this);
    }

    public void OnDisable()
    {
        PhotonNetwork.RemoveCallbackTarget(this);
    }

    void IOnEventCallback.OnEvent(EventData photonEvent)
    {
        if (eventExecuting)
            return;

        eventExecuting = true;

        Debug.Log($"PlayerManager.OnEvent with code {photonEvent.Code} and with object {photonEvent.CustomData}");

        if (photonEvent.Code == movePiecePlacementEventCode)
        {
            // Check if the custom data is right
            if (!(photonEvent.CustomData is MoveInfoPlacement))
                throw new ArgumentException($"Custom data given in the event with the code {photonEvent.Code} is not a {typeof(MoveInfoPlacement)}");

            // Cast the custom data
            MoveInfoPlacement moveInfo = (MoveInfoPlacement)photonEvent.CustomData;

            // Flip the board
            moveInfo.From.Row = GameCoordinator.nr + 1 - moveInfo.From.Row;
            moveInfo.From.Column = GameCoordinator.nc + 1 - moveInfo.From.Column;
            moveInfo.To.Row = GameCoordinator.nr + 1 - moveInfo.To.Row;
            moveInfo.To.Column = GameCoordinator.nc + 1 - moveInfo.To.Column;
            //moveInfo.From = new Coordinate(GameCoordinator.nr + 1 - moveInfo.From.Row, GameCoordinator.nc + 1 - moveInfo.From.Column);
            //moveInfo.To = new Coordinate(GameCoordinator.nr + 1 - moveInfo.To.Row, GameCoordinator.nc + 1 - moveInfo.To.Column);

            // If the piece is moved from the game board to game board
            if (!moveInfo.FromPlacementBoard && !moveInfo.ToPlacementBoard)
            {
                Debug.Log("Opponent moved from game board to game board. Trying to manage that.");
                if (GameCoordinator.Instance.IsPieceNull(moveInfo.From, false))
                {
                    Debug.Log("There is a logic error. The piece you are trying to move is null. This event could be a duplicate!");
                    return;
                }

                GameCoordinator.Instance.MoveOpponentPieceInGameBoardPlacement(moveInfo.From, moveInfo.To);
            }
            // If the piece is moved from the placement board to game board
            else if (moveInfo.FromPlacementBoard && !moveInfo.ToPlacementBoard)
            {
                Debug.Log("Opponent moved from placement board to game board. Trying to manage that.");
                GameCoordinator.Instance.PlaceOpponentPieceToGameBoard(moveInfo.To, moveInfo.PieceType);
            }
            else if (!moveInfo.FromPlacementBoard && moveInfo.ToPlacementBoard)
            {
                Debug.Log("Opponent moved from game board to placement board. Trying to manage that.");
                GameCoordinator.Instance.RemoveOpponentPieceFromGameBoard(moveInfo.From);
            }
        }
        else if (photonEvent.Code == readyEventCode)
        {
            GameCoordinator.Instance.OpponentReady();
        }
        else if (photonEvent.Code == movePieceGameEventCode)
        {
            // Check if the custom data is right
            if (!(photonEvent.CustomData is MoveInfoGame))
                throw new ArgumentException($"Custom data given in the event with the code {photonEvent.Code} is not a {typeof(MoveInfoGame)}");

            // Cast the custom data
            MoveInfoGame moveInfo = (MoveInfoGame)photonEvent.CustomData;

            // Flip the board
            moveInfo.From.Row = GameCoordinator.nr + 1 - moveInfo.From.Row;
            moveInfo.From.Column = GameCoordinator.nc + 1 - moveInfo.From.Column;
            moveInfo.To.Row = GameCoordinator.nr + 1 - moveInfo.To.Row;
            moveInfo.To.Column = GameCoordinator.nc + 1 - moveInfo.To.Column;

            // Call the move method
            Debug.Log("Opponent moved a piece. Trying to manage that.");
            GameCoordinator.Instance.OpponentMove(moveInfo);
        }

        Debug.Log("Event execution ended");
        eventExecuting = false;
    }
}