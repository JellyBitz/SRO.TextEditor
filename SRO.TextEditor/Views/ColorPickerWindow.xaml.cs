using HandyControl.Controls;

namespace SRO.TextEditor.Views
{
    /// <summary>
    /// Lógica de interacción para ColorPickerWindow.xaml
    /// </summary>
    public partial class ColorPickerWindow : Window
    {
        public ColorPickerWindow()
        {
            InitializeComponent();
        }
        private void OnAccept(object sender, HandyControl.Data.FunctionEventArgs<System.Windows.Media.Color> e)
        {
            DialogResult = true;
            Close();
        }
        private void OnCancel(object sender, System.EventArgs e)
        {

            DialogResult = false;
            Close();
        }
    }
}
