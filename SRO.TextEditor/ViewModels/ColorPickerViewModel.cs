using System.Windows.Media;

namespace SRO.ViewModels
{
    public class ColorPickerViewModel : BaseViewModel
    {
        private Color mSelectedColor;
        public Color SelectedColor { get => mSelectedColor; set => SetProperty(ref mSelectedColor, value); }

    }
}
