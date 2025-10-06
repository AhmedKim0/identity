using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Identity.Domain.SharedEntities
{
    public interface IAuditableEntity
    {
        [Key]
        public int Id { get; set; }
        public bool IsDeleted { get; set; } 
        public DateTime CreatedAtUtc { get; set; }
        public int CreatedBy { get; set; }
        public DateTime? UpdatedAtUtc { get; set; }
        public int? UpdatedBy { get; set; }
    }
}
