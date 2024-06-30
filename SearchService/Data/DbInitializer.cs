using MongoDB.Driver;
using MongoDB.Entities;
using SearchService.Models;
using SearchService.Services;

namespace SearchService.Data
{
    public class DbInitializer
    {
        public static async Task InitDb(WebApplication app)
        {
            await DB.InitAsync("SearchDb",
                MongoClientSettings.FromConnectionString(app.Configuration.GetConnectionString("MongoDbConnection")));

            await DB.Index<Item>()
            .Key(item => item.Make, KeyType.Text)
            .Key(item => item.Model, KeyType.Text)
            .Key(item => item.Color, KeyType.Text)
            .CreateAsync();

            var count = await DB.CountAsync<Item>();

            //if (count == 0)
            //{
                //Console.WriteLine("No data - will attempt to seed data");

                //var itemData = await File.ReadAllTextAsync("Data/auctions.json");

                //JsonSerializerOptions options = new JsonSerializerOptions() { PropertyNameCaseInsensitive = true };
                //var items = JsonSerializer.Deserialize<List<Item>>(itemData, options);
                //await DB.SaveAsync(items);
            //}

            using var scope = app.Services.CreateScope();
            var httpClient = scope.ServiceProvider.GetService<AuctionSvcHttpClient>();
            var items = await httpClient.GetItemsForSearchDb();

            Console.WriteLine(items.Count + " returned from auction service");

            if (items.Count > 0)
            {
                //await DB.SaveAsync(items);
            }
        }
    }
}
