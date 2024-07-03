using Contracts;
using MassTransit;

namespace AuctionService.Consumers
{
    public class AuctionCreatedFaultConsumer : IConsumer<Fault<AuctionCreated>>
    {
        public async Task Consume(ConsumeContext<Fault<AuctionCreated>> context)
        {
            Console.WriteLine("--> Consuming faulty auction creation");
            var exception = context.Message.Exceptions.First();

            if(exception.ExceptionType == "System.ArgumentException")
            {
                //fix the issue
                //context.Message.Message.Model = "FooBar";
                await context.Publish(context.Message.Message);
            }
            else
            {
                Console.WriteLine("Update exception somehwere in a dealletter queue etc.");
            }
        }
    }
}
