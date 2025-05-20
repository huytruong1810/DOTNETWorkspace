namespace DOTNETWorkspace.Components.Models;

/// <summary>
/// A Tile object represents a chess board square
/// </summary>
/// <param name="x">x-offset of the tile w.r.t. to the top-left origin</param>
/// <param name="y">y-offset of the tile w.r.t. to the top-left origin</param>
public class Tile(int x, int y)
{
    public readonly int X = x;
    public readonly int Y = y;
    /// <summary>
    /// The chess piece on the tile; can be null (no piece is on this tile)
    /// </summary>
    public Piece? Piece { get; set; }
}

/// <summary>
/// A Board object represents a wrapper around the 2D chess board
/// </summary>
public class Board
{
    public const int Rows = 8;
    public const int Cols = 8;
    /// <summary>
    /// List of pieces that have been captured by both sides
    /// </summary>
    public List<Piece> CapturedPieces { get; } = new();
    /// <summary>
    /// The 2D Tile array which is the board itself
    /// </summary>
    private readonly Tile[,] _board = new Tile[Rows, Cols];

    /// <summary>
    /// Construct a new board with all the chess piece setup
    /// </summary>
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

    /// <summary>
    /// Place a piece at the specified location on the board
    /// </summary>
    /// <param name="piece">The piece to be placed; no effect if null</param>
    /// <param name="x">The x-offset to place the piece</param>
    /// <param name="y">The y-offset to place the piece</param>
    public void PlacePiece(Piece? piece, int x, int y)
    {
        _board[x, y].Piece = piece;
    }

    /// <summary>
    /// Remove a piece (if any) at the specified location on the board
    /// </summary>
    /// <param name="x">The x-offset to remove</param>
    /// <param name="y">The y-offset to remove</param>
    /// <returns></returns>
    public Piece? RemovePiece(int x, int y)
    {
        var piece = _board[x, y].Piece;
        _board[x, y].Piece = null;
        return piece;
    }

    /// <summary>
    /// Provide an interface with the UI/Controller to retrieve the valid positions from a piece's location
    /// </summary>
    /// <param name="x">The x-offset of the inquired piece (must guarantee to exist)</param>
    /// <param name="y">The y-offset of the inquired piece (must guarantee to exist)</param>
    /// <param name="isWhiteTurn">Client caller can only see their own piece's moves</param>
    /// <returns>A list of passable destinations and capturable destinations; mutually disjunctive</returns>
    public (List<(int, int)>, List<(int, int)>) ValidNextPositions(int x, int y, bool isWhiteTurn)
    {
        var (passableTiles, capturableTiles) = _board[x, y].Piece!.ValidMoves(this, x, y);
        if (IsWhite(x, y) != isWhiteTurn) return ([], []); // Cannot check valid moves of enemy
        return 
            (passableTiles
                    .Select(tile => (tile.X, tile.Y))
                    .ToList(), 
            capturableTiles
                .Select(tile => (tile.X, tile.Y))
                .ToList());
    }

    /// <summary>
    /// Provide an interface with the UI/Controller to retrieve the symbol of a piece
    /// </summary>
    /// <param name="x">The x-offset of the inquired piece (must guarantee to exist)</param>
    /// <param name="y">The y-offset of the inquired piece (must guarantee to exist)</param>
    /// <returns>The piece's symbol</returns>
    public char Symbol(int x, int y) => _board[x, y].Piece!.GetSymbol();

    /// <summary>
    /// Check if a tile is occupied by a piece
    /// </summary>
    /// <param name="x">The x-offset of the inquired tile</param>
    /// <param name="y">The y-offset of the inquired tile</param>
    /// <returns>True if occupied; false otherwise</returns>
    public bool IsOccupied(int x, int y) => _board[x, y].Piece is not null;

    /// <summary>
    /// Check if an occupied tile is a white piece
    /// </summary>
    /// <param name="x">The x-offset of the inquired piece (must guarantee to exist)</param>
    /// <param name="y">The y-offset of the inquired piece (must guarantee to exist)</param>
    /// <returns>True if white; false otherwise</returns>
    public bool IsWhite(int x, int y) => _board[x, y].Piece!.IsWhite;
    
    /// <summary>
    /// Return reference to the chess piece if it is a pawn
    /// </summary>
    /// <param name="x">The x-offset of the inquired piece</param>
    /// <param name="y">The y-offset of the inquired piece</param>
    /// <returns>Reference to the pawn; can be null</returns>
    public Pawn? GetIfIsPawn(int x, int y) => _board[x, y].Piece as Pawn;

    public void RemoveKings()
    {
        foreach (var tile in _board)
        {
            if (tile.Piece is King) RemovePiece(tile.X, tile.Y);
        }
    }
}

/// <summary>
/// The abstract chess piece; must be fully implemented by specific types of pieces
/// </summary>
/// <param name="isWhite">True = the instantiated piece is white; false = black</param>
public abstract class Piece(bool isWhite)
{
    public readonly bool IsWhite = isWhite;

    /// <summary>
    /// Return the piece's representation; must be implemented for specific pieces
    /// </summary>
    /// <returns>The piece's char symbol</returns>
    public abstract char GetSymbol();
    /// <summary>
    /// Return the piece's valid passable and capturable destinations; must be implemented for specific pieces
    /// </summary>
    /// <param name="board">Reference to the board</param>
    /// <param name="x">x-offset of this piece on the board</param>
    /// <param name="y">y-offset of this piece on the board</param>
    /// <returns>A list of passable tiles and capturable tiles; mutually disjunctive</returns>
    public abstract (List<Tile>, List<Tile>) ValidMoves(Board board, int x, int y);

    /// <summary>
    /// Add inplace to the list of capturable tiles, i.e., the given location is occupied by an enemy piece
    /// </summary>
    /// <param name="board">Reference to the board</param>
    /// <param name="tiles">Reference to the list of capturable tiles to be updated</param>
    /// <param name="x">The x-offset of the inquired piece; can be null, e.g., from pawn's caller context</param>
    /// <param name="y">The y-offset of the inquired piece; can be null, e.g., from pawn's caller context</param>
    protected void TryAddCapture(Board board, List<Tile> tiles, int x, int y)
    {
        if (!InBorders(x, y)) return; // For pawn case
        if (!board.IsOccupied(x, y)) return; // For pawn case
        if (IsWhite != board.IsWhite(x, y)) tiles.Add(new Tile(x, y));
    }

    /// <summary>
    /// Check if given location is within the 4 borders of the chess board
    /// </summary>
    /// <param name="x">The x-offset of the inquired tile</param>
    /// <param name="y">The y-offset of the inquired tile</param>
    /// <returns>True if within borders; false otherwise</returns>
    private static bool InBorders(int x, int y) => x >= 0 && y >= 0 && x < Board.Rows && y < Board.Cols;
    
    /// <summary>
    /// A helper method for sliding across the board in increments and add inplace to the lists of moves
    /// </summary>
    /// <param name="board">Reference to the board</param>
    /// <param name="passableTiles">Reference to the list of passable tiles to be updated</param>
    /// <param name="capturableTiles">Reference to the list of capturable tiles to be updated</param>
    /// <param name="x">The x-offset of the inquired piece; can be null</param>
    /// <param name="y">The y-offset of the inquired piece; can be null</param>
    /// <param name="dx">Incrementation amount w.r.t. x</param>
    /// <param name="dy">Incrementation amount w.r.t. y</param>
    /// <param name="maxSteps">Number of increments to perform before hitting obstructions;
    /// -1 means unrestricted number of steps</param>
    protected void SlideAdd(
        Board board, 
        List<Tile> passableTiles, 
        List<Tile>? capturableTiles, 
        int x,
        int y,
        int dx,
        int dy,
        int maxSteps)
    {
        while (InBorders(x + dx, y + dy)) // Slide until hitting a wall
        {
            if (maxSteps != -1 && maxSteps == 0) break; // Meet allowed number of incrementation
            if (board.IsOccupied(x + dx, y + dy)) // Hit some piece
            {
                // If the piece we've just hit is an enemy, it is capturable
                if (capturableTiles is not null) TryAddCapture(board, capturableTiles, x + dx, y + dy);
                break; // No more sliding regardless
            }
            x += dx;
            y += dy;
            if (maxSteps != -1) maxSteps--;
            passableTiles.Add(new Tile(x, y));
        }
    }
    
    /// <summary>
    /// A reusable method for sliding on the 2 diagonal directions; add inplace to lists of moves
    /// </summary>
    /// <param name="board">Reference to the board</param>
    /// <param name="passableTiles">Reference to the list of passable tiles to be updated</param>
    /// <param name="capturableTiles">Reference to the list of capturable tiles to be updated</param>
    /// <param name="x">The x-offset of the inquired piece; can be null</param>
    /// <param name="y">The y-offset of the inquired piece; can be null</param>
    /// <param name="maxSteps">Number of increments to perform before hitting obstructions</param>
    protected void AddDiagonals(Board board, List<Tile> passableTiles, List<Tile> capturableTiles, int x, int y, int maxSteps) 
    {
        SlideAdd(board, passableTiles, capturableTiles, x, y, -1, -1, maxSteps); // North-west
        SlideAdd(board, passableTiles, capturableTiles, x, y, -1, 1, maxSteps); // North-east
        SlideAdd(board, passableTiles, capturableTiles, x, y, 1, -1, maxSteps); // South-west
        SlideAdd(board, passableTiles, capturableTiles, x, y, 1, 1, maxSteps); // South-east
    }

    /// <summary>
    /// A reusable method for sliding on the 2 cardinal directions; add inplace to lists of moves
    /// </summary>
    /// <param name="board">Reference to the board</param>
    /// <param name="passableTiles">Reference to the list of passable tiles to be updated</param>
    /// <param name="capturableTiles">Reference to the list of capturable tiles to be updated</param>
    /// <param name="x">The x-offset of the inquired piece; can be null</param>
    /// <param name="y">The y-offset of the inquired piece; can be null</param>
    /// <param name="maxSteps">Number of increments to perform before hitting obstructions</param>
    protected void AddCardinals(Board board, List<Tile> passableTiles, List<Tile> capturableTiles, int x, int y, int maxSteps)
    {
        SlideAdd(board, passableTiles, capturableTiles, x, y, -1, 0, maxSteps); // North
        SlideAdd(board, passableTiles, capturableTiles, x, y, 0, -1, maxSteps); // West
        SlideAdd(board, passableTiles, capturableTiles, x, y, 1, 0, maxSteps); // South
        SlideAdd(board, passableTiles, capturableTiles, x, y, 0, 1, maxSteps); // East
    }
}

/// <summary>
/// The King chess piece
/// </summary>
/// <param name="isWhite">White King?</param>
public class King(bool isWhite) : Piece(isWhite)
{
    /// <summary>
    /// King's symbol
    /// </summary>
    /// <returns>King's symbol</returns>
    public override char GetSymbol() => IsWhite ? '♔' : '♚';

    /// <summary>
    /// King's valid moves are 1 step away in all directions without hitting obstructions
    /// </summary>
    /// <param name="board">Reference to the board</param>
    /// <param name="x">x-offset of this King</param>
    /// <param name="y">y-offset of this King</param>
    /// <returns>A list of passable tiles and capturable tiles; mutually disjunctive</returns>
    public override (List<Tile>, List<Tile>) ValidMoves(Board board, int x, int y)
    {
        List<Tile> passableTiles = [];
        List<Tile> capturableTiles = [];
        AddCardinals(board, passableTiles, capturableTiles, x, y, 1);
        AddDiagonals(board, passableTiles, capturableTiles, x, y, 1);
        return (passableTiles, capturableTiles);
    }
}

/// <summary>
/// The Queen chess piece
/// </summary>
/// <param name="isWhite">White Qheen?</param>
public class Queen(bool isWhite) : Piece(isWhite)
{
    /// <summary>
    /// Queen's symbol
    /// </summary>
    /// <returns>Queen's symbol</returns>
    public override char GetSymbol() => IsWhite ? '♕' : '♛';

    /// <summary>
    /// Queen's valid moves are infinitely many steps away in all directions until hitting obstructions
    /// </summary>
    /// <param name="board">Reference to the board</param>
    /// <param name="x">x-offset of this Queen</param>
    /// <param name="y">y-offset of this Queen</param>
    /// <returns>A list of passable tiles and capturable tiles; mutually disjunctive</returns>
    public override (List<Tile>, List<Tile>) ValidMoves(Board board, int x, int y)
    {
        List<Tile> passableTiles = [];
        List<Tile> capturableTiles = [];
        AddCardinals(board, passableTiles, capturableTiles, x, y, -1);
        AddDiagonals(board, passableTiles, capturableTiles, x, y, -1);
        return (passableTiles, capturableTiles);
    }
}

/// <summary>
/// The Bishop chess piece
/// </summary>
/// <param name="isWhite">White Bishop?</param>
public class Bishop(bool isWhite) : Piece(isWhite)
{
    /// <summary>
    /// Bishop's symbol
    /// </summary>
    /// <returns>Bishop's symbol</returns>
    public override char GetSymbol() => IsWhite ? '♗' : '♝';

    /// <summary>
    /// Bishop's valid moves are infinitely many steps away in diagonal directions until hitting obstructions
    /// </summary>
    /// <param name="board">Reference to the board</param>
    /// <param name="x">x-offset of this Bishop</param>
    /// <param name="y">y-offset of this Bishop</param>
    /// <returns>A list of passable tiles and capturable tiles; mutually disjunctive</returns>
    public override (List<Tile>, List<Tile>) ValidMoves(Board board, int x, int y)
    {
        List<Tile> passableTiles = [];
        List<Tile> capturableTiles = [];
        AddDiagonals(board, passableTiles, capturableTiles, x, y, -1);
        return (passableTiles, capturableTiles);
    }
}

/// <summary>
/// The Rook chess piece
/// </summary>
/// <param name="isWhite">White Rook?</param>
public class Rook(bool isWhite) : Piece(isWhite)
{
    /// <summary>
    /// Rook's symbol
    /// </summary>
    /// <returns>Rook's symbol</returns>
    public override char GetSymbol() => IsWhite ? '♖' : '♜';

    /// <summary>
    /// Rook's valid moves are infinitely many steps away in cardinal directions until hitting obstructions
    /// </summary>
    /// <param name="board">Reference to the board</param>
    /// <param name="x">x-offset of this Rook</param>
    /// <param name="y">y-offset of this Rook</param>
    /// <returns>A list of passable tiles and capturable tiles; mutually disjunctive</returns>
    public override (List<Tile>, List<Tile>) ValidMoves(Board board, int x, int y)
    {
        List<Tile> passableTiles = [];
        List<Tile> capturableTiles = [];
        AddCardinals(board, passableTiles, capturableTiles, x, y, -1);
        return (passableTiles, capturableTiles);
    }
}

/// <summary>
/// The Knight chess piece
/// </summary>
/// <param name="isWhite">White Knight?</param>
public class Knight(bool isWhite) : Piece(isWhite)
{
    /// <summary>
    /// Knight's symbol
    /// </summary>
    /// <returns>Knight's symbol</returns>
    public override char GetSymbol() => IsWhite ? '♘' : '♞';

    /// <summary>
    /// Knight's valid moves are 1 L-step away in all directions without hitting obstructions at destinations
    /// </summary>
    /// <param name="board">Reference to the board</param>
    /// <param name="x">x-offset of this Knight</param>
    /// <param name="y">y-offset of this Knight</param>
    /// <returns>A list of passable tiles and capturable tiles; mutually disjunctive</returns>
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

/// <summary>
/// The Pawn chess piece
/// </summary>
/// <param name="isWhite">White Pawn?</param>
public class Pawn(bool isWhite) : Piece(isWhite)
{
    /// <summary>
    /// Tracks pawn's first move because they can move 2 steps instead of 1 if it is
    /// </summary>
    public bool IsFirstMove { get; set; } = true;
    /// <summary>
    /// Pawn's symbol
    /// </summary>
    /// <returns>Pawn's symbol</returns>
    public override char GetSymbol() => IsWhite ? '♙' : '♟';

    /// <summary>
    /// Pawn's valid moves are 1 or 2 step(s) up north (white) or down south (black) without hitting obstructions
    /// </summary>
    /// <param name="board">Reference to the board</param>
    /// <param name="x">x-offset of this Pawn</param>
    /// <param name="y">y-offset of this Pawn</param>
    /// <returns>A list of passable tiles and capturable tiles; mutually disjunctive</returns>
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

