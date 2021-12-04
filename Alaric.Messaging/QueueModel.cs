using System;
using Alaric.DB.Models;

namespace Alaric.Messaging
{
    public class QueueModel
    {
        public AuditLog AuditLog { get; set; }

        public Trade Trade { get; set; }
    }
}
