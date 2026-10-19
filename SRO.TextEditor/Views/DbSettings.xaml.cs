using HandyControl.Controls;

namespace SRO.TextEditor.Views
{
    /// <summary>
    /// Lógica de interacción para DbSettings.xaml
    /// </summary>
    public partial class DbSettings : Window
    {
        public DbSettings()
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
