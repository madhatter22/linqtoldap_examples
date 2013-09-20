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
        private CustomTextLogger _logger;

        public MainViewModel()
            : this(Get<IMessenger>(), Get<CustomTextLogger>())
        {
        }

        public MainViewModel(IMessenger messenger, CustomTextLogger logger)
        {
            _logger = logger;
            _logger.TraceEnabled = false;

            messenger.Register<ToggleBusyMessage>(this, HandleToggleBusyMessage);

            ShowAboutCommand = new RelayCommand(ChangeView<AboutViewModel>);
            ShowServerInfoCommand = new RelayCommand(ChangeView<ServerInfoViewModel>);
            ShowUsersCommand = new RelayCommand(ChangeView<UsersViewModel>);
            ShowPerformanceCommand = new RelayCommand(ChangeView<PerformanceViewModel>);

            ChangeView<AboutViewModel>();
        }

        public ICommand ShowAboutCommand { get; private set; }
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
                var oldView = CurrentView;
                CurrentView = new T();
                if (oldView != null)
                {
                    oldView.Cleanup();
                }
            }
        }

        public bool CanClose()
        {
            return !IsBusy;
        }

        public override void Cleanup()
        {
            var oldView = CurrentView;
            CurrentView = null;
            if (oldView != null)
            {
                oldView.Cleanup();
            }

            base.Cleanup();
        }
    }
}
