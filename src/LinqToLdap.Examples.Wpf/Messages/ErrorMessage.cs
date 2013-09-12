using System;

namespace LinqToLdap.Examples.Wpf.Messages
{
    public class ErrorMessage
    {
        public ErrorMessage(Exception ex)
        {
            Error = ex;
        }

        public Exception Error { get; private set; }
    }
}
