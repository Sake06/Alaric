using System;
using Alaric.DB.Auditor;

namespace Alaric.DB
{
    public class Dispatcher
    {
        
        public event EventHandler<AuditLogGeneratedEventArgs> OnAuditLogGenerated;

        public  void RaiseOnAuditLogGenerated(object sender, AuditLogGeneratedEventArgs e)
        {
            this.OnAuditLogGenerated?.Invoke(sender, e);
        }
    }
}
