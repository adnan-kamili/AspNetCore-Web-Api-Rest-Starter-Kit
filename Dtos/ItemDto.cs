using System;
using System.Linq.Expressions;
using AutoMapper;
using SampleApi.Models;

namespace SampleApi.Dtos
{
    public class ItemDto : BaseDto
    {
        public string Name { get; set; }
        public int Cost { get; set; }
        public string Color { get; set; }
    }
}
