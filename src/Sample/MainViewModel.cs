using Catel.MVVM;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace Sample
{
    public class MainViewModel : ViewModelBase
    {
        public MainViewModel()
        {
            AcceptTokenCommand = new Command<string>(AddToken);
            RemoveTokenCommand = new Command<string>(RemoveToken);
        }

        public ICommand AcceptTokenCommand { get; }

        public List<string> BasicList { get; private set; } = new List<string>();

        public ObservableCollection<string> Items { get; private set; } = new ObservableCollection<string>();

        public HashSet<string> ItemSet { get; private set; } = new HashSet<string>();

        public ICommand RemoveTokenCommand { get; }

        private void AddToken(string token)
        {
            ItemSet.Add(token);
        }

        private void RemoveToken(string token)
        {
            ItemSet.Remove(token);
        }
    }
}
