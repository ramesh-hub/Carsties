using MongoDB.Entities;
using SearchService.Models;

namespace SearchService.Services
{
    public class AuctionSvcHttpClient
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _config;

        public AuctionSvcHttpClient(HttpClient httpClient, IConfiguration config) 
        {
            _httpClient = httpClient;
            _config = config;
        }

        public async Task<List<Item>> GetItemsForSearchDb()
        {
            var count = await DB.CountAsync<Item>();
            var url = string.Empty;

            if (count == 0)
            {
                url = $"{_config["AuctionServiceUrl"] + "/api/auctions"}";
            }
            else
            {
                var lastUpdated = await DB.Find<Item>().Sort(item => item.Descending(i => i.UpdatedAt)).ExecuteFirstAsync();

                url = $"{_config["AuctionServiceUrl"] + "/api/auctions?date=" + lastUpdated.UpdatedAt.ToShortDateString()}";
            }
            return await _httpClient.GetFromJsonAsync<List<Item>>(url);

        }
    }
}
