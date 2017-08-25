using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Linq;
using SampleApi.Models;

namespace SampleApi.Dtos
{
    public class UserDto: BaseDto
    {
        public string Email { get; set; }
        public string Name { get; set; }
        public List<string> Roles{ get; set; }
    }
}
