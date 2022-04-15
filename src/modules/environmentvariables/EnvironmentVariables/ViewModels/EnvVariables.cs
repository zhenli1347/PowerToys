using EnvironmentVariables.Models;
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
                        envVariable.Values.Add(new EnvVariable() {  Name = key, Label = valueSplit});
                    }

                }
                else
                {
                    envVariable.Type = EnvVariableType.Single;


                }

                variables.Add(envVariable);

                ObservableCollection<string> values = new ObservableCollection<string>();
             
               
                System.Diagnostics.Debug.WriteLine(values.Count);
             
            }
        }

        internal void AddVariable(string key, string value)
        {
            if (!string.IsNullOrEmpty(key))
            {
                Environment.SetEnvironmentVariable(key, value, target);
            }
        }

        internal void EditVariable(string key, string value)
        {
            if (!string.IsNullOrEmpty(key))
            {
                Environment.SetEnvironmentVariable(key, value, target);
            }
        }

        internal void DeleteVariable(string key)
        {
            if (!string.IsNullOrEmpty(key))
            {
                Environment.SetEnvironmentVariable(key, null, target);
            }
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
