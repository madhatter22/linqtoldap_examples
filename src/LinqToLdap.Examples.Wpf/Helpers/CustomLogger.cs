using System;
using System.Collections;
using System.DirectoryServices.Protocols;
using System.IO;
using System.Linq;
using System.Reflection;
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

        private class ObjectDumper
        {
            private TextWriter _writer;
            private int _pos;
            private int _level;
            private readonly int _depth;
            
            public static void Write(object element, int depth, TextWriter log)
            {
                var dumper = new ObjectDumper(depth) { _writer = log };
                dumper.WriteObject(null, element);
            }

            private ObjectDumper(int depth)
            {
                _depth = depth;
            }

            private void Write(string s)
            {
                if (s == null) return;

                _writer.Write(s);
                _pos += s.Length;
            }

            private void WriteIndent()
            {
                for (int i = 0; i < _level; i++) _writer.Write("  ");
            }

            private void WriteLine()
            {
                _writer.WriteLine();
                _pos = 0;
            }

            private void WriteTab()
            {
                Write("  ");
                while (_pos % 8 != 0) Write(" ");
            }

            private void WriteObject(string prefix, object element)
            {
                if (element == null || element is ValueType || element is string)
                {
                    WriteIndent();
                    Write(prefix);
                    WriteValue(element);
                    WriteLine();
                }
                else
                {
                    var enumerableElement = element as IEnumerable;
                    if (enumerableElement != null)
                    {
                        foreach (object item in enumerableElement)
                        {
                            if (item is IEnumerable && !(item is string))
                            {
                                WriteIndent();
                                Write(prefix);
                                Write("...");
                                WriteLine();
                                if (_level < _depth)
                                {
                                    _level++;
                                    WriteObject(prefix, item);
                                    _level--;
                                }
                            }
                            else
                            {
                                WriteObject(prefix, item);
                            }
                        }
                    }
                    else
                    {
                        var members = element.GetType().GetMembers(BindingFlags.Public | BindingFlags.Instance);
                        WriteIndent();
                        Write(prefix);
                        bool propWritten = false;
                        foreach (MemberInfo m in members)
                        {
                            var f = m as FieldInfo;
                            var p = m as PropertyInfo;
                            if (f == null && p == null) continue;

                            if (propWritten)
                            {
                                WriteTab();
                            }
                            else
                            {
                                propWritten = true;
                            }
                            Write(m.Name);
                            Write("=");
                            Type t = f != null ? f.FieldType : p.PropertyType;
                            if (t.IsValueType || t == typeof(string))
                            {
                                WriteValue(f != null ? f.GetValue(element) : p.GetValue(element, null));
                            }
                            else if (typeof(DirectoryResponse).IsAssignableFrom(t))
                            {
                                var value = (f != null ? f.GetValue(element) : p.GetValue(element, null)) as DirectoryResponse;

                                if (value != null)
                                {
                                    Write(
                                        string.Format(
                                            "[ ErrorMessage: {0}, MatchedDN: {1}, ResultCode: {2}, RequestId: {3}, Controls: {4}, Referrals: {5} ]",
                                            value.ErrorMessage,
                                            value.MatchedDN,
                                            value.ResultCode,
                                            value.RequestId,
                                            string.Join(" | ", value.Controls.Select(c => c.Type)),
                                            string.Join(" | ", value.Referral.Select(u => u.ToString()))));
                                }
                            }
                            else
                            {
                                Write(typeof(IEnumerable).IsAssignableFrom(t) ? "..." : "{ }");
                            }
                        }
                        if (propWritten) WriteLine();
                        if (_level < _depth)
                        {
                            foreach (MemberInfo m in members)
                            {
                                var f = m as FieldInfo;
                                var p = m as PropertyInfo;
                                if (f == null && p == null) continue;

                                Type t = f != null ? f.FieldType : p.PropertyType;
                                if ((t.IsValueType || t == typeof(string))) continue;

                                object value = f != null ? f.GetValue(element) : p.GetValue(element, null);
                                if (value == null) continue;

                                _level++;
                                WriteObject(m.Name + ": ", value);
                                _level--;
                            }
                        }
                    }
                }
            }

            private void WriteValue(object o)
            {
                if (o == null)
                {
                    Write("null");
                }
                else if (o is DateTime)
                {
                    Write(((DateTime)o).ToShortDateString());
                }
                else if (o is ValueType || o is string)
                {
                    Write(o.ToString());
                }
                else if (o is IEnumerable)
                {
                    Write("...");
                }
                else
                {
                    Write("{ }");
                }
            }
        }
    }
}
