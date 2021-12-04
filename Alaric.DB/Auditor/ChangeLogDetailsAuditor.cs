using System.Collections.Generic;
using Alaric.DB.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Alaric.DB.Auditor
{
    public class ChangeLogDetailsAuditor : ILogDetailsAuditor
    { 
        protected readonly EntityEntry DbEntry;
         
        private readonly AuditLog _log;
         
         
        public ChangeLogDetailsAuditor(EntityEntry dbEntry, AuditLog log)
        {
            this.DbEntry = dbEntry;
            this._log = log;
        }
         

        public IEnumerable<AuditLogDetail> CreateLogDetails()
        {
            var entityType = this.DbEntry.Entity.GetType();

            foreach (var propertyName in this.PropertyNamesOfEntity())
                if (this.IsValueChanged(propertyName.Name))
                    yield return
                        new AuditLogDetail
                        {
                            PropertyName = propertyName.Name,
                            OriginalValue = this.OriginalValue(propertyName.Name)
                                    ?.ToString(),
                            NewValue = this.CurrentValue(propertyName.Name)
                                    ?.ToString() 
                        };
        }

        protected internal virtual EntityState StateOfEntity()
        {
            return this.DbEntry.State;
        }

        protected virtual object CurrentValue(string propertyName)
        {
            var value = this.DbEntry.Property(propertyName)
                .CurrentValue;
            return value;
        }
        protected virtual bool IsValueChanged(string propertyName)
        {
            var prop = this.DbEntry.Property(propertyName);
            var propertyType = this.DbEntry.Entity.GetType()
                .GetProperty(propertyName)
                .PropertyType;

            var originalValue = this.OriginalValue(propertyName);


            var changed = this.StateOfEntity() == EntityState.Modified
                          && prop.IsModified
                          && !Equals(this.CurrentValue(propertyName), originalValue);
            return changed;
        } 
        protected virtual object OriginalValue(string propertyName)
        {
            object originalValue = null;

            originalValue = this.DbEntry.Property(propertyName)
                .OriginalValue;

            return originalValue;
        }
        private IEnumerable<IProperty> PropertyNamesOfEntity()
        {
            var propertyValues = this.StateOfEntity() == EntityState.Added
                                     ? this.DbEntry.CurrentValues
                                     : this.DbEntry.OriginalValues;
            return propertyValues.Properties;
        }

    }
}