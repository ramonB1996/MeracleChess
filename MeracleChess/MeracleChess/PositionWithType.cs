using MeracleChess.Enums;

namespace MeracleChess
{
	public record struct PositionWithType
	{
		public Position Position { get; }

		public TypeOfMove Type { get; }

		public PositionWithType(Position position, TypeOfMove type)
		{
			Position = position;
			Type = type;
		}
	}
}

