using System;
using System.IO;
using System.Threading.Tasks;
using LinqToTwitter;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace TwitterBingBanner
{
    public static class TwitterBannerUpdater
    {
        [FunctionName("TwitterBannerUpdater")]
        public static async Task Run([TimerTrigger("0 0 0 * * *")] TimerInfo timer, ILogger log)
        {

            string bingImageUrl = "?";
            var client = new ComputerVisionClient(new ApiKeyServiceClientCredentials(""));
            using var imageStream = await client.GenerateThumbnailAsync(1500, 500, bingImageUrl, smartCropping: true);
            using var streamReader = new MemoryStream();
            imageStream.CopyTo(streamReader);
            var imageData = streamReader.ToArray();
            var twitterCtx = new TwitterContext();
            await twitterCtx.UpdateProfileBannerAsync(imageData, 1500, 500, 0, 0);


            log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");
        }
    }
}
