using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using LinqToTwitter;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using TwitterBingBanner.Models;

namespace TwitterBingBanner
{
    public class TwitterBannerUpdater
    {
        private readonly IConfiguration _configuration;

        public TwitterBannerUpdater(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [FunctionName("TwitterBannerUpdater")]
        public async Task Run([TimerTrigger("0 0 0 * * *")] TimerInfo timer, ILogger log)
        {
            Uri bingImageUrl = await GetBingImageUrlAsync();
            var client = CreateVisionClient();
            using var imageStream = await client.GenerateThumbnailAsync(1020, 340, bingImageUrl.AbsoluteUri, smartCropping: false);
            using var streamReader = new MemoryStream();
            imageStream.CopyTo(streamReader);
            var imageData = streamReader.ToArray();
            var context = CreateTwitterContext();
            await context.UpdateProfileBannerAsync(imageData, 1020, 340, 0, 0);
        }

        private ComputerVisionClient CreateVisionClient()
        {
            var visionSection = _configuration.GetSection("CognitiveServices");
            var client = new ComputerVisionClient(new ApiKeyServiceClientCredentials(visionSection.GetValue<string>("Key")));
            client.Endpoint = visionSection.GetValue<string>("Endpoint");
            return client;
        }

        private async Task<Uri> GetBingImageUrlAsync()
        {
            var url = _configuration.GetValue<string>("BingPhotoOfTheDayUrl");
            var client = new HttpClient();
            var bingJson = await client.GetStringAsync(url);
            var bingImageInfoRoot = JsonConvert.DeserializeObject<BingImageInfoRoot>(bingJson);
            return new Uri(new Uri("https://bing.com"), bingImageInfoRoot.Images?.FirstOrDefault()?.Url);
        }

        private TwitterContext CreateTwitterContext()
        {
            var section = _configuration.GetSection("Twitter");
            var credentialStore = new SingleUserInMemoryCredentialStore
            {
                ConsumerKey = section.GetValue<string>("ApiKey"),
                ConsumerSecret = section.GetValue<string>("ApiSecretKey"),
                AccessToken = section.GetValue<string>("AccessToken"),
                AccessTokenSecret = section.GetValue<string>("AccessTokenSecret")
            };
            var authorizer = new SingleUserAuthorizer
            {
                CredentialStore = credentialStore
            };

            return new TwitterContext(authorizer);
        }
    }
}
