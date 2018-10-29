# Keyzoid.Core.MediaManager

A media manager program to create/format/append media files for presentation on the web.

## Getting Started

This solution creates and manipulates local media files for presentation on the web. It currently supports two modes: Music and Photography.

Music mode reads local .m3u and .m3u8 playlists (exported from your favorite music player) and creates .json files based on Playlist, Track and Video Model objects. The idea is to take the basic data present in the local playlist and pull out artist and track info and add videos (with thumbmails) by searching YouTube. The output is much more rich, displays nicely on a webpage, and shares well with others.

Photography mode formats raw local images (from your phone or camera) for web viewing, pretties up the titles and file names by using image metadata, and creates .json files based on PhotoAlbum and Picture Model objects. This makes for quicker web publishing by letting you add subjects and tags to your images locally on the file system, quickly and in bulk, and then generating pretty titles and scaled images for web viewing.

See examples of this in action here:

* [Music Playlists](http://jasondkeys.com/music/playlists) - Compiled from .m3u files exported from Google Play with videos links from YouTube.
* [Photo Albums](http://jasondkeys.com/albums) - Rendered in 3 different sizes from raw photo folders exported from a phone.

### Prerequisites

.m3u or .m3u8 playlists and a YouTube API key for Music Mode and a folder with pictures for Photography mode. Photos should have EXIF data tags or subjects for best results.

### Installing

Update the appsettings.json file with details of the prerequisites above. You can alter the "mode" appsetting to run in one mode or the other: "Music" or "Photography".

## Built With

* [.NET Core](https://github.com/dotnet/core) - The console application framework used.
* [YouTube API](https://github.com/googleapis/google-api-dotnet-client) - Google API.

## Authors

* **Jason Keys** - *Initial work* - [Keyzoid](http://keyzoid.com)

## Acknowledgments

The following links proved useful on the journey to creating this solution.

* [exiv2.org](http://www.exiv2.org/tags.html) - Standard Exif Tags