namespace DOTNETWorkspace.Components.Models;

public class Tile(int x, int y)
{
    public readonly int X = x;
    public readonly int Y = y;
    public Piece? Piece { get; set; }
}

public class Board
{
    public const int Rows = 8;
    public const int Cols = 8;
    public List<Piece> CapturedPieces { get; } = new();
    private readonly Tile[,] _board = new Tile[Rows, Cols];

    public Board()
    {
        // Clear board
        for (var i = 0; i < Rows; i++)
        {
            for (var j = 0; j < Cols; j++)
            {
                _board[i, j] = new Tile(i, j);
            }
        }
        
        // Black side
        PlacePiece(new Rook(false), 0, 0);
        PlacePiece(new Knight(false), 0, 1);
        PlacePiece(new Bishop(false), 0, 2);
        PlacePiece(new Queen(false), 0, 3);
        PlacePiece(new King(false), 0, 4);
        PlacePiece(new Bishop(false), 0, 5);
        PlacePiece(new Knight(false), 0, 6);
        PlacePiece(new Rook(false), 0, 7);
        for (var i = 0; i < Cols; i++)
        {
            PlacePiece(new Pawn(false), 1, i);
        }
        
        // White side
        PlacePiece(new Rook(true), 7, 0);
        PlacePiece(new Knight(true), 7, 1);
        PlacePiece(new Bishop(true), 7, 2);
        PlacePiece(new Queen(true), 7, 3);
        PlacePiece(new King(true), 7, 4);
        PlacePiece(new Bishop(true), 7, 5);
        PlacePiece(new Knight(true), 7, 6);
        PlacePiece(new Rook(true), 7, 7);
        for (var i = 0; i < Cols; i++)
        {
            PlacePiece(new Pawn(true), 6, i);
        }
    }

    public void PlacePiece(Piece? piece, int x, int y)
    {
        _board[x, y].Piece = piece;
    }

    public Piece? RemovePiece(int x, int y)
    {
        var piece = _board[x, y].Piece;
        _board[x, y].Piece = null;
        return piece;
    }

    public (List<(int, int)>, List<(int, int)>) ValidNextPositions(int x, int y, bool isWhiteTurn)
    {
        var (passableTiles, capturableTiles) = _board[x, y].Piece!.ValidMoves(this, x, y);
        if (IsWhiteOccupied(x, y) != isWhiteTurn) return ([], []); // Cannot check valid moves of enemy
        return 
            (passableTiles
                    .Select(tile => (tile.X, tile.Y))
                    .ToList(), 
            capturableTiles
                .Select(tile => (tile.X, tile.Y))
                .ToList());
    }

    public char Symbol(int x, int y) => _board[x, y].Piece!.GetSymbol();

    public bool IsOccupied(int x, int y) => _board[x, y].Piece is not null;

    public bool IsWhiteOccupied(int x, int y) => _board[x, y].Piece!.IsWhite;
    
    public Piece? GetIfIsPawn(int x, int y) => _board[x, y].Piece as Pawn;
}

public abstract class Piece(bool isWhite)
{
    public readonly bool IsWhite = isWhite;

    public abstract char GetSymbol();
    public abstract (List<Tile>, List<Tile>) ValidMoves(Board board, int x, int y);

    protected void TryAddCapture(Board board, List<Tile> tiles, int x, int y)
    {
        if (!InBorders(x, y)) return; // For pawn case
        if (!board.IsOccupied(x, y)) return; // For pawn case
        if ((IsWhite && !board.IsWhiteOccupied(x, y)) || (!IsWhite && board.IsWhiteOccupied(x, y))) 
            tiles.Add(new Tile(x, y));
    }

    private static bool InBorders(int x, int y) => x >= 0 && y >= 0 && x < Board.Rows && y < Board.Cols;
    
    protected void SlideAdd(
        Board board, 
        List<Tile> passableTiles, 
        List<Tile>? capturableTiles, 
        int x, // start x
        int y, // start y
        int dx, // increment w.r.t. x
        int dy, // increment w.r.t. y
        int maxSteps) // -1 means unrestricted number of steps
    {
        while (InBorders(x + dx, y + dy)) // Slide until hitting a wall or another piece
        {
            if (maxSteps != -1 && maxSteps == 0) break;
            if (board.IsOccupied(x + dx, y + dy)) // Hit some piece
            {
                // If that piece we hit is an enemy, it is capturable
                if (capturableTiles is not null) TryAddCapture(board, capturableTiles, x + dx, y + dy);
                break;
            }
            x += dx;
            y += dy;
            if (maxSteps != -1) maxSteps--;
            passableTiles.Add(new Tile(x, y));
        }
    }
    
    protected void AddDiagonals(Board board, List<Tile> passableTiles, List<Tile> capturableTiles, int x, int y, int maxSteps) 
    {
        SlideAdd(board, passableTiles, capturableTiles, x, y, -1, -1, maxSteps); // North-west
        SlideAdd(board, passableTiles, capturableTiles, x, y, -1, 1, maxSteps); // North-east
        SlideAdd(board, passableTiles, capturableTiles, x, y, 1, -1, maxSteps); // South-west
        SlideAdd(board, passableTiles, capturableTiles, x, y, 1, 1, maxSteps); // South-east
    }

    protected void AddCardinals(Board board, List<Tile> passableTiles, List<Tile> capturableTiles, int x, int y, int maxSteps)
    {
        SlideAdd(board, passableTiles, capturableTiles, x, y, -1, 0, maxSteps); // North
        SlideAdd(board, passableTiles, capturableTiles, x, y, 0, -1, maxSteps); // West
        SlideAdd(board, passableTiles, capturableTiles, x, y, 1, 0, maxSteps); // South
        SlideAdd(board, passableTiles, capturableTiles, x, y, 0, 1, maxSteps); // East
    }
}

public class King(bool isWhite) : Piece(isWhite)
{
    public override char GetSymbol() => IsWhite ? '♔' : '♚';

    public override (List<Tile>, List<Tile>) ValidMoves(Board board, int x, int y)
    {
        List<Tile> passableTiles = [];
        List<Tile> capturableTiles = [];
        AddCardinals(board, passableTiles, capturableTiles, x, y, 1);
        AddDiagonals(board, passableTiles, capturableTiles, x, y, 1);
        return (passableTiles, capturableTiles);
    }
}

public class Queen(bool isWhite) : Piece(isWhite)
{
    public override char GetSymbol() => IsWhite ? '♕' : '♛';

    public override (List<Tile>, List<Tile>) ValidMoves(Board board, int x, int y)
    {
        List<Tile> passableTiles = [];
        List<Tile> capturableTiles = [];
        AddCardinals(board, passableTiles, capturableTiles, x, y, -1);
        AddDiagonals(board, passableTiles, capturableTiles, x, y, -1);
        return (passableTiles, capturableTiles);
    }
}

public class Bishop(bool isWhite) : Piece(isWhite)
{
    public override char GetSymbol() => IsWhite ? '♗' : '♝';

    public override (List<Tile>, List<Tile>) ValidMoves(Board board, int x, int y)
    {
        List<Tile> passableTiles = [];
        List<Tile> capturableTiles = [];
        AddDiagonals(board, passableTiles, capturableTiles, x, y, -1);
        return (passableTiles, capturableTiles);
    }
}

public class Rook(bool isWhite) : Piece(isWhite)
{
    public override char GetSymbol() => IsWhite ? '♖' : '♜';

    public override (List<Tile>, List<Tile>) ValidMoves(Board board, int x, int y)
    {
        List<Tile> passableTiles = [];
        List<Tile> capturableTiles = [];
        AddCardinals(board, passableTiles, capturableTiles, x, y, -1);
        return (passableTiles, capturableTiles);
    }
}

public class Knight(bool isWhite) : Piece(isWhite)
{
    public override char GetSymbol() => IsWhite ? '♘' : '♞';

    public override (List<Tile>, List<Tile>) ValidMoves(Board board, int x, int y)
    {
        List<Tile> passableTiles = [];
        List<Tile> capturableTiles = [];
        SlideAdd(board, passableTiles, capturableTiles, x, y, -2, -1, 1);
        SlideAdd(board, passableTiles, capturableTiles, x, y, -1, -2, 1);
        SlideAdd(board, passableTiles, capturableTiles, x, y, 2, -1, 1);
        SlideAdd(board, passableTiles, capturableTiles, x, y, 1, -2, 1);
        SlideAdd(board, passableTiles, capturableTiles, x, y, -2, 1, 1);
        SlideAdd(board, passableTiles, capturableTiles, x, y, -1, 2, 1);
        SlideAdd(board, passableTiles, capturableTiles, x, y, 2, 1, 1);
        SlideAdd(board, passableTiles, capturableTiles, x, y, 1, 2, 1);
        return (passableTiles, capturableTiles);
    }
}

public class Pawn(bool isWhite) : Piece(isWhite)
{
    public bool IsFirstMove { get; set; } = true;
    public override char GetSymbol() => IsWhite ? '♙' : '♟';

    public override (List<Tile>, List<Tile>) ValidMoves(Board board, int x, int y)
    {
        List<Tile> passableTiles = [];
        List<Tile> capturableTiles = [];
        if (IsWhite) // White pawns only go north and cannot capture in the direction of movement
        {
            SlideAdd(board, passableTiles, null, x, y, -1, 0, IsFirstMove ? 2 : 1);
            TryAddCapture(board, capturableTiles, x - 1, y - 1); // Capture northwest
            TryAddCapture(board, capturableTiles, x - 1, y + 1); // Capture northeast
        }
        else // Black pawns only go south and cannot capture in the direction of movement
        {
            SlideAdd(board, passableTiles, null, x, y, 1, 0, IsFirstMove ? 2 : 1);
            TryAddCapture(board, capturableTiles, x + 1, y - 1); // Capture southwest
            TryAddCapture(board, capturableTiles, x + 1, y + 1); // Capture southeast
        }
        return (passableTiles, capturableTiles);
    }
}

