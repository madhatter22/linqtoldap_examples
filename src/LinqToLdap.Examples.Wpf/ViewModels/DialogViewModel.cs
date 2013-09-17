using LinqToLdap.Examples.Wpf.Messages;

namespace LinqToLdap.Examples.Wpf.ViewModels
{
    public class DialogViewModel : ViewModel
    {
        public DialogViewModel(DialogMessage message)
        {
            Header = message.Header;
            Message = message.Message;
            Type = message.DialogType;
        }

        public string Header { get; set; }
        public string Message { get; set; }
        public DialogType Type { get; set; }
    }
}
