using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace LinqToLdap.Examples.Mvc
{
    public class DebugWriter : TextWriter
    {
        /// <summary>
        /// Lazy instance of this class.
        /// 
        /// </summary>
        public static DebugWriter Instance
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

        private DebugWriter()
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
            Debug.Write(new String(buffer, index, count));
        }

        /// <summary>
        /// Delegates writing to <see cref="Debug.Write(string)"/>
        /// </summary>
        /// <param name="value">The value.</param>
        public override void Write(string value)
        {
            Debug.Write(value);
        }

        /// <summary>
        /// Delegates writing to <see cref="Debug.WriteLine(string)"/>
        /// </summary>
        /// <param name="buffer">buffer</param>
        /// <param name="index">index</param>
        /// <param name="count">count</param>
        public override void WriteLine(char[] buffer, int index, int count)
        {
            Debug.WriteLine(new String(buffer, index, count));
        }

        /// <summary>
        /// Delegates writing to <see cref="Debug.WriteLine(string)"/>
        /// </summary>
        /// <param name="value">The value.</param>
        public override void WriteLine(string value)
        {
            Debug.WriteLine(value);
        }

        /// <summary>
        /// Calls <see cref="Debug.WriteLine(string)"/> and passes <see cref="string.Empty"/>.
        /// </summary>
        public override void WriteLine()
        {
            Debug.WriteLine(string.Empty);
        }

        private class Nested
        {
            internal static readonly DebugWriter NestedInstance = new DebugWriter();

            static Nested()
            {
            }
        }
    }
}
