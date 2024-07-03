using AuctionService.Data;
using Contracts;
using MassTransit;
using Microsoft.EntityFrameworkCore.Internal;

namespace AuctionService.Consumers
{
    public class AuctionFinishedConsumer : IConsumer<AuctionFinished>
    {
        private readonly AuctionDBContext _auctionDBContext;

        public AuctionFinishedConsumer(AuctionDBContext auctionDBContext)
        {
            _auctionDBContext = auctionDBContext;
        }
        public async Task Consume(ConsumeContext<AuctionFinished> context)
        {
            Console.WriteLine("--> Consuming auction finished.");

            var finishedAcution = context.Message;
            var auction = await _auctionDBContext.Auctions.FindAsync(context.Message.AcutionId);

            if (auction != null)
            {
                if(finishedAcution.ItemSold)
                {
                    auction.Winner = finishedAcution.Seller;
                    auction.SoldAmount = finishedAcution.Amount;

                    auction.Status = auction.SoldAmount > auction.ReservePrice ? Entities.Status.Finished : Entities.Status.ReserveNotMet;
                }

                await _auctionDBContext.SaveChangesAsync();
            }
        }
    }
}
