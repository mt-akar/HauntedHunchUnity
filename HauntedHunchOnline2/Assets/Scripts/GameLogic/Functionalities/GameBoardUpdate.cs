using System.Collections.Generic;

public class GameBoardUpdate
{
    public List<Shift> Shifts { get; set; } = new List<Shift>();

    public List<Coordinate> Removals { get; set; } = new List<Coordinate>();

    public List<Coordinate> Reveals { get; set; } = new List<Coordinate>();

    public GameBoardUpdate() { }

    public GameBoardUpdate(Shift singleShift)
    {
        Shifts.Add(singleShift);
    }
}

