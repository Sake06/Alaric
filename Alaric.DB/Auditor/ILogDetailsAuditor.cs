using System.Collections.Generic;

namespace Alaric.DB.Models
{
    public interface ILogDetailsAuditor
    { 
        IEnumerable<AuditLogDetail> CreateLogDetails();
         
    }
}