using MeracleChess.Enums;
using MeracleChess.Pieces;

namespace MeracleChess.Services
{
    // https://en.wikipedia.org/wiki/Forsyth-Edwards_Notation
    // https://www.chess.com/terms/fen-chess
    public class FENService : IFENService
	{
        public string NewGameFENWhiteBottom => "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";
        public string NewGameFENBlackBottom => "RNBQKBNR/PPPPPPPP/8/8/8/8/pppppppp/rnbqkbnr w KQkq - 0 1";

        public Board CreateBoardFromFEN(string fen, Color colorOnBottom = Color.White)
        {
            if (fen == NewGameFENBlackBottom)
            {
                colorOnBottom = Color.Black;
            }
            
            Board board = new Board(colorOnBottom);
            int row = 7;

            string[] fenParts = fen.Split("/");

            // Step 1: Piece placement data
            for (int i = 0; i < fenParts.Length -1; i++)
            {
                CalculatePiecesForRowFENString(board, row, fenParts[i]);
                row--;
            }

            string[] lastParts = fenParts.Last().Split(" ");
            string lastRowPart = lastParts[0];

            CalculatePiecesForRowFENString(board, row, lastRowPart);

            // Step 2: Active color
            board.TurnOfColor = lastParts[1] == "w" ? Color.White : Color.Black;

            // Step 3: Castling availability
            string castlingString = lastParts[2];

            DetermineRooksThatMoved(castlingString, board);

            // Step 4: En passant target square
            string enPassantTargetString = lastParts[3];

            DetermineLastMoveWithPawn(enPassantTargetString, board);

            // Step 5: Halfmove clock -> todo: use it!
            // Step 6: Fullmove number -> todo: use it!

            return board;
        }

        public string CreateFENFromBoard(Board board)
        {
            string result = string.Empty;

            // Step 1: Piece placement data
            for (int i = 0; i < 8; i++)
            {
                string rowResult = DetermineRowString(board, i);

                result += i <= 6 ? rowResult + "/" : rowResult;
            }

            result += " ";

            // Step 2: Active color
            result += board.TurnOfColor == Color.White ? "w " : "b ";

            // Step 3: Castling availability
            string whiteCastlingResult = DetermineCastlingString(board, Color.White);
            string blackCastlingResult = DetermineCastlingString(board, Color.Black);

            if (!string.IsNullOrWhiteSpace(whiteCastlingResult) || !string.IsNullOrWhiteSpace(blackCastlingResult))
            {
                result += whiteCastlingResult;
                result += blackCastlingResult;
                result += " ";
            }
            else
            {
                result += "- ";
            }

            // Step 4: En passant target square
            string enPassantResult = "- ";

            if (board.Moves.Count > 0)
            {
                Move lastMove = board.Moves.Last();

                if (lastMove.Piece is Pawn pawn)
                {
                    int yDiff = Math.Abs(pawn.CurrentPosition.Y - pawn.StartingPosition.Y);

                    if (yDiff == 2)
                    {
                        Position newPosition = new Position(pawn.CurrentPosition.X, pawn.CurrentPosition.Y - pawn.DirectionToMoveInY);
                        enPassantResult = newPosition.ToString() + " ";
                    }
                }
            }

            result += enPassantResult;

            // Step 5: Halfmove clock. todo: implement correctly.
            result += "0 ";

            // Step 6: Fullmove number
            int moveCount = board.Moves.Count();

            if (moveCount > 1)
            {
                decimal fullMoveNumber = Math.Floor((decimal)moveCount / 2) + 1;
                result += fullMoveNumber.ToString();
            }
            else
            {
                result += "1";
            }

            return result;
        }

        private Piece GetPieceByCharacter(char character, Board board, Position position)
        {
            Color color = char.IsUpper(character) ? Color.White : Color.Black;
            character = char.ToLower(character);

            if (character == 'p')
            {
                int startingY = color == board.ColorOnBottom ? 1 : 6;

                return new Pawn(board, color, position, position.Y != startingY);
            }

            return character switch
            {
                'r' => new Rook(board, color, position),
                'n' => new Knight(board, color, position),
                'b' => new Bishop(board, color, position),
                'q' => new Queen(board, color, position),
                'k' => new King(board, color, position),
                _ => throw new Exception($"Character '{character}' is not a piece on the chessboard!")
            };
        }

        private void CalculatePiecesForRowFENString(Board board, int row, string rowFENString)
        {
            int column = 0;

            foreach (char character in rowFENString)
            {
                if (int.TryParse(character.ToString(), out int amountToSkip))
                {
                    column += amountToSkip;
                    continue;
                }

                Position position = new Position(column, row);
                Piece piece = GetPieceByCharacter(character, board, position);

                board.Pieces.Add(piece);

                column++;
            }
        }

        private string DetermineCastlingString(Board board, Color color)
        {
            string result = string.Empty;

            King king = board.OurKing(color);
            Position kingSideRookPosition = new Position(king.CurrentPosition.X + 3, king.CurrentPosition.Y);

            if (board.PositionContainsPiece(kingSideRookPosition))
            {
                Piece piece = board.GetPieceOnPosition(kingSideRookPosition);

                if (piece is Rook kingSideRook && kingSideRook.Color == color && !kingSideRook.HasMovedSinceStart)
                {
                    result += king.FenSymbol;
                }
            }

            Position queenSideRookPosition = new Position(king.CurrentPosition.X - 4, king.CurrentPosition.Y);

            if (board.PositionContainsPiece(queenSideRookPosition))
            {
                Piece piece = board.GetPieceOnPosition(queenSideRookPosition);

                if (piece is Rook queenSideRook && queenSideRook.Color == color && !queenSideRook.HasMovedSinceStart)
                {
                    result += color == Color.White ? "Q" : "q";
                }
            }

            return result;
        }

        private string DetermineRowString(Board board, int row)
        {
            string result = string.Empty;
            int counter = 0;

            int swappedRow = 7 - row;

            List<Tile> tiles = board.Tiles.Where(x => x.Position!.Y == swappedRow).ToList();

            for (int i = 0; i < 8; i++)
            {
                Tile currentTile = tiles[i];

                bool positionContainsPiece = board.PositionContainsPiece(currentTile.Position!);

                if (!positionContainsPiece)
                {
                    counter++;

                    if (counter == 8)
                    {
                        result += counter;
                        counter = 0;
                    }
                }
                else
                {
                    if (counter > 0)
                    {
                        result += counter;
                        counter = 0;
                    }

                    Piece piece = board.GetPieceOnPosition(currentTile.Position!);
                    result += piece.FenSymbol;
                }
            }

            if (counter > 0)
            {
                result += counter;
            }

            return result;
        }

        private void DetermineLastMoveWithPawn(string input, Board board)
        {
            if (input == "-")
            {
                return;
            }

            int yDiff = board.TurnOfColor == Color.White ? 2 : -2;
            Position toPosition = new Position(input);
            Position fromPosition = new Position(toPosition.X, toPosition.Y + yDiff);
            Piece pawn = board.GetPieceOnPosition(toPosition);
            pawn.HasMovedSinceStart = true;

            board.Moves.Add(new Move(fromPosition, toPosition, pawn, TypeOfMove.Move, KingState.Normal, null));
        }

        private void DetermineRooksThatMoved(string input, Board board)
        {
            if (!input.Contains('K'))
            {
                RookHasMovedSinceStartIfCanNotCastle(new Position(7, 7), board);
            }

            if (!input.Contains('Q'))
            {
                RookHasMovedSinceStartIfCanNotCastle(new Position(0, 7), board);
            }

            if (!input.Contains('k'))
            {
                RookHasMovedSinceStartIfCanNotCastle(new Position(7, 0), board);
            }

            if (!input.Contains('q'))
            {
                RookHasMovedSinceStartIfCanNotCastle(new Position(0, 0), board);
            }
        }

        private void RookHasMovedSinceStartIfCanNotCastle(Position position, Board board)
        {
            if (board.PositionContainsPiece(position))
            {
                Piece rook = board.GetPieceOnPosition(position);
                rook.HasMovedSinceStart = true;
            }
        }
    }
}

