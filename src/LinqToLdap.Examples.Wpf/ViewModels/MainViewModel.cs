using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using LinqToLdap.Examples.Wpf.Helpers;
using LinqToLdap.Examples.Wpf.Messages;

namespace LinqToLdap.Examples.Wpf.ViewModels
{
    public class MainViewModel : ViewModel
    {
        private IMessenger _messenger;
        private CustomTextLogger _logger;

        public MainViewModel()
            : this(Get<IMessenger>(), Get<CustomTextLogger>())
        {
        }

        public MainViewModel(IMessenger messenger, CustomTextLogger logger)
        {
            _messenger = messenger;
            _logger = logger;
            _logger.TraceEnabled = false;

            _messenger.Register<ToggleBusyMessage>(this, HandleToggleBusyMessage);

            ShowServerInfoCommand = new RelayCommand(ChangeView<ServerInfoViewModel>);
            ShowUsersCommand = new RelayCommand(ChangeView<UsersViewModel>);
            ShowPerformanceCommand = new RelayCommand(ChangeView<PerformanceViewModel>);
        }

        public ICommand ShowServerInfoCommand { get; private set; }
        public ICommand ShowUsersCommand { get; private set; }
        public ICommand ShowPerformanceCommand { get; private set; }

        private bool _isBusy;
        public bool IsBusy
        {
            get { return _isBusy; }
            set
            {
                if (_isBusy != value)
                {
                    _isBusy = value;
                    RaisePropertyChanged("IsBusy");
                }
            }
        }

        public bool EnableLogging
        {
            get { return _logger.TraceEnabled; }
            set
            {
                if (_logger.TraceEnabled != value)
                {
                    _logger.TraceEnabled = value;
                    RaisePropertyChanged("EnableLogging");
                }
            }
        }

        private ViewModelBase _currentView;
        public ViewModelBase CurrentView
        {
            get { return _currentView; }
            set
            {
                _currentView = value;
                RaisePropertyChanged("CurrentView");
            }
        }

        private void HandleToggleBusyMessage(ToggleBusyMessage message)
        {
            IsBusy = !IsBusy;
        }

        private void ChangeView<T>() where T : ViewModelBase, new()
        {
            if (!IsBusy)
            {
                if (CurrentView != null)
                {
                    CurrentView.Cleanup();
                }
                CurrentView = new T();
            }
        }
    }
}
