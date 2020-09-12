using System;

public class MoveInfoPlacement
{
    #region Public Members

    public bool FromPlacementBoard;
    public bool ToPlacementBoard;
    public Coordinate From;
    public Coordinate To;
    public byte PieceType;

    #endregion

    #region Constructor

    public MoveInfoPlacement(bool fromPlacementBoard, bool toPlacementBoard, Coordinate from, Coordinate to, byte pieceType)
    {
        FromPlacementBoard = fromPlacementBoard;
        ToPlacementBoard = toPlacementBoard;
        From = from;
        To = to;
        PieceType = pieceType;
    }

    #endregion

    #region Serialization

    public static byte[] Serialize(object customType)
    {
        if (!(customType is MoveInfoPlacement))
            throw new ArgumentException();

        var mi = (MoveInfoPlacement)customType;

        byte firstByte = 0;
        if (!mi.FromPlacementBoard)
            firstByte += 1;
        if (!mi.ToPlacementBoard)
            firstByte += 2;

        return new byte[] { firstByte, (byte)mi.From.Row, (byte)mi.From.Column, (byte)mi.To.Row, (byte)mi.To.Column, mi.PieceType };
    }

    public static object Deserialize(byte[] data)
    {
        return new MoveInfoPlacement(data[0] % 2 == 0, (data[0] / 2) % 2 == 0, new Coordinate(data[1], data[2]), new Coordinate(data[3], data[4]), data[5]);
    }

    #endregion

    #region ToString

    public override string ToString()
    {
        var fromBoard = FromPlacementBoard ? "PlacementBoard" : "GameBoard";
        var toBoard = ToPlacementBoard ? "PlacementBoard" : "GameBoard";

        return $"MoveInfo From: ({fromBoard}, {From.Column}, {From.Row}), To: ({toBoard}, {To.Column}, {To.Row})";
    }

    #endregion
}