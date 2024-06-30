using AutoMapper;
using Contracts;
using MassTransit;
using MongoDB.Entities;
using SearchService.Models;

namespace SearchService.Consumers
{
    /// <summary>
    /// Consumes AuctionCreated message and save to mongo db.
    /// </summary>
    public class AuctionCreatedConsumer : IConsumer<AuctionCreated>
    {
        private readonly IMapper _mapper;

        public AuctionCreatedConsumer(IMapper mapper)
        {
            _mapper = mapper;
        }

        public async Task Consume(ConsumeContext<AuctionCreated> context)
        {
            Console.WriteLine("--> Consuming acution created message: " + context.Message.Id);
             var item = _mapper.Map<Item>(context.Message);
            await item.SaveAsync();
        }
    }
}
