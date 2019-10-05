using System;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Refresher10
{
    public sealed partial class MainPage
    {
        private int _reloadCounter;

        private const int MAX_TIMEOUT_MS = 2000;
        private readonly DispatcherTimer _countdownTimer = new DispatcherTimer()
        {
            Interval = new TimeSpan(0, 0, 0, 0, MAX_TIMEOUT_MS)
        };

        public MainPage()
        {
            InitializeComponent();
            GoButton.Click += GoButton_OnClick;
            NewWindowButton.Click += NewWindowButton_Click;

            UrlTextBox.Text = "";
            KeywordTextBox.Text = "free space";

            WebBrowserControl.DOMContentLoaded += WebBrowserControl_DOMContentLoaded;
            _countdownTimer.Tick += OnTimeoutElapsed;
        }

        private async void NewWindowButton_Click(object sender, RoutedEventArgs e)
        {
            ApplicationView newAppView = null;
            CoreApplicationView newCoreView = CoreApplication.CreateNewView();

            await newCoreView.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                () =>
                    {
                        newAppView = ApplicationView.GetForCurrentView();
                        Window.Current.Content = new MainPage();
                        Window.Current.Activate();
                    });

            await ApplicationViewSwitcher.TryShowAsStandaloneAsync(newAppView.Id);
        }

        private void GoButton_OnClick(object sender, RoutedEventArgs e)
        {
            GoButton.IsEnabled = false;
            Reload();
        }

        private void OnTimeoutElapsed(object sender, object e)
        {
            WebBrowserControl.Stop();
            _countdownTimer.Stop();

            Reload();
        }

        private async void WebBrowserControl_DOMContentLoaded(WebView sender, WebViewDOMContentLoadedEventArgs args)
        {
            _countdownTimer.Stop();

            var htmlText = await sender.InvokeScriptAsync("eval", new[] { "document.documentElement.outerHTML;" });
            if (string.IsNullOrWhiteSpace(htmlText))
            {
                Reload();
            }
            else if (htmlText.IndexOf(KeywordTextBox.Text, StringComparison.OrdinalIgnoreCase) >= 0)
            {
                Reload();
            }
            else
            {
                Stop();
            }
        }

        private void Stop()
        {
            GoButton.IsEnabled = true;
        }

        private void Reload()
        {
            _countdownTimer.Start();

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                () =>
                {
                    WebBrowserControl.Navigate(new Uri(UrlTextBox.Text));
                    _reloadCounter++;
                    RefreshCountTextBlock.Text = _reloadCounter.ToString();
                });
        }
    }
}
