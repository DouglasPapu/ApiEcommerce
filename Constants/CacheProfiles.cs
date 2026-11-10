using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace ApiEcommerce.Constants
{
    public static class CacheProfiles
    {
        public const string DEFAULT_10 = "Default10";
        public const string DEFAULT_2O = "Default20";


        public static readonly CacheProfile Profile10 = new()
        {
            Duration = 10
        };

        public static readonly CacheProfile Profile20 = new()
        {
          Duration = 20  
        };
    }
}