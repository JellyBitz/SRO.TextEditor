using GTranslate.Translators;
using Newtonsoft.Json;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SRO.ViewModels
{
    public class SettingsViewModel : BaseViewModel
    {
        #region Private Members
        private string mHost = ".\\VSRO_TESTIN";
        private string mLogin = "admin";
        private string mPassword = "123123123";
        private string mDatabase = "SRO_TEXTDATA";
        private bool mUseEnglishSourceLang;
        private string mMediaPath = string.Empty;
        private string mMediaKey = "169841";
        #endregion

        #region Public Properties
        [JsonIgnore]
        public string Path { get; set; }
        public string Host { get => mHost; set => SetProperty(ref mHost, value); }
        public string Login { get => mLogin; set => SetProperty(ref mLogin, value); }
        public string Password { get => mPassword; set => SetProperty(ref mPassword, value); }
        public string Database { get => mDatabase; set => SetProperty(ref mDatabase, value); }
        public ObservableCollection<string> Translators { get; set; } = new ObservableCollection<string>()
        {
            nameof(GoogleTranslator2),
            nameof(BingTranslator),
            nameof(MicrosoftTranslator),
            nameof(YandexTranslator),
            nameof(GoogleTranslator)
        };
        public bool UseEnglishSourceLang { get => mUseEnglishSourceLang; set => SetProperty(ref mUseEnglishSourceLang, value); }
        public string MediaPath { get => mMediaPath; set => SetProperty(ref mMediaPath, value); }
        public string MediaKey { get => mMediaKey; set => SetProperty(ref mMediaKey, value); }
        #endregion

        #region Commands
        [JsonIgnore]
        public ICommand SaveCommand { get; }
        [JsonIgnore]
        public ICommand CancelCommand { get; }
        [JsonIgnore]
        public ICommand TranslatorMoveUpCommand { get; }
        [JsonIgnore]
        public ICommand TranslatorMoveDownCommand { get; }
        #endregion

        #region Constructor
        /// <summary>
        /// Default constructor.
        /// </summary>
        public SettingsViewModel()
        {
            SaveCommand = new RelayCommand<Window>((window) =>
            {
                window.DialogResult = true;
                window.Close();
            });
            CancelCommand = new RelayCommand<Window>((window) =>
            {
                window.DialogResult = false;
                window.Close();
            });
            TranslatorMoveUpCommand = new RelayCommand<ListBox>((lbx) =>
            {
                int index = lbx.SelectedIndex;
                if (index > 0)
                    Translators.Move(index, index - 1);
            });
            TranslatorMoveDownCommand = new RelayCommand<ListBox>((lbx) =>
            {
                int index = lbx.SelectedIndex;
                if (index != -1 && index < Translators.Count - 1)
                    Translators.Move(index, index + 1);
            });
        }
        #endregion

        #region Public Methods
        public void Save()
        {
            File.WriteAllText(Path, JsonConvert.SerializeObject(this, Formatting.Indented));
        }
        public SettingsViewModel Clone()
        {
            return new SettingsViewModel()
            {
                Path = this.Path,
                Host = this.Host,
                Login = this.Login,
                Password = this.Password,
                mDatabase = this.mDatabase,
                Translators = new ObservableCollection<string>(this.Translators),
                UseEnglishSourceLang = this.UseEnglishSourceLang,
                MediaPath = this.MediaPath,
                MediaKey = this.MediaKey,
            };
        }
        #endregion

        #region Public Helpers
        public static SettingsViewModel LoadOrCreate(string path)
        {
            SettingsViewModel settings;
            if (File.Exists(path))
            {
                settings = JsonConvert.DeserializeObject<SettingsViewModel>(File.ReadAllText(path),new JsonSerializerSettings()
                {
                    ObjectCreationHandling = ObjectCreationHandling.Replace
                });
            }
            else
            {
                settings = new SettingsViewModel();
                File.WriteAllText(path, JsonConvert.SerializeObject(settings, Formatting.Indented));
            }
            settings.Path = path;
            return settings;
        }
        #endregion
    }
}
