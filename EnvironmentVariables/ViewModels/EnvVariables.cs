using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnvironmentVariables.ViewModels
{
    internal class EnvVariables : INotifyPropertyChanged
    {
        private const int MAX_VALUE_LENTGH = 2048;

        public EnvVariables(EnvironmentVariableTarget target)
        {
            this.target = target;
            LoadEnvVariables(this.target);
        }

        private void LoadEnvVariables(EnvironmentVariableTarget target)
        {
            var envVariables = Environment.GetEnvironmentVariables(target);
            foreach (DictionaryEntry envVariable in envVariables)
            {
                string key = envVariable.Key as string;
                string value = envVariable.Value as string;
                if (string.IsNullOrEmpty(key))
                {
                    continue;
                }

                ObservableCollection<string> values = new ObservableCollection<string>();
                var splitValues = value.Split(';');
                foreach(string valueSplit in splitValues)
                {
                    values.Add(valueSplit);
                }
                variables.Add(new EnvVariable(key, values));
            }
        }

        internal void AddVariable(string key, string value)
        {
            Environment.SetEnvironmentVariable(key, value, target);
        }

        internal void EditVariable(string key, string value)
        {
            Environment.SetEnvironmentVariable(key, value, target);
        }

        internal void DeleteVariable(string key)
        {
            Environment.SetEnvironmentVariable(key, null, target);
        }

        private ObservableCollection<EnvVariable> variables = new ObservableCollection<EnvVariable>();
        public ObservableCollection<EnvVariable> Variables {
            get
            {
                return variables;
            }
            set
            {
                variables = value;
            }
        }

        private EnvironmentVariableTarget target;

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
