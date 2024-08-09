using BStockBot.Worker.Models;
using System.Net;

namespace BStockBot.Worker.Services
{
    public class CustomCookieClientHandler : HttpClientHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            // Modify cookies here before sending the request.
            CookieCollection cookies = this.CookieContainer.GetAllCookies();
            string cookieHeader = "";
            foreach (Cookie cookie in cookies)
            {
                if (cookie.Domain == "bstock.com" || cookie.Domain == "auth.bstock.com")
                {
                    cookie.Domain = BotUrl.GetHost();
                }

                cookieHeader += $"{cookie.Name}={cookie.Value}; ";
            }

            request.Headers.TryAddWithoutValidation("Cookie", cookieHeader);
            return base.SendAsync(request, cancellationToken);
        }
    }
}
