using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using StackOverflow;

namespace ResizeImage
{
    public static class ResizeImage
    {
        /// <summary>
        /// Execute the image resize.
        /// </summary>
        /// <param name="file">The file to resize.</param>
        /// <param name="output">Where the new file will be written to</param>
        /// <param name="width">
        /// If zero, calculated to keep current apsect ratio with new height.
        /// </param>
        /// <param name="height">
        /// If zero, calculated to keep current apsect ratio with new width.
        /// </param>
        /// <param name="overwrite">
        /// When true, ignores the <paramref name="output"/> parameter and saves over the input
        /// <paramref name="file"/>.
        /// </param>
        public static void Exec(
            this FileInfo file,
            string output,
            int width,
            int height,
            long quality = 100L,
            bool overwrite = false)
        {
            Console.WriteLine($"Resize image {file}");
            try
            {
                MyConsole.Debug($"inputs: {file} ({width}x{height}) @ {quality} compression.");
                if (file == default)
                {
                    MyConsole.Warning("You must supply an file with --file.");
                    throw new MissingFieldException();
                }
                if (width == default && height == default)
                {
                    MyConsole.Warning("You must supply one or more of the following options: --width --height");
                    throw new MissingFieldException();
                }

                MyConsole.Debug(@$"Reading in image from ""{file.Name}"".");
                using (Image image = Image.FromFile(file.FullName))
                {
                    MyConsole.Debug(@$"Done reading image.");

                    MyConsole.Info($"Original dimensions: {image.Width}x{image.Height}");
                    MyConsole.Debug($"Checking for missing height or width.");
                    if (width == default)
                    {
                        MyConsole.Debug($"Width was {width}");
                        width = (int)( image.Width * (double)height / image.Height );
                        MyConsole.Debug($"Set width relative to new height: {width}");
                    }
                    if (height == default)
                    {
                        MyConsole.Debug($"Height was {height}");
                        height = (int)( image.Height * (double)width / image.Width );
                        MyConsole.Debug($"Set height relative to new width: {height}");
                    }

                    using Bitmap bitmap = Resize(image, width, height);

                    MyConsole.Info($"Saving image.");
                    if (overwrite)
                    {
                        output = file.FullName;
                    }
                    else
                    {
                        // Create a path for the output file.
                        output = file.CreateSavePath(output);
                    }

                    MyConsole.Debug($"Computed output: {output}");
                    MyConsole.Debug($"Saving bitmap");
                    bitmap.SaveJPEG(output, quality);
                }
                MyConsole.Success($@"Wrote image to: {output}");
            }
            catch (Exception e)
            {
                if (e is MissingFieldException || MyConsole.Verbosity < 1)
                {
                    MyConsole.Error("Failed to write image.");
                }
                else
                {
                    throw;
                }
            }
            System.Console.WriteLine("Done.");
        }

        /// <summary>
        /// Create a new path relative to this file. If the <paramref name="output"/> file path is 
        /// absolute, uses that alone. The extension of the original file is appended to the new file.
        /// </summary>
        /// <param name="file"></param>
        /// <param name="output"></param>
        /// <returns></returns>
        public static string CreateSavePath(this FileInfo file, string output = default)
        {
            MyConsole.Info($"Creating new save path: {file}->{output}");
            if (output == default)
            {
                MyConsole.Debug($"--output not supplied: calculating from original file name ({file}).");
                output = $"{Path.GetFileNameWithoutExtension(file.Name)}_copy";
            }
            MyConsole.Debug($"Given output: {output}");
            return Path.Combine(
                Path.GetDirectoryName(file.FullName),
                $"{output}{Path.GetExtension(file.Name)}");
        }

        /// <summary>
        /// Resize the image to the specified width and height.
        /// <para>
        /// Source: https://stackoverflow.com/a/24199315/6789816
        /// </para>
        /// </summary>
        /// <param name="image">The image to resize.</param>
        /// <param name="width">The width to resize to.</param>
        /// <param name="height">The height to resize to.</param>
        /// <returns>The resized image.</returns>
        public static Bitmap Resize(Image image, int width, int height)
        {
            MyConsole.Info($"Resizing image: {width}x{height}");
            MyConsole.Debug("Creating bitmap destination image.");
            Rectangle destRect = new Rectangle(0, 0, width, height);
            Bitmap destImage = new Bitmap(width, height);
            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            MyConsole.Debug("Get graphics from image.");
            using (Graphics graphics = Graphics.FromImage(destImage))
            {
                MyConsole.Debug(@"  - Setting up graphics modes:");
                graphics.CompositingMode = CompositingMode.SourceCopy;

                MyConsole.Debug("  - CompositingQuality: HighQuality");
                graphics.CompositingQuality = CompositingQuality.HighQuality;

                MyConsole.Debug("  - InterpolationMode: HighQualityBicubic");
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;

                MyConsole.Debug("  - SmoothingMode: HighQuality");
                graphics.SmoothingMode = SmoothingMode.HighQuality;

                MyConsole.Debug("  - PixelOffsetMode: HighQuality");
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                MyConsole.Debug("  - WrapMode: TileFlipXY");
                using ImageAttributes wrapMode = new ImageAttributes();
                wrapMode.SetWrapMode(WrapMode.TileFlipXY);

                MyConsole.Debug("  - Drawing image...");
                graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                MyConsole.Debug("Image drawn");
            }

            return destImage;
        }

        /// <summary>
        /// Wrapper for saving a <see cref="Bitmap"/> as a JPEG file.
        /// </summary>
        /// <param name="bitmap"></param>
        /// <param name="output"></param>
        /// <param name="quality"></param>
        public static void SaveJPEG(this Bitmap bitmap, string output, long quality)
        {
            MyConsole.Info($"Saving image as JPEG with {quality} compression quality.");

            MyConsole.Debug("Use ImageFormat.Jpeg encoder.");
            ImageCodecInfo jpgEncoder = GetEncoder(ImageFormat.Jpeg);

            // Create an Encoder object based on the GUID  for the Quality parameter category. 
            MyConsole.Debug("Create a quality encoder.");
            Encoder myEncoder = Encoder.Quality;

            // Create an EncoderParameters object.  
            // An EncoderParameters object has an array of EncoderParameter objects. 
            // In this case, there is only one EncoderParameter object in the array.  
            MyConsole.Debug("Declare 1 encoder parameter:");
            EncoderParameters myEncoderParameters = new EncoderParameters(1);
            EncoderParameter myEncoderParameter = new EncoderParameter(myEncoder, quality);

            MyConsole.Debug("  - Adding quality encoder.");
            myEncoderParameters.Param[0] = myEncoderParameter;

            MyConsole.Debug($"Saving bitmap to {output} with encoders.");
            bitmap.Save(output, jpgEncoder, myEncoderParameters);
        }

        private static ImageCodecInfo GetEncoder(ImageFormat format)
        {
            MyConsole.Debug($"Get Encoder for {format.Guid}");
            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();
            foreach (ImageCodecInfo codec in codecs)
            {
                MyConsole.Debug($"  - Trying Image Codec with FormatID: {codec.FormatID}.");
                if (codec.FormatID == format.Guid)
                {
                    MyConsole.Debug($"  - Image Codec matched for {codec.FormatID}.");
                    return codec;
                }
            }
            MyConsole.Debug($"No encoder found for format.");
            return null;
        }
    }
}
