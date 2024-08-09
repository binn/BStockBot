using BStockBot.Api.Data;
using BStockBot.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace BStockBot.Api.Controllers
{
    [ApiController]
    [Route("/api/auctions/list")]
    public class AuctionListController : ControllerBase
    {
        private readonly BStockContext _ctx;
        private readonly ReportGenerationService _reports;
        private readonly ILogger<AuctionListController> _logger;

        public AuctionListController(ReportGenerationService reports, BStockContext ctx, ILogger<AuctionListController> logger)
        {
            _ctx = ctx;
            _reports = reports;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> CreateAuctionListAsync([FromBody] List<BStockAuction> auctions)
        {
            var dct = new Dictionary<int, object>();
            var list = auctions.GroupBy(x => x.EndDate);

            foreach (var datedAuction in list)
            {
                var auctionsOnDate = datedAuction.GroupBy(x => x.AuctionId);
                var created = new List<Auction>();

                foreach (var auctionList in auctionsOnDate)
                {
                    var rndAuction = auctionList.FirstOrDefault();
                    var auction = new Auction()
                    {
                        AuctionId = int.Parse(auctionList.Key),
                        EndDate = DateTime.Parse(rndAuction.EndDate),
                        IsClosed = false,
                        ManifestId = rndAuction.LotNo,
                        StartDate = DateTime.Today
                    };

                    foreach (var act in auctionList)
                    {
                        var bstockManifest = new BStockAuction()
                        {
                            AuctionId = act.AuctionId,
                            Capacity = act.Capacity,
                            Category = act.Category,
                            Color = act.Color,
                            EndDate = act.EndDate,
                            Grade = act.Grade,
                            ItemDescription = act.ItemDescription,
                            LotNo = act.LotNo,
                            Model = act.Model,
                            OEM = act.OEM,
                            PackageType = act.PackageType,
                            PartNo = act.PartNo,
                            Quantity = act.Quantity,
                        };

                        auction.Manifest.Add(bstockManifest);
                    }

                    dct.Add(auction.AuctionId, new { auction.Id, auction.Manifest.Count });
                    await _ctx.Auctions.AddAsync(auction);
                    created.Add(auction);
                }

                await _ctx.SaveChangesAsync();
                await _reports.CreateAuctionListReport(created);
            }

            return Ok(dct);
        }
    }
}