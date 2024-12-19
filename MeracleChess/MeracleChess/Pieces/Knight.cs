using MeracleChess.Enums;

namespace MeracleChess.Pieces
{
    public class Knight(Board board, Color color, Position startingPosition, bool hasMovedSinceStart = false)
        : Piece(board, color, startingPosition, hasMovedSinceStart)
    {
        public override int Value => 3;
        public override string NotationSymbol => this.Color == Color.White ? "N" : "n";
        public override string FenSymbol => NotationSymbol;

        public override string ToString() => "knight";

        public override IEnumerable<Position> GetAttackedPositions() => PossiblePositionsInBounds();

        public override IEnumerable<PositionWithType> GetValidMoves()
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

        private IEnumerable<Position> PossiblePositionsInBounds()
        {
            List<Position> result = new List<Position>()
            {
                new(CurrentPosition.X - 1, CurrentPosition.Y + 2),
                new(CurrentPosition.X + 1, CurrentPosition.Y + 2),
                new(CurrentPosition.X + 2, CurrentPosition.Y + 1),
                new(CurrentPosition.X + 2, CurrentPosition.Y - 1),
                new(CurrentPosition.X + 1, CurrentPosition.Y - 2),
                new(CurrentPosition.X - 1, CurrentPosition.Y - 2),
                new(CurrentPosition.X - 2, CurrentPosition.Y - 1),
                new(CurrentPosition.X - 2, CurrentPosition.Y + 1)
            };

            return result.Where(x => Board.PositionIsInBounds(x)).ToList();
        }
    }
}
