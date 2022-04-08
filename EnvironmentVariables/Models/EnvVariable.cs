using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnvironmentVariables.Models
{
    internal class EnvVariable
    {
        public string Name { get; set; }
        public string Label { get; set; }
        
        public EnvVariableType Type { get; set; }
        public ObservableCollection<EnvVariable> Values { get; set; }
    }

    public enum EnvVariableType
    {
#pragma warning disable CA1720 // Identifier contains type name
        Single,
#pragma warning restore CA1720 // Identifier contains type name
        Multi
    }
}
