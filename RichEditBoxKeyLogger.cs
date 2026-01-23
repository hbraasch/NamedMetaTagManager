using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.UI.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace CodexNamedMetaTagManager
{
    public static class RichEditBoxKeyLogger
    {
        private sealed class LoggerState
        {
            public LoggerState(string key, DispatcherTimer timer, EventHandler<object> tickHandler)
            {
                Key = key;
                Timer = timer;
                TickHandler = tickHandler;
            }

            public string Key { get; }
            public DispatcherTimer Timer { get; }
            public EventHandler<object> TickHandler { get; }
        }

        public static readonly DependencyProperty IsEnabledProperty = DependencyProperty.RegisterAttached(
            "IsEnabled",
            typeof(bool),
            typeof(RichEditBoxKeyLogger),
            new PropertyMetadata(false, OnIsEnabledChanged));

        public static readonly DependencyProperty KeyProperty = DependencyProperty.RegisterAttached(
            "Key",
            typeof(string),
            typeof(RichEditBoxKeyLogger),
            new PropertyMetadata(string.Empty));

        private static readonly Dictionary<RichEditBox, LoggerState> States = new();
        private static readonly Dictionary<string, string> Snapshots = new(StringComparer.Ordinal);
        private static readonly TimeSpan Interval = TimeSpan.FromSeconds(10);

        public static bool GetIsEnabled(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsEnabledProperty);
        }

        public static void SetIsEnabled(DependencyObject obj, bool value)
        {
            obj.SetValue(IsEnabledProperty, value);
        }

        public static string GetKey(DependencyObject obj)
        {
            return (string)obj.GetValue(KeyProperty);
        }

        public static void SetKey(DependencyObject obj, string value)
        {
            obj.SetValue(KeyProperty, value);
        }

        public static async Task GetLatestKeystrokes(XamlRoot xamlRoot)
        {
            var combinedText = BuildCombinedText();
            var textBox = new TextBox
            {
                Text = combinedText,
                IsReadOnly = true,
                AcceptsReturn = true,
                TextWrapping = TextWrapping.Wrap,
                MinWidth = 400,
                MinHeight = 300
            };

            var dialog = new ContentDialog
            {
                XamlRoot = xamlRoot,
                Title = "Recovered Drafts",
                Content = textBox,
                CloseButtonText = "Close"
            };

            await dialog.ShowAsync();
        }

        private static void OnIsEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not RichEditBox richEditBox)
            {
                return;
            }

            if ((bool)e.NewValue)
            {
                Attach(richEditBox);
            }
            else
            {
                Detach(richEditBox);
            }
        }

        private static void Attach(RichEditBox richEditBox)
        {
            if (States.ContainsKey(richEditBox))
            {
                return;
            }

            var key = ResolveKey(richEditBox);
            var timer = new DispatcherTimer { Interval = Interval };
            EventHandler<object> tickHandler = (_, _) => CaptureSnapshot(richEditBox, key);
            timer.Tick += tickHandler;
            timer.Start();

            richEditBox.Unloaded += OnRichEditBoxUnloaded;
            States[richEditBox] = new LoggerState(key, timer, tickHandler);

            CaptureSnapshot(richEditBox, key);
        }

        private static void Detach(RichEditBox richEditBox)
        {
            if (!States.TryGetValue(richEditBox, out var state))
            {
                return;
            }

            state.Timer.Stop();
            state.Timer.Tick -= state.TickHandler;
            richEditBox.Unloaded -= OnRichEditBoxUnloaded;
            States.Remove(richEditBox);
        }

        private static void OnRichEditBoxUnloaded(object sender, RoutedEventArgs e)
        {
            if (sender is RichEditBox richEditBox)
            {
                Detach(richEditBox);
            }
        }

        private static string ResolveKey(RichEditBox richEditBox)
        {
            var key = GetKey(richEditBox);
            if (!string.IsNullOrWhiteSpace(key))
            {
                return key;
            }

            if (!string.IsNullOrWhiteSpace(richEditBox.Name))
            {
                return richEditBox.Name;
            }

            key = Guid.NewGuid().ToString("N");
            SetKey(richEditBox, key);
            return key;
        }

        private static void CaptureSnapshot(RichEditBox richEditBox, string key)
        {
            richEditBox.Document.GetText(TextGetOptions.None, out var text);
            Snapshots[key] = text;
        }

        private static string BuildCombinedText()
        {
            if (Snapshots.Count == 0)
            {
                return "No drafts captured yet.";
            }

            var sections = Snapshots
                .OrderBy(pair => pair.Key, StringComparer.Ordinal)
                .Select(pair => $"=== {pair.Key} ==={Environment.NewLine}{pair.Value}");

            return string.Join(Environment.NewLine + Environment.NewLine, sections);
        }
    }
}
