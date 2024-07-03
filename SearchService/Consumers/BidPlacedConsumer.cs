using Contracts;
using MassTransit;
using MongoDB;
using MongoDB.Entities;
using SearchService.Models;

namespace SearchService.Consumers
{
    public class BidPlacedConsumer : IConsumer<BidPlaced>
    {
        public async Task Consume(ConsumeContext<BidPlaced> context)
        {
            Console.WriteLine("--> Consuming bid placed");
            var bid = context.Message;

            var auction = await DB.Find<Item>().OneAsync(bid.AuctionId);

            if ((auction.CurrentHighBid ?? 0) < bid.Amount)
            {
                auction.CurrentHighBid = bid.Amount;

                await DB.SaveAsync(auction);
            }
        }
    }
}
