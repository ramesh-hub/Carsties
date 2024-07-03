using AuctionService.Data;
using Contracts;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace AuctionService.Consumers
{
    public class BidPlacedConsumer : IConsumer<BidPlaced>
    {
        private readonly AuctionDBContext _dBContext;

        public BidPlacedConsumer(AuctionDBContext dBContext)
        {
            _dBContext = dBContext;
        }

        public async Task Consume(ConsumeContext<BidPlaced> context)
        {
            Console.WriteLine("--> Consuming bid placed.");
            var bid = context.Message;
            var auction = await _dBContext.Auctions.FindAsync(bid.AuctionId);

            if (auction != null)
            {
                if((auction.CurrentHighBid ?? 0) < bid.Amount)
                    auction.CurrentHighBid = bid.Amount;

                await _dBContext.SaveChangesAsync();
            }
        }
    }
}
