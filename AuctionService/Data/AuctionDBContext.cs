using Microsoft.EntityFrameworkCore;
using AuctionService.Entities;
namespace AuctionService.Data
{
    public class AuctionDBContext : DbContext
    {
        public AuctionDBContext(DbContextOptions<AuctionDBContext> options) : base(options)
        {
        
        }
        
        public DbSet<Auction> Auctions { get; set; }
    }
}
