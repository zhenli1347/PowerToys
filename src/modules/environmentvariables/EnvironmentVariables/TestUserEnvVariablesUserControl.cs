using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnvironmentVariables
{
    public sealed partial class TestUserEnvVariablesUserControl : TestEnvVariablesUserControl
    {
        public TestUserEnvVariablesUserControl() : base(EnvironmentVariableTarget.User)
        {
            this.InitializeComponent();
        }
    }
}
