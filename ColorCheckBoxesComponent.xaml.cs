using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using SystemDrawingColor = System.Drawing.Color;
using WinUIColor = Windows.UI.Color;

namespace NamedMetaTagManager
{
    public sealed partial class ColorCheckBoxesComponent : UserControl, IColorCheckBoxesComponent
    {
        private readonly List<(SystemDrawingColor Color, CheckBox CheckBox)> _checkboxes = new();

        public ColorCheckBoxesComponent()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Optional event to subscribe to checkbox state changes.
        /// </summary>
        public event Action<SystemDrawingColor, bool>? CheckboxStateChanged;

        public void Init(List<SystemDrawingColor> colors, List<bool> isChecked)
        {
            if (colors == null)
            {
                throw new ArgumentNullException(nameof(colors));
            }

            if (isChecked == null)
            {
                throw new ArgumentNullException(nameof(isChecked));
            }

            if (colors.Count != isChecked.Count)
            {
                throw new ArgumentException("Colors and isChecked must have the same length.");
            }

            CheckboxPanel.Children.Clear();
            _checkboxes.Clear();

            for (var i = 0; i < colors.Count; i++)
            {
                AddCheckbox(colors[i], isChecked[i]);
            }
        }

        public void Update(List<SystemDrawingColor> colors, List<bool> isChecked)
        {
            Init(colors, isChecked);
        }

        public List<SystemDrawingColor> GetCheckedColors()
        {
            return _checkboxes
                .Where(pair => pair.CheckBox.IsChecked == true)
                .Select(pair => pair.Color)
                .ToList();
        }

        public Task<Action<SystemDrawingColor, bool>> CheckboxChanged()
        {
            return Task.FromResult<Action<SystemDrawingColor, bool>>(NotifyCheckboxChanged);
        }

        private void AddCheckbox(SystemDrawingColor color, bool isChecked)
        {
            var checkBox = new CheckBox
            {
                IsChecked = isChecked,
                Tag = color,
                VerticalAlignment = VerticalAlignment.Center,
                Background = new SolidColorBrush(ToWinUIColor(color)),
                BorderBrush = new SolidColorBrush(WinUIColor.FromArgb(255, 200, 200, 200)),
                Style = (Style)Resources["ColorCheckboxStyle"]
            };

            checkBox.Checked += OnCheckboxToggled;
            checkBox.Unchecked += OnCheckboxToggled;

            _checkboxes.Add((color, checkBox));
            CheckboxPanel.Children.Add(checkBox);
        }

        private void OnCheckboxToggled(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox checkBox && checkBox.Tag is SystemDrawingColor color)
            {
                var isChecked = checkBox.IsChecked == true;
                NotifyCheckboxChanged(color, isChecked);
            }
        }

        private void NotifyCheckboxChanged(SystemDrawingColor color, bool isChecked)
        {
            CheckboxStateChanged?.Invoke(color, isChecked);
        }

        private static WinUIColor ToWinUIColor(SystemDrawingColor color)
        {
            return WinUIColor.FromArgb(color.A, color.R, color.G, color.B);
        }
    }
}
