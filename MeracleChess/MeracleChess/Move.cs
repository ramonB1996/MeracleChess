using MeracleChess.Enums;
using MeracleChess.Exceptions;
using MeracleChess.Pieces;

namespace MeracleChess
{
    public class Move : EventArgs
    {
        public Position From { get; }

        public Position To { get; }

        public Piece Piece { get; }

        public TypeOfMove TypeOfMove { get; }

        public KingState KingState { get; }

        public Piece? PromotedToPiece { get; }

        public Move(Position from, Position to, Piece piece, TypeOfMove typeOfMove, KingState kingState, Piece? promotedToPiece)
        {
            From = from;
            To = to;
            Piece = piece;
            TypeOfMove = typeOfMove;
            KingState = kingState;
            PromotedToPiece = promotedToPiece;
        }

        public override string ToString()
        {
            string fromString = Piece switch
            {
                Pawn => string.Empty,
                _ => From.ToString()
            };

            string? promotionSymbol = null;

            if (PromotedToPiece != null)
            {
                promotionSymbol = PromotedToPiece.Color == Color.White ? PromotedToPiece.NotationSymbol : PromotedToPiece.NotationSymbol.ToLower();
            }

            string typeOfMoveResult = TypeOfMove switch
            {
                TypeOfMove.Move => $"{Piece.NotationSymbol}{fromString}{To}",
                TypeOfMove.Attack => $"{Piece.NotationSymbol}{From}x{To}",
                TypeOfMove.CastleKingSide => "O-O",
                TypeOfMove.CastleQueenSide => "O-O-O",
                TypeOfMove.EnPassant => $"{Piece.NotationSymbol}{From}x{To} e.p.",
                TypeOfMove.PawnPromotion => $"{To}={promotionSymbol}",
                TypeOfMove.PawnPromotionWithAttack => $"{From}x{To}={promotionSymbol}",
                _ => throw new NotFoundException($"Could not find {nameof(TypeOfMove)} with value {TypeOfMove}")
            };

            string kingStateSymbol = KingState switch
            {
                KingState.Checked => "+",
                KingState.CheckMated => "#",
                _ => string.Empty
            };

            return $"{typeOfMoveResult}{kingStateSymbol}";
        }
    }
}
