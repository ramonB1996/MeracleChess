using MeracleChess.Enums;
using MeracleChess.Exceptions;

namespace MeracleChess.Pieces
{
    public abstract class Piece(Board board, Color color, Position startingPosition, bool hasMovedSinceStart)
    {
        protected readonly Board Board = board;

        /// <summary>
        /// Color is white or black.
        /// </summary>
        public Color Color { get; } = color;

        /// <summary>
        /// Position of the piece at the very start of the game.
        /// </summary>
        public Position StartingPosition { get; } = startingPosition;

        /// <summary>
        /// Position of the piece that it has now, after being moved (or not).
        /// Will be equal to StartingPosition as long as HasMovedSinceStart = false.
        /// </summary>
        public Position CurrentPosition { get; set; } = startingPosition;

        /// <summary>
        /// Determines if the piece has moved yet. 
        /// Used for determining 'en passant' and 'castling' rights.
        /// </summary>
        public bool HasMovedSinceStart { get; set; } = hasMovedSinceStart;

        /// <summary>
        /// Value of the piece:
        /// Queen = 8
        /// Rook = 5
        /// Knight/Bishop = 3
        /// Pawn = 1
        /// King = 0
        /// </summary>
        public virtual int Value => 0;

        public virtual string NotationSymbol => string.Empty;
        public virtual string FenSymbol => string.Empty;
        
        /// <summary>
        /// List of positions that are valid for this piece with the type of the action needed to claim the position.
        /// </summary>
        /// <returns>Returns an <see cref="IEnumerable{T}" /> of type <see cref="PositionWithType"/></returns>
        public virtual IEnumerable<PositionWithType> GetValidPositions()
        {
            IEnumerable<PositionWithType> result = GetValidMoves();

            (bool isDenyingCheck, List<Position> allowedPositions) = IsDenyingCheckWithPositions();

            if (isDenyingCheck)
            {
                return result.Where(x => allowedPositions.Contains(x.Position)).ToList();
            }

            if (Board.OurKing(Color).IsChecked)
            {
                IEnumerable<Position> possibleMovesToDenyCheck = ShowPossibleMovesToDenyCheck();

                return result.Where(x => possibleMovesToDenyCheck.Contains(x.Position)).ToList();
            }

            return result;
        }

        /// <summary>
        /// Move the piece to the given position.
        /// </summary>
        /// <param name="positionWithType"></param>
        /// <exception cref="PositionNotValidException">Given position is not possible for this piece, hence invalid.</exception>
        /// <exception cref="NotFoundException">Rook was not in correct position needed for castling.</exception>
        public void MoveToPosition(PositionWithType positionWithType)
        {
            IEnumerable<PositionWithType> validPositions = GetValidPositions();

            if (!validPositions.Contains(positionWithType))
            {
                throw new PositionNotValidException($"Can not move to position {positionWithType.Position.X}:{positionWithType.Position.Y} with {ToString()}. {nameof(validPositions)} does not contain this position, hence it is not a valid position.");
            }

            switch (positionWithType.Type)
            {
                case TypeOfMove.Move:
                    Board.MovePiece(CurrentPosition, positionWithType, this);
                    break;

                case TypeOfMove.Attack:
                    Board.TakePiece(positionWithType.Position);
                    Board.MovePiece(CurrentPosition, positionWithType, this);
                    break;

                case TypeOfMove.EnPassant:
                    int diffOfYPosition = positionWithType.Position.Y > CurrentPosition.Y ? -1 : 1;
                    Position positionOfTakenPawn = new Position(positionWithType.Position.X, positionWithType.Position.Y + diffOfYPosition);

                    Board.TakePiece(positionOfTakenPawn);
                    Board.MovePiece(CurrentPosition, positionWithType, this);
                    break;

                case TypeOfMove.CastleKingSide:
                    Position currentKingSideRookPosition = new Position(CurrentPosition.X + 3, CurrentPosition.Y);
                    Piece possibleKingSideRookPiece = Board.GetPieceOnPosition(currentKingSideRookPosition);

                    if (possibleKingSideRookPiece is not Rook kingSideRook)
                    {
                        throw new NotFoundException($"Rook could not be found on position: {currentKingSideRookPosition} while castling on king's side!");
                    }

                    Position newKingSideRookPosition = new Position(kingSideRook.CurrentPosition.X - 2, kingSideRook.CurrentPosition.Y);

                    Board.MovePiece(currentKingSideRookPosition, new PositionWithType(newKingSideRookPosition, TypeOfMove.Move), kingSideRook, isCastlingMove: true);
                    Board.MovePiece(CurrentPosition, positionWithType, this);
                    break;

                case TypeOfMove.CastleQueenSide:
                    Position currentQueenSideRookPosition = new Position(CurrentPosition.X - 4, CurrentPosition.Y);
                    Piece possibleQueenSideRookPiece = Board.GetPieceOnPosition(currentQueenSideRookPosition);

                    if (possibleQueenSideRookPiece is not Rook queenSideRook)
                    {
                        throw new NotFoundException($"Rook could not be found on position: {currentQueenSideRookPosition} while castling on queen's side!");
                    }

                    Position newQueenSideRookPosition = new Position(queenSideRook.CurrentPosition.X + 3, queenSideRook .CurrentPosition.Y);

                    Board.MovePiece(currentQueenSideRookPosition, new PositionWithType(newQueenSideRookPosition, TypeOfMove.Move), queenSideRook, isCastlingMove: true);
                    Board.MovePiece(CurrentPosition, positionWithType, this);
                    break;

                case TypeOfMove.PawnPromotion:
                case TypeOfMove.PawnPromotionWithAttack:
                    Board.StartPawnPromotionProcedure(this, positionWithType);    
                    break;
            }
        }
        
        public abstract IEnumerable<Position> GetAttackedPositions();
        
        public abstract IEnumerable<PositionWithType> GetValidMoves();
        
        private IEnumerable<Position> ShowPossibleMovesToDenyCheck()
        {
            IList<Position> result = new List<Position>();

            IEnumerable<Piece> opponentPiecesCausingCheck = Board.OpponentPiecesCausingCheck(Color);

            if (!opponentPiecesCausingCheck.Any() || opponentPiecesCausingCheck.Count() > 2)
            {
                throw new Exception("OurKing.OpponentPiecesCausingCheck not working correctly!");
            }

            if (opponentPiecesCausingCheck.Count() == 1)
            {
                Piece pieceCausingCheck = opponentPiecesCausingCheck.First();
                result.Add(pieceCausingCheck.CurrentPosition);

                List<Position> allowedPositions = Position.GetPositionsBetweenPieces(pieceCausingCheck, Board.OurKing(Color));

                foreach (Position position in allowedPositions)
                {
                    if (!Board.PositionContainsPiece(position))
                    {
                        result.Add(position);
                    }
                    else
                    {
                        if (Board.GetPieceOnPosition(position).Color != Color)
                        {
                            result.Add(position);
                        }
                    }
                }

                return result;
            }

            return new List<Position>();
        }

        private (bool isDenyingCheck, List<Position> allowedPositions) IsDenyingCheckWithPositions()
        {
            Board.Pieces.Remove(this);

            King ourKing = Board.OurKing(Color);

            if (ourKing.IsChecked)
            {
                List<Piece> opponentPiecesCausingCheck = Board.OpponentPiecesCausingCheck(Color).ToList();

                if (opponentPiecesCausingCheck.Count() <= 0 || opponentPiecesCausingCheck.Count() > 2)
                {
                    throw new Exception("OurKing.OpponentCausingCheck not working correctly!");
                }

                if (opponentPiecesCausingCheck.Count() == 1)
                {
                    Board.Pieces.Add(this);

                    Piece opponentPieceCausingCheck = opponentPiecesCausingCheck.First();
                    List<Position> positionsAllowed = Position.GetPositionsBetweenPieces(opponentPieceCausingCheck, ourKing);
                    positionsAllowed.Add(opponentPieceCausingCheck.CurrentPosition);

                    return (true, positionsAllowed);
                }

                if (opponentPiecesCausingCheck.Count() == 2)
                {
                    Board.Pieces.Add(this);

                    return (true, new List<Position>());
                }
            }

            Board.Pieces.Add(this);

            return (false, new List<Position>());
        }
        
        public abstract override string ToString();
    }
}