using MeracleChess.Enums;

namespace MeracleChess.Services
{
	public interface IFENService
	{
        public string NewGameFENWhiteBottom { get; }
        public string NewGameFENBlackBottom { get; }

        public string CreateFENFromBoard(Board board);

        public Board CreateBoardFromFEN(string fen, Color colorOnBottom = Color.White);
    }
}

