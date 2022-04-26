using EnvironmentVariables.Helpers;
using EnvironmentVariables.Models;
using Microsoft.Toolkit.Mvvm.Input;
using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows.Input;

namespace EnvironmentVariables.ViewModels
{
    public partial class UserViewModel : BaseViewModel<EnvVariable>
    {
        public UserViewModel()
        {
            var uservars = EnvironmentVariablesHelper.GetVariables(EnvironmentVariableTarget.User);

            foreach (EnvVariable var in uservars)
            {
                Items.Add(var);
            }
            Items.CollectionChanged += Items_CollectionChanged;
        }

        private void Items_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            Debug.WriteLine($"Collection changed: {e.Action}.");
        }
    }
}