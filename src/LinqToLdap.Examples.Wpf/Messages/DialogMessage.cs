using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LinqToLdap.Examples.Wpf.Messages
{
    public enum DialogType
    {
        Info,
        Error,
        Critical,
    }
    public class DialogMessage
    {
        public DialogMessage(string message, string header, DialogType type)
        {
            DialogType = type;
            Message = message;
            Header = header;
        }

        public DialogType DialogType { get; private set; }
        public string Message { get; private set; }
        public string Header { get; private set; }
    }
}
