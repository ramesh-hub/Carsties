﻿using SearchService.Models;
using AutoMapper;
using Contracts;

namespace SearchService.RequestHelpers
{
    public class MappingProfiles : Profile
    {
        public MappingProfiles() 
        {
            CreateMap<AuctionCreated, Item>();
        }
    }
}
