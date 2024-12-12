namespace MeracleChess.Exceptions
{
	public class ParsePositionException : Exception
	{
		public ParsePositionException()
		{
		}

        public ParsePositionException(string? message) : base(message)
        {
        }

        public ParsePositionException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}

