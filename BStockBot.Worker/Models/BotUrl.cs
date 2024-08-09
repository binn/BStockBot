namespace BStockBot.Worker.Models
{
    public class BotUrl
    {
        public static string GetHost() =>
            "198.91.24.83";

        public static string GetBaseUri(string marketplace) =>
            $"https://{GetHost()}/{marketplace}/";

        public static string GetBaseUriNormal(string marketplace) =>
            $"https://bstock.com/{marketplace}/";

        public static string GetRedirectUri(string marketplace) =>
            GetBaseUriNormal(marketplace) + "sso/index/login/";

        public static string GetAuctionUri(string marketplace, string auctionId) =>
            GetBaseUri(marketplace) + "auction/auction/view/id/" + auctionId + "/";

        public static string GetNotFoundUri(string marketplace) =>
            GetBaseUri(marketplace) + "auction404/";

        public static string GetListingUri(string marketplace) =>
            GetBaseUri(marketplace) + "auction/auction/list/";
    }
}
