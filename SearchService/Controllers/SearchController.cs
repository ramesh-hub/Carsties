using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using MongoDB.Entities;
using SearchService.Models;
using SearchService.RequestHelpers;

namespace SearchService.Controllers
{
    [ApiController]
    [Route("api/search")]
    public class SearchController : ControllerBase
    {
        public SearchController() { }

        [HttpGet]
        public async Task<ActionResult<List<Item>>> SearchItems([FromQuery]SearchParams searchParams)
        {
            var query = DB.PagedSearch<Item, Item>();

            if(!string.IsNullOrEmpty(searchParams.SearchTerm))
            {
                query.Match(Search.Full, searchParams.SearchTerm);
            }

            if (!string.IsNullOrEmpty(searchParams.Seller))
            {
                query.Match(item => item.Seller == searchParams.Seller);
            }

            if (!string.IsNullOrEmpty(searchParams.Winner))
            {
                query.Match(item => item.Winner == searchParams.Winner);
            }

            query = searchParams.OrderBy switch
            {
                "make" => query.Sort(item => item.Ascending(i => i.Make)),
                "new" => query.Sort(item => item.Descending(i => i.CreatedAt)),
                _ => query.Sort(item => item.Ascending(i => i.AuctionEnd))
            };

            query = searchParams.FilterBy switch
            {
                "finished" => query.Match(item => item.AuctionEnd < DateTime.UtcNow),
                "endingsoon" => query.Match(item => item.AuctionEnd < DateTime.UtcNow.AddHours(6) && item.AuctionEnd > DateTime.UtcNow),
                _ => query.Match(item => item.AuctionEnd > DateTime.UtcNow)
            };

            query.PageNumber(searchParams.PageNumber);
            query.PageSize(searchParams.PageSize);

            var result = await query.ExecuteAsync(); 

            return Ok(new
            {
                results = result.Results,
                pageCount = result.PageCount,
                totalCount = result.TotalCount
                
            });
        }
    }
}
