using MeracleChess.Enums;

namespace MeracleChess.Pieces
{
    public class Knight : Piece
    {
        public override int Value => 3;
        public override string NotationSymbol => this.Color == Color.White ? "N" : "n";
        public override string FenSymbol => NotationSymbol;

        public Knight(Board board, Color color, Position startingPosition, bool hasMovedSinceStart = false) : base(board, color, startingPosition, hasMovedSinceStart)
        {
        }

        public override string ToString() => "knight";

        public override List<Position> GetAttackedPositions() => PossiblePositionsInBounds();

        public override List<PositionWithType> GetValidMoves()
        {
            List<PositionWithType> result = new List<PositionWithType>();

            foreach (Position position in PossiblePositionsInBounds())
            {
                if (!Board.PositionContainsPiece(position))
                {
                    result.Add(new PositionWithType(position, TypeOfMove.Move));
                    continue;
                }

                Piece piece = Board.GetPieceOnPosition(position);

                if (piece.Color != Color && piece is not King)
                {
                    result.Add(new PositionWithType(position, TypeOfMove.Attack));
                }
            }

            return result;
        }

        private List<Position> PossiblePositionsInBounds()
        {
            List<Position> result = new List<Position>()
            {
                new Position(CurrentPosition.X - 1, CurrentPosition.Y + 2),
                new Position(CurrentPosition.X + 1, CurrentPosition.Y + 2),
                new Position(CurrentPosition.X + 2, CurrentPosition.Y + 1),
                new Position(CurrentPosition.X + 2, CurrentPosition.Y - 1),
                new Position(CurrentPosition.X + 1, CurrentPosition.Y - 2),
                new Position(CurrentPosition.X - 1, CurrentPosition.Y - 2),
                new Position(CurrentPosition.X - 2, CurrentPosition.Y - 1),
                new Position(CurrentPosition.X - 2, CurrentPosition.Y + 1)
            };

            return result.Where(x => Board.PositionIsInBounds(x)).ToList();
        }
    }
}
