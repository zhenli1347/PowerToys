// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Windows.Input;
using Microsoft.PowerToys.Settings.UI.Library;
using Microsoft.PowerToys.Settings.UI.Library.Utilities;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;

namespace Microsoft.PowerToys.Settings.UI.Controls
{
    public sealed partial class SoftwareVersionControl : UserControl
    {
        public static readonly DependencyProperty UpdateCheckedDateProperty = DependencyProperty.Register("UpdateCheckedDate", typeof(string), typeof(SoftwareVersionControl), null);
        public static readonly DependencyProperty PowerToysVersionProperty = DependencyProperty.Register("PowerToysVersion", typeof(string), typeof(SoftwareVersionControl), null);
        public static readonly DependencyProperty PowerToysNewAvailableVersionProperty = DependencyProperty.Register("PowerToysNewAvailableVersion", typeof(string), typeof(SoftwareVersionControl), null);
        public static readonly DependencyProperty PowerToysNewAvailableVersionLinkProperty = DependencyProperty.Register("PowerToysNewAvailableVersionLink", typeof(string), typeof(SoftwareVersionControl), null);
        public static readonly DependencyProperty IsUpdatePanelVisibleProperty = DependencyProperty.Register("IsUpdatePanelVisible", typeof(bool), typeof(SoftwareVersionControl), null);
        public static readonly DependencyProperty IsNewVersionDownloadingProperty = DependencyProperty.Register("IsNewVersionDownloading", typeof(bool), typeof(SoftwareVersionControl), null);
        public static readonly DependencyProperty IsNewVersionCheckedAndUpToDateProperty = DependencyProperty.Register("IsNewVersionCheckedAndUpToDate", typeof(bool), typeof(SoftwareVersionControl), null);
        public static readonly DependencyProperty IsNoNetworkProperty = DependencyProperty.Register("IsNoNetwork", typeof(bool), typeof(SoftwareVersionControl), null);
        public static readonly DependencyProperty IsDownloadAllowedProperty = DependencyProperty.Register("IsDownloadAllowed", typeof(bool), typeof(SoftwareVersionControl), null);
        public static readonly DependencyProperty IsAutoDownloadUpdatesCardEnabledProperty = DependencyProperty.Register("IsAutoDownloadUpdatesCardEnabled", typeof(bool), typeof(SoftwareVersionControl), null);
        public static readonly DependencyProperty IsAdminProperty = DependencyProperty.Register("IsAdmin", typeof(bool), typeof(SoftwareVersionControl), null);
        public static readonly DependencyProperty AutoDownloadUpdatesProperty = DependencyProperty.Register("AutoDownloadUpdates", typeof(bool), typeof(SoftwareVersionControl), null);
        public static readonly DependencyProperty ShowAutoDownloadUpdatesGpoInformationProperty = DependencyProperty.Register("ShowAutoDownloadUpdatesGpoInformation", typeof(bool), typeof(SoftwareVersionControl), null);
        public static readonly DependencyProperty CheckForUpdatesEventHandlerProperty = DependencyProperty.Register("CheckForUpdatesEventHandler", typeof(ICommand), typeof(SoftwareVersionControl), null);
        public static readonly DependencyProperty UpdateNowButtonEventHandlerProperty = DependencyProperty.Register("UpdateNowButtonEventHandler", typeof(ICommand), typeof(SoftwareVersionControl), null);
        public static readonly DependencyProperty PowerToysUpdatingStateProperty = DependencyProperty.Register("PowerToysUpdatingState", typeof(UpdatingSettings.UpdatingState), typeof(SoftwareVersionControl), null);

        public SoftwareVersionControl()
        {
            this.InitializeComponent();
        }

        public string UpdateCheckedDate
        {
            get
            {
                return (string)GetValue(UpdateCheckedDateProperty);
            }

            set
            {
                SetValue(UpdateCheckedDateProperty, value);
                UpdateDate.Text = value;
            }
        }

        public string PowerToysVersion
        {
            get
            {
                return (string)GetValue(PowerToysVersionProperty);
            }

            set
            {
                SetValue(PowerToysVersionProperty, value);
                SettingsCard.Header = value;
            }
        }

        public string PowerToysNewAvailableVersion
        {
            get
            {
                return (string)GetValue(PowerToysNewAvailableVersionProperty);
            }

            set
            {
                SetValue(PowerToysNewAvailableVersionProperty, value);
                NewVersionAvailableInfoBar.Message = value;
                NewVersionReadyToInstallInfoBar.Message = value;
                FailedToDownloadInfoBar.Message = value;
            }
        }

        public string PowerToysNewAvailableVersionLink
        {
            get
            {
                return (string)GetValue(PowerToysNewAvailableVersionLinkProperty);
            }

            set
            {
                SetValue(PowerToysNewAvailableVersionLinkProperty, value);
                SeeWhatsNewHyperlink.NavigateUri = new Uri(value);
                NewVersionReadyToInstallHyperlinkButton.NavigateUri = new Uri(value);
                TryAgainyperlinkButton.NavigateUri = new Uri(value);
            }
        }

        public bool IsUpdatePanelVisible
        {
            get
            {
                return (bool)GetValue(IsUpdatePanelVisibleProperty);
            }

            set
            {
                SetValue(IsUpdatePanelVisibleProperty, value);
                UpdatePanel.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        public bool IsNewVersionDownloading
        {
            get
            {
                return (bool)GetValue(IsNewVersionDownloadingProperty);
            }

            set
            {
                SetValue(IsNewVersionDownloadingProperty, value);
                UpdateStackPanel.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
                UpdateButton.Visibility = value ? Visibility.Collapsed : Visibility.Visible;
                DownloadAndInstallButton.Visibility = value ? Visibility.Collapsed : Visibility.Visible;
                DownloadAndInstallStackPanel.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
                TryAgainButton.Visibility = value ? Visibility.Collapsed : Visibility.Visible;
                TryAgainStackPanel.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        public bool IsNewVersionCheckedAndUpToDate
        {
            get
            {
                return (bool)GetValue(IsNewVersionCheckedAndUpToDateProperty);
            }

            set
            {
                SetValue(IsNewVersionCheckedAndUpToDateProperty, value);
                UpToDateInfoBar.IsOpen = value;
                UpToDateInfoBar.IsTabStop = value;
            }
        }

        public bool IsNoNetwork
        {
            get
            {
                return (bool)GetValue(IsNoNetworkProperty);
            }

            set
            {
                SetValue(IsNoNetworkProperty, value);
                CantCheckInfoBar.IsOpen = value;
                CantCheckInfoBar.IsTabStop = value;
            }
        }

        public bool IsDownloadAllowed
        {
            get
            {
                return (bool)GetValue(IsDownloadAllowedProperty);
            }

            set
            {
                SetValue(IsDownloadAllowedProperty, value);
                UpdateButton.IsEnabled = value;
                DownloadAndInstallButton.IsEnabled = value;
                NewVersionReadyToInstallButton.IsEnabled = value;
                TryAgainButton.IsEnabled = value;
            }
        }

        public bool IsAutoDownloadUpdatesCardEnabled
        {
            get
            {
                return (bool)GetValue(IsAutoDownloadUpdatesCardEnabledProperty);
            }

            set
            {
                SetValue(IsAutoDownloadUpdatesCardEnabledProperty, value);
                ToggleSwitchSettingsCard.IsEnabled = value;
            }
        }

        public bool IsAdmin
        {
            get
            {
                return (bool)GetValue(IsAdminProperty);
            }

            set
            {
                SetValue(IsAdminProperty, value);
                ToggleSwitchSettingsCard.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        public bool AutoDownloadUpdates
        {
            get
            {
                return (bool)GetValue(AutoDownloadUpdatesProperty);
            }

            set
            {
                SetValue(AutoDownloadUpdatesProperty, value);
                AutoDownloadToggleSwitch.IsOn = value;
            }
        }

        public bool ShowAutoDownloadUpdatesGpoInformation
        {
            get
            {
                return (bool)GetValue(ShowAutoDownloadUpdatesGpoInformationProperty);
            }

            set
            {
                SetValue(ShowAutoDownloadUpdatesGpoInformationProperty, value);
                GPO_AutoDownloadInfoBar.IsOpen = value;
                GPO_AutoDownloadInfoBar.IsTabStop = value;
            }
        }

        public ICommand CheckForUpdatesEventHandler
        {
            get
            {
                return (ICommand)GetValue(CheckForUpdatesEventHandlerProperty);
            }

            set
            {
                SetValue(CheckForUpdatesEventHandlerProperty, value);
                UpdateButton.Command = value;
            }
        }

        public ICommand UpdateNowButtonEventHandler
        {
            get
            {
                return (ICommand)GetValue(UpdateNowButtonEventHandlerProperty);
            }

            set
            {
                SetValue(UpdateNowButtonEventHandlerProperty, value);
                DownloadAndInstallButton.Command = value;
                NewVersionReadyToInstallButton.Command = value;
                TryAgainButton.Command = value;
            }
        }

        public UpdatingSettings.UpdatingState PowerToysUpdatingState
        {
            get
            {
                return (UpdatingSettings.UpdatingState)GetValue(PowerToysUpdatingStateProperty);
            }

            set
            {
                SetValue(PowerToysUpdatingStateProperty, value);
                NewVersionAvailableInfoBar.IsOpen = value == UpdatingSettings.UpdatingState.ReadyToDownload;
                NewVersionAvailableInfoBar.IsTabStop = value == UpdatingSettings.UpdatingState.ReadyToDownload;
                NewVersionReadyToInstallInfoBar.IsOpen = value == UpdatingSettings.UpdatingState.ReadyToDownload;
                NewVersionReadyToInstallInfoBar.IsTabStop = value == UpdatingSettings.UpdatingState.ReadyToDownload;
                FailedToDownloadInfoBar.IsOpen = value == UpdatingSettings.UpdatingState.ErrorDownloading;
                FailedToDownloadInfoBar.IsTabStop = value == UpdatingSettings.UpdatingState.ErrorDownloading;
            }
        }
    }
}
