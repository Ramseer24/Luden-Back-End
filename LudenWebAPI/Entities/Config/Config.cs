﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Config
{
    public class Config
    {
        public Authentication Authentication { get; set; }
        public JwtSettings Jwt { get; set; }
    }
}
