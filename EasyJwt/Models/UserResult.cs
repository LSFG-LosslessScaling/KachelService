using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyJwt.Models
{
    public class UserResult
    {
        public string Name { get; set; }

        public List<ClaimDto> ExposedClaims { get; set; }

        public List<string> Roles { get; set; }
    }

    public class ClaimDto
    {
        public string Type { get; set; }
        public string Value { get; set; }
    }
}
