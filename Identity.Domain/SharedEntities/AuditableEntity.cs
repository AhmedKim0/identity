namespace Identity.Domain.SharedEntities
{

    public abstract class AuditableEntity :BaseEntity, IAuditableEntity
    {


        public DateTime CreatedAtUtc { get; protected set; }

        public int CreatedBy { get; protected set; }

        public DateTime? UpdatedAtUtc { get; protected set; }

        public int? UpdatedBy { get; protected set; }

        public void SetCreatedAudit(
            int userId,
            DateTime createdAtUtc)
        {
            CreatedBy = userId;
            CreatedAtUtc = createdAtUtc;
        }

        public void SetUpdatedAudit(
            int userId,
            DateTime updatedAtUtc)
        {
            UpdatedBy = userId;
            UpdatedAtUtc = updatedAtUtc;
        }

        public void SoftDelete()
        {
            IsDeleted = true;
        }

        public void Restore()
        {
            IsDeleted = false;
        }
    }
}
