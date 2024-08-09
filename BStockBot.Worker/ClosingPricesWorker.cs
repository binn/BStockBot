using BStockBot.Worker.Models;
using BStockBot.Worker.Services;
using System.Net.Http.Json;

namespace BStockBot.Worker
{
    public class ClosingPricesWorker : BackgroundService
    {
        private readonly ILogger<ClosingPricesWorker> _logger;
        private readonly BStockService _bStock;
        private readonly BStockOptions _options;
        private readonly HttpClient _client;

        public ClosingPricesWorker(IHttpClientFactory factory, BStockOptions options, ILogger<ClosingPricesWorker> logger, BStockService bStock)
        {
            _logger = logger;
            _bStock = bStock;
            _options = options;
            _client = factory.CreateClient();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (true)
            {
                await Task.Delay(1000, stoppingToken);
                if (stoppingToken.IsCancellationRequested)
                    return;

                var closingPricesRequested = await _client.GetFromJsonAsync<List<ClosingPriceAuctionRequest>>(_options.Endpoint + "/api/auctions/closing/next", stoppingToken);
                if (closingPricesRequested.Count == 0)
                    continue;

                _logger.LogInformation("Fetched {count} auctions for closing prices.", closingPricesRequested.Count);

                var status = await _bStock.LoginAsync();
                _logger.LogInformation("Login Status: {status}", status);

                Dictionary<Guid, decimal> prices = new Dictionary<Guid, decimal>();
                foreach(var auction in closingPricesRequested)
                {
                    var price = await _bStock.GetClosingPriceAsync(auction.AuctionId);
                    if (price == null)
                        price = -1.00m;

                    prices.Add(auction.Id, price.GetValueOrDefault());
                    await Task.Delay(1250, stoppingToken);
                }

                _logger.LogInformation("Sending updated prices to server...");
                await _client.PostAsJsonAsync(_options.Endpoint + "/api/auctions/closing/complete", prices, stoppingToken);

                _logger.LogInformation("Operation complete. Restarting loop.");
            }
        }
    }
}