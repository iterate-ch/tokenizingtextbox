using Catel.MVVM;
using System.Collections.ObjectModel;

namespace Sample
{
    public class MainViewModel : ViewModelBase
    {
        public ObservableCollection<string> Items { get; private set; } = new ObservableCollection<string>();
    }
}
