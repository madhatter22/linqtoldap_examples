using System;
using System.IO;
using LinqToLdap.Logging;

namespace LinqToLdap.Examples.Wpf.Helpers
{
    public class CustomTextLogger : ILinqToLdapLogger
    {
        private readonly WeakReference _textWriter;

        /// <summary>
        /// Indicates if trace logging is enabled.
        /// 
        /// </summary>
        public bool TraceEnabled { get; set; }

        /// <summary>
        /// Creates a new logger from a <see cref="T:System.IO.TextWriter"/>.
        /// 
        /// </summary>
        /// <param name="textWriter">The log destination</param>
        public CustomTextLogger(TextWriter textWriter)
        {
            this._textWriter = new WeakReference(textWriter);
            this.TraceEnabled = true;
        }

        /// <summary>
        /// Writes the message to the trace if <see cref="P:LinqToLdap.Logging.SimpleTextLogger.TraceEnabled"/> is true.
        /// 
        /// </summary>
        /// <param name="message"/>
        public void Trace(string message)
        {
            if (!this._textWriter.IsAlive) return;
            ((TextWriter)this._textWriter.Target).WriteLine(message);
        }

        /// <summary>
        /// Writes the optional message and exception.
        /// 
        /// </summary>
        /// <param name="message">The message.</param><param name="ex">The exception.</param>
        public void Error(Exception ex, string message = null)
        {
            if (!this._textWriter.IsAlive) return;
            var log = (TextWriter)this._textWriter.Target;
            if (message != null)
                log.WriteLine(message);
            ObjectDumper.Write(ex, 0, log);
        }
    }
}
