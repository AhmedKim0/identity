using Identity.Domain.SharedEntities;

namespace Identity.Domain.Entities
{
    public class OTPTry : BaseEntity
    {
        public int OTPCodeId { get; set; }
        public DateTime TryAt { get; set; }
        public bool IsSuccess { get; set; }
        public OTPCode OTPCode { get; set; }
    }

}


