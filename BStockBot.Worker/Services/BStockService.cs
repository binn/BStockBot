using AngleSharp;
using AngleSharp.Dom;
using BStockBot.Worker.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BStockBot.Worker.Services
{
    public partial class BStockService
    {
        private readonly HttpClient _client;
        private readonly IBrowsingContext _context;
        private readonly BStockOptions _options;
        private static readonly Regex _exportRegex = ExportRegex();

        public BStockService(IHttpClientFactory factory, BStockOptions options)
        {
            _options = options;
            _context = BrowsingContext.New();
            _client = factory.CreateClient("BStock");
        }

        public async Task<LoginStatus> LoginAsync()
        {
            var nvc = new Dictionary<string, string>()
            {
                ["tenantId"] = "17e51778-a4db-4dd3-b31e-1f990f373099",
                ["client_id"] = "1b094c5f-c8a6-416c-8c62-4dc77ca88ce9", // these are most likely related to the marketplaces individually so i can go ahead and implement this later as things change
                ["metaData.device.name"] = "Windows+Chrome",
                ["metaData.device.type"] = "BROWSER",
                ["redirect_uri"] = BotUrl.GetRedirectUri(_options.Marketplace),
                ["response_type"] = "code",
                ["scope"] = "offline_access",
                ["state"] = "isRedirect",
                ["timezone"] = "America/Chicago",
                ["loginId"] = _options.Username,
                ["password"] = _options.Password
            };

            using var res = await _client.PostAsync("https://auth.bstock.com/oauth2/authorize", new FormUrlEncodedContent(nvc));
            if (res.StatusCode == HttpStatusCode.Found)
            {
                //string url = res.Headers.Location?.OriginalString.Replace("bstock.com", BotUrl.GetHost());
                using var response = await _client.GetAsync(res.Headers.Location?.OriginalString); // call the token authorize url, which causes the cookies to be set in the HttpClient

                if (response.StatusCode == HttpStatusCode.Found) // Have no way to look into the CookieContainer for now, so we'll just assume login was successful
                    return LoginStatus.OK;
            }
            else if (res.StatusCode == HttpStatusCode.OK) // this isn't really ok lmao
            {
                string content = await res.Content.ReadAsStringAsync();
                if (content.Contains("Invalid login credentials.")) // check if password details are incorrect, this would be different compared to failing for unknown oauth2 reasons
                    return LoginStatus.IncorrectDetails;
            }

            return LoginStatus.Failed; // return this otherwise if something went wrong
        }

        public async Task<string> GetAuctionListAsync()
        {
            List<string> lists = new List<string>();
            string nextPageUrl = BotUrl.GetListingUri(_options.Marketplace);

            do
            {
                using var res = await _client.GetAsync(nextPageUrl);
                res.EnsureSuccessStatusCode();


                string content = await res.Content.ReadAsStringAsync();
                var document = await _context.OpenAsync(req => req.Content(content));

                var li = document.QuerySelector("ul.pages")?.Children?
                    .Where(x => x.FirstElementChild?.FirstElementChild?.ClassName == "fa fa-chevron-right")?
                    .FirstOrDefault();

                nextPageUrl = li?.FirstElementChild.GetAttribute("href");

                var match = _exportRegex.Match(content);
                string url = match.Groups["url"].Value;
                lists.Add(await _client.GetStringAsync(url));
            }
            while (nextPageUrl != null);
            return string.Join("\n", lists);
        }

        [GeneratedRegex(@"\(""#my_export""\)\.click\(function\(\)\s{\n\s+window\.open\('(?<url>.+)'\);\n\s+}\);")]
        private static partial Regex ExportRegex();

        public async Task<decimal?> GetClosingPriceAsync(int auctionId)
        {
            string url = BotUrl.GetAuctionUri(_options.Marketplace, auctionId.ToString());
            using var res = await _client.GetAsync(url);

            if (res.StatusCode == HttpStatusCode.TooManyRequests)
            {
                await Task.Delay(25_000);
                return await this.GetClosingPriceAsync(auctionId);
            }
            //else if (res.StatusCode == HttpStatusCode.Found || res.StatusCode == HttpStatusCode.MovedPermanently)
            //{
            //    // double check against 404 before returning null, implying 404.
            //    if (res.Headers.Location?.OriginalString == BotUrl.GetBaseUri(_options.Marketplace)
            //        || res.Headers.Location?.OriginalString == BotUrl.GetNotFoundUri(_options.Marketplace))
            //        return null;
            //    else
            //        return null;
            //    // Figure this stuff out later.
            //    //throw new Exception("Failed to obtain auction due to unknown redirect! Auction redirect: " + res.Headers.Location?.OriginalString ?? "null");
            //}
            //else if (res.StatusCode != HttpStatusCode.OK)
            //    return null;
            // I'll do some proper error handling and possibly password changing later.
            //throw new Exception("Failed to obtain auction! Auction status code: " + res.StatusCode);

            // Due to for some reason BStock still sending a 302 rather than something else, I just have to trust this will work

            string content = await res.Content.ReadAsStringAsync();
            var document = await _context.OpenAsync(req => req.Content(content));
            var element = document.QuerySelector("script[type='application/ld+json']");

            if (element != null)
            {
                var data = JsonSerializer.Deserialize<AuctionOffer>(element.InnerHtml, new JsonSerializerOptions(JsonSerializerDefaults.Web));
                return data.Offers.Price;
            }

            return null;
        }
    }
}
