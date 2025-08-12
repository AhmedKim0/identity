using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Identity.Domain.Entities
{
    public class AppUser : IdentityUser<int>
    {
        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiryTime { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? UpdatedAtUtc { get; set; }
        public int? UpdatedBy { get; set; }
        public bool IsDeleted { get; set; } = false;

    }
}
