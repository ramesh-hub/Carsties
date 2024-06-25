using System.ComponentModel.DataAnnotations.Schema;

namespace AuctionService.Entities
{
    /// <summary>
    /// Describes a car or any other item on aution
    /// </summary>
    [Table("Items")]
    public class Item
    {
        public Guid Id { get; set; }
        public string Make { get; set; }
        public string Model { get; set; }
        public int Year { get; set; }
        public string Color { get; set; }
        public int Mileage { get; set; }
        public string ImageUrl { get; set; }

        //nav or relationship properties
        public Auction Auction { get; set; }
        public Guid AuctionId { get; set; }
    }
}