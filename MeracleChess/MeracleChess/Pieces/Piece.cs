using MeracleChess.Enums;
using MeracleChess.Exceptions;

namespace MeracleChess.Pieces
{
    public abstract class Piece
    {
        protected readonly Board Board;

        public Color Color { get; }

        public Position StartingPosition { get; }

        public Position CurrentPosition { get; set; }

        public bool HasMovedSinceStart { get; set; }

        public virtual int Value { get; } = 0;
        public virtual string NotationSymbol { get; } = string.Empty;
        public virtual string FenSymbol { get; } = string.Empty;

        public Piece(Board board, Color color, Position startingPosition, bool hasMovedSinceStart)
        {
            Board = board;
            Color = color;
            StartingPosition = startingPosition;
            CurrentPosition = startingPosition;
            HasMovedSinceStart = hasMovedSinceStart;
        }

        public abstract List<PositionWithType> GetValidMoves();

        public abstract List<Position> GetAttackedPositions();

        public override abstract string ToString();

        public virtual List<PositionWithType> GetValidPositions()
        {
            List<PositionWithType> result = GetValidMoves();

            (bool isDenyingCheck, List<Position> allowedPositions) = IsDenyingCheckWithPositions();

            if (isDenyingCheck)
            {
                return result.Where(x => allowedPositions.Contains(x.Position)).ToList();
            }

            if (Board.OurKing(Color).IsChecked)
            {
                List<Position> possibleMovesToDenyCheck = ShowPossibleMovesToDenyCheck();

                return result.Where(x => possibleMovesToDenyCheck.Contains(x.Position)).ToList();
            }

            return result;
        }

        public void MoveToPosition(PositionWithType positionWithType)
        {
            List<PositionWithType> validPositions = GetValidPositions();

            if (!validPositions.Contains(positionWithType))
            {
                throw new Exception($"Can not move to position {positionWithType.Position.X}:{positionWithType.Position.Y} with {this.ToString()}. {nameof(validPositions)} does not contain this position, hence it is not a valid position.");
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
                        throw new NotFoundException($"Rook could not be found on position: {currentKingSideRookPosition} while castling on Kingside!");
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
                        throw new NotFoundException($"Rook could not be found on position: {currentQueenSideRookPosition} while castling on Queenside!");
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

        protected List<Position> ShowPossibleMovesToDenyCheck()
        {
            List<Position> result = new List<Position>();

            IEnumerable<Piece> opponentPiecesCausingCheck = Board.OpponentPiecesCausingCheck(Color);

            if (opponentPiecesCausingCheck.Count() <= 0 || opponentPiecesCausingCheck.Count() > 2)
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

        protected (bool isDenyingCheck, List<Position> allowedPositions) IsDenyingCheckWithPositions()
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
    }
}