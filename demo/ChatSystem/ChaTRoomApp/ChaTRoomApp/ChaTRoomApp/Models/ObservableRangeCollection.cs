using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Text;

namespace ChaTRoomApp.Models
{
    public class ObservableRangeCollection<T> : ObservableCollection<T>
    {

        /// <inheritdoc />
        public ObservableRangeCollection()
            : base()
        {
        }

        /// <inheritdoc />
        public ObservableRangeCollection(IEnumerable<T> collection)
            : base(collection)
        {
        }

        /// <summary>
        /// Adds multiple Items to the List but only triggering OnCollectionChanged once
        /// </summary>
        /// <param name="collection">Items to add</param>
        /// <param name="notificationMode">The desired notification Mode (Add or Reset)</param>
        public void AddRange(IEnumerable<T> collection, NotifyCollectionChangedAction notificationMode = NotifyCollectionChangedAction.Add)
        {
            if (notificationMode != NotifyCollectionChangedAction.Add && notificationMode != NotifyCollectionChangedAction.Reset)
                throw new ArgumentException("Mode must be either Add or Reset for AddRange.", nameof(notificationMode));
            if (collection == null)
                throw new ArgumentNullException(nameof(collection));

            CheckReentrancy();

            if (notificationMode == NotifyCollectionChangedAction.Reset)
            {
                foreach (var i in collection)
                    Items.Add(i);

                OnPropertyChanged(new PropertyChangedEventArgs(nameof(Count)));
                OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));

                return;
            }

            var startIndex = Count;
            var changedItems = collection is List<T> list ? list : new List<T>(collection);
            foreach (var i in changedItems)
                Items.Add(i);

            OnPropertyChanged(new PropertyChangedEventArgs("Count"));
            OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, changedItems, startIndex));
        }

        /// <summary> 
        /// Removes the first occurence of each item in the specified collection from ObservableCollection(Of T). NOTE: with notificationMode = Remove, removed items starting index is not set because items are not guaranteed to be consecutive.
        /// </summary> 
        public void RemoveRange(IEnumerable<T> collection, NotifyCollectionChangedAction notificationMode = NotifyCollectionChangedAction.Reset)
        {
            if (notificationMode != NotifyCollectionChangedAction.Remove && notificationMode != NotifyCollectionChangedAction.Reset)
                throw new ArgumentException("Mode must be either Remove or Reset for RemoveRange.", nameof(notificationMode));
            if (collection == null)
                throw new ArgumentNullException(nameof(collection));

            CheckReentrancy();

            if (notificationMode == NotifyCollectionChangedAction.Reset)
            {

                foreach (var i in collection)
                    Items.Remove(i);

                OnPropertyChanged(new PropertyChangedEventArgs(nameof(Count)));
                OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));

                return;
            }

            var changedItems = collection is List<T> list ? list : new List<T>(collection);
            for (var i = 0; i < changedItems.Count; i++)
            {
                if (Items.Remove(changedItems[i])) continue;
                changedItems.RemoveAt(i); //Can't use a foreach because changedItems is intended to be (carefully) modified
                i--;
            }

            OnPropertyChanged(new PropertyChangedEventArgs(nameof(Count)));
            OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, changedItems, -1));
        }


        public void Sort()
           => Sort(0, Count, null);

        public void Sort(IComparer<T> comparer)
           => Sort(0, Count, comparer);



        public void Sort(int index, int count, IComparer<T> comparer)
        {
           

            if (index < 0)
            {
                throw new ArgumentOutOfRangeException("index <0");
            }

            if (count < 0)
            {
                throw new ArgumentOutOfRangeException("cout <0");
            }

            if (Items.Count - index < count)
                throw new ArgumentOutOfRangeException("index > count");

            if (count > 1)
            {
                var list = new List<T>(Items).ToArray();               

                Array.Sort<T>(list, index, count, comparer);

                ReplaceRange(list);
            }           

        }

        /// <summary> 
        /// Clears the current collection and replaces it with the specified item. 
        /// </summary> 
        public void Replace(T item) => ReplaceRange(new T[] { item });

        /// <summary> 
        /// Clears the current collection and replaces it with the specified collection. 
        /// </summary> 
        public void ReplaceRange(IEnumerable<T> collection)
        {
            if (collection == null)
                throw new ArgumentNullException(nameof(collection));

            Items.Clear();
            AddRange(collection, NotifyCollectionChangedAction.Reset);
        }

    }
}
