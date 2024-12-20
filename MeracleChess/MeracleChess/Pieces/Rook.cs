using MeracleChess.Enums;

namespace MeracleChess.Pieces;

public class Rook(Board board, Color color, Position startingPosition, bool hasMovedSinceStart = false)
    : Piece(board, color, startingPosition, hasMovedSinceStart)
{
    public override int Value => 5;
    public override string NotationSymbol => this.Color == Color.White ? "R" : "r";
    public override string FenSymbol => NotationSymbol;

    public override string ToString() => "rook";

    public override List<PositionWithType> GetValidMoves()
    {
        List<PositionWithType> result = new List<PositionWithType>();

        result.AddRange(Board.GetValidPositionsForDirection(this, MoveDirection.Up));
        result.AddRange(Board.GetValidPositionsForDirection(this, MoveDirection.Right));
        result.AddRange(Board.GetValidPositionsForDirection(this, MoveDirection.Down));
        result.AddRange(Board.GetValidPositionsForDirection(this, MoveDirection.Left));

        return result;
    }

    public override List<Position> GetAttackedPositions()
    {
        List<Position> result = new List<Position>();

        result.AddRange(Board.GetAttackingPositionsForDirection(this, MoveDirection.Up));
        result.AddRange(Board.GetAttackingPositionsForDirection(this, MoveDirection.Right));
        result.AddRange(Board.GetAttackingPositionsForDirection(this, MoveDirection.Down));
        result.AddRange(Board.GetAttackingPositionsForDirection(this, MoveDirection.Left));

        return result;
    }
}