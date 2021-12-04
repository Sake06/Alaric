using System;
using System.Dynamic;
using Alaric.DB.Models;

namespace Alaric.DB.Auditor
{
    public class AuditLogGeneratedEventArgs : EventArgs
    {
        #region Constructors
         
        public AuditLogGeneratedEventArgs(AuditLog log, object entity)
        {
            this.Log = log;
            this.Entity = entity; 
        }

        #endregion

        #region Properties 
        public object Entity { get; internal set; } 
        public AuditLog Log { get; internal set; }
         

        

        #endregion
    }
}
