using EnvironmentVariables.Helpers;
using EnvironmentVariables.Models;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace EnvironmentVariables.ViewModels
{
    public abstract partial class BaseViewModel<T> : ObservableObject
    {
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
            EnvVariable selectedVar = item as EnvVariable;
            if (!string.IsNullOrEmpty(selectedVar.Key))
            {
                EnvironmentVariablesHelper.AddVariable(selectedVar);
                items.Add(item);

                OnPropertyChanged(nameof(Items));
            }

            return item;
        }

        public virtual T UpdateItem(T item, T original)
        {
            EnvVariable selectedVar = item as EnvVariable;

            if (!string.IsNullOrEmpty(selectedVar.Key))
            {
                EnvironmentVariablesHelper.UpdateVariable(selectedVar);
                var hasCurrent = HasCurrent;

                var i = items.IndexOf(original);
                items[i] = item; // Raises CollectionChanged.


                OnPropertyChanged(nameof(Items));

                if (hasCurrent && !HasCurrent)
                {
                    // Restore Current.
                    Current = item;
                }
            }
            return item;
        }

        public virtual void DeleteItem(T item)
        {
            EnvVariable selectedVar = item as EnvVariable;
            if (!string.IsNullOrEmpty(selectedVar.Key))
            {
                EnvironmentVariablesHelper.DeleteVariable(selectedVar);
                items.Remove(item);
                OnPropertyChanged(nameof(Items));
            }
        }
    }
}