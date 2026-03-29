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

            const string sendToAiLabel = "Send to AI for refinement";
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

            if (editor.SelectionFlyout is TextCommandBarFlyout selectionFlyout)
            {
                EnsureCommand(selectionFlyout);
            }
            else
            {
                var flyout = new TextCommandBarFlyout();
                EnsureCommand(flyout);
                editor.SelectionFlyout = flyout;
            }

            if (editor.ContextFlyout is TextCommandBarFlyout contextFlyout)
            {
                EnsureCommand(contextFlyout);
            }
        }
    }
}
