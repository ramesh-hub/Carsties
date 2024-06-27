using AuctionService.Data;
using AuctionService.DTOs;
using AuctionService.Entities;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuctionService.Controllers
{
    [ApiController]
    [Route("api/auctions")]
    public class AuctionsController : ControllerBase
    {
        private readonly AuctionDBContext _context;
        private readonly IMapper _mapper;

        public AuctionsController(AuctionDBContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<List<AuctionDto>>> GetAllAuctions(string date)
        {
            var query = _context.Auctions
                .Include(a => a.Item)
                .OrderBy(a => a.Item.Make).AsQueryable();

            if (!string.IsNullOrEmpty(date))
            {
                DateTime dateBy = DateTime.UtcNow;
                DateTime.TryParse(date, out DateTime result);
                query = query.Where(auction => auction.UpdatedAt.CompareTo(dateBy) > 0);            
            }

            var auctions = await query.ProjectTo<AuctionDto>(_mapper.ConfigurationProvider).ToListAsync();
            return Ok(auctions);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<AuctionDto>> GetAuctionById(Guid id)
        {
            var acution = await _context.Auctions.Include(a=>a.Item).FirstOrDefaultAsync<Auction>(a => a.Id == id);

            if (acution == null)
            {
                return NotFound();
            }

            return _mapper.Map<AuctionDto>(acution);
        }

        [HttpPost]
        public async Task<ActionResult<AuctionDto>> CreateAuction(CreateAuctionDto auctionDto)
        {
            var auction = _mapper.Map<Auction>(auctionDto);
            // TODO: Add current user as seller
            auction.Seller = "test";

            _context.Auctions.Add(auction);
            var result = await _context.SaveChangesAsync() > 0;

            if(!result)
            {
                return BadRequest("Could not save changes to the database");
            }

            return CreatedAtAction(nameof(GetAuctionById), new { auction.Id }, _mapper.Map<AuctionDto>(auction));
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateAuction(Guid id, [FromBody] UpdateAuctionDto auctionDto)
        {
            var auction = await _context.Auctions.Include(a => a.Item).FirstOrDefaultAsync(a => a.Id == id);

            if (auction == null)
                return NotFound();

            //_mapper.Map(auctionDto, auction, typeof(AuctionDto), typeof(Auction));
            auction.Item.Year = auctionDto.Year ?? auction.Item.Year;
            auction.Item.Model = auctionDto.Model ?? auction.Item.Model;
            auction.Item.Color = auctionDto.Color ?? auction.Item.Color;
            auction.Item.Make = auctionDto.Make ?? auction.Item.Make;
            auction.Item.Mileage = auctionDto?.Mileage ?? auction.Item.Mileage;

            // TODO: Add current user as seller
            auction.Seller = "test";

            var result = await _context.SaveChangesAsync() > 0;

            if (!result)
            {
                return BadRequest("Problem saving changes");
            }

            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteAuction(Guid id)
        {
            var auction = await _context.Auctions.FirstOrDefaultAsync<Auction>(a => a.Id == id);

            if (auction == null)
                return NotFound();

            //TODO: check seller == username

            _context.Auctions.Remove(auction);

            var result = await _context.SaveChangesAsync() > 0;

            if (!result)
                return BadRequest("Unable to update database");
            
            return Ok(result);
        }
    }
}
