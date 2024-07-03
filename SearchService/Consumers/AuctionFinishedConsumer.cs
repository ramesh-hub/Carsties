using Contracts;
using MassTransit;
using MongoDB;
using MongoDB.Entities;
using SearchService.Models;

namespace SearchService.Consumers
{
    public class AuctionFinishedConsumer : IConsumer<AuctionFinished>
    {
        public async Task Consume(ConsumeContext<AuctionFinished> context)
        {
            var auctionFinished = context.Message;
            var auction = await DB.Find<Item>().OneAsync(context.Message.AcutionId);

            if(auctionFinished.ItemSold)
            {
                auction.SolidAmount = (int)auctionFinished.Amount;
                auction.Winner = auctionFinished.Winner;                
            }

            auction.Status = "Finished";

            await auction.SaveAsync();
        }
    }
}
