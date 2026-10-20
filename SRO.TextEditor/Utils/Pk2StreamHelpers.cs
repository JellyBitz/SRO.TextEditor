using SRO.PK2API;

using System;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace SRO.TextEditor.Utils
{
    /// <summary>
    /// Extensions helper for Pk2Stream.
    /// </summary>
    public static class Pk2StreamHelpers
    {
        /// <summary>
        /// Loads an image as BitmapImage from specified pk2 path (.ddj/.dds), otherwise returns null.
        /// </summary>
        public static BitmapImage GetBitmapImage(this Pk2Stream stream, string path)
        {
            try
            {
                // Find path
                var file = stream.GetFile(path);
                if (file == null)
                    return null;

                // Load into memory
                using (var ms = new MemoryStream(file.GetContent()))
                using (var br = new BinaryReader(ms, System.Text.Encoding.ASCII))
                {
                    // Check if it's DDJ or DDS
                    if (new string(br.ReadChars(12)) == "JMXVDDJ 1000")
                        br.BaseStream.Seek(20, SeekOrigin.Begin);
                    else
                    {
                        br.BaseStream.Seek(0, SeekOrigin.Begin);
                        if (new string(br.ReadChars(4)) == "DDS ")
                            br.BaseStream.Seek(0, SeekOrigin.Begin);
                        else
                            return null;
                    }

                    // Decode image data
                    byte[] imgData = null;
                    PixelFormat wpfPixelFormat = PixelFormats.Default;
                    using (var image = Pfim.Pfimage.FromStream(ms))
                    {
                        var stride = image.Stride;
                        // Pfim format to WPF PixelFormat
                        switch (image.Format)
                        {
                            case Pfim.ImageFormat.Rgb24:
                                wpfPixelFormat = PixelFormats.Bgr24;
                                imgData = new byte[image.Data.Length];
                                Buffer.BlockCopy(image.Data, 0, imgData, 0, image.Data.Length);
                                break;
                            case Pfim.ImageFormat.Rgba32:
                                wpfPixelFormat = PixelFormats.Bgra32;
                                imgData = new byte[image.Data.Length];
                                Buffer.BlockCopy(image.Data, 0, imgData, 0, image.Data.Length);
                                // Fix alpha channel if required
                                bool hasValidAlpha = false;
                                for (int i = 3; i < image.Data.Length; i += 4)
                                {
                                    if (image.Data[i] > 0)
                                    {
                                        hasValidAlpha = true;
                                        break;
                                    }
                                }
                                if (!hasValidAlpha)
                                    for (int i = 3; i < imgData.Length; i += 4)
                                        imgData[i] = 255;
                                break;
                            case Pfim.ImageFormat.R5g6b5:
                                wpfPixelFormat = PixelFormats.Bgr565;
                                imgData = new byte[image.Data.Length];
                                Buffer.BlockCopy(image.Data, 0, imgData, 0, image.Data.Length);
                                break;
                            case Pfim.ImageFormat.R5g5b5a1:
                                // Convert 16-bit A1R5G5B5 to 32-bit Bgra32 since WPF lacks a native 16-bit alpha format
                                wpfPixelFormat = PixelFormats.Bgra32;
                                imgData = Convert16BitTo32BitBgra(image.Data, image.Width, image.Height);
                                stride = image.Width * 4;
                                break;
                            default:
                                throw new NotSupportedException($"DDS format not supported [{image.Format}]");
                        }
                        // Create WPF BitmapSource from raw pixel bytes
                        BitmapSource bitmapSource = BitmapSource.Create(
                            image.Width,
                            image.Height,
                            96d,
                            96d,
                            wpfPixelFormat,
                            null,
                            imgData,
                            stride);

                        // Convert BitmapSource to BitmapImage for WPF binding
                        BitmapImage bitmapImage = new BitmapImage();
                        using (MemoryStream outStream = new MemoryStream())
                        {
                            var encoder = new PngBitmapEncoder();
                            encoder.Frames.Add(BitmapFrame.Create(bitmapSource));
                            encoder.Save(outStream);

                            // Create Bitmap Image
                            bitmapImage.BeginInit();
                            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                            bitmapImage.StreamSource = new MemoryStream(outStream.ToArray());
                            bitmapImage.EndInit();
                            // Freeze makes the BitmapImage cross-thread accessible
                            bitmapImage.Freeze();
                        }
                        return bitmapImage;
                    }
                }
            }
            catch
            {
                return null;
            }
        }
        // Helper method to expand 16-bit 5551 to standard 32-bit Bgra32
        private static byte[] Convert16BitTo32BitBgra(byte[] srcData, int width, int height)
        {
            byte[] destData = new byte[width * height * 4];
            int srcIndex = 0;
            int destIndex = 0;

            for (int i = 0; i < width * height; i++)
            {
                ushort pixel = BitConverter.ToUInt16(srcData, srcIndex);
                srcIndex += 2;

                // Bit extraction for A1R5G5B5
                byte alpha = (byte)((pixel & 0x8000) != 0 ? 255 : 0);
                byte red = (byte)(((pixel >> 10) & 0x1F) * 255 / 31);
                byte green = (byte)(((pixel >> 5) & 0x1F) * 255 / 31);
                byte blue = (byte)((pixel & 0x1F) * 255 / 31);

                destData[destIndex + 0] = blue;   // B
                destData[destIndex + 1] = green; // G
                destData[destIndex + 2] = red;   // R
                destData[destIndex + 3] = alpha; // A
                destIndex += 4;
            }

            return destData;
        }
    }
}
