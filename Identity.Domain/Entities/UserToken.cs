using Identity.Domain.SharedEntities;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Identity.Domain.Entities
{
    public class UserToken : BaseEntity,IHardDelete
    {
        public int UserId { get; set; } 
        public string AccessToken { get; set; }  
        public string RefreshToken { get; set; }
        public DateTime ATExpiryDate { get; set; }
        public DateTime RTExpiryDate { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public AppUser User { get; set; }


    }
}
