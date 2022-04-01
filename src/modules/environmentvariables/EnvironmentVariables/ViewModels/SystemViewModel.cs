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
    public partial class SystemViewModel : BaseViewModel<EnvVariable>
    {
        public SystemViewModel()
        {
            var uservars = EnvironmentVariablesHelper.GetVariables(EnvironmentVariableTarget.Machine);

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