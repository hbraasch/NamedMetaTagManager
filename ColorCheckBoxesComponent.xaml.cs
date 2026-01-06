using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using WinUIColor = Windows.UI.Color;

namespace NamedMetaTagManager
{
    public sealed partial class ColorCheckBoxesComponent : UserControl, IColorCheckBoxesComponent
    {
        private readonly List<(WinUIColor Color, CheckBox CheckBox)> _checkboxes = new();

        public ColorCheckBoxesComponent()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Optional event to subscribe to checkbox state changes.
        /// </summary>
        public event Action<WinUIColor, bool>? CheckboxStateChanged;

        public void Init(List<WinUIColor> colors, List<bool> isChecked)
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

        public void Update(List<WinUIColor> colors, List<bool> isChecked)
        {
            Init(colors, isChecked);
        }

        public List<WinUIColor> GetCheckedColors()
        {
            return _checkboxes
                .Where(pair => pair.CheckBox.IsChecked == true)
                .Select(pair => pair.Color)
                .ToList();
        }

        public Task<Action<WinUIColor, bool>> CheckboxChanged()
        {
            return Task.FromResult<Action<WinUIColor, bool>>(NotifyCheckboxChanged);
        }

        public (List<WinUIColor> Colors, List<bool> IsChecked) GetCurrentState()
        {
            var colors = new List<WinUIColor>(_checkboxes.Count);
            var states = new List<bool>(_checkboxes.Count);

            foreach (var (color, checkBox) in _checkboxes)
            {
                colors.Add(color);
                states.Add(checkBox.IsChecked == true);
            }

            return (colors, states);
        }

        private void AddCheckbox(WinUIColor color, bool isChecked)
        {
            var checkBox = new CheckBox
            {
                IsChecked = isChecked,
                Tag = color,
                VerticalAlignment = VerticalAlignment.Center,
                Background = new SolidColorBrush(color),
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
            if (sender is CheckBox checkBox && checkBox.Tag is WinUIColor color)
            {
                var isChecked = checkBox.IsChecked == true;
                NotifyCheckboxChanged(color, isChecked);
            }
        }

        private void NotifyCheckboxChanged(WinUIColor color, bool isChecked)
        {
            CheckboxStateChanged?.Invoke(color, isChecked);
        }
    }
}
