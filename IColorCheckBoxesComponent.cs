using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NamedMetaTagManager
{
    internal interface IColorCheckBoxesComponent
    {
        /// <summary>
        /// Used to set amount of checkboxes, their colours and their checked status
        /// </summary>
        /// <param name="colors"></param>
        /// <param name="isChecked"></param>
        public void Init(List<Color> colors, List<bool> isChecked);
        /// <summary>
        /// Used to update amount of checkboxes, their colours and their checked status
        /// </summary>
        /// <param name="colors"></param>
        /// <param name="isChecked"></param>
        public void Update(List<Color> colors, List<bool> isChecked);
        /// <summary>
        /// Used to determine which checkboxes are checked
        /// </summary>
        /// <returns></returns>
        public List<Color> GetCheckedColors();
        /// <summary>
        /// Callback which triggers when one of the checkboxes change
        /// </summary>
        /// <returns>
        ///     Color: To indicate which checkbox changed
        ///     bool: To indicate the final checked state of the checkbox
        /// </returns>
        public Task<Action<Color, bool>> CheckboxChanged();
    }
}
