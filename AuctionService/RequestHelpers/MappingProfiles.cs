using AuctionService.DTOs;
using AuctionService.Entities;
using AutoMapper;
using Contracts;

namespace AuctionService.RequestHelpers
{
    public class MappingProfiles : Profile
    {
        public MappingProfiles() 
        {
            CreateMap<Auction, AuctionDto>().IncludeMembers(x => x.Item);
            CreateMap<Item, AuctionDto>();

            CreateMap<CreateAuctionDto, Auction>().ForMember(d => d.Item, o => o.MapFrom(s => s));
            CreateMap<CreateAuctionDto, Item>();

            CreateMap<UpdateAuctionDto, Auction>().ForMember(dest => dest.Item, options => options.MapFrom(s => s));
            CreateMap<UpdateAuctionDto, Item>();
                        
            CreateMap<AuctionDto,AuctionCreated>();
            CreateMap<Auction, AuctionUpdated>().IncludeMembers(x => x.Item);
            CreateMap<Item, AuctionUpdated>();
            //CreateMap<Auction, AuctionDeleted>().IncludeMembers(x => x.Item);
            //CreateMap<Auction, Item>();
        }
    }
}
