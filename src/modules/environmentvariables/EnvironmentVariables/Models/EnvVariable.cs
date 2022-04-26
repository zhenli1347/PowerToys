using Microsoft.Toolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnvironmentVariables.Models
{
    public partial class EnvVariable : ObservableObject
    {
        [ObservableProperty]
        private string key;

        [ObservableProperty]
        private string value;

        [ObservableProperty]
        private EnvironmentVariableTarget target;

        [ObservableProperty]
        private EnvVariableType type;

        [ObservableProperty]
        private ObservableCollection<EnvVariable> values;




        public EnvVariable()
        {
            key = "(new)";
            value = string.Empty;
            target = EnvironmentVariableTarget.User;
            type = EnvVariableType.Single;
            values = null;
        }

        public EnvVariable Clone()
        {
            return new EnvVariable
            {
                Key = $"{Key} clone",
                Value = Value,
                Target = Target,
                Type = Type,
                Values = Values
            };
        }

        public EnvVariable UpdateFrom(EnvVariable otherCharacter)
        {
            Key = otherCharacter.Key;
            Value = otherCharacter.Value;
            Target = otherCharacter.Target;
            Type = otherCharacter.Type;
            Values = otherCharacter.Values;

            return this;
        }
        public override string ToString()
        {
            return Key;
        }
    }
    public enum EnvVariableType
    {
#pragma warning disable CA1720 // Identifier contains type name
        Single,
#pragma warning restore CA1720 // Identifier contains type name
        Multi
    }

}