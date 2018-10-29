using Newtonsoft.Json;

namespace Keyzoid.Core.MediaManager.Models
{
    public class Video
    {
        public string id { get; set; }
        public string title { get; set; }
        public string description { get; set; }
        public Thumbnail[] thumbnails { get; set; }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
        }
    }

    public class Thumbnail
    {
        public string type { get; set; }
        public string url { get; set; }
        public string height { get; set; }
        public string width { get; set; }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
        }
    }
}
