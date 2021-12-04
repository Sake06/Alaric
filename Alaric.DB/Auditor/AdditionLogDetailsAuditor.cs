using Alaric.DB.Models;
using Alaric.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Alaric.DB.Auditor
{
    public class AdditionLogDetailsAuditor : ChangeLogDetailsAuditor
    { 
        public AdditionLogDetailsAuditor(EntityEntry dbEntry, AuditLog log)
            : base(dbEntry, log)
        {
        } 

        protected internal override EntityState StateOfEntity()
        {
            if (this.DbEntry.State == EntityState.Unchanged) return EntityState.Added;

            return base.StateOfEntity();
        } 
        protected override bool IsValueChanged(string propertyName)
        {
            var propertyType = this.DbEntry.Entity.GetType()
                .GetProperty(propertyName)
                .PropertyType;
            var defaultValue = propertyType.DefaultValue();
            var currentValue = this.CurrentValue(propertyName);

            return !Equals(defaultValue, currentValue);
        }

        protected override object OriginalValue(string propertyName)
        {
            return null;
        }
    }
}