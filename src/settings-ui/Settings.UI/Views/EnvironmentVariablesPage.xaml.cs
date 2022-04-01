// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.PowerToys.Settings.UI.Library.ViewModels;
using Microsoft.UI.Xaml.Controls;

namespace Microsoft.PowerToys.Settings.UI.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class EnvironmentVariablesPage : Page
    {
        private EnvironmentVariablesViewModel ViewModel { get; set; }

        public EnvironmentVariablesPage()
        {
            this.InitializeComponent();
            ViewModel = new EnvironmentVariablesViewModel(ShellPage.SendDefaultIPCMessage);
            DataContext = ViewModel;
        }
    }
}
