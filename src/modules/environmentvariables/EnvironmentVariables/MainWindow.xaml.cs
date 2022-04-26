using EnvironmentVariables.Views;
using Microsoft.UI;           // Needed for WindowId
using Microsoft.UI.Windowing; // Needed for AppWindow
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using WinRT.Interop;          // Needed for XAML/HWND interop.

namespace EnvironmentVariables
{
    public sealed partial class MainWindow : Window
    {

        private AppWindow m_AppWindow;

        public MainWindow()
        {
            this.InitializeComponent();
            m_AppWindow = GetAppWindowForCurrentWindow();
            // Check to see if customization is supported.
            // Currently only supported on Windows 11.
            if (AppWindowTitleBar.IsCustomizationSupported())
            {
                var titleBar = m_AppWindow.TitleBar;
                // Hide default title bar.
                titleBar.ExtendsContentIntoTitleBar = true;
            }
            else
            {
                // Title bar customization using these APIs is currently
                // supported only on Windows 11. In other cases, hide
                // the custom title bar element.
                AppTitleBar.Visibility = Visibility.Collapsed;
            }
        }

        private AppWindow GetAppWindowForCurrentWindow()
        {
            IntPtr hWnd = WindowNative.GetWindowHandle(this);
            WindowId wndId = Win32Interop.GetWindowIdFromWindow(hWnd);
            return AppWindow.GetFromWindowId(wndId);
        }

        private void NavigationView_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
        {
            if (args.IsSettingsInvoked)
            {
                // TO DO: Open Settings
            }
            else
            {
                switch (args.InvokedItem.ToString())
                {
                    case "User": NavFrame.Navigate(typeof(UserPage)); break;
                    case "System": NavFrame.Navigate(typeof(UserPage)); break;
                }
            }
        }

        private void NavigationView_Loaded(object sender, RoutedEventArgs e)
        {
            NavFrame.Navigate(typeof(UserPage));
        }
    }
}
