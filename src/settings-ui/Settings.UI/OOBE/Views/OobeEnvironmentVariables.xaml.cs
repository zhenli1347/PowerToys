// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Threading;
using Microsoft.PowerToys.Settings.UI.Library;
using Microsoft.PowerToys.Settings.UI.OOBE.Enums;
using Microsoft.PowerToys.Settings.UI.OOBE.ViewModel;
using Microsoft.PowerToys.Settings.UI.Views;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace Microsoft.PowerToys.Settings.UI.OOBE.Views
{
    public sealed partial class OobeEnvironmentVariables : Page
    {
        public OobePowerToysModule ViewModel { get; set; }

        public OobeEnvironmentVariables()
        {
            this.InitializeComponent();
            ViewModel = new OobePowerToysModule(OobeShellPage.OobeShellHandler.Modules[(int)PowerToysModules.EnvironmentVariables]);
            DataContext = ViewModel;
        }

        private void Start_EnvironmentVariables_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            if (OobeShellPage.EnvironmentVariablesSharedEventCallback != null)
            {
                using (var eventHandle = new EventWaitHandle(false, EventResetMode.AutoReset, OobeShellPage.EnvironmentVariablesSharedEventCallback()))
                {
                    eventHandle.Set();
                }
            }

            ViewModel.LogRunningModuleEvent();
        }

        private void SettingsLaunchButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            if (OobeShellPage.OpenMainWindowCallback != null)
            {
                OobeShellPage.OpenMainWindowCallback(typeof(EnvironmentVariablesPage));
            }

            ViewModel.LogOpeningSettingsEvent();
        }
    }
}
