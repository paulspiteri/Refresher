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

        public MainPage()
        {
            InitializeComponent();
            NavButton.Click += NavButton_Click;
            GoButton.Click += GoButton_OnClick;
            NewWindowButton.Click += NewWindowButton_Click;

            UrlTextBox.Text = "";
            KeywordTextBox.Text = "holding";
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

        private void NavButton_Click(object sender, RoutedEventArgs e)
        {
            WebBrowserControl.Source = new Uri(UrlTextBox.Text);
        }

        private void GoButton_OnClick(object sender, RoutedEventArgs e)
        {
            GoButton.IsEnabled = false;
            WebBrowserControl.DOMContentLoaded += WebBrowserControl_DOMContentLoaded;
            if (WebBrowserControl.Source != null)
            {
                Reload();
            }
        }

        private async void WebBrowserControl_DOMContentLoaded(WebView sender, WebViewDOMContentLoadedEventArgs args)
        {
            var htmlText = await sender.InvokeScriptAsync("eval", new[] { "document.documentElement.outerHTML;" });
            if (string.IsNullOrWhiteSpace(htmlText))
            {
                Reload();
                return;
            }

            if (htmlText.IndexOf(KeywordTextBox.Text, StringComparison.OrdinalIgnoreCase) >= 0)
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
            WebBrowserControl.DOMContentLoaded -= WebBrowserControl_DOMContentLoaded;
            GoButton.IsEnabled = true;
        }

        private void Reload()
        {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                () =>
                {
                    WebBrowserControl.Refresh();
                    _reloadCounter++;
                    RefreshCountTextBlock.Text = _reloadCounter.ToString();
                });
        }
    }
}
