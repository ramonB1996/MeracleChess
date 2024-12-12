using MeracleChess.Pieces;

namespace MeracleChess.EventArguments
{
	public class PawnWithPositionWithType
	{
		public Pawn? Pawn { get; init; }

		public PositionWithType PositionWithType { get; init; }
	}
}

