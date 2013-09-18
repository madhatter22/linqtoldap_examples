using GalaSoft.MvvmLight;
using System.Linq;

namespace LinqToLdap.Examples.Wpf.ViewModels
{
    public class ViewModel : ViewModelBase
    {
        protected static T Get<T>() where T : class
        {
            return App.Container.GetInstance<T>();
        }

        protected void NotifyAllPropertiesChanged()
        {
            foreach (var property in GetType().GetProperties().Where(p => p.GetGetMethod() != null && p.GetSetMethod() != null))
            {
                RaisePropertyChanged(property.Name);
            }
        }
    }
}
