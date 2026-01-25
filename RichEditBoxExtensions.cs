using System;
using System.Runtime.CompilerServices;
using Microsoft.UI.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace CodexNamedMetaTagManager
{
    public static class RichEditBoxExtensions
    {
        private static readonly ConditionalWeakTable<RichEditBox, SaveActionState> SaveActionStates = new();

        public static void SetSaveAction(this RichEditBox editor, Action<string> saveAction, int timeoutMilliseconds)
        {
            if (editor is null)
            {
                throw new ArgumentNullException(nameof(editor));
            }

            if (saveAction is null)
            {
                throw new ArgumentNullException(nameof(saveAction));
            }

            if (timeoutMilliseconds <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(timeoutMilliseconds), "Timeout must be greater than zero.");
            }

            var state = SaveActionStates.GetOrCreateValue(editor);
            state.Initialize(editor);
            state.Update(saveAction, TimeSpan.FromMilliseconds(timeoutMilliseconds));
        }

        private sealed class SaveActionState
        {
            private RichEditBox? _editor;
            private DispatcherTimer? _timer;
            private Action<string>? _saveAction;
            private string _lastSavedRtf = string.Empty;
            private bool _initialized;

            public void Initialize(RichEditBox editor)
            {
                if (_initialized)
                {
                    return;
                }

                _initialized = true;
                _editor = editor;
                editor.TextChanged += (_, __) =>
                {
                    if (_timer is null)
                    {
                        return;
                    }

                    _timer.Stop();
                    _timer.Start();
                };
                editor.LostFocus += (_, __) => SaveIfChanged();
            }

            public void Update(Action<string> saveAction, TimeSpan timeout)
            {
                if (_editor is null)
                {
                    return;
                }

                _saveAction = saveAction;

                _timer ??= new DispatcherTimer();
                _timer.Interval = timeout;
                _timer.Tick -= OnTimerTick;
                _timer.Tick += OnTimerTick;

                _lastSavedRtf = GetRtf(_editor);
            }

            private void OnTimerTick(object? sender, object e)
            {
                if (_timer is null)
                {
                    return;
                }

                _timer.Stop();
                SaveIfChanged();
            }

            private static string GetRtf(RichEditBox editor)
            {
                editor.Document.GetText(TextGetOptions.FormatRtf, out var text);
                return text?.TrimEnd('\0') ?? string.Empty;
            }

            private void SaveIfChanged()
            {
                if (_editor is null || _saveAction is null)
                {
                    return;
                }

                var currentRtf = GetRtf(_editor);
                if (!string.Equals(currentRtf, _lastSavedRtf, StringComparison.Ordinal))
                {
                    _lastSavedRtf = currentRtf;
                    _saveAction(currentRtf);
                }
            }
        }
    }
}
