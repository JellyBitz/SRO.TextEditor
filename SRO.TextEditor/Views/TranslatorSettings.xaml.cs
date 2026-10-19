using HandyControl.Controls;

namespace SRO.TextEditor.Views
{
    /// <summary>
    /// Lógica de interacción para TranslatorSettings.xaml
    /// </summary>
    public partial class TranslatorSettings : Window
    {
        public TranslatorSettings()
        {
            InitializeComponent();
        }
        private void OnAccept(object sender, System.Windows.RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }
        private void OnCancel(object sender, System.Windows.RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
