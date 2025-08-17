namespace Identity.Domain.SharedEntities
{
    public abstract class AuditableEntity : BaseEntity
    {
        public DateTime CreatedAtUtc { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? UpdatedAtUtc { get; set; }
        public int? UpdatedBy { get; set; }
    }
}
