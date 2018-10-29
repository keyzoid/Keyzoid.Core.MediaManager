namespace Keyzoid.Core.MediaManager.Models
{
    public partial class PhotoAlbum : Content
    {
        public string uniqueName { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public Picture[] pictures { get; set; }
        public string author { get; set; }
        public string feature { get; set; }
        public string thumb { get; set; }
        public string[] tags { get; set; }
    }
}
