using MeracleChess.Enums;
using MeracleChess.EventArguments;
using MeracleChess.Exceptions;
using MeracleChess.Pieces;

namespace MeracleChess
{
    public class Board
    {
        public event EventHandler<Piece>? PieceWasAdded;

        public event EventHandler<RemovedPiece>? PieceWasRemoved;

        public event EventHandler<Move>? PieceWasMoved;

        public event EventHandler<KingWithState>? KingStateChanged;

        public event EventHandler<PawnWithPositionWithType>? PawnPromotable;

        public event EventHandler<Color>? TurnChanged;

        public List<Tile> Tiles { get; } = new();

        public List<Piece> Pieces { get; private set; } = new();

        public List<Move> Moves { get;} = new();

        public King OurKing(Color color) => Pieces.OfType<King>().First(x => x.Color == color);

        public King TheirKing(Color color) => Pieces.OfType<King>().First(x => x.Color != color);

        public IEnumerable<Piece> OpponentPiecesCausingCheck(Color color) => Pieces.Where(x => x is not King && x.Color != color && x.GetAttackedPositions().Contains(OurKing(color).CurrentPosition));

        public int ScoreForWhite => Pieces.Where(x => x.Color == Color.White).Sum(x => x.Value);
        public int ScoreForBlack => Pieces.Where(x => x.Color == Color.Black).Sum(x => x.Value);

        public Color ColorOnBottom { get; }

        public Color TurnOfColor { get; set; } = Color.White;

        public Board(Color colorOnBottom)
        {
            ColorOnBottom = colorOnBottom;

            if (colorOnBottom == Color.White)
            {
                CreateTilesWhiteOnBottom();
            }
            else
            {
                CreateTilesBlackOnBottom();
            }
        }
        
        /// <summary>
        /// Create the pieces on the board.
        /// </summary>
        public void InitializePieces()
        {
            // White pieces
            Pieces.Add(new Rook(this, Color.White, new Position(0, 0)));
            Pieces.Add(new Knight(this, Color.White, new Position(1, 0)));
            Pieces.Add(new Bishop(this, Color.White, new Position(2, 0)));
            Pieces.Add(new Queen(this, Color.White, new Position(3, 0)));
            Pieces.Add(new King(this, Color.White, new Position(4, 0)));
            Pieces.Add(new Bishop(this, Color.White, new Position(5, 0)));
            Pieces.Add(new Knight(this, Color.White, new Position(6, 0)));
            Pieces.Add(new Rook(this, Color.White, new Position(7, 0)));
            
            // White pawns
            for (int i = 0; i < 8; i++)
            {
                Pieces.Add(new Pawn(this, Color.White, new Position(i, 1)));
            }

            // Black pieces
            Pieces.Add(new Rook(this, Color.Black, new Position(0, 7)));
            Pieces.Add(new Knight(this, Color.Black, new Position(1, 7)));
            Pieces.Add(new Bishop(this, Color.Black, new Position(2, 7)));
            Pieces.Add(new Queen(this, Color.Black, new Position(3, 7)));
            Pieces.Add(new King(this, Color.Black, new Position(4, 7)));
            Pieces.Add(new Bishop(this, Color.Black, new Position(5, 7)));
            Pieces.Add(new Knight(this, Color.Black, new Position(6, 7)));
            Pieces.Add(new Rook(this, Color.Black, new Position(7, 7)));
            
            // Black pawns
            for (int i = 0; i < 8; i++)
            {
                Pieces.Add(new Pawn(this, Color.Black, new Position(i, 6)));
            }
        }

        /// <summary>
        /// Create the pieces on the board, with a custom setup (for a game that is already in progress or a puzzle).
        /// </summary>
        /// <param name="piecesOnBoard"></param>
        public void InitializePieces(IEnumerable<Piece> piecesOnBoard)
        {
            Pieces = piecesOnBoard.ToList();
        }

        public void MovePiece(Position from, PositionWithType toWithType, Piece piece, Piece? promotedToPiece = null, bool isCastlingMove = false)
        {
            piece.CurrentPosition = toWithType.Position;
            piece.HasMovedSinceStart = true;

            King opponentKing = TheirKing(piece.Color);
            KingState kingState = opponentKing.GetKingState();

            Move move = new Move(from, toWithType.Position, piece, toWithType.Type, kingState, promotedToPiece);

            if (!isCastlingMove)
            {
                Moves.Add(move);
                PieceWasMoved?.Invoke(null, move);

                KingStateChanged?.Invoke(null, new KingWithState { King = opponentKing, State = kingState });

                bool isDraw = CheckIfGameIsDraw();

                //todo: implement a way callback for a draw

                TurnOfColor = TurnOfColor == Color.White ? Color.Black : Color.White;
                TurnChanged?.Invoke(null, TurnOfColor);
            }
        }

        private bool CheckIfGameIsDraw()
        {
            // Insufficient material, only 2 kings left
            if (Pieces.Count == 2)
            {
                return true;
            }

            // Insufficient material, only king + knight or king + bishop left vs. king.
            if (Pieces.Count == 3 && Pieces.FirstOrDefault(x => x is Bishop || x is Knight) != null)
            {
                return true;
            }

            return false;
        }

        public void TakePiece(Position position)
        {
            Piece pieceToRemove = GetPieceOnPosition(position);
            RemovePiece(pieceToRemove);
        }

        public void StartPawnPromotionProcedure(Piece piece, PositionWithType toPosition)
        {
            if (piece is Pawn pawn)
            {
                PawnPromotable?.Invoke(null, new PawnWithPositionWithType { Pawn = pawn, PositionWithType = toPosition });
            }
        }

        public void PromotePawn(Pawn pawn, PositionWithType toPositionWithType, PawnPromotionOptions chosenOption)
        {
            if (toPositionWithType.Type == TypeOfMove.PawnPromotionWithAttack)
            {
                TakePiece(toPositionWithType.Position);
            }

            Piece newPiece = chosenOption switch
            {
                PawnPromotionOptions.Queen => new Queen(this, pawn.Color, pawn.CurrentPosition),
                PawnPromotionOptions.Knight => new Knight(this, pawn.Color, pawn.CurrentPosition),
                PawnPromotionOptions.Rook => new Rook(this, pawn.Color, pawn.CurrentPosition),
                PawnPromotionOptions.Bishop => new Bishop(this, pawn.Color, pawn.CurrentPosition),
                _ => throw new NotFoundException($"Could not find value {chosenOption} in {nameof(PawnPromotionOptions)} enumeration.")
            };

            RemovePiece(pawn, isRemovedByPromotion: true);
            AddPiece(newPiece);

            MovePiece(newPiece.CurrentPosition, toPositionWithType, newPiece, newPiece);
        }

        public Piece GetPieceOnPosition(Position position)
        {
            return Pieces.First(piece => piece.CurrentPosition == position);
        }

        public bool PositionContainsPiece(Position position)
        {
            return Pieces.Any(x => x.CurrentPosition == position);
        }

        public bool PositionIsInBounds(Position position)
        {
            return Tiles.Any(x => x.Position == position);
        }

        public IEnumerable<PositionWithType> GetValidPositionsForDirection(Piece currentPiece, MoveDirection direction)
        {
            List<PositionWithType> result = new List<PositionWithType>();

            foreach (Position position in GetPositionsInBoundsAndNotBlockedForDirection(currentPiece, direction))
            {
                if (!PositionContainsPiece(position))
                {
                    result.Add(new PositionWithType(position, TypeOfMove.Move));
                    continue;
                }

                Piece foundPiece = GetPieceOnPosition(position);

                if (foundPiece.Color != currentPiece.Color && foundPiece is not King)
                {
                    result.Add(new PositionWithType(position, TypeOfMove.Attack));
                    break;
                }
            }

            return result;
        }

        public IEnumerable<Position> GetPositionsInBoundsAndNotBlockedForDirection(Piece currentPiece, MoveDirection direction)
        {
            List<Position> result = new List<Position>();
            Position position = currentPiece.CurrentPosition;

            do
            {
                position = Position.GetPositionInDirection(position, direction);

                if (!PositionIsInBounds(position))
                {
                    break;
                }

                if (PositionContainsPiece(position))
                {
                    result.Add(position);

                    Piece piece = GetPieceOnPosition(position);

                    if (piece is not King king || king.Color == currentPiece.Color)
                    {
                        break;
                    }

                    continue;
                }

                result.Add(position);
            }
            while (true);

            return result;
        }

        public IEnumerable<Position> GetAttackingPositionsForDirection(Piece currentPiece, MoveDirection direction) => GetPositionsInBoundsAndNotBlockedForDirection(currentPiece, direction);

        public bool PositionIsAttackedByOpponentPiece(Color ourColor, Position position) => Pieces.Any(x => x.Color != ourColor && x.GetAttackedPositions().Contains(position));

        private void CreateTilesWhiteOnBottom()
        {
            Color tileColor = Color.White;

            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    Position position = new Position(j, i);

                    if (j == 0)
                    {
                        tileColor = i % 2 == 0 ? Color.Black : Color.White;
                    }

                    Tiles.Add(new Tile { Position = position, Color = tileColor });

                    tileColor = tileColor == Color.White ? Color.Black : Color.White;
                }
            }
        }

        private void CreateTilesBlackOnBottom()
        {
            Color tileColor = Color.Black;

            for (int i = 7; i >= 0; i--)
            {
                for (int j = 7; j >= 0; j--)
                {
                    Position position = new Position(j, i);

                    if (j == 7)
                    {
                        tileColor = i % 2 == 0 ? Color.White : Color.Black;
                    }

                    Tiles.Add(new Tile { Position = position, Color = tileColor });

                    tileColor = tileColor == Color.White ? Color.Black : Color.White;
                }
            }
        }

        private void AddPiece(Piece piece)
        {
            Pieces.Add(piece);
            PieceWasAdded?.Invoke(null, piece);
        }

        private void RemovePiece(Piece piece, bool isRemovedByPromotion = false)
        {
            Pieces.Remove(piece);
            PieceWasRemoved?.Invoke(null, new RemovedPiece(piece, isRemovedByPromotion));
        }
    }
}