using AutoMapper;
using SampleApi.Models;
using SampleApi.Dtos;
using SampleApi.ViewModels;

namespace SampleApi.Services
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<ItemViewModel, Item>();
            CreateMap<Item, ItemDto>();
        }

    }
}