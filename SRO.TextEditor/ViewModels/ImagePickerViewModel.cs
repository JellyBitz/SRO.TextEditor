using SRO.PK2API;
using SRO.TextEditor.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace SRO.ViewModels
{
    public class ImagePickerViewModel : BaseViewModel
    {
        #region Private Members
        private Pk2Stream mPk2;
        private Dictionary<string,Pk2File> mImageList = new Dictionary<string, Pk2File>();
        private string mSelectedPath = null;
        private BitmapImage mSelectedImage = null;
        private int mSelectedImageWidth = 0;
        private int mSelectedImageHeight = 0;
        private string mSelectedImageSize = "0 B";
        #endregion

        #region Public Properties
        public ObservableCollection<string> ImagePaths { get; } = new ObservableCollection<string>();
        public string SelectedPath { get => mSelectedPath; set => SetProperty(ref mSelectedPath, value); }
        public BitmapImage SelectedImage { get => mSelectedImage; set => SetProperty(ref mSelectedImage, value); }
        public int SelectedImageWidth { get => mSelectedImageWidth; set => SetProperty(ref mSelectedImageWidth, value); }
        public int SelectedImageHeight { get => mSelectedImageHeight; set => SetProperty(ref mSelectedImageHeight, value); }
        public string SelectedImageSize { get => mSelectedImageSize; set => SetProperty(ref mSelectedImageSize, value); }
        #endregion

        #region Commands
        /// <summary>
        /// Search list by similar name.
        /// </summary>
        public ICommand SearchCommand { get; }
        #endregion

        #region Constructor
        public ImagePickerViewModel(Pk2Stream pk2)
        {
            mPk2 = pk2;

            // Lower down the scope
            LoadImageListFromFolder(pk2.GetFolder("icon"));
            LoadImageListFromFolder(pk2.GetFolder("icon64"));
            LoadImageListFromFolder(pk2.GetFolder("interface/image"));

            SearchCommand = new RelayCommand<string>((value) =>
            {
                // Populates the available images
                ImagePaths.Clear();
                if (string.IsNullOrEmpty(value))
                {
                    foreach (var kv in mImageList)
                        ImagePaths.Add(kv.Key);
                }
                else
                {
                    var valueToLower = value.ToLowerInvariant();
                    foreach (var kv in mImageList)
                        if (kv.Key.Contains(valueToLower))
                            ImagePaths.Add(kv.Key);
                }
            });

            // Try loading preview when selected path changes
            this.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName.Equals(nameof(SelectedPath)))
                {
                    if (string.IsNullOrEmpty(SelectedPath))
                    {
                        var file = mPk2.GetFile(SelectedPath);
                        if (file != null)
                        {
                            SelectedImage = mPk2.GetBitmapImage(SelectedPath);
                            SelectedImageWidth = (int)SelectedImage.Width;
                            SelectedImageHeight = (int)SelectedImage.Height;
                            SelectedImageSize = FormatFileSize(file.Size);
                            return;
                        }
                        else
                        {
                            SelectedImageSize = FormatFileSize(file.Size);
                        }
                    }
                    else
                    {
                        SelectedImageSize = FormatFileSize(0);
                    }
                    SelectedImageWidth = SelectedImageHeight = 0;
                }
            };

            SearchCommand.Execute("");
        }
        #endregion

        #region Private Helpers
        private void LoadImageListFromFolder(Pk2Folder folder)
        {
            if (folder == null)
                return;
            foreach (var file in folder.Files)
            {
                if (file.Value.Name.EndsWith(".ddj"))
                    mImageList[file.Value.GetFullPath()] = file.Value;
            }
            foreach (var subfolder in folder.Folders)
                LoadImageListFromFolder(subfolder.Value);
        }
        private string FormatFileSize(uint fileSize)
        {
            string[] units = { "B", "KB", "MB", "GB", "TB" };
            double size = fileSize;
            int unitIndex = 0;
            while (size >= 1024 && unitIndex < units.Length - 1)
            {
                size /= 1024;
                unitIndex++;
            }
            // Use 0 decimal places for Bytes, and 2 decimal places for KB, MB, etc.
            return unitIndex == 0
                ? $"{size:0} {units[unitIndex]}"
                : $"{size:0.02} {units[unitIndex]}";
        }
        #endregion
    }
}
