using MeracleChess.Pieces;

namespace MeracleChess.EventArguments
{
	public class RemovedPiece
	{
		public Piece Piece { get; }

		public bool IsRemovedByPromotion { get; }

		public RemovedPiece(Piece piece, bool isRemovedByPromotion)
		{
			Piece = piece;
			IsRemovedByPromotion = isRemovedByPromotion;
		}
	}
}

