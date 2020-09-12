using System;

public class MoveInfoGame
{
    #region Public Members

    public MoveType MoveType;
    public Coordinate From;
    public Coordinate To;

    #endregion

    #region Constructor

    public MoveInfoGame(MoveType moveType, Coordinate from, Coordinate to)
    {
        MoveType = moveType;
        From = from;
        To = to;
    }

    #endregion

    #region Serialization

    public static byte[] Serialize(object customType)
    {
        if (!(customType is MoveInfoGame))
            throw new ArgumentException();

        var mi = (MoveInfoGame)customType;

        return new byte[] { (byte)mi.MoveType, (byte)mi.From.Row, (byte)mi.From.Column, (byte)mi.To.Row, (byte)mi.To.Column };
    }

    public static object Deserialize(byte[] data)
    {
        return new MoveInfoGame((MoveType)data[0], new Coordinate(data[1], data[2]), new Coordinate(data[3], data[4]));
    }

    #endregion

    #region ToString

    public override string ToString()
    {
        return string.Empty;
    }

    #endregion
}