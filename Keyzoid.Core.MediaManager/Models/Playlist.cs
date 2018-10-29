namespace Keyzoid.Core.MediaManager.Models
{
    public partial class Playlist : Content
    {
        public string uniqueName { get; set; }
        public string title { get; set; }
        public string description { get; set; }
        public string author { get; set; }
        public string image { get; set; }
        public string thumbnail { get; set; }
        public Track[] tracks { get; set; }
        public string[] tags { get; set; }
    }
}
