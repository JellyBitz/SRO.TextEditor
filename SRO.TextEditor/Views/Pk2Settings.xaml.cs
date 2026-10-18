using HandyControl.Controls;
using Microsoft.Win32;

namespace SRO.TextEditor.Views
{
    /// <summary>
    /// Lógica de interacción para Pk2Settings.xaml
    /// </summary>
    public partial class Pk2Settings : Window
    {
        public Pk2Settings()
        {
            InitializeComponent();
        }

        private void SearchButton_OnClick(object sender, System.Windows.RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Title = "Select Media.pk2 File",
                Filter = "PK2 Files (*.pk2)|Media.pk2|All Files (*.*)|*.*",
                CheckFileExists = true
            };

            if (openFileDialog.ShowDialog() == true)
                tbxMediaPath.Text = openFileDialog.FileName;
        }
    }
}
