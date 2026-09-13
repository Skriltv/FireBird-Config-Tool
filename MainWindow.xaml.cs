using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using Microsoft.Web.WebView2.Core;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Forms = System.Windows.Forms;

namespace FireBirdConfigTool
{
    public partial class MainWindow : Window
    {
        // Official FireBird Config Tool. This launcher is an unofficial,
        // fan-made shortcut — it just opens this same page in its own window.
        private const string ConfigToolUrl = "https://bzl-web.com/tool/firebird/";
        private Forms.NotifyIcon? _trayIcon;
        private Forms.ContextMenuStrip? _trayMenu;
        private System.IO.Stream? _trayIconStream;
        private bool _exiting;

        private const int WmNcHitTest = 0x0084;
        private const int HtLeft = 10;
        private const int HtRight = 11;
        private const int HtTop = 12;
        private const int HtTopLeft = 13;
        private const int HtTopRight = 14;
        private const int HtBottom = 15;
        private const int HtBottomLeft = 16;
        private const int HtBottomRight = 17;
        private const int ResizeBorder = 10;

        [StructLayout(LayoutKind.Sequential)]
        private struct NativeRect
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        [DllImport("user32.dll")]
        private static extern bool GetWindowRect(IntPtr hWnd, out NativeRect rect);

        public MainWindow()
        {
            InitializeComponent();
            SourceInitialized += MainWindow_SourceInitialized;
            Loaded += MainWindow_Loaded;
            StateChanged += MainWindow_StateChanged;
            Closing += MainWindow_Closing;
            InitializeTrayIcon();
        }

        private void MainWindow_SourceInitialized(object? sender, EventArgs e)
        {
            if (PresentationSource.FromVisual(this) is HwndSource source)
                source.AddHook(WindowMessageHook);
        }

        private IntPtr WindowMessageHook(IntPtr hWnd, int message, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (message != WmNcHitTest || WindowState == WindowState.Maximized || !GetWindowRect(hWnd, out var rect))
                return IntPtr.Zero;

            var point = lParam.ToInt64();
            var x = unchecked((int)(short)(point & 0xFFFF));
            var y = unchecked((int)(short)((point >> 16) & 0xFFFF));
            var left = x <= rect.Left + ResizeBorder;
            var right = x >= rect.Right - ResizeBorder;
            var top = y <= rect.Top + ResizeBorder;
            var bottom = y >= rect.Bottom - ResizeBorder;

            var hitTest = (top, right, bottom, left) switch
            {
                (true, true, false, false) => HtTopRight,
                (true, false, false, true) => HtTopLeft,
                (false, true, true, false) => HtBottomRight,
                (false, false, true, true) => HtBottomLeft,
                (true, false, false, false) => HtTop,
                (false, true, false, false) => HtRight,
                (false, false, true, false) => HtBottom,
                (false, false, false, true) => HtLeft,
                _ => 0
            };

            if (hitTest == 0)
                return IntPtr.Zero;

            handled = true;
            return new IntPtr(hitTest);
        }

        private void InitializeTrayIcon()
        {
            _trayMenu = new Forms.ContextMenuStrip();
            _trayMenu.Items.Add("Show FireBird Config Tool", null, (_, _) => RestoreFromTray());
            _trayMenu.Items.Add(new Forms.ToolStripSeparator());
            _trayMenu.Items.Add("Exit", null, (_, _) => ExitApplication());

            _trayIcon = new Forms.NotifyIcon
            {
                Icon = LoadTrayIcon(),
                Text = "FireBird Config Tool",
                ContextMenuStrip = _trayMenu,
                Visible = false
            };
            _trayIcon.DoubleClick += (_, _) => RestoreFromTray();
        }



        private System.Drawing.Icon LoadTrayIcon()
        {
            // Load the ICO from the WPF resource so the published single-file
            // EXE does not need an external assets folder beside it.
            var resource = System.Windows.Application.GetResourceStream(
                new Uri("pack://application:,,,/assets/icon.ico", UriKind.Absolute));

            if (resource?.Stream == null)
                return System.Drawing.SystemIcons.Application;

            _trayIconStream = new System.IO.MemoryStream();
            resource.Stream.CopyTo(_trayIconStream);
            resource.Stream.Dispose();
            _trayIconStream.Position = 0;
            return new System.Drawing.Icon(_trayIconStream);
        }

        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                ToggleMaximize();
                return;
            }

            if (e.ButtonState == MouseButtonState.Pressed)
            {
                try { DragMove(); } catch (InvalidOperationException) { }
            }
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            MinimizeToTray();
        }

        private void MainWindow_StateChanged(object? sender, EventArgs e)
        {
            if (WindowState == WindowState.Minimized && !_exiting)
            {
                MinimizeToTray();
            }
        }

        private void MinimizeToTray()
        {
            if (_exiting) return;

            WindowState = WindowState.Normal;
            Hide();
            if (_trayIcon != null)
                _trayIcon.Visible = true;
        }

        private void RestoreFromTray()
        {
            if (_exiting) return;

            Show();
            WindowState = WindowState.Normal;
            Activate();
            Topmost = true;
            Topmost = false;
            if (_trayIcon != null)
                _trayIcon.Visible = false;
        }

        private void MaximizeButton_Click(object sender, RoutedEventArgs e)
        {
            ToggleMaximize();
        }

        private void ToggleMaximize()
        {
            WindowState = WindowState == WindowState.Maximized
                ? WindowState.Normal
                : WindowState.Maximized;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            ExitApplication();
        }

        private void MainWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            if (_exiting)
            {
                _trayIcon?.Dispose();
                _trayMenu?.Dispose();
                _trayIconStream?.Dispose();
                return;
            }

            _trayIcon?.Dispose();
            _trayMenu?.Dispose();
            _trayIconStream?.Dispose();
        }

        private void ExitApplication()
        {
            if (_exiting) return;
            _exiting = true;
            _trayIcon?.Dispose();
            _trayMenu?.Dispose();
            _trayIconStream?.Dispose();
            System.Windows.Application.Current.Shutdown();
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                // Store WebView2's browser data under %LocalAppData% instead of
                // next to the .exe — folders like Downloads or Program Files can
                // silently refuse to let WebView2 create its data folder there,
                // which otherwise fails init with no visible error at all.
                var userDataFolder = System.IO.Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "FireBirdConfigTool", "WebView2");

                var environment = await CoreWebView2Environment.CreateAsync(
                    browserExecutableFolder: null,
                    userDataFolder: userDataFolder);

                await webView.EnsureCoreWebView2Async(environment);
            }
            catch (WebView2RuntimeNotFoundException)
            {
                StatusText.Text = "Microsoft Edge WebView2 Runtime is not installed.";
                var result = System.Windows.MessageBox.Show(
                    "Microsoft Edge WebView2 Runtime is required to run FireBird Config Tool.\n\n" +
                    "Would you like to open the official Microsoft download page?",
                    "WebView2 Required", System.Windows.MessageBoxButton.YesNo, System.Windows.MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        Process.Start(new ProcessStartInfo
                        {
                            FileName = "https://developer.microsoft.com/microsoft-edge/webview2/",
                            UseShellExecute = true
                        });
                    }
                    catch { }
                }
                return;
            }
            catch (Exception ex)
            {
                StatusText.Text = $"Couldn't start the embedded browser:\n{ex.Message}";
                System.Windows.MessageBox.Show(
                    $"Couldn't start the embedded browser component:\n\n{ex}",
                    "FireBird Config Launcher — Error",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Error);
                return;
            }

            var core = webView.CoreWebView2;

            // The config tool talks to the FireBird board over WebUSB.
            // WebView2 uses the same Chromium engine as Edge, so the native
            // device-picker dialog should appear automatically when the page
            // calls navigator.usb.requestDevice() — WebUSB isn't one of the
            // permission kinds routed through PermissionRequested, so there's
            // nothing extra to wire up here.

            core.NavigationCompleted += (_, args) =>
            {
                StatusText.Visibility = Visibility.Collapsed;
            };

            core.DocumentTitleChanged += (_, __) =>
            {
                // Keep our own title instead of following the page's <title>
                Title = "FireBird Config Tool";
            };

            core.Navigate(ConfigToolUrl);
        }
    }
}
