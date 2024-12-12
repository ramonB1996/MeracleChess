using MeracleChess.Enums;

namespace MeracleChess.Pieces
{
    public class King : Piece
    {
        public override string NotationSymbol => this.Color == Color.White ? "K" : "k";
        public override string FenSymbol => NotationSymbol;

        public bool IsChecked => Board.PositionIsAttackedByOpponentPiece(Color, CurrentPosition);

        public King(Board board, Color color, Position startingPosition, bool hasMovedSinceStart = false) : base(board, color, startingPosition, hasMovedSinceStart)
        {
        }

        public override string ToString() => "king";

        public KingState GetKingState()
        {
            KingState state = KingState.Normal;
            List<Piece> piecesOfSameColor = Board.Pieces.Where(x => x.Color == Color).ToList();

            bool validPositionsAvailableInTeam = piecesOfSameColor.Any(x => x.GetValidPositions().Any());

            if (IsChecked)
            {
                state = validPositionsAvailableInTeam ? KingState.Checked : KingState.CheckMated;
            }
            else
            {
                state = validPositionsAvailableInTeam ? KingState.Normal : KingState.StaleMated;
            }

            return state;
        }

        public override List<PositionWithType> GetValidPositions()
        {
            List<PositionWithType> result = GetValidMoves();

            if (IsChecked)
            {
                List<Piece> opponentPiecesCausingCheck = Board.OpponentPiecesCausingCheck(Color).ToList();

                foreach(Piece pieceCausingCheck in opponentPiecesCausingCheck)
                {
                    List<Position> blockedPositions = Position.GetPositionsBetweenPieces(this, pieceCausingCheck);

                    result.RemoveAll(x => blockedPositions.Contains(x.Position));
                }                
            }

            return result;
        }

        public override List<Position> GetAttackedPositions()
        {
            return new List<Position>()
            {
                new Position(CurrentPosition.X - 1, CurrentPosition.Y + 1),
                new Position(CurrentPosition.X, CurrentPosition.Y + 1),
                new Position(CurrentPosition.X + 1, CurrentPosition.Y + 1),
                new Position(CurrentPosition.X + 1, CurrentPosition.Y),
                new Position(CurrentPosition.X + 1, CurrentPosition.Y - 1),
                new Position(CurrentPosition.X, CurrentPosition.Y - 1),
                new Position(CurrentPosition.X - 1, CurrentPosition.Y - 1),
                new Position(CurrentPosition.X - 1, CurrentPosition.Y),
            };
        }

        public override List<PositionWithType> GetValidMoves()
        {
            List<PositionWithType> result = new List<PositionWithType>();
            List<Position> possibleMoves = new List<Position>()
            {
                new Position(CurrentPosition.X - 1, CurrentPosition.Y + 1),
                new Position(CurrentPosition.X, CurrentPosition.Y + 1),
                new Position(CurrentPosition.X + 1, CurrentPosition.Y + 1),
                new Position(CurrentPosition.X + 1, CurrentPosition.Y),
                new Position(CurrentPosition.X + 1, CurrentPosition.Y - 1),
                new Position(CurrentPosition.X, CurrentPosition.Y - 1),
                new Position(CurrentPosition.X - 1, CurrentPosition.Y - 1),
                new Position(CurrentPosition.X - 1, CurrentPosition.Y),
            };

            foreach (Position position in possibleMoves)
            {
                if (!Board.PositionIsInBounds(position) || Board.PositionIsAttackedByOpponentPiece(Color, position))
                {
                    continue;
                }

                if (!Board.PositionContainsPiece(position))
                {
                    result.Add(new PositionWithType(position, TypeOfMove.Move));
                    continue;
                }

                Piece piece = Board.GetPieceOnPosition(position);

                if (piece.Color == Color)
                {
                    continue;
                }

                result.Add(new PositionWithType(position, TypeOfMove.Attack));
            }

            if (CanCastle(isKingSide: true))
            {
                result.Add(new PositionWithType(new Position(CurrentPosition.X + 2, CurrentPosition.Y), TypeOfMove.CastleKingSide));
            }

            if (CanCastle(isKingSide: false))
            {
                result.Add(new PositionWithType(new Position(CurrentPosition.X - 2, CurrentPosition.Y), TypeOfMove.CastleQueenSide));
            }

            return result;
        }

        private bool CanCastle(bool isKingSide)
        {
            if (HasMovedSinceStart || IsChecked)
            {
                return false;
            }

            List<Position> positionsThatNeedToBeEmptyAndNotCausingCheck;
            Position positionOfRook;

            if (isKingSide)
            {
                positionsThatNeedToBeEmptyAndNotCausingCheck = new List<Position>()
                {
                    new Position(CurrentPosition.X + 1, CurrentPosition.Y),
                    new Position(CurrentPosition.X + 2, CurrentPosition.Y)
                };

                positionOfRook = new Position(CurrentPosition.X + 3, CurrentPosition.Y);
            }
            else
            {
                positionsThatNeedToBeEmptyAndNotCausingCheck = new List<Position>()
                {
                    new Position(CurrentPosition.X - 1, CurrentPosition.Y),
                    new Position(CurrentPosition.X - 2, CurrentPosition.Y),
                    new Position(CurrentPosition.X - 3, CurrentPosition.Y)
                };

                positionOfRook = new Position(CurrentPosition.X - 4, CurrentPosition.Y);
            }

            foreach (Position position in positionsThatNeedToBeEmptyAndNotCausingCheck)
            {
                if (Board.PositionContainsPiece(position) || Board.PositionIsAttackedByOpponentPiece(Color, position))
                {
                    return false;
                }
            }

            if (!Board.PositionContainsPiece(positionOfRook))
            {
                return false;
            }

            Piece piece = Board.GetPieceOnPosition(positionOfRook);

            if (piece is not Rook rook || rook.Color != Color || rook.HasMovedSinceStart)
            {
                return false;
            }

            return true;
        }
    }
}
