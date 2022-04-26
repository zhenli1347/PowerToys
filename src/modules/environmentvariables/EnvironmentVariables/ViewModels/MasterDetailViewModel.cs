using EnvironmentVariables.Models;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace EnvironmentVariables.ViewModels
{
    public abstract partial class BaseModel<T> : ObservableObject
    {
        //  private readonly ObservableCollection<T> items = new();

        // Too bad these don't work (yet?).
        // [ObservableProperty]
        // [AlsoNotifyChangeFor(nameof(HasCurrent))]
        private T current;



        private ObservableCollection<T> items = new ObservableCollection<T>();
        public ObservableCollection<T> Items
        {
            get
            {
                return items;
            }
            set
            {
                items = value;
            }
        }

        //   public ObservableCollection<T> Items => new ObservableCollection<T>(items);

        public T Current
        {
            get => current;
            set
            {
                SetProperty(ref current, value);
                OnPropertyChanged(nameof(HasCurrent));
            }
        }

        public bool HasCurrent => current is not null;

        public virtual T AddItem(T item)
        {
            items.Add(item);
            //if (filter is not null)
            //{
            OnPropertyChanged(nameof(Items));
            // }

            return item;
        }

        public virtual T UpdateItem(T item, T original)
        {
            var hasCurrent = HasCurrent;

            var i = items.IndexOf(original);
            items[i] = item; // Raises CollectionChanged.

            //if (filter is not null)
            //{
            OnPropertyChanged(nameof(Items));
            //}

            if (hasCurrent && !HasCurrent)
            {
                // Restore Current.
                Current = item;
            }

            return item;
        }

        public virtual void DeleteItem(T item, string x)
        {
            items.Remove(item);

            EnvVariable x = item as EnvVariable;
            if (!string.IsNullOrEmpty(x.Name))
            {
                Environment.SetEnvironmentVariable(x.Name, null, target);
            }
            OnPropertyChanged(nameof(Items));
        }
    }
}