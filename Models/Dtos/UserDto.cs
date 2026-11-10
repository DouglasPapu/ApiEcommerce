using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ApiEcommerce.Models.Dtos
{
    public class UserDto
    {   
        public string Id { get; set; } = string.Empty;
        public string? Name { get; set; }
        public string? Username { get; set; }
    }
}