using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Alaric.DB.Models
{
    public class AuditLogDetail
    {
        #region Properties

        public virtual long AuditLogId { get; set; }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [ForeignKey("AuditLogId")]
        public virtual AuditLog Log { get; set; }

        public string NewValue { get; set; }

        public string OriginalValue { get; set; }

        public string PropertyName { get; set; }

        #endregion
    }
}