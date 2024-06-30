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
            var lastUpdated = await DB.Find<Item>().Sort(item => item.Descending(i => i.UpdatedAt)).ExecuteFirstAsync();
            
            var url = $"{_config["AuctionServiceUrl"] + "/api/auctions?date=" + lastUpdated.UpdatedAt.ToShortDateString()}";
            return await _httpClient.GetFromJsonAsync<List<Item>>(url);

        }
    }
}
