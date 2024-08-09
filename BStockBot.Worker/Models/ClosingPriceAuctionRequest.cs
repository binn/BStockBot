namespace BStockBot.Worker.Models
{
    public class ClosingPriceAuctionRequest
    {
        public Guid Id { get; set; }
        public string ManifestId { get; set; }
        public int AuctionId { get; set; }
        public bool IsClosed { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}
