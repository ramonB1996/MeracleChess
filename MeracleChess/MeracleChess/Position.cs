using MeracleChess.Enums;
using MeracleChess.Exceptions;
using MeracleChess.Pieces;

namespace MeracleChess
{
    public class Position : IEquatable<Position>
    {
        private static readonly List<char> _files = new List<char> { 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h' };
        private static readonly List<char> _ranks = new List<char> { '1', '2', '3', '4', '5', '6', '7', '8' };

        public int X { get; set; }

        public int Y { get; set; }

        public Position(int x, int y)
        {
            X = x;
            Y = y;
        }

        public Position(string notation)
        {
            Position position = ToPosition(notation);

            X = position.X;
            Y = position.Y;
        }

        public static int GetColumnFromCharacter(char character) => _files.IndexOf(character);
        public static int GetRowFromCharacter(char character) => _ranks.IndexOf(character);

        public static Position GetPositionInDirection(Position position, MoveDirection direction)
        {
            return direction switch
            {
                MoveDirection.Left => new Position(position.X - 1, position.Y),
                MoveDirection.Right => new Position(position.X + 1, position.Y),
                MoveDirection.Up => new Position(position.X, position.Y + 1),
                MoveDirection.Down => new Position(position.X, position.Y - 1),
                MoveDirection.UpLeft => new Position(position.X - 1, position.Y + 1),
                MoveDirection.UpRight => new Position(position.X + 1, position.Y + 1),
                MoveDirection.DownLeft => new Position(position.X - 1, position.Y - 1),
                MoveDirection.DownRight => new Position(position.X + 1, position.Y - 1),
                _ => throw new Exception($"Unknown Position to check in {nameof(Position.GetPositionInDirection)}")
            };
        }

        public static List<Position> GetPositionsBetweenPieces(Piece fromPiece, Piece toPiece)
        {
            List<Position> positions = new List<Position>();

            // No positions can be determined for a knight and a pawn, as they are special pieces.
            if (fromPiece is Knight || toPiece is Knight || fromPiece is Pawn || toPiece is Pawn)
            {
                return positions;
            }

            Position from = fromPiece.CurrentPosition;
            Position to = toPiece.CurrentPosition;

            // Diagonal
            if (from.X != to.X && from.Y != to.Y)
            {
                Position position = from;
                int xToAdd = position.X < to.X ? 1 : -1;
                int yToAdd = position.Y < to.Y ? 1 : -1;

                positions.Add(position);

                do
                {
                    position = new Position(position.X + xToAdd, position.Y + yToAdd);
                    positions.Add(position);
                }
                while (position != to);
            }

            // Horizontal
            if (from.X != to.X && from.Y == to.Y)
            {
                int start = Math.Min(from.X, to.X);
                int end = Math.Max(from.X, to.X);

                Position position = new Position(start, from.Y);
                positions.Add(position);

                do
                {
                    position = new Position(position.X + 1, position.Y);
                    positions.Add(position);
                }
                while (position.X < end);
            }

            // Vertical
            if (from.X == to.X && from.Y != to.Y)
            {
                int start = Math.Min(from.Y, to.Y);
                int end = Math.Max(from.Y, to.Y);

                Position position = new Position(from.X, start);
                positions.Add(position);

                do
                {
                    position = new Position(position.X, position.Y + 1);
                    positions.Add(position);
                }
                while (position.Y < end);
            }

            return positions.Except(new List<Position>() { from, to }).ToList();
        }

        public Position ToPosition(string notation)
        {
            if (string.IsNullOrWhiteSpace(notation) || notation.Length != 2)
            {
                throw new ParsePositionException($"{nameof(notation)} did not have expected length of 2. Value was: {notation}.");
            }

            char xCharacter = notation.ToLower().First();
            char yCharacter = notation.ToLower().Last();

            if (!_files.Contains(xCharacter) || !_ranks.Contains(yCharacter))
            {
                throw new ParsePositionException($"{nameof(notation)} was outside the board. Needs to be between e1 and h8. Value was: {notation}.");
            }

            int x = GetColumnFromCharacter(xCharacter);
            int y = GetRowFromCharacter(yCharacter);

            return new Position(x, y);
        }

        public override string ToString()
        {
            return $"{_files[X]}{_ranks[Y]}";
        }

        public static bool operator == (Position? p1, Position? p2)
        {
            if (p1 is null)
                return p2 is null;

            return p1.Equals(p2);
        }

        public static bool operator != (Position? p1, Position? p2)
        {
            return !(p1 == p2);
        }

        public override bool Equals(object? obj)
        {
            return Equals(obj as Position);
        }

        public bool Equals(Position? other)
        {
            return X == other?.X && Y == other?.Y;
        }

        public override int GetHashCode()
        {
            return X.GetHashCode() * 17 + Y.GetHashCode();
        }
    }
}
