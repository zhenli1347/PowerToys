// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using ArchivePreviewHandler.Helpers;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Windows.Graphics;
using WinRT.Interop;

namespace ArchivePreviewHandler
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        private readonly AppWindow _appWindow;
        private IntPtr _parentWindowHandle;

        public MainWindow()
        {
            this.InitializeComponent();

            var hwnd = WindowNative.GetWindowHandle(this);
            var windowId = Win32Interop.GetWindowIdFromWindow(hwnd);
            _appWindow = AppWindow.GetFromWindowId(windowId);
            var presenter = OverlappedPresenter.Create();
            presenter.SetBorderAndTitleBar(false, false);
            presenter.IsResizable = false;
            _appWindow.SetPresenter(presenter);
        }

        public void SetParent(IntPtr hwnd)
        {
            _parentWindowHandle = hwnd;

            // We must set the WS_CHILD style to change the form to a control within the Explorer preview pane
            int windowStyle = NativeMethods.GetWindowLong(WindowNative.GetWindowHandle(this), NativeMethods.GWL_STYLE);
            if ((windowStyle & NativeMethods.WS_CHILD) == 0)
            {
                _ = NativeMethods.SetWindowLong(WindowNative.GetWindowHandle(this), NativeMethods.GWL_STYLE, windowStyle | NativeMethods.WS_CHILD);
            }

            _ = NativeMethods.SetParent(WindowNative.GetWindowHandle(this), hwnd);
            Resize();
        }

        public void Resize()
        {
            RECT s = default;
            if (NativeMethods.GetClientRect(_parentWindowHandle, ref s) && (Bounds.Right != s.Right || Bounds.Bottom != s.Bottom || Bounds.Left != s.Left || Bounds.Top != s.Top))
            {
                _appWindow.MoveAndResize(new RectInt32(s.Left, s.Top, unchecked(s.Right - s.Left), unchecked(s.Bottom - s.Top)));
            }
        }

        public void DoPreview(string filePath)
        {
            mainPage.DoPreview(filePath);
        }
    }
}
