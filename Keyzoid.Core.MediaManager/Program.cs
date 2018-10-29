using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.FileExtensions;
using Microsoft.Extensions.Configuration.Json;
using System;
using System.Threading.Tasks;

namespace Keyzoid.Core.MediaManager
{
    /// <summary>
    /// Represents a media manager program to create/format/append media files for presentation on the web.
    /// </summary>
    public class Program
    {
        /// <summary>
        /// The type of media management to be performed.
        /// </summary>
        public enum Mode
        {
            Music,
            Photography
        }

        /// <summary>
        /// Main entry point into the application.
        /// </summary>
        /// <param name="args">The argument array.</param>
        static void Main(string[] args)
        {
            IConfiguration config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", true, true)
                .Build();

            var youTubeApplicationName = config["youTubeApplicationName"];
            var youTubeApiKey = config["youTubeApiKey"];
            var playlistSourcePath = config["playlistSourcePath"];
            var successExport = bool.TryParse(config["exportPlaylists"], out var exportPlaylists);
            var imageSourcePath = config["imageSourcePath"];
            var imageExportPath = config["imageExportPath"];
            var successMode = Enum.TryParse(typeof(Mode), config["mode"], out var mode);

            switch (mode)
            {
                case Mode.Music:
                    Console.WriteLine($"Started generating playlists from source folder: " +
                        $"{playlistSourcePath}.");

                    MusicManager.YouTubeApplicationName = youTubeApplicationName;
                    MusicManager.YouTubeApiKey = youTubeApiKey;
                    Task.Run(async () => {
                        await MusicManager.GeneratePlaylists(playlistSourcePath,
                            successExport && exportPlaylists);
                    }).Wait();
                    Console.WriteLine($"Completed generating playlists.");

                    break;
                case Mode.Photography:
                    Console.WriteLine($"Started generating photo albums from source folder: " +
                        $"{imageSourcePath}.");
                    PhotoManager.GenerateAlbum(imageSourcePath,
                        imageExportPath, PhotoManager.Mode.File);
                    Console.WriteLine($"Completed generating photo albums to destination folder: " +
                        $"{imageExportPath}.");

                    break;
                default:
                    break;
            }
        }
    }
}
