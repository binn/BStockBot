using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace BStockBot.Api.Data
{
    [Index(nameof(AuctionId), nameof(Id))]
    public class Auction
    {
        public Guid Id { get; set; }
        public int AuctionId { get; set; }
        public decimal Price { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsClosed { get; set; }
        public string ManifestId { get; set; }

        [Column(TypeName = "jsonb")]
        public List<BStockAuction> Manifest { get; set; } = new List<BStockAuction>();
    }
}