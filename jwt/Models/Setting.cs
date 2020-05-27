using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace jwt.Models
{
    public class Tenant
    {
        public string Name { get; set; }
        public string Key { get; set; }
        public string PrivateKey { get; set; }
    }
}
