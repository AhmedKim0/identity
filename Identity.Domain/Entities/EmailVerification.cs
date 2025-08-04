using Identity.Domain.SharedEntities;

namespace Identity.Domain.Entities
{
    public class EmailVerification : BaseEntity
    {
        public int UserId { get; set; }

        public string Email { get; set; }
        public bool IsVerified { get; set; }
        public DateTime? BlockedUntil { get; set; }
        public ICollection<OTPCode> OTPCodes { get; set; } = new List<OTPCode>();



    }

}


