using Pfim;
using SRO.PK2API;
using SRO.TextEditor.Models;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace SRO.ViewModels
{
    public class AppViewModel : BaseViewModel
    {
        #region Private Members
        private SearchResult mSearchSelectedResult;
        private SettingsViewModel mSettings;
        private bool mCanConnect;
        private SearchType mSearchType = SearchType.Codename;
        private string mEditingCodename;
        private string mEditingCategory;
        private string mEditingLanguage;
        private string mEditingText = string.Empty;
        private string mPreviewText;
        private List<Translator> mTranslators = new List<Translator>();
        private bool mIsTranslating;
        private Pk2Stream mPk2 = null;
        private string mPk2Path = null;
        private string mPk2Key = null;
        private Color mSelectedForegroundColor = Colors.White;
        private Color mSelectedBackgroundColor = Colors.White;
        #endregion

        #region Public Properties
        /// <summary>
        /// App title.
        /// </summary>
        public string Title { get; }
        /// <summary>
        /// Check if the connection to database can be established.
        /// </summary>
        public bool CanConnect { get => mCanConnect; set => SetProperty(ref mCanConnect, value); }
        /// <summary>
        /// All text categories (tables) found in the database.
        /// </summary>
        public ObservableCollection<string> Categories { get; } = new ObservableCollection<string>();
        /// <summary>
        /// All languages (columns) found in the database.
        /// </summary>
        public ObservableCollection<string> Languages { get; } = new ObservableCollection<string>();
        /// <summary>
        /// All search type availables.
        /// </summary>
        public IEnumerable<SearchType> SearchTypeOptions { get; } = Enum.GetValues(typeof(SearchType)).Cast<SearchType>();
        /// <summary>
        /// Search type being selected.
        /// </summary>
        public SearchType SearchType { get => mSearchType; set => SetProperty(ref mSearchType, value); }
        /// <summary>
        /// Results from the searching codenames.
        /// </summary>
        public ObservableCollection<SearchResult> SearchResults { get; set; } = new ObservableCollection<SearchResult>();
        /// <summary>
        /// Current selected from search results list.
        /// </summary>
        public SearchResult SelectedSearchResult { get => mSearchSelectedResult; set => SetProperty(ref mSearchSelectedResult, value); }
        /// <summary>
        /// Codename which is going to be edited.
        /// </summary>
        public string EditingCodename { get => mEditingCodename; set => SetProperty(ref mEditingCodename, value); }
        /// <summary>
        /// Category/table which is going to be edited.
        /// </summary>
        public string EditingCategory { get => mEditingCategory; set => SetProperty(ref mEditingCategory, value); }
        /// <summary>
        /// Language/column which is going to be edited.
        /// </summary>
        public string EditingLanguage { get => mEditingLanguage; set => SetProperty(ref mEditingLanguage, value); }
        /// <summary>
        /// Text which is going to be edited.
        /// </summary>
        public string EditingText { get => mEditingText; set => SetProperty(ref mEditingText, value); }
        /// <summary>
        /// Check if the translation is being executed.
        /// </summary>
        public bool IsTranslating { get => mIsTranslating; set => SetProperty(ref mIsTranslating, value); }
        /// <summary>
        /// Text to be used as preview.
        /// </summary>
        public string PreviewText { get => mPreviewText; set => SetProperty(ref mPreviewText, value); }
        /// <summary>
        /// Foreground color being selected.
        /// </summary>
        public Color SelectedForegroundColor { get => mSelectedForegroundColor; set => SetProperty(ref mSelectedForegroundColor, value); }
        /// <summary>
        /// Background color being selected.
        /// </summary>
        public Color SelectedBackgroundColor { get => mSelectedBackgroundColor; set => SetProperty(ref mSelectedBackgroundColor, value); }
        #endregion

        #region Commands
        /// <summary>
        /// Search codename in database.
        /// </summary>
        public ICommand SearchCommand { get; }
        /// <summary>
        /// Opens the database settings window.
        /// </summary>
        public ICommand OpenDbSettingsCommand { get; }
        /// <summary>
        /// Starts editing the selected result from searching list.
        /// </summary>
        public ICommand EditSelectedResultCommand { get; }
        public ICommand MakeTextBoldCommand { get; }
        public ICommand ClearTextBoldCommand { get; }
        public ICommand MakeBreaklineCommand { get; }
        public ICommand ClearBreaklineCommand { get; }
        public ICommand MakeTextAlignLeftCommand { get; }
        public ICommand ClearTextAlignLeftCommand { get; }
        public ICommand MakeTextAlignCenterCommand { get; }
        public ICommand ClearTextAlignCenterCommand { get; }
        public ICommand MakeTextAlignRightCommand { get; }
        public ICommand ClearTextAlignRightCommand { get; }
        public ICommand MakeForegroundColorCommand { get; }
        public ICommand ClearForegroundColorCommand { get; }
        public ICommand OpenForegroundColorPickerCommand { get; }
        public ICommand MakeBackgroundColorCommand { get; }
        public ICommand ClearBackgroundColorCommand { get; }
        public ICommand OpenBackgroundColorPickerCommand { get; }
        /// <summary>
        /// Preview the text being edited.
        /// </summary>
        public ICommand PreviewCommand { get; }
        /// <summary>
        /// Opens the Pk2 settings window.
        /// </summary>
        public ICommand OpenPk2SettingsCommand { get; }
        public Func<string, BitmapImage> LoadImageCommand => LoadMediaPK2Image;
        /// <summary>
        /// Translate the text being edited into the language selected.
        /// </summary>
        public ICommand TranslateCommand { get; }
        /// <summary>
        /// Opens the translator settings window.
        /// </summary>
        public ICommand OpenTranslatorSettingsCommand { get; }
        /// <summary>
        /// Update text into database.
        /// </summary>
        public ICommand UpdateCommand { get; }
        #endregion

        #region Constructor
        public AppViewModel()
        {
            // App title
            var info = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location);
            Title = $"{info.ProductName} v{info.FileVersion}";

            // App settings
            mSettings = SettingsViewModel.LoadOrCreate("Settings.json");
            
            // Add services providers in same order as settings
            foreach (var tName in mSettings.Translators)
                mTranslators.Add(new Translator { Name = tName });

            // Check sql connection
            CheckDatabaseConnection();

            SearchCommand = new RelayCommand<string>((value) =>
            {
                if (string.IsNullOrEmpty(value))
                    return;

                SearchResults.Clear();

                var db = new SQLDriver(mSettings.Host, mSettings.Login, mSettings.Password, mSettings.Database);
                foreach (var table in Categories)
                {
                    // Build query type
                    string unsafeQuery = string.Empty;
                    switch (SearchType)
                    {
                        default:
                        case SearchType.Codename:
                            unsafeQuery = $"SELECT ID, Codename FROM [{table}] WHERE Codename LIKE '%' + @value + '%'";
                            break;
                        case SearchType.SimilarText:
                            unsafeQuery = $"SELECT ID, Codename FROM [{table}] WHERE ";
                            foreach (var lang in Languages)
                                unsafeQuery += $"{lang} LIKE '%' + @value + '%' OR ";
                            unsafeQuery = unsafeQuery.Remove(unsafeQuery.Length - 3); // Remove last "OR "
                            break;
                        case SearchType.ExactText:
                            unsafeQuery = $"SELECT ID, Codename FROM [{table}] WHERE ";
                            foreach (var lang in Languages)
                                unsafeQuery += $"{lang} = @value OR ";
                            unsafeQuery = unsafeQuery.Remove(unsafeQuery.Length - 3); // Remove last "OR "
                            break;
                    }

                    // Search and add as results
                    var rows = db.ExecuteQuery(unsafeQuery, new KeyValuePair<string, object>[]
                    {
                        new KeyValuePair<string, object>("@value",value)
                    });
                    foreach (var r in rows)
                    {
                        var result = new SearchResult()
                        {
                            Id = (int)r["ID"],
                            Codename = r["Codename"] as string,
                            Category = table,
                        };
                        SearchResults.Add(result);
                    }
                }
            });
            OpenDbSettingsCommand = new RelayCommand<object>((param) =>
            {
                var window = new TextEditor.Views.DbSettings
                {
                    Title = $"{info.ProductName} - Database Settings",
                    Owner = System.Windows.Application.Current.MainWindow,
                    DataContext = mSettings.Clone(),
                };

                bool? result = window.ShowDialog();
                if (result == true)
                {
                    // Update settings
                    mSettings = window.DataContext as SettingsViewModel;
                    mSettings.Save();
                    CheckDatabaseConnection();
                }
            });
            EditSelectedResultCommand = new RelayCommand<object>((param) =>
            {
                EditingCodename = SelectedSearchResult.Codename;
                EditingCategory = SelectedSearchResult.Category;
                // Select current or first language found
                if (string.IsNullOrEmpty(EditingLanguage))
                    EditingLanguage = Languages[0];

                // Find the text
                var unsafeQuery = $"SELECT [{EditingLanguage}] FROM [{EditingCategory}] WHERE Codename LIKE '%' + @codename + '%'";

                var db = new SQLDriver(mSettings.Host, mSettings.Login, mSettings.Password, mSettings.Database);
                var row = db.ExecuteQuery(unsafeQuery, new KeyValuePair<string, object>[]
                {
                        new KeyValuePair<string, object>("@codename",EditingCodename)
                });

                // No record found, so empty as default
                if (row.Count == 0)
                {
                    EditingText = string.Empty;
                    return;
                }

                EditingText = row[0][EditingLanguage] as string;
            });
            PreviewCommand = new RelayCommand<object>((param) =>
            {
                if (string.IsNullOrEmpty(EditingText))
                    return;

                var html = EditingText;
                // Plain text detected
                if (!html.ToLowerInvariant().StartsWith("<sml2>"))
                    html += "<br>";
                // <sml2>, "\n" are worthless but visually
                else
                    html = html.Replace("\r\n", "").Replace("\n","");

                // Wrapper to add a base css style
                html = $"<body>{html}</body>";

                // Preprocess legacy html (order does matter)
                // font
                html = FixLegacyHtmlFontStyleSRO(html);
                html = FixLegacyHtmlFontIndexSRO(html);
                // color / bgcolor
                html = FixLegacyHtmlColor(html);
                html = FixLegacyHtmlColor(html);
                // line_margin
                html = FixLegacyHtmlLineMargin(html);

                // Combine multiples attributes in one
                html = FixAndJoinTagAttributes(html);
                PreviewText = html;
            });
            OpenPk2SettingsCommand = new RelayCommand<object>((param) =>
            {
                var window = new TextEditor.Views.Pk2Settings
                {
                    Title = $"{info.ProductName} - PK2 Settings",
                    Owner = System.Windows.Application.Current.MainWindow,
                    DataContext = mSettings.Clone(),
                };
                bool? result = window.ShowDialog();
                if (result == true)
                {
                    // Update settings
                    mSettings = window.DataContext as SettingsViewModel;
                    mSettings.Save();
                }
            });
            OpenTranslatorSettingsCommand = new RelayCommand<object>((param) =>
            {
                var window = new TextEditor.Views.TranslatorSettings
                {
                    Title = $"{info.ProductName} - Translator Settings",
                    Owner = System.Windows.Application.Current.MainWindow,
                    DataContext = mSettings.Clone(),
                };
                bool? result = window.ShowDialog();
                if (result == true)
                {
                    // Update settings
                    mSettings = window.DataContext as SettingsViewModel;
                    mSettings.Save();
                    // Sort it
                    mTranslators = mTranslators.OrderBy(x => x.Name).ToList();
                }
            });
            TranslateCommand = new RelayCommand<object>(async (param) =>
            {
                // Lock command execution
                IsTranslating = true;

                // Fix language codes
                string tl = EditingLanguage.Replace("_", "-"); // pt_BR -> pt-BR

                // Update result
                var result = await TranslateAsync(EditingText, tl, mSettings.UseEnglishSourceLang ? "en" : null);

                if (!string.IsNullOrEmpty(result))
                    EditingText = result;

                // Unlock command execution
                IsTranslating = false;
            }, (param) =>
            {
                return !IsTranslating
                && !string.IsNullOrEmpty(EditingText)
                && !string.IsNullOrEmpty(EditingLanguage);
            });
            UpdateCommand = new RelayCommand<object>((param) =>
            {
                var db = new SQLDriver(mSettings.Host, mSettings.Login, mSettings.Password, mSettings.Database);

                // Check if record exists, to create or update it
                var unsafeQuery = $@"
                    IF EXISTS (SELECT 1 FROM [{EditingCategory}] WITH (UPDLOCK, HOLDLOCK) WHERE Codename = @codename)
                        BEGIN
                            UPDATE [{EditingCategory}]
                            SET [{EditingLanguage}] = @text
                            WHERE Codename = @codename;
                        END
                    ELSE
                        BEGIN
                            INSERT INTO [{EditingCategory}] (Codename, [{EditingLanguage}])
                            VALUES (@codename, @text);
                        END";
                db.ExecuteQuery(unsafeQuery, new KeyValuePair<string, object>[]
                {
                    new KeyValuePair<string, object>("@codename",EditingCodename),
                    new KeyValuePair<string, object>("@text",EditingText)
                });
            }, (param) =>
            {
                return CanConnect
                && !string.IsNullOrEmpty(EditingCodename)
                && !string.IsNullOrEmpty(EditingCategory)
                && !string.IsNullOrEmpty(EditingLanguage);
            });
            MakeTextBoldCommand = new RelayCommand<TextBox>((tbx) =>
            {
                int start = tbx.SelectionStart;
                int length = tbx.SelectionLength;
                string text = tbx.Text;
                // Add sml2 if required
                AddSML2Tag(ref start, ref text);
                // Set values back
                var openTag = "<strong>";
                EditingText = text.Substring(0, start)
                    + openTag // Open
                    + text.Substring(start, length)
                    + "</strong>" // Close
                    + text.Substring(start + length);
                // Avoid focus flickering
                tbx.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Loaded, new Action(() =>
                {
                    tbx.Focus();
                    tbx.SelectionStart = start + openTag.Length;
                    tbx.SelectionLength = length;
                }));
            });
            ClearTextBoldCommand = new RelayCommand<TextBox>((tbx) =>
            {
                int start = tbx.SelectionStart;
                int length = tbx.SelectionLength;
                string text = tbx.Text;
                // Delete first found outside cursor
                if (length == 0)
                    RemoveEnclosingTagsAroundCursor(ref start, ref text, "<strong>", "</strong>");
                // Delete from selection
                else
                    RemoveEnclosingTagsInsideSelection(ref start, ref length, ref text, "strong");
                EditingText = text;
                // Avoid focus flickering
                tbx.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Loaded, new Action(() =>
                {
                    tbx.Focus();
                    tbx.SelectionStart = start;
                    tbx.SelectionLength = length;
                }));
            });
            MakeBreaklineCommand = new RelayCommand<TextBox>((tbx) =>
            {
                int start = tbx.SelectionStart;
                int length = tbx.SelectionLength;
                string text = EditingText;
                // Add sml2 if required
                AddSML2Tag(ref start, ref text);
                // Set values back
                EditingText = text.Substring(0, start + length)
                    + "<br>" // At the end of selection
                    + text.Substring(start + length);
                // Avoid focus flickering
                tbx.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Loaded, new Action(() =>
                {
                    tbx.Focus();
                    tbx.SelectionStart = start;
                    tbx.SelectionLength = length;
                }));
            });
            ClearBreaklineCommand = new RelayCommand<TextBox>((tbx) =>
            {
                int start = tbx.SelectionStart;
                int length = tbx.SelectionLength;
                string text = tbx.Text;
                // Delete from selection ONLY
                if (length == 0)
                    return;
                RemoveTagInSelection(ref start, ref length, ref text, "br");
                EditingText = text;
                // Avoid focus flickering
                tbx.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Loaded, new Action(() =>
                {
                    tbx.Focus();
                    tbx.SelectionStart = start;
                    tbx.SelectionLength = length;
                }));
            });
            MakeTextAlignLeftCommand = new RelayCommand<TextBox>((tbx) =>
            {
                int start = tbx.SelectionStart;
                int length = tbx.SelectionLength;
                string text = EditingText;
                // Add sml2 if required
                AddSML2Tag(ref start, ref text);
                // Set values back
                var openTag = "<left>";
                EditingText = text.Substring(0, start)
                    + openTag // Open
                    + text.Substring(start, length)
                    + "</left>" // Close
                    + text.Substring(start + length);
                // Avoid focus flickering
                tbx.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Loaded, new Action(() =>
                {
                    tbx.Focus();
                    tbx.SelectionStart = start + openTag.Length;
                    tbx.SelectionLength = length;
                }));
            });
            ClearTextAlignLeftCommand = new RelayCommand<TextBox>((tbx) =>
            {
                int start = tbx.SelectionStart;
                int length = tbx.SelectionLength;
                string text = tbx.Text;
                // Delete first found outside cursor
                if (length == 0)
                    RemoveEnclosingTagsAroundCursor(ref start, ref text, "<left>", "</left>");
                // Delete from selection
                else
                    RemoveEnclosingTagsInsideSelection(ref start, ref length, ref text,"left");
                EditingText = text;
                // Avoid focus flickering
                tbx.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Loaded, new Action(() =>
                {
                    tbx.Focus();
                    tbx.SelectionStart = start;
                    tbx.SelectionLength = length;
                }));
            });
            MakeTextAlignCenterCommand = new RelayCommand<TextBox>((tbx) =>
            {
                int start = tbx.SelectionStart;
                int length = tbx.SelectionLength;
                string text = EditingText;
                // Add sml2 if required
                AddSML2Tag(ref start, ref text);
                // Set values back
                var openTag = "<center>";
                EditingText = text.Substring(0, start)
                    + openTag // Open
                    + text.Substring(start, length)
                    + "</center>" // Close
                    + text.Substring(start + length);
                // Avoid focus flickering
                tbx.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Loaded, new Action(() =>
                {
                    tbx.Focus();
                    tbx.SelectionStart = start + openTag.Length;
                    tbx.SelectionLength = length;
                }));
            });
            ClearTextAlignCenterCommand = new RelayCommand<TextBox>((tbx) =>
            {
                int start = tbx.SelectionStart;
                int length = tbx.SelectionLength;
                string text = tbx.Text;
                // Delete first found outside cursor
                if (length == 0)
                    RemoveEnclosingTagsAroundCursor(ref start, ref text, "<center>", "</center>");
                // Delete from selection
                else
                    RemoveEnclosingTagsInsideSelection(ref start, ref length, ref text, "center");
                EditingText = text;
                // Avoid focus flickering
                tbx.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Loaded, new Action(() =>
                {
                    tbx.Focus();
                    tbx.SelectionStart = start;
                    tbx.SelectionLength = length;
                }));
            });
            MakeTextAlignRightCommand = new RelayCommand<TextBox>((tbx) =>
            {
                int start = tbx.SelectionStart;
                int length = tbx.SelectionLength;
                string text = EditingText;
                // Add sml2 if required
                AddSML2Tag(ref start, ref text);
                // Set values back
                var openTag = "<right>";
                EditingText = text.Substring(0, start)
                    + openTag // Open
                    + text.Substring(start, length)
                    + "</right>" // Close
                    + text.Substring(start + length);
                // Avoid focus flickering
                tbx.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Loaded, new Action(() =>
                {
                    tbx.Focus();
                    tbx.SelectionStart = start + openTag.Length;
                    tbx.SelectionLength = length;
                }));
            });
            ClearTextAlignRightCommand = new RelayCommand<TextBox>((tbx) =>
            {
                int start = tbx.SelectionStart;
                int length = tbx.SelectionLength;
                string text = tbx.Text;
                // Delete first found outside cursor
                if (length == 0)
                    RemoveEnclosingTagsAroundCursor(ref start, ref text, "<right>", "</right>");
                // Delete from selection
                else
                    RemoveEnclosingTagsInsideSelection(ref start, ref length, ref text, "right");
                EditingText = text;
                // Avoid focus flickering
                tbx.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Loaded, new Action(() =>
                {
                    tbx.Focus();
                    tbx.SelectionStart = start;
                    tbx.SelectionLength = length;
                }));
            });
            MakeForegroundColorCommand = new RelayCommand<TextBox>((tbx) =>
            {
                int start = tbx.SelectionStart;
                int length = tbx.SelectionLength;
                string text = tbx.Text;
                // Add sml2 if required
                AddSML2Tag(ref start, ref text);
                // Try to add color to selected tag
                if (length == 0)
                {
                    AddAttributeSROColorToTag(ref start, ref text, "color", SelectedForegroundColor, new string[] { "font" });
                    EditingText = text;
                }
                // Add a font tag with color
                else
                {
                    var openTag = $"<font color=\"{SelectedForegroundColor.A},{SelectedForegroundColor.R},{SelectedForegroundColor.G},{SelectedForegroundColor.B}\">";
                    EditingText = text.Substring(0, start)
                                + openTag // Open
                                + text.Substring(start, length)
                                + "</font>" // Close
                                + text.Substring(start + length);
                    start += openTag.Length;
                }
                // Avoid focus flickering
                tbx.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Loaded, new Action(() =>
                {
                    tbx.Focus();
                    tbx.SelectionStart = start;
                    tbx.SelectionLength = length;
                }));
            });
            ClearForegroundColorCommand = new RelayCommand<TextBox>((tbx) =>
            {
                int start = tbx.SelectionStart;
                int length = tbx.SelectionLength;
                string text = tbx.Text;
                // Delete from selection ONLY
                if (length == 0)
                    return;
                RemoveAttributeInSelection(ref start, ref length, ref text, "color"); 
                EditingText = text;
                // Avoid focus flickering
                tbx.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Loaded, new Action(() =>
                {
                    tbx.Focus();
                    tbx.SelectionStart = start;
                    tbx.SelectionLength = length;
                }));
            });
            OpenForegroundColorPickerCommand = new RelayCommand<object>((param) =>
            {
                var window = new TextEditor.Views.ColorPickerWindow
                {
                    Title = $"{info.ProductName} - Color Picker",
                    Owner = System.Windows.Application.Current.MainWindow,
                    DataContext = new ColorPickerViewModel() { SelectedColor = SelectedForegroundColor },
                };
                bool? result = window.ShowDialog();
                if (result == true)
                {
                    // Update settings
                    var ctx = window.DataContext as ColorPickerViewModel;
                    SelectedForegroundColor = ctx.SelectedColor;
                }
            });
            MakeBackgroundColorCommand = new RelayCommand<TextBox>((tbx) =>
            {
                int start = tbx.SelectionStart;
                int length = tbx.SelectionLength;
                string text = tbx.Text;
                // Add sml2 if required
                AddSML2Tag(ref start, ref text);
                // Try to add color to selected tag
                if (length == 0)
                {
                    AddAttributeSROColorToTag(ref start, ref text, "bgcolor", SelectedForegroundColor, new string[] { "font", "td" });
                    EditingText = text;
                }
                // Add a font tag with color
                else
                {
                    var openTag = $"<font bgcolor=\"{SelectedForegroundColor.A},{SelectedForegroundColor.R},{SelectedForegroundColor.G},{SelectedForegroundColor.B}\">";
                    EditingText = text.Substring(0, start)
                                + openTag // Open
                                + text.Substring(start, length)
                                + "</font>" // Close
                                + text.Substring(start + length);
                    start += openTag.Length;
                }
                // Avoid focus flickering
                tbx.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Loaded, new Action(() =>
                {
                    tbx.Focus();
                    tbx.SelectionStart = start;
                    tbx.SelectionLength = length;
                }));
            });
            ClearBackgroundColorCommand = new RelayCommand<TextBox>((tbx) =>
            {
                int start = tbx.SelectionStart;
                int length = tbx.SelectionLength;
                string text = tbx.Text;
                // Delete from selection ONLY
                if (length == 0)
                    return;
                RemoveAttributeInSelection(ref start, ref length, ref text, "bgcolor");
                EditingText = text;
                // Avoid focus flickering
                tbx.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Loaded, new Action(() =>
                {
                    tbx.Focus();
                    tbx.SelectionStart = start;
                    tbx.SelectionLength = length;
                }));
            });
            OpenBackgroundColorPickerCommand = new RelayCommand<object>((param) =>
            {
                var window = new TextEditor.Views.ColorPickerWindow
                {
                    Title = $"{info.ProductName} - Color Picker",
                    Owner = System.Windows.Application.Current.MainWindow,
                    DataContext = new ColorPickerViewModel() { SelectedColor = SelectedBackgroundColor },
                };
                bool? result = window.ShowDialog();
                if (result == true)
                {
                    // Update settings
                    var ctx = window.DataContext as ColorPickerViewModel;
                    SelectedBackgroundColor = ctx.SelectedColor;
                }
            });

            // Make some commands to change their executing status
            this.PropertyChanged += (s, e) =>
            {
                // TranslateCommand
                if (e.PropertyName.Equals(nameof(IsTranslating))
                || e.PropertyName.Equals(nameof(EditingText))
                || e.PropertyName.Equals(nameof(EditingLanguage)))
                    (TranslateCommand as RelayCommand<object>).RaiseCanExecuteChanged();
                // UpdateCommand
                if (e.PropertyName.Equals(nameof(CanConnect))
                || e.PropertyName.Equals(nameof(EditingCodename))
                || e.PropertyName.Equals(nameof(EditingText))
                || e.PropertyName.Equals(nameof(EditingLanguage)))
                    (UpdateCommand as RelayCommand<object>).RaiseCanExecuteChanged();
            };
        }
        #endregion

        #region Private Helpers
        /// <summary>
        /// Check if database settings has been set successfully to proceed loading categories and languages availables.
        /// </summary>
        private void CheckDatabaseConnection()
        {
            var db = new SQLDriver(mSettings.Host, mSettings.Login, mSettings.Password, mSettings.Database);

            CanConnect = db.CanConnect();

            // Save current selection
            var editingCategory = EditingCategory;
            Categories.Clear();
            var editingLanguage = EditingLanguage;
            Languages.Clear();

            if (CanConnect)
            {
                var rows = db.ExecuteQuery("SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE' ORDER BY TABLE_NAME;");
                foreach (var r in rows)
                {
                    Categories.Add(r["TABLE_NAME"] as string); // TABLE_NAME
                    var columns = db.ExecuteQuery($"SELECT [name] FROM sys.columns WHERE object_id = OBJECT_ID('[{r["TABLE_NAME"]}]');");
                    foreach (var c in columns)
                    {
                        var lang = c["name"] as string;
                        // Ignore repeated ones
                        if (Languages.Contains(lang))
                            continue;
                        // Filter id and codename
                        if (lang.ToLowerInvariant() == "id" || lang.ToLowerInvariant() == "codename")
                            continue;

                        Languages.Add(lang);
                    }
                }
            }
            // Keep last selections after refresh if still exists
            if (Categories.Contains(editingCategory))
                EditingCategory = editingCategory;
            if (Languages.Contains(editingLanguage))
                EditingLanguage = editingLanguage;
        }
        private int MathClamp(int value, int min, int max)
        {
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }
        /// <summary>
        /// Translate a text using the available services.
        /// </summary>
        /// <param name="text">Text to be translated</param>
        /// <param name="targetLang">Target language code</param>
        /// <param name="sourceLang">Source language code, auto detected by default</param>
        /// <returns>Returns an empty string if the translation failed</returns>
        private async Task<string> TranslateAsync(string text, string targetLang, string sourceLang = null, CancellationToken cancellationToken = default)
        {
            foreach (var t in mTranslators)
            {
                cancellationToken.ThrowIfCancellationRequested();
                try
                {
                    var result = await t.GetInstance().TranslateAsync(text, targetLang, sourceLang).ConfigureAwait(false);
                    System.Diagnostics.Debug.WriteLine($"Translated using {t.GetType().Name} as provider");
                    return result?.Translation;
                }
                catch (OperationCanceledException)
                {
                    // Re-throw if cancellation was explicitly requested
                    throw;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Translation failed for {t.GetType().Name}: {ex.Message}");
                }
            }
            return string.Empty;
        }
        private void AddSML2Tag(ref int start, ref string text)
        {
            if (!text.StartsWith("<sml2>"))
            {
                text = "<sml2>\n" + text + "\n</sml2>";
                start += "<sml2>\n".Length;
            }
        }
        private void RemoveEnclosingTagsAroundCursor(ref int selectionStart,ref string text, string openTag, string closeTag)
        {
            int cursor = selectionStart;

            // 1. Scan left from the caret with nesting balance to find the matching open tag
            int leftIndex = -1;
            int depth = 0;
            for (int i = cursor - openTag.Length; i >= 0; i--)
            {
                if (i + closeTag.Length <= text.Length && text.Substring(i, closeTag.Length) == closeTag)
                {
                    depth++; // Enter a nested scope
                }
                else if (i + openTag.Length <= text.Length && text.Substring(i, openTag.Length) == openTag)
                {
                    if (depth > 0)
                    {
                        depth--; // Exit a nested scope
                    }
                    else
                    {
                        leftIndex = i; // Found our target open tag
                        break;
                    }
                }
            }
            if (leftIndex == -1) return;

            // 2. Scan right from the caret with nesting balance to find the matching close tag
            int rightIndex = -1;
            depth = 0;
            for (int i = cursor; i <= text.Length - closeTag.Length; i++)
            {
                if (i + openTag.Length <= text.Length && text.Substring(i, openTag.Length) == openTag)
                {
                    depth++; // Enter a nested scope
                }
                else if (i + closeTag.Length <= text.Length && text.Substring(i, closeTag.Length) == closeTag)
                {
                    if (depth > 0)
                    {
                        depth--; // Exit a nested scope
                    }
                    else
                    {
                        rightIndex = i; // Found our target close tag
                        break;
                    }
                }
            }
            if (rightIndex == -1) return;

            // 3. Remove ONLY the tags themselves. 
            // .Remove(index, count) only deletes 'count' characters (the tag length), 
            // leaving all the text in between untouched.
            text = text.Remove(rightIndex, closeTag.Length).Remove(leftIndex, openTag.Length);

            // 4. Adjust cursor position back by the length of the removed opening tag
            selectionStart = Math.Max(0, cursor - openTag.Length);
        }
        private void RemoveEnclosingTagsInsideSelection(ref int selectionStart, ref int selectionLength, ref string text, string tag)
        {
            if (selectionLength <= 0)
                return;

            int start = selectionStart;
            int length = selectionLength;

            // 1. Extract the currently selected text
            string selectedText = text.Substring(start, length);

            // 2. Build a pattern that matches the opening tag (with optional attributes) and closing tag
            // Group 1 captures everything inside the tags so we can keep it
            string pattern = $@"<{tag}(\s+[^>]*)?>(.*?)</\s*{tag}>";

            // 3. Use RegexOptions.Singleline so '.' matches newlines (crucial for multi-line text)
            string cleanedText = Regex.Replace(selectedText, pattern, "$2", RegexOptions.Singleline | RegexOptions.IgnoreCase);

            // 4. Rebuild the main text with the cleaned selection
            text = text.Substring(0, start) + cleanedText + text.Substring(start + length);

            // 5. Update the selection length to reflect only the removed tag characters
            selectionLength = cleanedText.Length;
        }
        private void RemoveTagInSelection(ref int selectionStart, ref int selectionLength, ref string text, string tag)
        {
            int start = selectionStart;
            int length = selectionLength;

            // 1. Extract the selected text
            string selectedText = text.Substring(start, length);

            // 2. Regex matches opening tags (with optional attributes) and closing tags for the given name
            // Example for "p": matches <p>, <p class="foo">, and </p>
            string pattern = $@"</?\s*{Regex.Escape(tag)}(\s+[^>]*)?>";

            // 3. Remove the tags from the selection
            string cleanedText = Regex.Replace(selectedText, pattern, string.Empty, RegexOptions.IgnoreCase);

            // 4. Rebuild the main text and update selection length
            text = text.Substring(0, start) + cleanedText + text.Substring(start + length);
            selectionLength = cleanedText.Length;
        }
        private void RemoveAttributeInSelection(ref int selectionStart, ref int selectionLength, ref string text, string attribute)
        {
            int start = selectionStart;
            int length = selectionLength;

            // 1. Extract the currently selected text
            string selectedText = text.Substring(start, length);

            // 2. Match any opening or self-closing tag within the selection
            string tagPattern = @"<\s*([a-zA-Z0-9]+)\b([^>]*)>";

            string cleanedText = Regex.Replace(selectedText, tagPattern, match =>
            {
                string tagName = match.Groups[1].Value;
                string attrBlock = match.Groups[2].Value;

                // 3. Pattern to target the specific attribute and its value (quoted or unquoted),
                // capturing leading whitespace so it doesn't leave double spaces behind.
                string attrRegex = $@"\s*\b{Regex.Escape(attribute)}\s*=\s*(?:""[^""]*""|'[^']*'|[^\s>]+)";
                // Remove the attribute from the attribute block
                string cleanedAttrBlock = Regex.Replace(attrBlock, attrRegex, string.Empty, RegexOptions.IgnoreCase);
                return $"<{tagName}{cleanedAttrBlock}>";
            }, RegexOptions.IgnoreCase | RegexOptions.Singleline);

            // 4. Rebuild the main text with the cleaned selection
            text = text.Substring(0, start) + cleanedText + text.Substring(start + length);

            // 5. Update the selection length to reflect the removed attribute characters
            selectionLength = cleanedText.Length;
        }
        private string FixLegacyHtmlColor(string html)
        {
            return Regex.Replace(html, @"<\s*([a-zA-Z0-9]+)\b([^>]*)color\s*=\s*([""']?)\s*(\d+)\s*,\s*(\d+)\s*,\s*(\d+)(?:\s*,\s*(\d+))?\s*\3([^>]*)>", match =>
            {
                string tagName = match.Groups[1].Value;        // e.g., "font" or "span"
                string beforeColorAttrs = match.Groups[2].Value; // Attributes before color (e.g., index="1")
                string quote = match.Groups[3].Value;          // Captures " or ' if present
                int r = MathClamp(int.Parse(match.Groups[4].Value), 0, 255);
                int g = MathClamp(int.Parse(match.Groups[5].Value), 0, 255);
                int b = MathClamp(int.Parse(match.Groups[6].Value), 0, 255);
                // If four parameters are provided, the first is alpha, so shift RGB channels
                if (match.Groups[7].Success)
                {
                    r = g;
                    g = b;
                    b = MathClamp(int.Parse(match.Groups[7].Value), 0, 255);
                }
                string afterColorAttrs = match.Groups[8].Value;  // Attributes after color (e.g., style="bold")
                                                                 // Reconstruct the exact same tag name and keep all non-color attributes intact
                return $"<{tagName}{beforeColorAttrs}color={quote}#{r:X2}{g:X2}{b:X2}{quote}{afterColorAttrs}>";
            }, RegexOptions.IgnoreCase | RegexOptions.Singleline);
        }
        private void AddAttributeSROColorToTag(ref int selectionStart, ref string text, string attributeName, Color color, string[] allowedTagNames)
        {
            int cursor = selectionStart;
            int textLength = text.Length;
            // Color format
            string hexColor = $"{color.A},{color.R},{color.G},{color.B}";
            // 1. Scan left from cursor to find the nearest enclosing opening tag
            int openTagStart = -1;
            int openTagEnd = -1;
            int depth = 0;
            for (int i = cursor - 1; i >= 0; i--)
            {
                if (text[i] == '>')
                {
                    // If we hit '>' moving left, check if it's a closing tag </tag>
                    int lastSlash = text.LastIndexOf('/', i);
                    int lastOpen = text.LastIndexOf('<', i);
                    if (lastOpen != -1 && lastSlash > lastOpen)
                        depth++; // Enclosed in a deeper nested scope
                    else if (lastOpen != -1)
                    {
                        // Hit a completed opening tag to the left, skip past its content scope
                        depth++;
                    }
                }
                else if (text[i] == '<')
                {
                    if (depth > 0)
                        depth--;
                    else
                    {
                        // Found our outer opening tag start
                        openTagStart = i;
                        openTagEnd = text.IndexOf('>', i);
                        break;
                    }
                }
            }
            if (openTagStart == -1 || openTagEnd == -1 || openTagEnd < openTagStart)
                return; // No enclosing tag found

            // Extract tag name and check if allowed
            string fullTag = text.Substring(openTagStart, openTagEnd - openTagStart + 1);
            Match nameMatch = Regex.Match(fullTag, @"^<\s*([a-zA-Z0-9]+)");
            if (!nameMatch.Success)
                return;

            string matchedTagName = nameMatch.Groups[1].Value;
            bool isAllowed = allowedTagNames.Any(t => string.Equals(t, matchedTagName, StringComparison.OrdinalIgnoreCase));
            if (!isAllowed)
                return;

            // 2. Scan right from cursor to verify matching closing tag exists
            int closeTagStart = -1;
            depth = 0;

            for (int i = cursor; i <= text.Length - 1; i++)
            {
                if (text[i] == '<')
                {
                    if (i + 1 < text.Length && text[i + 1] == '/')
                    {
                        if (depth > 0)
                            depth--;
                        else
                        {
                            closeTagStart = i;
                            break;
                        }
                    }
                    else
                    {
                        depth++; // Nested open tag
                    }
                }
            }

            if (closeTagStart == -1)
                return; // Cursor is not properly enclosed in a matching pair

            // 3. Update or Add the color attribute inside the tag
            string attrRegex = $@"\b{Regex.Escape(attributeName)}\s*=\s*(?:""[^""]*""|'[^']*'|[^\s>]+)";
            string newAttribute = $"{attributeName}=\"{hexColor}\"";
            string updatedTag;

            if (Regex.IsMatch(fullTag, attrRegex, RegexOptions.IgnoreCase))
            {
                // Replace existing color attribute
                updatedTag = Regex.Replace(fullTag, attrRegex, newAttribute, RegexOptions.IgnoreCase);
            }
            else
            {
                // Insert new color attribute right after the tag name
                int insertPos = nameMatch.Index + nameMatch.Length;
                updatedTag = fullTag.Insert(insertPos, $" {newAttribute}");
            }

            // 4. Rebuild the text and adjust cursor offset
            int oldContentStart = openTagEnd + 1;
            text = text.Substring(0, openTagStart) + updatedTag + text.Substring(oldContentStart);
            int newContentStart = openTagStart + updatedTag.Length;
            if (cursor >= oldContentStart)
            {
                // Cursor was in the content text; shift it by the difference in tag length
                selectionStart = cursor + (newContentStart - oldContentStart);
            }
            else
            {
                // Cursor was inside the tag itself; clamp it safely within the modified tag bounds
                selectionStart = Math.Min(Math.Max(openTagStart, cursor), newContentStart);
            }
        }
        private string FixLegacyHtmlLineMargin(string html)
        {
            return Regex.Replace(html, @"\bline_margin\s*=\s*([""']?)\s*(\d+)\s*\1", match =>
            {
                string quote = match.Groups[1].Value; // Captures " or ' if present
                int line_margin = int.Parse(match.Groups[2].Value);
                line_margin += 20;
                // Map the custom line_margin value to a standard CSS margin-bottom style
                return $"style=\"line-height:{line_margin}px;\"";
            }, RegexOptions.IgnoreCase | RegexOptions.Singleline);
        }
        private string FixLegacyHtmlFontIndexSRO(string html)
        {
            return Regex.Replace(html, @"<font\b([^>]*)index\s*=\s*([""']?)\s*(\d+)\s*\2([^>]*)>", match =>
            {
                string beforeAttrs = match.Groups[1].Value;
                string quote = match.Groups[2].Value;
                // Extract the single index value
                int index = int.Parse(match.Groups[3].Value);
                int size = 16;
                switch (index)
                {
                    case 1:
                        size = 13;
                        break;
                    case 2:
                        size = 24;
                        break;
                    case 3:
                        size = 18;
                        break;
                    case 4:
                        size = 32;
                        break;
                }
                string afterAttrs = match.Groups[4].Value;
                // Reconstruct the opening tag safely, keeping all other attributes intact
                return $"<font{beforeAttrs}style={quote}font-size:{size}px;line-height:0px;{quote}{afterAttrs}>";
            }, RegexOptions.IgnoreCase | RegexOptions.Singleline);
        }
        private string FixLegacyHtmlFontStyleSRO(string html)
        {
            return Regex.Replace(html, @"<\s*font\b([^>]*)style\s*=\s*([""']?)\s*(?!font-style:)([^""']*?)\s*\2([^>]*)>", match =>
            {
                string beforeAttrs = match.Groups[1].Value;
                string quote = match.Groups[2].Value;
                string fontStyle = match.Groups[3].Value;
                if(fontStyle == "bold")
                    fontStyle = $"font-weight:bold;";
                else
                    fontStyle = $"font-style:{fontStyle};";
                string afterAttrs = match.Groups[4].Value;
                return $"<font{beforeAttrs}style={quote}{fontStyle}{quote}{afterAttrs}>";
            }, RegexOptions.IgnoreCase | RegexOptions.Singleline);
        }
        private string FixAndJoinTagAttributes(string html)
        {
            // 1. Match ANY opening tag (e.g., <font>, <span, <div, etc.)
            return Regex.Replace(html, @"<\s*([a-zA-Z0-9]+)\b([^>]*)>", match =>
            {
                string tagName = match.Groups[1].Value;
                string attrBlock = match.Groups[2].Value;

                // Dictionary to group attributes by name (case-insensitive)
                Dictionary<string, List<string>> attributeMap = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);

                // 2. Regex to extract individual attributes: name="value" or name='value'
                string attrPattern = @"([a-zA-Z0-9\-]+)\s*=\s*([""']?)(.*?)\2";
                MatchCollection attrMatches = Regex.Matches(attrBlock, attrPattern);

                foreach (Match attrMatch in attrMatches)
                {
                    string attrName = attrMatch.Groups[1].Value.ToLower();
                    string attrVal = attrMatch.Groups[3].Value;

                    if (!attributeMap.ContainsKey(attrName))
                    {
                        attributeMap[attrName] = new List<string>();
                    }
                    attributeMap[attrName].Add(attrVal);
                }

                // 3. Rebuild attributes, combining duplicates where appropriate
                List<string> rebuiltAttrs = new List<string>();
                foreach (var kvp in attributeMap)
                {
                    string name = kvp.Key;
                    List<string> values = kvp.Value;

                    if (name == "style")
                    {
                        // Join multiple style blocks together with a semicolon, ensuring no trailing mess
                        string combinedStyle = string.Join(";", values.Select(v => v.Trim().TrimEnd(';')));
                        if (!string.IsNullOrEmpty(combinedStyle)) combinedStyle += ";";
                        rebuiltAttrs.Add($"style=\"{combinedStyle}\"");
                    }
                    else
                    {
                        // For other unique attributes, keep the latest value
                        rebuiltAttrs.Add($"{name}=\"{values.Last()}\"");
                    }
                }

                // Reconstruct the tag with the merged attribute list
                string newAttrString = rebuiltAttrs.Count > 0 ? " " + string.Join(" ", rebuiltAttrs) : "";
                return $"<{tagName}{newAttrString}>";
            }, RegexOptions.IgnoreCase | RegexOptions.Singleline);
        }
        /// <summary>
        /// Loads image from media.pk2 path.
        /// </summary>
        private BitmapImage LoadMediaPK2Image(string path)
        {
            // Make sure the path has been set
            if (string.IsNullOrEmpty(mSettings.MediaPath) || !File.Exists(mSettings.MediaPath))
                return null;

            // Open pk2 file and try to get the file
            try
            {
                // Open or reopen on read only mode if something changed
                if(mPk2 == null || mPk2Path != mSettings.MediaPath ||mPk2Key != mSettings.MediaKey)
                {
                    mPk2?.Dispose();
                    mPk2 = new Pk2Stream(mSettings.MediaPath, mSettings.MediaKey, true);
                    mPk2Path = mSettings.MediaPath;
                    mPk2Key = mSettings.MediaKey;
                }
                var file = mPk2.GetFile(path);
                if (file == null)
                    throw new FileNotFoundException($"File not found {path}");
                MemoryStream imgStream = new MemoryStream(file.GetContent());
                // Open media.pk2 and get file stream
                // Check if file from path is .dds or .ddj
                using (var br = new BinaryReader(imgStream, System.Text.Encoding.ASCII))
                {
                    if (new string(br.ReadChars(12)) == "JMXVDDJ 1000")
                    {
                        br.BaseStream.Seek(20, SeekOrigin.Begin);
                        // MY CODE HERE
                    }
                    else
                    {
                        br.BaseStream.Seek(0, SeekOrigin.Begin);
                        if (new string(br.ReadChars(4)) == "DDS ")
                        {
                            br.BaseStream.Seek(0, SeekOrigin.Begin);
                            // MY CODE HERE
                        }
                        else
                        {
                            throw new NotSupportedException("File format not supported");
                        }
                    }
                    // Convert to bitmap
                    using (IImage image = Pfimage.FromStream(imgStream))
                    {
                        // Find pixel format being use, covering just DXT1~DXT5
                        var pixelFormat = System.Drawing.Imaging.PixelFormat.Undefined;
                        var rawData = image.Data;
                        var stride = image.Stride;
                        switch (image.Format)
                        {
                            case Pfim.ImageFormat.Rgb24:
                                pixelFormat = System.Drawing.Imaging.PixelFormat.Format24bppRgb;
                                break;
                            case Pfim.ImageFormat.Rgba32:
                                pixelFormat = System.Drawing.Imaging.PixelFormat.Format32bppArgb;
                                break;
                            case Pfim.ImageFormat.R5g6b5:
                                pixelFormat = System.Drawing.Imaging.PixelFormat.Format16bppRgb565;
                                break;
                            case Pfim.ImageFormat.R5g5b5a1:
                                pixelFormat = System.Drawing.Imaging.PixelFormat.Format16bppArgb1555;
                                break;
                            default:
                                throw new NotSupportedException($"DDS format not supported [{image.Format}]");
                        }
                        // Create bitmap
                        using (var bitmap = new System.Drawing.Bitmap(image.Width, image.Height, pixelFormat))
                        {
                            System.Drawing.Imaging.BitmapData rectData = bitmap.LockBits(
                                new System.Drawing.Rectangle(0, 0, image.Width, image.Height),
                                System.Drawing.Imaging.ImageLockMode.WriteOnly,
                                pixelFormat
                            );
                            // Copy Pfim's buffer straight into System.Drawing's allocated memory
                            Marshal.Copy(image.Data, 0, rectData.Scan0, image.Data.Length);
                            // Unlock and get it
                            bitmap.UnlockBits(rectData);
                            using (var memoryStream = new MemoryStream())
                            {
                                bitmap.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Bmp);
                                memoryStream.Position = 0;

                                var bitmapImage = new BitmapImage();
                                bitmapImage.BeginInit();
                                bitmapImage.CacheOption = BitmapCacheOption.OnLoad; // Fully loads data into memory
                                bitmapImage.StreamSource = memoryStream;
                                bitmapImage.EndInit();
                                bitmapImage.Freeze(); // Makes it thread-safe for the UI thread
                                return bitmapImage;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
                return null;
            }
        }
        #endregion
    }
}
