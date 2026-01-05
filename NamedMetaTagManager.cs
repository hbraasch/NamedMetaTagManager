using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Microsoft.UI;
using Microsoft.UI.Text;
using Microsoft.UI.Xaml.Controls;
using Windows.UI;

namespace NamedMetaTagManager
{
    internal interface NamedMetaTagManager
    {
        /// <summary>
        /// Used to add a named metatag to the editor at the current cursor position. If text is selected, the metatag will wrap the selected text. If no text is selected, the closed metatag will be inserted at the cursor position.
        /// Metatags can be nested within each other.
        ///     The function will throw exception if:
        ///         The selected text to be encapsulated already contains the same named metatag
        ///         The exit tag is inside a child tag, which cannot be closed before the parent tag is closed. Therefore check for invalid boundary placements
        /// The editor can have multiple named tages with the same name, as long as they are not nested within each other.
        /// </summary>
        /// <param name="editor"></param>
        /// <param name="metatagName"></param>
        public void AddNamedTagToEditor(RichEditBox editor, string metatagName);
        /// <summary>
        /// Used to remove a named metatag from the editor by finding the first one by its name. If a encapsulated metatag, the metatag will be removed but the content within the metatag will be preserved. Child metatags will also be preserved.
        /// </summary>
        /// <param name="editor"></param>
        /// <param name="metatagName"></param>
        /// Returns true if a metatag was found and removed, false otherwise.
        public bool RemoveNamedTagFromEditor(RichEditBox editor, string metatagName);
        /// <summary>
        /// Used to hide a named metatag in the editor by finding the first one by its name. If a encapsulated metatag, the metatag tags will be hidden but the content within the metatag will be preserved. Child metatags will remain visible.
        /// </summary>
        /// <param name="editor"></param>
        /// <param name="metatagName"></param>
        /// <param name="isHidden">Set to true to hide the metatag, false to show it.</param>
        /// <returns></returns>
        public bool HideNamedTagInEditor(RichEditBox editor, string metatagName, bool isHidden);
        public bool HiliteNamedTagInEditor(RichEditBox editor, string metatagName, bool isHilited, Color hiliteColor);
        /// <summary>
        /// Used to get a list of all named metatags currently in the editor. Parent as well as child metatags will be included.
        /// </summary>
        /// <param name="editor"></param>
        /// <returns></returns>
        public List<string> GetNamedTagsInEditor(RichEditBox editor);
        public bool IsNamedTagPresentInEditor(RichEditBox editor, string metatagName);
        /// <summary>
        /// Used to get the content within the first occurrence of a named metatag in the editor. If the metatag is a closed tag, an empty string will be returned. If any child metatags are present within the named metatag, they will be removed (only the tags) in the returned content.
        /// </summary>
        /// <param name="editor"></param>
        /// <param name="metatagName"></param>
        /// <returns></returns>
        public string GetNamedTagContentFromEditor(RichEditBox editor, string metatagName);
    }

    internal class NamedMetaTagManagerService : NamedMetaTagManager
    {
        public void AddNamedTagToEditor(RichEditBox editor, string metatagName)
        {
            var selection = editor.Document.Selection;
            selection.GetText(TextGetOptions.None, out var selectedText);
            var trimmedSelection = selectedText?.TrimEnd('\0') ?? string.Empty;
            if (string.IsNullOrWhiteSpace(metatagName))
            {
                throw new ArgumentException("Metatag name must be provided.", nameof(metatagName));
            }

            if (!string.IsNullOrEmpty(trimmedSelection))
            {
                if (ContainsTag(trimmedSelection, metatagName))
                {
                    throw new InvalidOperationException("Selected text already contains the same metatag.");
                }

                selection.SetText(TextSetOptions.None, $"<{metatagName}>{trimmedSelection}</{metatagName}>");
            }
            else
            {
                selection.SetText(TextSetOptions.None, $"<{metatagName}/>");
            }
        }

        public bool RemoveNamedTagFromEditor(RichEditBox editor, string metatagName)
        {
            var text = GetEditorText(editor);
            var (startIndex, endIndex, isSelfClosing) = FindFirstTag(text, metatagName);
            if (startIndex < 0)
            {
                return false;
            }

            string updated;
            if (isSelfClosing)
            {
                updated = text.Remove(startIndex, endIndex - startIndex);
            }
            else
            {
                var startTag = $"<{metatagName}>";
                var endTag = $"</{metatagName}>";
                var contentStart = startIndex + startTag.Length;
                var contentLength = endIndex - contentStart - endTag.Length;
                var innerContent = text.Substring(contentStart, contentLength);
                updated = text.Remove(startIndex, endIndex - startIndex).Insert(startIndex, innerContent);
            }

            SetEditorText(editor, updated);
            return true;
        }

        public bool HideNamedTagInEditor(RichEditBox editor, string metatagName, bool isHidden)
        {
            var text = GetEditorText(editor);

            var (startIndex, endIndex, isSelfClosing) = FindFirstTag(text, metatagName);
            if (startIndex < 0)
            {
                return false;
            }

            if (isSelfClosing)
            {
                ApplyHidden(editor, startIndex, endIndex - startIndex, isHidden);
            }
            else
            {
                ApplyHidden(editor, startIndex, endIndex - startIndex, isHidden);
            }

            return true;
        }

        public bool HiliteNamedTagInEditor(RichEditBox editor, string metatagName, bool isHilited, Color hiliteColor)
        {
            var text = GetEditorText(editor);
            var (startIndex, endIndex, _) = FindFirstTag(text, metatagName);
            if (startIndex < 0)
            {
                return false;
            }

            ApplyHighlight(editor, startIndex, endIndex - startIndex, isHilited ? hiliteColor : Colors.Transparent);
            return true;
        }

        public List<string> GetNamedTagsInEditor(RichEditBox editor)
        {
            var text = GetEditorText(editor);
            var matches = Regex.Matches(text, "<(/?)([A-Za-z0-9_\\-]+)(/?)>");
            var tags = new List<string>();
            foreach (Match match in matches)
            {
                var name = match.Groups[2].Value;
                if (!string.IsNullOrWhiteSpace(name))
                {
                    tags.Add(name);
                }
            }

            return tags;
        }

        public bool IsNamedTagPresentInEditor(RichEditBox editor, string metatagName)
        {
            var text = GetEditorText(editor);
            return text.Contains($"<{metatagName}>", StringComparison.Ordinal) ||
                   text.Contains($"<{metatagName}/>", StringComparison.Ordinal);
        }

        public string GetNamedTagContentFromEditor(RichEditBox editor, string metatagName)
        {
            var text = GetEditorText(editor);
            var (startIndex, endIndex, isSelfClosing) = FindFirstTag(text, metatagName);
            if (startIndex < 0 || isSelfClosing)
            {
                return string.Empty;
            }

            var startTag = $"<{metatagName}>";
            var endTag = $"</{metatagName}>";
            var contentStart = startIndex + startTag.Length;
            var contentLength = endIndex - contentStart - endTag.Length;
            var innerContent = text.Substring(contentStart, contentLength);
            var withoutChildren = Regex.Replace(innerContent, "<[^>]+>", string.Empty);
            return withoutChildren;
        }

        private static bool ContainsTag(string value, string metatagName)
        {
            return value.Contains($"<{metatagName}>", StringComparison.Ordinal) ||
                   value.Contains($"<{metatagName}/>", StringComparison.Ordinal) ||
                   value.Contains($"</{metatagName}>", StringComparison.Ordinal);
        }

        private static string GetEditorText(RichEditBox editor)
        {
            editor.Document.GetText(TextGetOptions.None, out var text);
            return text?.TrimEnd('\0') ?? string.Empty;
        }

        private static void SetEditorText(RichEditBox editor, string text)
        {
            editor.Document.SetText(TextSetOptions.None, text);
        }

        private static (int Start, int End, bool IsSelfClosing) FindFirstTag(string text, string metatagName)
        {
            var selfClosing = $"<{metatagName}/>";
            var startTag = $"<{metatagName}>";
            var endTag = $"</{metatagName}>";

            var selfIndex = text.IndexOf(selfClosing, StringComparison.Ordinal);
            var openIndex = text.IndexOf(startTag, StringComparison.Ordinal);

            if (selfIndex >= 0 && (openIndex < 0 || selfIndex < openIndex))
            {
                return (selfIndex, selfIndex + selfClosing.Length, true);
            }

            if (openIndex >= 0)
            {
                var closeIndex = text.IndexOf(endTag, openIndex + startTag.Length, StringComparison.Ordinal);
                if (closeIndex >= 0)
                {
                    return (openIndex, closeIndex + endTag.Length, false);
                }
            }

            return (-1, -1, false);
        }

        private static void ApplyHighlight(RichEditBox editor, int start, int length, Color color)
        {
            var range = editor.Document.GetRange(start, start + length);
            range.CharacterFormat.BackgroundColor = color;
        }

        private static void ApplyHidden(RichEditBox editor, int start, int length, bool isHidden)
        {
            var range = editor.Document.GetRange(start, start + length);
            range.CharacterFormat.Hidden = isHidden ? FormatEffect.On : FormatEffect.Off;
        }
    }
}
