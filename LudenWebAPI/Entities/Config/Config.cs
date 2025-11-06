using Entities.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Stripe;
namespace Entities.Config
{
    public class Config
    {
        public StripeOptions StripeOptions { get; set; }
        public Authentication Authentication { get; set; }
        public JwtSettings Jwt { get; set; }
    }
}
