using GalaSoft.MvvmLight;

namespace LinqToLdap.Examples.Wpf.ViewModels
{
    public class ViewModel : ViewModelBase
    {
        protected static T Get<T>() where T : class
        {
            return App.Container.GetInstance<T>();
        }
    }
}
