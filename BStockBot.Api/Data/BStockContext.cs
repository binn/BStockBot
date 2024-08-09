using Microsoft.EntityFrameworkCore;

namespace BStockBot.Api.Data
{
    public class BStockContext : DbContext
    {
        public BStockContext(DbContextOptions<BStockContext> options) : base(options) { }

        public DbSet<Auction> Auctions { get; set; }
    }
}
