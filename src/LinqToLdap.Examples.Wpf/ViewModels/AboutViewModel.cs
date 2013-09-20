using System.Collections.ObjectModel;
using System.Diagnostics;
using GalaSoft.MvvmLight.Command;

namespace LinqToLdap.Examples.Wpf.ViewModels
{
    public class AboutViewModel : ViewModel
    {
        public AboutViewModel()
        {
            Hyperlinks = new ObservableCollection<object>(new object[]
                {
                    new {Name = "LINQ to LDAP", LaunchCommand = new RelayCommand(() => Launch("http://linqtoldap.codeplex.com/"))},
                    new {Name = "Test LDAP Server", LaunchCommand = new RelayCommand(() => Launch("http://blog.stuartlewis.com/2008/07/07/test-ldap-service/"))},
                    new {Name = "Performance Test LDAP Server", LaunchCommand = new RelayCommand(() => Launch("http://its.virginia.edu/network/ldap.html"))},
                    new {Name = "MVVM Light Toolkit", LaunchCommand = new RelayCommand(() => Launch("http://mvvmlight.codeplex.com/"))},
                    new {Name = "Extended WPF Toolkit™ Community Edition", LaunchCommand = new RelayCommand(() => Launch("https://wpftoolkit.codeplex.com/"))}
                });
        }

        public ObservableCollection<object> Hyperlinks { get; private set; }

        private void Launch(string url)
        {
            using (Process.Start(url)){}
        }
    }
}
