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
        private string name;

        [ObservableProperty]
        private string label;

        [ObservableProperty]
        private EnvVariableType type;

        [ObservableProperty]
        private ObservableCollection<EnvVariable> values;




        public EnvVariable()
        {
            name = "(new)";
            label = string.Empty;
            type = EnvVariableType.Single;
            values = null;
        }

        public EnvVariable Clone()
        {
            return new EnvVariable
            {
                Name = $"{Name} clone",
                Label = Label,
                Type = Type,
                Values = Values
            };
        }

        public EnvVariable UpdateFrom(EnvVariable otherCharacter)
        {
            Name = otherCharacter.Name;
            Label = otherCharacter.Label;
            Type = otherCharacter.Type;
            Values = otherCharacter.Values;

            return this;
        }
        public override string ToString()
        {
            return Name;
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