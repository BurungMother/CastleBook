﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Claims;

namespace CastleBook.Utility
{
    public static class ClaimsPrincipalExtension
    {
        public static string GetName(this ClaimsPrincipal principal)
        {
            var fullName = principal.Claims.FirstOrDefault(c => c.Type == "Name");
            return fullName?.Value;
        }
    }
}
