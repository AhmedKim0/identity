using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Identity.Domain.SharedEntities;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Identity.Domain.Entities
{
    public class OTPCode : BaseEntity
    {
        public int EmailVerificationId { get; set; }
        public string Code { get; set; }
        public bool IsExpired { get; set; }
        public bool IsUsed { get; set; }
        public DateTime CreatedAtUTC { get; set; }

        public DateTime ExpireAt { get; set; }
        public EmailVerification EmailVerification { get; set; }
        public ICollection<OTPTry> OTPTries { get; set; } = new List<OTPTry>();

    }

}


