using MeracleChess.Enums;
using MeracleChess.Pieces;

namespace MeracleChess.EventArguments
{
	public class KingWithState
	{
		public King? King { get; init; }

		public KingState State { get; init; }
	}
}

