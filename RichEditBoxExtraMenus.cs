using System;
using System.Linq;
using Microsoft.UI.Xaml.Controls;

namespace RichEditBoxExtraMenus
{
    public static class RichEditBoxExtraMenuLibrary
    {
        public static void AttachSendToAiForRefinementMenu(RichEditBox editor, Action onSendToAiForRefinement)
        {
            if (editor is null)
            {
                throw new ArgumentNullException(nameof(editor));
            }

            if (onSendToAiForRefinement is null)
            {
                throw new ArgumentNullException(nameof(onSendToAiForRefinement));
            }

            const string sendToAiLabel = "Send to AI";
            void EnsureCommand(TextCommandBarFlyout flyout)
            {
                var alreadyPresent = flyout.PrimaryCommands
                    .OfType<AppBarButton>()
                    .Any(button => string.Equals(button.Label, sendToAiLabel, StringComparison.Ordinal));

                if (alreadyPresent)
                {
                    return;
                }

                var sendToAiButton = new AppBarButton
                {
                    Label = sendToAiLabel,
                    Icon = new SymbolIcon(Symbol.Message)
                };

                sendToAiButton.Click += (_, __) => onSendToAiForRefinement();
                flyout.PrimaryCommands.Add(sendToAiButton);
            }

            void AttachOpeningHandler(TextCommandBarFlyout flyout)
            {
                flyout.Opening += (_, __) => EnsureCommand(flyout);
            }

            if (editor.SelectionFlyout is TextCommandBarFlyout selectionFlyout)
            {
                AttachOpeningHandler(selectionFlyout);
            }
            else
            {
                var selectionCommandBar = new TextCommandBarFlyout();
                AttachOpeningHandler(selectionCommandBar);
                editor.SelectionFlyout = selectionCommandBar;
            }

            if (editor.ContextFlyout is TextCommandBarFlyout contextFlyout)
            {
                AttachOpeningHandler(contextFlyout);
            }
            else
            {
                var contextCommandBar = new TextCommandBarFlyout();
                AttachOpeningHandler(contextCommandBar);
                editor.ContextFlyout = contextCommandBar;
            }
        }
    }
}
