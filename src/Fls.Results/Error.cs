namespace Fls.Results
{
    /// <summary>
    /// Error message container class.
    /// </summary>
    public sealed class Error
    {
        /// <summary>
        /// Error code.
        /// </summary>
        public int? Code { get; private set; }

        /// <summary>
        /// Error message text.
        /// </summary>
        public string Message { get; private set; }

        /// <summary>
        /// Constructs an instance of an error data set.
        /// </summary>
        /// <param name="message">Error message text.</param>
        /// <param name="code">Error code.</param>
        public Error(string message, int? code = null)
        {
            Message = message;
            Code = code;
        }
    }
}
