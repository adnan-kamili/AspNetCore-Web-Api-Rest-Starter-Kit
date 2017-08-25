using System;
using System.Linq;
using System.Linq.Expressions;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;
using SampleApi.Models;

namespace SampleApi.Dtos
{
    public class RoleDto : BaseDto
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public List<string> Claims { get; set; }
    }
}
