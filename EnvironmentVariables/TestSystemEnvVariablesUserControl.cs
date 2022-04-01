using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnvironmentVariables
{
    public sealed partial class TestSystemEnvVariablesUserControl : TestEnvVariablesUserControl
    {
        public TestSystemEnvVariablesUserControl() : base(EnvironmentVariableTarget.Machine)
        {
            this.InitializeComponent();
        }
    }
}
