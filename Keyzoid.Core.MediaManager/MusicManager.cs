using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using Keyzoid.Core.MediaManager.Models;
using Keyzoid.Core.MediaManager.Utilities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Keyzoid.Core.MediaManager
{
    /// <summary>
    /// Represents a class for managing music files, playlists and other audio media.
    /// </summary>
    public class MusicManager
    {
        public static string YouTubeApiKey { get; set; }
        public static string YouTubeApplicationName { get; set; }

        /// <summary>
        /// Generates playlist models with YouTube videos from device playlists files.
        /// </summary>
        /// <param name="path">Local path with m3u/m3u8 playlists from which to generate local model playlists.</param>
        /// <param name="export">True to export the resulting playlist locally to the path provided.</param>
        public static async Task GeneratePlaylists(string path, bool export = false)
        {
            try
            {
                var playlists = new List<Playlist>();
                var m3uExtensions = new[] { ".m3u8", ".m3u" };
                var m3ufiles = Directory
                    .GetFiles(path)
                    .Where(file => m3uExtensions.Any(file.ToLower().EndsWith))
                    .ToList();

                foreach (var file in m3ufiles)
                {
                    var fInfo = new FileInfo(file);
                    var fName = fInfo.Name.Replace(fInfo.Extension, string.Empty);

                    var playlist = new Playlist
                    {
                        uniqueName = fName,
                        createdOn = TimeUtility.GetUnixTime()
                    };

                    // If playlists are simply named YYYYMMDD.m3u, default the local playlist title to a prettier full date string
                    if (int.TryParse(fName, out var n))
                    {
                        var success = DateTime.TryParseExact(fName, "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var d);

                        if (success)
                        {
                            playlist.title = $"{d.ToString("m")}, {d.Year.ToString()}";
                        }
                    }

                    var tracks = new List<Track>();

                    // Read the m3u playlist and create tracks
                    using (var reader = new StreamReader(file))
                    {
                        var track = new Track();

                        while (reader.Peek() >= 0)
                        {
                            var line = reader.ReadLine();

                            if (line.StartsWith("#EXTINF"))
                            {
                                track = new Track();
                                track.artist = line.Split(',')[1];
                            }
                            else if (line.StartsWith("\\") || line.StartsWith("D:\\") || line.StartsWith("E:\\"))
                            {
                                var row = line.Split('\\');
                                var fileName = row.Last();

                                track.name = track.artist.Split('-').Last().TrimStart();
                                track.artist = track.artist.Split('-').First().TrimEnd();

                                var videos = await Search($"{track.artist} {track.name}", 1);

                                if (videos.Any())
                                    track.video = videos.First();
                                else
                                {
                                    Console.WriteLine($"{track.artist} {track.name}");
                                }

                                tracks.Add(track);
                            }
                        }
                    }

                    playlist.tracks = tracks.ToArray();
                    playlists.Add(playlist);
                }

                if (export)
                {
                    var json = JsonConvert.SerializeObject(playlists, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                    var jsonFileName = $"{path}\\playlists_{DateTime.Now.ToString("yyyyMMddhhmmss")}.json";

                    using (var writer = new StreamWriter(jsonFileName))
                    {
                        writer.WriteLine(json);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR: Handler exception. Error: {ex.Message}. {ex.StackTrace}.");
                throw;
            }
        }

        /// <summary>
        /// Performs a YouTube search, including videos, channels, and playlists.
        /// </summary>
        /// <param name="searchText">The text for which to search.</param>
        /// <returns>A collection of YouTube items.</returns>
        public static async Task<IEnumerable<Video>> Search(string searchText, int maxResults = 50)
        {
            try
            {
                var youtubeService = new YouTubeService(new BaseClientService.Initializer()
                {
                    ApiKey = YouTubeApiKey,
                    ApplicationName = YouTubeApplicationName
                });

                var searchListRequest = youtubeService.Search.List("snippet");
                searchListRequest.Q = searchText;
                searchListRequest.MaxResults = maxResults;

                var searchListResponse = await searchListRequest.ExecuteAsync();
                var results = new List<Video>();
                var rand = new Random();

                foreach (var searchResult in searchListResponse.Items)
                {
                    switch (searchResult.Id.Kind)
                    {
                        case "youtube#video":

                            var thumbs = new List<Thumbnail>();
                            var item = new Video
                            {
                                id = searchResult.Id.VideoId,
                                title = searchResult.Snippet.Title,
                                description = searchResult.Snippet.Description
                            };

                            if (searchResult.Snippet.Thumbnails.Default__ != null)
                            {
                                thumbs.Add(new Thumbnail
                                {
                                    type = "default",
                                    url = searchResult.Snippet.Thumbnails.Default__.Url,
                                    height = searchResult.Snippet.Thumbnails.Default__.Height.ToString(),
                                    width = searchResult.Snippet.Thumbnails.Default__.Width.ToString()
                                });
                            }

                            if (searchResult.Snippet.Thumbnails.Medium != null)
                            {
                                thumbs.Add(new Thumbnail
                                {
                                    type = "medium",
                                    url = searchResult.Snippet.Thumbnails.Medium.Url,
                                    height = searchResult.Snippet.Thumbnails.Medium.Height.ToString(),
                                    width = searchResult.Snippet.Thumbnails.Medium.Width.ToString()
                                });
                            }

                            if (searchResult.Snippet.Thumbnails.High != null)
                            {
                                thumbs.Add(new Thumbnail
                                {
                                    type = "high",
                                    url = searchResult.Snippet.Thumbnails.High.Url,
                                    height = searchResult.Snippet.Thumbnails.High.Height.ToString(),
                                    width = searchResult.Snippet.Thumbnails.High.Width.ToString()
                                });
                            }

                            if (thumbs.Any())
                            {
                                item.thumbnails = thumbs.ToArray();
                            }

                            results.Add(item);
                            break;
                    }
                }

                return results;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR: Handler exception. Error: {ex.Message}. {ex.StackTrace}.");
                throw;
            }
        }
    }
}
