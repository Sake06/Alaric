using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Alaric.DB.Models
{
    public class AuditLog
    {
        #region Properties

 
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long AuditLogId { get; set; }


        [Required]
        public DateTime TimeStamp { get; set; }


        [Required]
        public EventType EventType { get; set; }


        public virtual ICollection<AuditLogDetail> LogDetails { get; set; } = new List<AuditLogDetail>();

        [Required]
        [MaxLength(256)]
        public string RecordId { get; set; }

        [Required]
        [MaxLength(512)]
        public string TypeFullName { get; set; }

        public Guid? InsertedByApp { get; set; }


        #endregion
    }
}