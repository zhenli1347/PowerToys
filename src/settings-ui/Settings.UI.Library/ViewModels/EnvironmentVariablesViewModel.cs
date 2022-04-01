// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using Microsoft.PowerToys.Settings.UI.Library.Helpers;
using Microsoft.PowerToys.Settings.UI.Library.ViewModels.Commands;

namespace Microsoft.PowerToys.Settings.UI.Library.ViewModels
{
    public class EnvironmentVariablesViewModel : Observable
    {
        private Func<string, int> SendConfigMSG { get; }

        public EnvironmentVariablesViewModel(Func<string, int> ipcMSGCallBackFunc)
        {
            SendConfigMSG = ipcMSGCallBackFunc;
            LaunchEventHandler = new ButtonClickCommand(LaunchEnvironmentVariablesApp);
        }

        public ButtonClickCommand LaunchEventHandler { get; set; }

        private void LaunchEnvironmentVariablesApp()
        {
            // send message to launch the zones editor;
            SendConfigMSG("{\"action\":{\"EnvironmentVariables\":{\"action_name\":\"LaunchApp\", \"value\":\"\"}}}");
        }
    }
}
