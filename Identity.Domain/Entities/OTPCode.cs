using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Identity.Domain.Entities
{
    public class OTPCode
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; } // FK to AppUser (int)

        [Required]
        [MaxLength(10)]
        public string Code { get; set; }

        [Required]
        [MaxLength(255)]
        public string SentTo { get; set; } // phone or email

        [Required]
        [MaxLength(50)]
        public string Provider { get; set; } // SMS, Email, WhatsApp, etc.

        [Required]
        public DateTime ExpiresAtUtc { get; set; }

        [Required]
        public bool IsUsed { get; set; } = false;

        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

        public DateTime? UsedAtUtc { get; set; }

        [MaxLength(45)]
        public string RequestIp { get; set; }

        [Required]
        public int AttemptsCount { get; set; } = 0;

        // Navigation Property (optional)
        [ForeignKey(nameof(UserId))]
        public AppUser User { get; set; }
    }
}

