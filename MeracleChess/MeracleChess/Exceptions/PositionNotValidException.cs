namespace MeracleChess.Exceptions;

public class PositionNotValidException : Exception
{
    public PositionNotValidException()
    {
    }

    public PositionNotValidException(string? message) : base(message)
    {
    }

    public PositionNotValidException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}