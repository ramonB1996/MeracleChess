using MeracleChess.Enums;

namespace MeracleChess.Pieces
{
    public class Pawn(Board board, Color color, Position startingPosition, bool hasMovedSinceStart = false)
        : Piece(board, color, startingPosition, hasMovedSinceStart)
    {
        private int PromotionYPosition => Color == Color.White ? 7 : 0;

        public int DirectionToMoveInY => Color == Color.White ? 1 : -1;

        public override int Value => 1;
        public override string FenSymbol => Color == Color.White ? "P" : "p";

        public override string ToString() => "pawn";

        public override IEnumerable<PositionWithType> GetValidMoves()
        {
            List<PositionWithType> result = new List<PositionWithType>();

            Position? enPassantPosition = CheckEnPassantPossible();

            if (enPassantPosition != null)
            {
                result.Add(new PositionWithType(enPassantPosition, TypeOfMove.EnPassant));
            }

            Position? move1Tile = Get1TileMove();

            if (move1Tile != null)
            {
                TypeOfMove typeOfMove = move1Tile.Y == PromotionYPosition ? TypeOfMove.PawnPromotion : TypeOfMove.Move;
                result.Add(new PositionWithType(move1Tile, typeOfMove));

                Position? move2Tiles = Get2TilesMove();

                if (move2Tiles != null)
                {
                    result.Add(new PositionWithType(move2Tiles, TypeOfMove.Move));
                }
            }

            List<PositionWithType> attackablePositions = GetAttackablePositions().Where(x => PositionIsAttackable(x.Position)).ToList();

            result.AddRange(attackablePositions);

            return result;
        }

        public override IEnumerable<Position> GetAttackedPositions() => GetAttackablePositions().Select(x => x.Position).ToList();

        private Position? Get1TileMove()
        {
            Position moveUp1 = new Position(CurrentPosition.X, CurrentPosition.Y + DirectionToMoveInY);

            if (Board.PositionContainsPiece(moveUp1) || !Board.PositionIsInBounds(moveUp1))
            {
                return null;
            }

            return moveUp1;
        }

        private Position? Get2TilesMove()
        {
            if (HasMovedSinceStart)
            {
                return null;
            }

            Position moveUp2 = new Position(CurrentPosition.X, CurrentPosition.Y + (DirectionToMoveInY * 2));

            if (Board.PositionContainsPiece(moveUp2))
            {
                return null;
            }

            return moveUp2;
        }

        private Position? CheckEnPassantPossible()
        {
            List<Position> possibleEnPassants = new List<Position>()
            {
                new(CurrentPosition.X - 1, CurrentPosition.Y),
                new(CurrentPosition.X + 1, CurrentPosition.Y)
            };

            foreach (Position position in possibleEnPassants)
            {
                if (!Board.PositionIsInBounds(position) || !Board.PositionContainsPiece(position))
                {
                    continue;
                }

                Piece piece = Board.GetPieceOnPosition(position);

                if (piece is not Pawn opponentPawn || opponentPawn.Color == Color)
                {
                    continue;
                }

                Move? lastMove = Board.Moves.LastOrDefault(x => x.From.Y - 2 == x.To.Y || x.From.Y + 2 == x.To.Y);
                 
                if (lastMove == null || lastMove.Piece != opponentPawn)
                {
                    continue;
                }

                Position newPosition = new Position(opponentPawn.CurrentPosition.X, opponentPawn.CurrentPosition.Y + DirectionToMoveInY);

                if (Board.PositionContainsPiece(newPosition))
                {
                    continue;
                }

                return newPosition;
            }

            return null;
        }
        
        private IEnumerable<PositionWithType> GetAttackablePositions()
        {
            List<PositionWithType> result = new List<PositionWithType>();

            List<Position> possibleAttacks = new List<Position>()
            {
                new Position(CurrentPosition.X - 1, CurrentPosition.Y + DirectionToMoveInY),
                new Position(CurrentPosition.X + 1, CurrentPosition.Y + DirectionToMoveInY)
            };

            foreach (Position position in possibleAttacks)
            {
                TypeOfMove typeOfMove = position.Y == PromotionYPosition ? TypeOfMove.PawnPromotionWithAttack : TypeOfMove.Attack;

                result.Add(new PositionWithType(position, typeOfMove));
            }

            return result;
        }

        private bool PositionIsAttackable(Position position)
        {
            if (!Board.PositionContainsPiece(position))
            {
                return false;
            }

            Piece piece = Board.GetPieceOnPosition(position);

            return piece.Color != Color && piece is not King;
        }
    }
}
