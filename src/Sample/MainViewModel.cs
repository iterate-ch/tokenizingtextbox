using Catel.MVVM;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace Sample
{
    public class MainViewModel : ViewModelBase
    {
        public ICollection<string> Labels { get; } = new LabelCollection();

        private class LabelCollection : KeyedCollection<string, string>, INotifyCollectionChanged
        {
            public event NotifyCollectionChangedEventHandler CollectionChanged;

            protected override void ClearItems()
            {
                base.ClearItems();
                CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            }

            protected override string GetKeyForItem(string item) => item;

            protected override void InsertItem(int index, string item)
            {
                base.InsertItem(index, item);
                CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, index));
            }

            protected override void RemoveItem(int index)
            {
                var item = GetKeyForItem(Items[index]);
                base.RemoveItem(index);
                CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item, index));
            }
        }
    }
}
