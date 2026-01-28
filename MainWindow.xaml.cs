using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.UI;
using WinUIColor = Windows.UI.Color;

namespace CodexNamedMetaTagManager
{
    public sealed partial class MainWindow : Window
    {
        private readonly NamedMetaTagManager _manager = new();

        public MainWindow()
        {
            InitializeComponent();
            SeedEditor();
            InitializeColorCheckboxes();
        }

        private void SeedEditor()
        {
            const string sample = """
Intro text before tags.
<note/> This paragraph includes a closed note tag.
Here is an encapsulated tag: <summary>This is wrapped content.</summary>
Here is a nested example: <important>Keep <childOne>child one</childOne> and <childTwo>child two</childTwo> safe</important>.
""";
            Editor.Document.SetText(Microsoft.UI.Text.TextSetOptions.None, sample);
            Editor.SetSaveAction((currrentRtfText) => { 
                // Save
            
            }, 10000);
        }

        private string CurrentName => string.IsNullOrWhiteSpace(TagNameInput.Text) ? "sample" : TagNameInput.Text.Trim();

        private void OnAddTagClicked(object sender, RoutedEventArgs e)
        {
            try
            {
                _manager.AddNamedTagToEditor(Editor, CurrentName);
                SetStatus($"Added tag '{CurrentName}'.");
            }
            catch (Exception ex)
            {
                SetStatus($"Add failed: {ex.Message}");
            }
        }

        private void OnRemoveTagClicked(object sender, RoutedEventArgs e)
        {
            var removed = _manager.RemoveNamedTagFromEditor(Editor, CurrentName);
            SetStatus(removed ? $"Removed tag '{CurrentName}'." : $"No tag '{CurrentName}' found to remove.");
        }

        private void OnHideTagClicked(object sender, RoutedEventArgs e)
        {
            var hidden = _manager.HideNamedTagInEditor(Editor, CurrentName, true);
            SetStatus(hidden ? $"Hid tag '{CurrentName}'." : $"No tag '{CurrentName}' hidden.");
        }

        private void OnShowTagClicked(object sender, RoutedEventArgs e)
        {
            var shown = _manager.HideNamedTagInEditor(Editor, CurrentName, false);
            SetStatus(shown ? $"Restored tag '{CurrentName}'." : $"No hidden tag '{CurrentName}' to restore.");
        }

        private void OnHiliteTagClicked(object sender, RoutedEventArgs e)
        {
            var hilited = _manager.HiliteNamedTagInEditor(Editor, CurrentName, true, Colors.Yellow);
            SetStatus(hilited ? $"Highlighted tag '{CurrentName}'." : $"No tag '{CurrentName}' to highlight.");
        }

        private void OnClearHiliteTagClicked(object sender, RoutedEventArgs e)
        {
            var cleared = _manager.HiliteNamedTagInEditor(Editor, CurrentName, false, Colors.Transparent);
            SetStatus(cleared ? $"Cleared highlight for '{CurrentName}'." : $"No tag '{CurrentName}' highlight to clear.");
        }

        private void OnListTagsClicked(object sender, RoutedEventArgs e)
        {
            var tags = _manager.GetNamedTagsInEditor(Editor);
            SetStatus(tags.Any() ? $"Tags: {string.Join(", ", tags)}" : "No tags found.");
        }

        private void OnListTagsWithDetailsClicked(object sender, RoutedEventArgs e)
        {
            var tags = _manager.GetNamedTagsInEditorWithDetails(Editor);
            if (!tags.Any())
            {
                SetStatus("No tags found.");
                return;
            }

            var formatted = tags.Select(tag => $"{tag.Name} ({(tag.IsEncapsulating ? "encapsulating" : "closed")})");
            SetStatus($"Tags: {string.Join(", ", formatted)}");
        }

        private void OnCheckTagClicked(object sender, RoutedEventArgs e)
        {
            var present = _manager.IsNamedTagPresentInEditor(Editor, CurrentName);
            SetStatus(present ? $"Tag '{CurrentName}' is present." : $"Tag '{CurrentName}' is not present.");
        }

        private void OnGetContentClicked(object sender, RoutedEventArgs e)
        {
            var content = _manager.GetNamedTagContentFromEditor(Editor, CurrentName);
            SetStatus($"Content for '{CurrentName}': {content}");
        }

        private void InitializeColorCheckboxes()
        {
            var sampleColors = new List<WinUIColor>
            {
                Colors.Red,
                Colors.Green,
                Colors.Blue,
                Colors.Goldenrod
            };
            var sampleChecked = new List<bool> { true, false, true, false };
            ColorCheckboxes.Init(sampleColors, sampleChecked);
            ColorCheckboxes.CheckboxStateChanged += OnColorCheckboxStateChanged;
        }

        private void OnColorCheckboxStateChanged(WinUIColor color, bool isChecked)
        {
            SetStatus($"Color '{DescribeColor(color)}' checkbox is {(isChecked ? "checked" : "unchecked")}.");
        }

        private void SetStatus(string message)
        {
            StatusText.Text = message;
        }

        private void OnAddColourCheckBoxClicked(object sender, RoutedEventArgs e)
        {
            var (colors, isChecked) = ColorCheckboxes.GetCurrentState();

            var nextColor = WinUIColor.FromArgb(
                255,
                (byte)((60 * colors.Count + 40) % 256),
                (byte)((110 * colors.Count + 90) % 256),
                (byte)((170 * colors.Count + 140) % 256));

            colors.Add(nextColor);
            isChecked.Add(false);
            ColorCheckboxes.Update(colors, isChecked);

            SetStatus($"Added colour checkbox for '{DescribeColor(nextColor)}'.");
        }


        private static string DescribeColor(WinUIColor color)
        {
            return $"#{color.A:X2}{color.R:X2}{color.G:X2}{color.B:X2}";

            
        }


    }
}
