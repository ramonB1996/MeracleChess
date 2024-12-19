using MeracleChess.Enums;

namespace MeracleChess
{
	public readonly record struct PositionWithType(Position Position, TypeOfMove Type)
	{
		public Position Position { get; } = Position;

		public TypeOfMove Type { get; } = Type;
	}
}

