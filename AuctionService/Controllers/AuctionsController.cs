using AuctionService.Data;
using AuctionService.DTOs;
using AuctionService.Entities;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Contracts;
using MassTransit;
using MassTransit.RabbitMqTransport;
using Microsoft.AspNetCore.Authorization;
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
        private readonly IPublishEndpoint _publishEndpoint;

        public AuctionsController(AuctionDBContext context, IMapper mapper, IPublishEndpoint publishEndpoint)
        {
            _context = context;
            _mapper = mapper;
            _publishEndpoint = publishEndpoint;
        }

        [HttpGet]
        public async Task<ActionResult<List<AuctionDto>>> GetAllAuctions(string date)
        {
            var query = _context.Auctions.OrderBy(a => a.Item.Make).AsQueryable();

            if (!string.IsNullOrEmpty(date))
            {
                DateTime dateBy = DateTime.UtcNow;
                DateTime.TryParse(date, out dateBy);
                query = query.Where(auction => auction.UpdatedAt.CompareTo(dateBy.ToUniversalTime()) > 0);            
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

        [Authorize]
        [HttpPost]
        public async Task<ActionResult<AuctionDto>> CreateAuction(CreateAuctionDto auctionDto)
        {
            var auction = _mapper.Map<Auction>(auctionDto);

            auction.Seller = User.Identity.Name;

            _context.Auctions.Add(auction);
            //Before outbox: 1. we save to db then 2. send message to service bus
            //var result = await _context.SaveChangesAsync() > 0;

            //var newauction = _mapper.Map<AuctionDto>(auction);

            //var newauctionMessage = _mapper.Map<AuctionCreated>(newauction);
            //await _publishEndpoint.Publish(newauctionMessage);

            //After outbox setup, making everything as part of an EF transaction in memory before you call save changes
            var newauction = _mapper.Map<AuctionDto>(auction);

            var newauctionMessage = _mapper.Map<AuctionCreated>(newauction);
            await _publishEndpoint.Publish(newauctionMessage);
            var result = await _context.SaveChangesAsync() > 0;

            if (!result)
            {
                return BadRequest("Could not save changes to the database");
            }

            return CreatedAtAction(nameof(GetAuctionById), new { auction.Id }, newauction);
        }

        [Authorize]
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
            if (auction.Seller != User.Identity.Name) return Forbid("Only seller can update the acution info.");

            await _publishEndpoint.Publish<AuctionUpdated>(_mapper.Map<AuctionUpdated>(auction));

            var result = await _context.SaveChangesAsync() > 0;

            if (!result)
            {
                return BadRequest("Problem saving changes");
            }

            return Ok(result);
        }

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteAuction(Guid id)
        {
            var auction = await _context.Auctions.FirstOrDefaultAsync<Auction>(a => a.Id == id);

            if (auction == null)
                return NotFound();

            //TODO: check seller == username
            auction.Seller = User.Identity.Name;

            _context.Auctions.Remove(auction);
            await _publishEndpoint.Publish<AuctionDeleted>(new { Id = auction.Id.ToString() });

            var result = await _context.SaveChangesAsync() > 0;

            if (!result)
                return BadRequest("Unable to update database");
            
            return Ok(result);
        }
    }
}
