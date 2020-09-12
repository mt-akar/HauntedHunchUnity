using System;
using System.ComponentModel;


public class Square : INotifyPropertyChanged, ICloneable
{
    #region Enums

    public enum PieceColor
    {
        Nothing,
        WhiteUnrevealed,
        WhiteRevealed,
        BlackUnrevealed,
        BlackRevealed
    }

    #endregion

    #region Public Properties

    public int Row { get; }

    public int Column { get; }

    /// <summary>
    /// Defines visual look of the square in game
    /// </summary>
    SquareState state;
    public SquareState State
    {
        get => state;
        set
        {
            if (value == SquareState.None)
                // Trap squares are reddish, rest of the board is white/black
                state = (Row == 3 || Row == 5) && (Column == 2 || Column == 5) ? SquareState.Trap : (Row + Column) % 2 == 0 ? SquareState.White : SquareState.Black;
            else
                state = value;
            OnPropertyChanged(nameof(State));
        }
    }

    Piece piece;
    public Piece Piece
    {
        get => piece;
        set
        {
            piece = value;

            // If a piece moves away from on top of a pseudo piece, pseudo piece comes back.
            // If you want to null both piece and pseudo piece, null pseudo piece first since you cannot null the piece that has a psuedo piece below it.
            if (piece == null && PseudoPiece != null)
                piece = PseudoPiece;

            // If piece is pseudo enabled, set its pseudo piece to its piece
            else if (piece is Passivable) // dont remember what this is for 28.03.2019
                PseudoPiece = piece;

            OnPropertyChanged(nameof(Piece));
            SetImageAccordingToPiece();
        }
    }

    /// <summary>
    /// The piece that can be under other pieces
    /// </summary>
    public Piece PseudoPiece { get; set; }

    /// <summary>
    /// Image source of the image of the active piece
    /// </summary>
    string imageSource;
    public string ImageSource
    {
        get => imageSource;
        private set
        {
            imageSource = value;
            OnPropertyChanged(nameof(ImageSource));
        }
    }

    /// <summary>
    /// Image color of the piece
    /// </summary>
    PieceColor imageColor;
    public PieceColor ImageColor
    {
        get => imageColor;
        private set
        {
            imageColor = value;
            OnPropertyChanged(nameof(ImageColor));
        }
    }

    #endregion

    #region Constructor

    public Square(int r, int c, Piece p = null)
    {
        Row = r;
        Column = c;
        Piece = p;
        State = SquareState.None;
    }

    #endregion

    #region Helpers

    public void SetImageAccordingToPiece()
    {
        // Set the image source
        ImageSource = @"pack://application:,,,/Images/" + (piece == null ? "Transparent.png" :
                      Piece.GetType().Name + ".png");

        // Set the image color
        if (Piece == null)
            ImageColor = PieceColor.Nothing;
        else if (Piece.Player == PlayerType.Self)
            ImageColor = Piece.Revealed ? PieceColor.WhiteRevealed : PieceColor.WhiteUnrevealed;
        else
            ImageColor = Piece.Revealed ? PieceColor.BlackRevealed : PieceColor.BlackUnrevealed;
    }

    #endregion

    #region INotifyPropertyChanged

    public event PropertyChangedEventHandler PropertyChanged;

    public void OnPropertyChanged(string property) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));

    #endregion

    #region IClonable

    public object Clone() => new Square(Row, Column, (Piece)Piece?.Clone()) { PseudoPiece = (Piece)PseudoPiece?.Clone() };

    #endregion
}
