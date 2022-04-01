using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace EnvironmentVariables.ViewModels
{
    internal class EnvVariable
    {
        public EnvVariable(string Key, ObservableCollection<string> Values)
        {
            this.Key = Key;
            this.Values = Values;
        }

        public string Key { get; }
        public ObservableCollection<string> Values{ get; } = new ObservableCollection<string>();
    }
}
