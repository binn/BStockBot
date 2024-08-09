using BStockBot.Api.Data;
using BStockBot.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BStockBot.Api.Controllers
{
    [ApiController]
    [Route("/api/auctions/closing")]
    public class ClosingPricesController : ControllerBase
    {
        private readonly BStockContext _ctx;
        private readonly ILogger<AuctionListController> _logger;
        private readonly ReportGenerationService _reports;

        public ClosingPricesController(BStockContext ctx, ILogger<AuctionListController> logger, ReportGenerationService reports)
        {
            _ctx = ctx;
            _logger = logger;
            _reports = reports;
        }

        [HttpGet]
        [Route("next")]
        public async Task<IActionResult> GetNextClosingAuctionAsync()
        {
            var latestAuctionDate = await _ctx.Auctions
                .Where(x => DateTime.Now > x.EndDate.AddHours(24) && !x.IsClosed)
                .MaxAsync(x => (DateTime?)x.EndDate);

            if (latestAuctionDate is null)
                return Ok(Array.Empty<object>());

            var auctions = await _ctx.Auctions
                .Where(x => x.EndDate == latestAuctionDate)
                .ToListAsync();

            var data = auctions.Select(x => new
            {
                x.Id,
                x.AuctionId,
                x.ManifestId,
                x.IsClosed,
                x.StartDate,
                x.EndDate,
            });

            return Ok(data);
        }

        [HttpPost]
        [Route("complete")]
        public async Task<IActionResult> CompleteClosingPriceAsync([FromBody] Dictionary<Guid, decimal> prices)
        {
            foreach(var price in prices)
            {
                var auction = await _ctx.Auctions.FirstOrDefaultAsync(x => x.Id == price.Key);
                auction.Price = price.Value;
                auction.IsClosed = true;
            }

            await _ctx.SaveChangesAsync();
            await _reports.CreateClosingPriceReport(prices.Select(x => x.Key).ToList());
            
            return NoContent();
        }
    }
}
