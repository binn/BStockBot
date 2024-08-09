using BStockBot.Worker.Models;
using BStockBot.Worker.Services;
using CsvHelper;
using System.Globalization;
using System.Net.Http.Json;

namespace BStockBot.Worker
{
    public class AuctionListWorker : BackgroundService
    {
        private readonly ILogger<AuctionListWorker> _logger;
        private readonly BStockService _bStock;
        private readonly BStockOptions _options;
        private readonly IHostApplicationLifetime _lifetime;
        private readonly HttpClient _client;

        public AuctionListWorker(IHostApplicationLifetime lifetime, IHttpClientFactory factory, BStockOptions options, ILogger<AuctionListWorker> logger, BStockService bStock)
        {
            _logger = logger;
            _bStock = bStock;
            _options = options;
            _lifetime = lifetime;
            _client = factory.CreateClient();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
#if !DEBUG
            _logger.LogInformation("Waiting 15s before starting process...");
            await Task.Delay(15_000, stoppingToken);
#endif
            var status = await _bStock.LoginAsync();
            _logger.LogInformation("Login Status: {status}", status);

            _logger.LogInformation("Fetching auction list.");
            var auctionListStr = await _bStock.GetAuctionListAsync();
            List<BStockAuction> auctions = new List<BStockAuction>();

            using var streamReader = new StringReader(auctionListStr);
            using var csv = new CsvReader(streamReader, CultureInfo.InvariantCulture);
            var records = csv.GetRecords<BStockAuction>();

            auctions.AddRange(records
                .Where(x => x.LotNo != "Lot #")
                .Where(x => !string.IsNullOrWhiteSpace(x.LotNo))
                .Where(x => !x.ClosesToday())
                .ToList());

            auctions.ForEach(x => x.AuctionId = x.Url?.Split('/')?.Last());
            _logger.LogInformation("Sending auction list to server.");

            using var res = await _client.PostAsJsonAsync(_options.Endpoint + "/api/auctions/list", auctions, stoppingToken);
            res.EnsureSuccessStatusCode();

            _logger.LogInformation("Operation complete. Exiting...");
            _lifetime.StopApplication();
        }
    }
}