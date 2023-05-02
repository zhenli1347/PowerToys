// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Threading;
using interop;
using ManagedCommon;
using Microsoft.UI.Dispatching;
using WinRT;
using WinUIEx;

namespace ArchivePreviewHandler
{
    public static class Program
    {
        private static CancellationTokenSource _tokenSource = new CancellationTokenSource();
        private static MainWindow _window;

        [STAThread]
        public static void Main(string[] args)
        {
            Logger.InitializeLogger("\\FileExplorer_localLow\\Archive\\logs", true);

            if (args != null && args.Length == 2)
            {
                ComWrappersSupport.InitializeComWrappers();

                string filePath = args[0];
                int hwnd = Convert.ToInt32(args[1], 16);

                Microsoft.UI.Xaml.Application.Start((p) =>
                {
                    var dispatcher = DispatcherQueue.GetForCurrentThread();
                    var context = new DispatcherQueueSynchronizationContext(dispatcher);
                    SynchronizationContext.SetSynchronizationContext(context);

                    _ = new App();

                    _window = new MainWindow();
                    _window.SetParent((IntPtr)hwnd);
                    _window.DoPreview(filePath);
                    _window.Show();

                    new Thread(() =>
                    {
                        var eventHandle = new EventWaitHandle(false, EventResetMode.ManualReset, Constants.ArchivePreviewResizeEvent());
                        while (true)
                        {
                            if (WaitHandle.WaitAny(new WaitHandle[] { _tokenSource.Token.WaitHandle, eventHandle }) == 1)
                            {
                                dispatcher.TryEnqueue(() =>
                                {
                                    _window.Resize();
                                });
                            }
                            else
                            {
                                return;
                            }
                        }
                    }).Start();
                });
            }
        }
    }
}
