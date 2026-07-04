// ============================================================
// BaseEntity — Base Entity
// ============================================================
// المسؤولية: تعريف Base Entity.
// ============================================================
﻿using System.ComponentModel.DataAnnotations;

namespace Domain.Models
{
    /// <summary>
    /// صنف Base Entity.
    /// </summary>
    public abstract class BaseEntity
    {
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public int? CreatedByUserId { get; set; }
        public int? UpdatedByUserId { get; set; }

        [Timestamp]
        public byte[] RowVersion { get; set; } = null!;

        // Navigation properties for audit trail
        public virtual Domain.Identity.ApplicationUser? CreatedByUser { get; set; }
        public virtual Domain.Identity.ApplicationUser? UpdatedByUser { get; set; }

    }
}
