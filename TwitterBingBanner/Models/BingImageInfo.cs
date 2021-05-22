using System.Collections.Generic;

namespace TwitterBingBanner.Models
{
    public class BingImage
    {
        public string Url { get; set; }
    }

    public class BingImageInfoRoot
    {
        public List<BingImage> Images { get; set; }
    }
}
