using Microsoft.EntityFrameworkCore;
using AuctionService.Entities;
using MassTransit;
namespace AuctionService.Data
{
    public class AuctionDBContext : DbContext
    {
        public AuctionDBContext(DbContextOptions<AuctionDBContext> options) : base(options)
        {
        
        }
        
        public DbSet<Auction> Auctions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.AddInboxStateEntity();
            modelBuilder.AddOutboxMessageEntity();
            modelBuilder.AddOutboxStateEntity();
        }
    }
}
