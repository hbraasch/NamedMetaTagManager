using System;
using Microsoft.UI.Xaml;
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

            var flyout = new TextCommandBarFlyout
            {
                IsUndoEnabled = true,
                IsRedoEnabled = true,
                IsCutEnabled = true,
                IsCopyEnabled = true,
                IsPasteEnabled = true,
                IsBoldEnabled = true,
                IsItalicEnabled = true,
                IsUnderlineEnabled = true,
                IsProofingMenuEnabled = true,
                IsTextPredictionEnabled = true
            };

            var sendToAiButton = new AppBarButton
            {
                Label = "Send to AI for refinement",
                Icon = new SymbolIcon(Symbol.Message)
            };

            sendToAiButton.Click += (_, __) => onSendToAiForRefinement();

            flyout.SecondaryCommands.Add(sendToAiButton);
            editor.ContextFlyout = flyout;
        }
    }
}
