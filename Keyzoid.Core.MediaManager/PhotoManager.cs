using Keyzoid.Core.MediaManager.Models;
using Keyzoid.Core.MediaManager.Utilities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Keyzoid.Core.MediaManager
{
    /// <summary>
    /// Photo class that resizes images in a folder and add watermarks to images.
    /// </summary>
    public class PhotoManager
    {
        #region Classes

        public class PhotoAlbumWithChanges
        {
            public PhotoAlbum PhotoAlbum { get; set; }
            public Dictionary<string, string> FilesToCopy { get; set; }
        }

        public enum Mode
        {
            None,
            File,
        }

        #endregion

        #region Fields

        private const int PageSize = 5;
        private const int ThumbSmallPixels = 150;
        private const int ThumbMediumPixels = 300;
        private const int ThumbMediumPlusPixels = 450;

        private const string WatermarkPath = @"C:\PATH-TO-FILE\watermark-65-65.png";

        #endregion

        #region Methods (Public)

        /// <summary>
        /// Generates photos of different sizes for directories of albums.
        /// </summary>
        public static void GeneratePhotos(string inputPath, string outputPath = null)
        {
            try
            {
                if (string.IsNullOrEmpty(outputPath))
                    outputPath = inputPath;

                Console.WriteLine($"Started generating photos for paths (input) '{inputPath}' (output) '{outputPath}'.");

                var pathSmall = Path.Combine(outputPath, "preview-small");
                var pathMedium = Path.Combine(outputPath, "preview-medium");
                var pathMediumPlus = Path.Combine(outputPath, "preview-medium-plus");

                if (!Directory.Exists(pathSmall))
                    Directory.CreateDirectory(pathSmall);
                if (!Directory.Exists(pathMedium))
                    Directory.CreateDirectory(pathMedium);
                if (!Directory.Exists(pathMediumPlus))
                    Directory.CreateDirectory(pathMediumPlus);

                foreach (var picture in Directory.GetFiles(inputPath, "*.jpg", SearchOption.AllDirectories))
                {
                    var pInfo = new FileInfo(picture);
                    var image = Image.FromFile(picture);
                    var currentHeight = (decimal)image.Height;
                    var currentWidth = (decimal)image.Width;
                    var smallPath = Path.Combine(pathSmall, pInfo.Name);
                    var mediumPath = Path.Combine(pathMedium, pInfo.Name);
                    var mediumPlusPath = Path.Combine(pathMediumPlus, pInfo.Name);
                    var ratio = currentWidth / currentHeight;

                    var thumbSizeSmall = ratio < 1
                        ? new Size(ThumbSmallPixels, (int)Math.Floor(ThumbSmallPixels / ratio))
                        : new Size((int)Math.Floor(ThumbSmallPixels * ratio), ThumbSmallPixels);

                    var thumbSizeMedium = ratio < 1
                        ? new Size(ThumbMediumPixels, (int)Math.Floor(ThumbMediumPixels / ratio))
                        : new Size((int)Math.Floor(ThumbMediumPixels * ratio), ThumbMediumPixels);

                    var thumbSizeMediumPlus = ratio < 1
                        ? new Size(ThumbMediumPlusPixels, (int)Math.Floor(ThumbMediumPlusPixels / ratio))
                        : new Size((int)Math.Floor(ThumbMediumPlusPixels * ratio), ThumbMediumPlusPixels);

                    if (!File.Exists(smallPath))
                    {
                        using (var small = ResizeImage(image, thumbSizeSmall.Width, thumbSizeSmall.Height, true, true))
                        {
                            small.Save(smallPath);
                        }
                    }

                    if (!File.Exists(mediumPath))
                    {
                        using (var medium = ResizeImage(image, thumbSizeMedium.Width, thumbSizeMedium.Height, true, true))
                        {
                            medium.Save(mediumPath);
                        }
                    }

                    if (!File.Exists(mediumPlusPath))
                    {
                        using (var mediumPlus = ResizeImage(image, thumbSizeMediumPlus.Width, thumbSizeMediumPlus.Height, true, true))
                        {
                            mediumPlus.Save(mediumPlusPath);
                        }
                    }
                }

                Console.WriteLine($"Completed generating photos for path: {inputPath}.");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }
        }

        /// <summary>
        /// Generates a json string of a new album and web-ready photo copies from a raw input directory of files with tags.
        /// </summary>
        /// <param name="inputFolder">The source folder with raw images (IMG_123.jpg) with tags that will become an album.</param>
        /// <param name="outputFolder">The destination folder for the image copies with pretty names.</param>
        /// <param name="mode">What actions to take.</param>
        public static string GenerateAlbum(string inputFolder, string outputFolder, Mode mode = Mode.None)
        {
            try
            {
                Console.WriteLine($"Started generating albums with input/output: {inputFolder} & {outputFolder}.");

                if (string.IsNullOrEmpty(inputFolder) || string.IsNullOrEmpty(outputFolder) || !Directory.Exists(inputFolder))
                {
                    Console.WriteLine("Missing inputs");
                    return null;
                }

                var dInfo = new DirectoryInfo(inputFolder);
                var albumName = dInfo.Name.Replace(' ', '-');

                PhotoAlbumWithChanges newAlbum = LocalToAlbum(dInfo, inputFolder);

                if (newAlbum == null)
                {
                    Console.WriteLine("New album result is empty.");
                    return null;
                }

                var outputPath = Path.Combine(outputFolder, $"{dInfo.Name}.{DateTime.Now.ToString("yyyyMMddhhmmss")}");
                DirectoryInfo outputInfo = null;

                if (mode == Mode.File)
                {
                    if (!Directory.Exists(outputPath))
                    {
                        outputInfo = Directory.CreateDirectory(outputPath);
                    }

                    foreach (var copy in newAlbum.FilesToCopy)
                    {
                        var from = Path.Combine(dInfo.FullName, copy.Key);
                        var to = Path.Combine(outputInfo.FullName, copy.Value);
                        File.Copy(from, to);
                    }

                    GeneratePhotos(outputInfo.FullName, outputPath);
                }

                var json = JsonConvert.SerializeObject(newAlbum.PhotoAlbum, 
                    new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                Console.WriteLine($"Album '{newAlbum.PhotoAlbum.name}' created.");
                Console.WriteLine("Completed generating album.");
                return json;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }
        }

        #endregion

        #region Methods (Private)

        /// <summary>
        /// Generates a local photo album from a directory of pictures.
        /// </summary>
        /// <param name="dInfo">The directory with the pictures.</param>
        /// <param name="pictureDirectory">The source photo directory path.</param>
        /// <returns>A local photo album with a collection of changed files to copy.</returns>
        private static PhotoAlbumWithChanges LocalToAlbum(DirectoryInfo dInfo, string pictureDirectory)
        {
            try
            {
                System.Console.WriteLine($"Started creating album from pictures: {dInfo.Name} & {pictureDirectory}.");

                var newAlbum = new PhotoAlbum
                {
                    uniqueName = dInfo.Name.ToLower().Replace(' ', '-'),
                    name = dInfo.Name,
                    description = null,
                    pictures = null,
                    feature = null,
                    thumb = null,
                    createdOn = TimeUtility.GetUnixTime(),
                    isActive = false
                };
                var pictures = new List<Picture>();
                var fileMap = new Dictionary<string, string>();

                foreach (var picture in Directory.GetFiles(pictureDirectory, "*.jpg"))
                {
                    var fInfo = new FileInfo(picture);
                    var albumPicture = new Picture
                    {
                        caption = null,
                        image = fInfo.Name,
                        name = fInfo.Name.Split('.')[0],
                        isPortrait = null,
                        tags = null
                    };

                    var image = new Bitmap(picture);
                    var pItemSubject = image.PropertyItems.FirstOrDefault(pi => pi.Id == 40095);
                    var pItemTags = image.PropertyItems.FirstOrDefault(pi => pi.Id == 40094);
                    var subject = string.Empty;
                    var tags = new string[] { };

                    if (pItemSubject == null && pItemTags == null)
                        continue;
                    if (pItemSubject != null)
                    {
                        subject = Encoding.Unicode.GetString(pItemSubject.Value).Replace("\0", string.Empty);
                    }
                    if (pItemTags != null)
                    {
                        tags = Encoding.Unicode.GetString(pItemTags.Value).Replace("\0", string.Empty).Split(';');
                    }

                    // Generate a pretty file name from the Subject metadata
                    if (pItemSubject != null && !string.IsNullOrEmpty(subject) && pItemTags == null)
                    {
                        var derivedImage = subject.Replace(" ", "-").ToLower();
                        derivedImage = Regex.Replace(derivedImage, @"[^\w\.@-]", "", RegexOptions.None, TimeSpan.FromSeconds(5));

                        var derivedName = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(subject);
                        var nameCount = pictures.Count(p => p.name == derivedName);

                        // Handle files with the same Subject and add a number to the end for uniqueness
                        if (nameCount > 0)
                        {
                            derivedImage += $"-{nameCount}";
                        }

                        derivedImage = $"{derivedImage}{fInfo.Extension}";
                        fileMap[albumPicture.image] = derivedImage;

                        if (albumPicture.name != derivedName)
                        {
                            albumPicture.name = derivedName;
                        }

                        if (albumPicture.image != derivedImage)
                        {
                            albumPicture.image = derivedImage;
                        }
                    }
                    else
                    {
                        // Generate a pretty file name from the Tags metadata
                        albumPicture.tags = tags;

                        var derivedImage = string.Join(" ", albumPicture.tags).Replace(" ", "-").ToLower();
                        derivedImage = Regex.Replace(derivedImage, @"[^\w\.@-]", "", RegexOptions.None, TimeSpan.FromSeconds(5));

                        var derivedName = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(string.Join(" ", albumPicture.tags));
                        var nameCount = pictures.Count(p => p.name == derivedName);

                        // Handle files with the same Tags and add a number to the end for uniqueness
                        if (nameCount > 0)
                        {
                            derivedImage += $"-{nameCount}";
                        }

                        derivedImage = $"{derivedImage}{fInfo.Extension}";
                        fileMap[albumPicture.image] = derivedImage;

                        if (albumPicture.name != derivedName)
                        {
                            albumPicture.name = derivedName;
                        }

                        if (albumPicture.image != derivedImage)
                        {
                            albumPicture.image = derivedImage;
                        }
                    }

                    if (image.Height > image.Width)
                    {
                        albumPicture.isPortrait = true;
                    }

                    pictures.Add(albumPicture);
                }

                newAlbum.pictures = pictures.ToArray();

                // Exclude Portrait photos from being features or thumbs
                var pics = newAlbum.pictures.Where(p => !p.isPortrait.HasValue || !p.isPortrait.Value).ToList();
                var rand = new Random();

                newAlbum.feature = pics[rand.Next(pics.Count)].image;
                newAlbum.thumb = pics[rand.Next(pics.Count)].image;
                System.Console.WriteLine($"Completed creating album from pictures: {dInfo.Name} & {pictureDirectory}.");

                return new PhotoAlbumWithChanges
                {
                    PhotoAlbum = newAlbum,
                    FilesToCopy = fileMap
                };
            }
            catch (Exception ex)
            {
                System.Console.WriteLine(ex);
                throw;
            }
        }

        /// <summary>Resizes an image to a new width and height.</summary>
        /// <param name="image">The image to resize.</param>
        /// <param name="width">When resizing the image, this is the maximum width to resize the image to.</param>
        /// <param name="height">When resizing the image, this is the maximum height to resize the image to.</param>
        /// <param name="enforceRatio">Indicates whether to keep the width/height ratio aspect or not. If set to false, images with an unequal width and height will be distorted and padding is disregarded. If set to true, the width/height ratio aspect is maintained and distortion does not occur.</param>
        /// <param name="addPadding">Indicates whether fill the smaller dimension of the image with a white background. If set to true, the white padding fills the smaller dimension until it reach the specified max width or height. This is used for maintaining a 1:1 ratio if the max width and height are the same.</param>
        private static Bitmap ResizeImage(Image image, int width, int height, bool enforceRatio, bool addPadding)
        {
            var canvasWidth = width;
            var canvasHeight = height;
            var newImageWidth = width;
            var newImageHeight = height;
            var xPosition = 0;
            var yPosition = 0;

            const int OrientationKey = 0x0112;
            const int NotSpecified = 0;
            const int NormalOrientation = 1;
            const int MirrorHorizontal = 2;
            const int UpsideDown = 3;
            const int MirrorVertical = 4;
            const int MirrorHorizontalAndRotateRight = 5;
            const int RotateLeft = 6;
            const int MirorHorizontalAndRotateLeft = 7;
            const int RotateRight = 8;

            if (enforceRatio)
            {
                var ratioX = width / (double)image.Width;
                var ratioY = height / (double)image.Height;
                var ratio = ratioX < ratioY ? ratioX : ratioY;
                newImageHeight = (int)(image.Height * ratio);
                newImageWidth = (int)(image.Width * ratio);

                if (addPadding)
                {
                    xPosition = (int)((width - (image.Width * ratio)) / 2);
                    yPosition = (int)((height - (image.Height * ratio)) / 2);
                }
                else
                {
                    canvasWidth = newImageWidth;
                    canvasHeight = newImageHeight;
                }
            }

            var thumbnail = new Bitmap(canvasWidth, canvasHeight);

            using (var graphic = Graphics.FromImage(thumbnail))
            {
                if (enforceRatio && addPadding)
                {
                    graphic.Clear(Color.Transparent);
                }

                graphic.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphic.SmoothingMode = SmoothingMode.HighQuality;
                graphic.PixelOffsetMode = PixelOffsetMode.HighQuality;
                graphic.CompositingQuality = CompositingQuality.HighQuality;
                graphic.DrawImage(image, xPosition, yPosition, newImageWidth, newImageHeight);

                // Fix orientation if needed.
                if (image.PropertyIdList.Contains(OrientationKey))
                {
                    var orientation = (int)image.GetPropertyItem(OrientationKey).Value[0];
                    switch (orientation)
                    {
                        case NotSpecified: // Assume it is good.
                        case NormalOrientation:
                            // No rotation required.
                            break;
                        case MirrorHorizontal:
                            thumbnail.RotateFlip(RotateFlipType.RotateNoneFlipX);
                            break;
                        case UpsideDown:
                            thumbnail.RotateFlip(RotateFlipType.Rotate180FlipNone);
                            break;
                        case MirrorVertical:
                            thumbnail.RotateFlip(RotateFlipType.Rotate180FlipX);
                            break;
                        case MirrorHorizontalAndRotateRight:
                            thumbnail.RotateFlip(RotateFlipType.Rotate90FlipX);
                            break;
                        case RotateLeft:
                            thumbnail.RotateFlip(RotateFlipType.Rotate90FlipNone);
                            break;
                        case MirorHorizontalAndRotateLeft:
                            thumbnail.RotateFlip(RotateFlipType.Rotate270FlipX);
                            break;
                        case RotateRight:
                            thumbnail.RotateFlip(RotateFlipType.Rotate270FlipNone);
                            break;
                        default:
                            throw new NotImplementedException("An orientation of " + orientation + " isn't implemented.");
                    }
                }

                return thumbnail;
            }
        }

        /// <summary>
        /// Adds the watermark to the image.
        /// </summary>
        /// <param name="image">The image to which to add watermark.</param>
        private static void AddWatermark(Image image)
        {
            using (var watermarkImage = Image.FromFile(WatermarkPath))
            using (var graphic = Graphics.FromImage(image))
            {
                int x = ((image.Width - watermarkImage.Width) / 2);
                int y = image.Height - watermarkImage.Height;

                graphic.DrawImage(watermarkImage, x, y, watermarkImage.Width, watermarkImage.Height);
            }
        }

        #endregion
    }
}
