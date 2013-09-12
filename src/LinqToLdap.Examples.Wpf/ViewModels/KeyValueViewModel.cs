using System;
using System.Collections.Generic;
using System.Linq;
using GalaSoft.MvvmLight;

namespace LinqToLdap.Examples.Wpf.ViewModels
{
    public class KeyValueViewModel : ViewModelBase
    {
        public KeyValueViewModel(string key, object value)
        {
            Key = key;
            if (value is string)
            {
                Value = value.ToString();
            }
            else if (value is IEnumerable<string>)
            {
                Value = string.Join(Environment.NewLine, value as IEnumerable<string>);
            }
            else if (value is IEnumerable<byte>)
            {
                Value = string.Join(", ", (value as IEnumerable<byte>));
            }
            else if (value is IEnumerable<byte[]>)
            {
                Value = string.Join(Environment.NewLine, (value as IEnumerable<byte[]>).Select(b => string.Format("[{0}]", string.Join(", ", b))));
            }
            else
            {
                Value = value == null ? "" : value.ToString();
            }
        }

        public string Key { get; private set; }
        public string Value { get; private set; }
    }
}
