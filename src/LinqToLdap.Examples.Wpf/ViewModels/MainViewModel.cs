using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using LinqToLdap.Examples.Wpf.Messages;
using LinqToLdap.Logging;

namespace LinqToLdap.Examples.Wpf.ViewModels
{
    public class MainViewModel : ViewModel
    {
        private IMessenger _messenger;
        private SimpleTextLogger _logger;

        public MainViewModel() : this(Get<IMessenger>(), Get<SimpleTextLogger>())
        {
        }

        public MainViewModel(IMessenger messenger, SimpleTextLogger logger)
        {
            _messenger = messenger;
            _logger = logger;

            _messenger.Register<ToggleBusyMessage>(this, HandleToggleBusyMessage);

            ShowServerInfoCommand = new RelayCommand(ShowServerInfo);
        }

        public ICommand ShowServerInfoCommand { get; private set; }

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
                if (_currentView != null)
                {
                    _currentView.Cleanup();
                }
                _currentView = value;
                RaisePropertyChanged("CurrentView");
            }
        }

        private void HandleToggleBusyMessage(ToggleBusyMessage message)
        {
            IsBusy = !IsBusy;
        }

        private void ShowServerInfo()
        {
            if (!IsBusy)
            {
                CurrentView = new ServerInfoViewModel();
            }
        }
    }
}
