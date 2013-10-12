using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace LinqToLdap.Examples.Mvc
{
    public class TraceWriter : TextWriter
    {
        /// <summary>
        /// Lazy instance of this class.
        /// 
        /// </summary>
        public static TraceWriter Instance
        {
            get { return Nested.NestedInstance; }
        }

        /// <summary>
        /// Defaults to <see cref="P:System.Text.Encoding.Default"/>.
        /// 
        /// </summary>
        public override Encoding Encoding
        {
            get { return Encoding.Default; }
        }

        private TraceWriter()
        {

        }

        /// <summary>
        /// Delegates writing to <see cref="Debug.Write(string)"/>
        /// </summary>
        /// <param name="buffer">buffer</param>
        /// <param name="index">index</param>
        /// <param name="count">count</param>
        public override void Write(char[] buffer, int index, int count)
        {
            Trace.Write(new String(buffer, index, count));
        }

        /// <summary>
        /// Delegates writing to <see cref="Debug.Write(string)"/>
        /// </summary>
        /// <param name="value">The value.</param>
        public override void Write(string value)
        {
            Trace.Write(value);
        }

        /// <summary>
        /// Delegates writing to <see cref="Debug.WriteLine(string)"/>
        /// </summary>
        /// <param name="buffer">buffer</param>
        /// <param name="index">index</param>
        /// <param name="count">count</param>
        public override void WriteLine(char[] buffer, int index, int count)
        {
            Trace.WriteLine(new String(buffer, index, count));
        }

        /// <summary>
        /// Delegates writing to <see cref="Debug.WriteLine(string)"/>
        /// </summary>
        /// <param name="value">The value.</param>
        public override void WriteLine(string value)
        {
            Trace.WriteLine(value);
        }

        /// <summary>
        /// Calls <see cref="Debug.WriteLine(string)"/> and passes <see cref="string.Empty"/>.
        /// </summary>
        public override void WriteLine()
        {
            Trace.WriteLine(string.Empty);
        }

        private class Nested
        {
            internal static readonly TraceWriter NestedInstance = new TraceWriter();

            static Nested()
            {
            }
        }
    }
}
