using System;
using System.Linq;
using AutoMapper;
using System.Linq.Expressions;

using SampleApi.Models;
using SampleApi.Dtos;
using SampleApi.ViewModels;

namespace SampleApi.Repository
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<UserViewModel, User>()
                .ForMember(userViewModel => userViewModel.Roles,
                    option => option.Ignore());
            CreateMap<User, UserDto>()
                .ForMember(userDto => userDto.Roles,
                    option => option.MapFrom(user => user.Roles.Select(role => role.RoleId)));

            CreateMap<RoleViewModel, Role>()
                .ForMember(roleViewModel => roleViewModel.Claims,
                    option => option.Ignore());
            CreateMap<Role, RoleDto>()
                .ForMember(roleDto => roleDto.Claims,
                    option => option.MapFrom(role => role.Claims.Select(claim => claim.ClaimValue)));

            CreateMap<ItemViewModel, Item>();
            CreateMap<Item, ItemDto>();
        }

    }
}