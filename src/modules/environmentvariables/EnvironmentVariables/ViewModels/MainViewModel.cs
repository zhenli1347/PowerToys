using EnvironmentVariables.Models;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows.Input;

namespace EnvironmentVariables.ViewModels
{
    public partial class HomePageViewModel : BaseModel<EnvVariable>
    {
        public HomePageViewModel()
        {
            // Populate list.
            var envVariables = Environment.GetEnvironmentVariables(EnvironmentVariableTarget.User);
            foreach (DictionaryEntry envVar in envVariables)
            {
                string key = envVar.Key as string;
                string value = envVar.Value as string;
                if (string.IsNullOrEmpty(key))
                {
                    continue;
                }

                EnvVariable envVariable = new EnvVariable();
                envVariable.Name = key;
                envVariable.Label = value;

                var splitValues = value.Split(';');

                if (splitValues.Length > 1)
                {
                    envVariable.Type = Models.EnvVariableType.Multi;
                    envVariable.Values = new ObservableCollection<EnvVariable>();
                    foreach (string valueSplit in splitValues)
                    {
                        envVariable.Values.Add(new EnvVariable() { Name = key, Label = valueSplit });
                    }

                }
                else
                {
                    envVariable.Type = EnvVariableType.Single;


                }

                Items.Add(envVariable);
            }
        


            Items.CollectionChanged += Items_CollectionChanged;
        }

        public ICommand DuplicateCommand => new RelayCommand<string>(DuplicateCommand_Executed);

        public ICommand DeleteCommand => new RelayCommand<string>(DeleteCommand_Executed);

        public override EnvVariable UpdateItem(EnvVariable item, EnvVariable original)
        {
            var hasCurrent = HasCurrent;

            var i = Items.IndexOf(original);
            Items[i] = item; // Raises CollectionChanged.

            //if (filter is not null)
            //{
                OnPropertyChanged(nameof(Items));
           // }

            if (hasCurrent && !HasCurrent)
            {
                // Restore Current.
                Current = item;
            }

            //return item;

            // Does not raise CollectionChanged.
            return original.UpdateFrom(item);
        }

        private void DeleteCommand_Executed(string parm)
        {
            if (parm is not null)
            {
                var toBeDeleted = Items.FirstOrDefault(c => c.Name == parm);

                // Not OK when a filter is applied.
                // Items.Remove(toBeDeleted);

                DeleteItem(toBeDeleted);
            }
        }

        private void DuplicateCommand_Executed(string parm)
        {
            var toBeDuplicated = Items.FirstOrDefault(c => c.Name == parm);
            var clone = toBeDuplicated.Clone();
            // Items.Add(clone);
            AddItem(clone);
            if (Items.Contains(clone))
            {
                Current = clone;
            }
        }

        private void Items_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            Debug.WriteLine($"Collection changed: {e.Action}.");
        }
    }
}