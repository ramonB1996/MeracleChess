using MeracleChess.Enums;

namespace MeracleChess.Pieces
{
    public class Queen(Board board, Color color, Position startingPosition, bool hasMovedSinceStart = false)
        : Piece(board, color, startingPosition, hasMovedSinceStart)
    {
        public override int Value => 8;
        public override string NotationSymbol => this.Color == Color.White ? "Q" : "q";
        public override string FenSymbol => NotationSymbol;

        public override string ToString() => "queen";

        public override IEnumerable<PositionWithType> GetValidMoves()
        {
            List<PositionWithType> result = new List<PositionWithType>();

            result.AddRange(Board.GetValidPositionsForDirection(this, MoveDirection.UpLeft));
            result.AddRange(Board.GetValidPositionsForDirection(this, MoveDirection.UpRight));
            result.AddRange(Board.GetValidPositionsForDirection(this, MoveDirection.DownLeft));
            result.AddRange(Board.GetValidPositionsForDirection(this, MoveDirection.DownRight));
            result.AddRange(Board.GetValidPositionsForDirection(this, MoveDirection.Up));
            result.AddRange(Board.GetValidPositionsForDirection(this, MoveDirection.Right));
            result.AddRange(Board.GetValidPositionsForDirection(this, MoveDirection.Down));
            result.AddRange(Board.GetValidPositionsForDirection(this, MoveDirection.Left));

            return result;
        }

        public override IEnumerable<Position> GetAttackedPositions()
        {
            List<Position> result = new List<Position>();

            result.AddRange(Board.GetAttackingPositionsForDirection(this, MoveDirection.UpLeft));
            result.AddRange(Board.GetAttackingPositionsForDirection(this, MoveDirection.UpRight));
            result.AddRange(Board.GetAttackingPositionsForDirection(this, MoveDirection.DownLeft));
            result.AddRange(Board.GetAttackingPositionsForDirection(this, MoveDirection.DownRight));
            result.AddRange(Board.GetAttackingPositionsForDirection(this, MoveDirection.Up));
            result.AddRange(Board.GetAttackingPositionsForDirection(this, MoveDirection.Right));
            result.AddRange(Board.GetAttackingPositionsForDirection(this, MoveDirection.Down));
            result.AddRange(Board.GetAttackingPositionsForDirection(this, MoveDirection.Left));

            return result;
        }
    }
}
