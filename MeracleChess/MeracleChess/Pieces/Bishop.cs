using MeracleChess.Enums;

namespace MeracleChess.Pieces
{
    public class Bishop : Piece
    {
        public override int Value => 3;
        public override string NotationSymbol => this.Color == Color.White ? "B" : "b";
        public override string FenSymbol => NotationSymbol;

        public Bishop(Board board, Color color, Position startingPosition, bool hasMovedSinceStart = false) : base(board, color, startingPosition, hasMovedSinceStart)
        {
        }

        public override string ToString() => "bishop";

        public override List<PositionWithType> GetValidMoves()
        {
            List<PositionWithType> result = new List<PositionWithType>();
            
            result.AddRange(Board.GetValidPositionsForDirection(this, MoveDirection.UpLeft));
            result.AddRange(Board.GetValidPositionsForDirection(this,MoveDirection.UpRight));
            result.AddRange(Board.GetValidPositionsForDirection(this, MoveDirection.DownLeft));
            result.AddRange(Board.GetValidPositionsForDirection(this, MoveDirection.DownRight));

            return result;
        }

        public override List<Position> GetAttackedPositions()
        {
            List<Position> result = new List<Position>();

            result.AddRange(Board.GetAttackingPositionsForDirection(this, MoveDirection.UpLeft));
            result.AddRange(Board.GetAttackingPositionsForDirection(this, MoveDirection.UpRight));
            result.AddRange(Board.GetAttackingPositionsForDirection(this, MoveDirection.DownLeft));
            result.AddRange(Board.GetAttackingPositionsForDirection(this, MoveDirection.DownRight));

            return result;
        }
    }
}
