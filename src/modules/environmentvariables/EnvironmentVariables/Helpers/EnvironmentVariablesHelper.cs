using EnvironmentVariables.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace EnvironmentVariables.Helpers
{
    public static class EnvironmentVariablesHelper
    {
        public static List<EnvVariable> GetVariables(EnvironmentVariableTarget target)
        {
            List<EnvVariable> variables = new List<EnvVariable>();
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
                envVariable.Key = key;
                envVariable.Value = value;
                envVariable.Target = target;

                var splitValues = value.Split(';');

                if (splitValues.Length > 1)
                {
                    envVariable.Type = Models.EnvVariableType.Multi;
                    envVariable.Values = new ObservableCollection<EnvVariable>();
                    foreach (string valueSplit in splitValues)
                    {
                        envVariable.Values.Add(new EnvVariable() { Key = key, Value = valueSplit });
                    }
                }
                else
                {
                    envVariable.Type = EnvVariableType.Single;
                }

                variables.Add(envVariable);
            }
            return variables;
        }

        public static void AddVariable(EnvVariable variable)
        {
            if (!string.IsNullOrEmpty(variable.Key))
            {
                Environment.SetEnvironmentVariable(variable.Key, variable.Value, variable.Target);
            }
        }

        public static void UpdateVariable(EnvVariable variable)
        {
            if (!string.IsNullOrEmpty(variable.Key))
            {
                Environment.SetEnvironmentVariable(variable.Key, variable.Value, variable.Target);
            }
        }

        public static void DeleteVariable(EnvVariable variable)
        {
            if (!string.IsNullOrEmpty(variable.Key))
            {
                Environment.SetEnvironmentVariable(variable.Key, null, variable.Target);
            }
        }

    }
}
