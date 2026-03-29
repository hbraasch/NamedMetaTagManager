using System;
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

            var flyout = new TextCommandBarFlyout();

            var sendToAiButton = new AppBarButton
            {
                Label = "Send to AI for refinement",
                Icon = new SymbolIcon(Symbol.Message)
            };

            sendToAiButton.Click += (_, __) => onSendToAiForRefinement();

            flyout.SecondaryCommands.Add(sendToAiButton);

            // RichEditBox uses SelectionFlyout for text editing context actions.
            // Setting ContextFlyout alone can leave the default text flyout in place.
            editor.SelectionFlyout = flyout;
            editor.ContextFlyout = flyout;
        }
    }
}
