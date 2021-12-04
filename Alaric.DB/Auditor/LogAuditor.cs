using System;
using System.Collections.Generic;
using System.Linq;
using Alaric.DB.Models;
using Alaric.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.Options;

namespace Alaric.DB.Auditor
{
    internal class LogAuditor : IDisposable
    { 

        private readonly EntityEntry _dbEntry;

        public IOptions<BaseOptions> Config { get; }

        internal LogAuditor(EntityEntry dbEntry, IOptions<BaseOptions> config)
        {
            this._dbEntry = dbEntry;
            Config = config;
        }
         
         

        public void Dispose()
        {
        }

        internal AuditLog CreateLogRecord
        (
            EventType eventType)
        {
            var entityType = this._dbEntry.Entity.GetType();


            var timestamp = DateTime.Now;


            var newlog = new AuditLog
            {
                TimeStamp = timestamp,
                EventType = eventType,
                TypeFullName = entityType.FullName,
                RecordId = GetPrimaryKeyValuesOf(this._dbEntry)
                                     .ToString(),
                InsertedByApp = Config.Value.ApplicationId

            };



            var detailsAuditor = this.GetDetailsAuditor(eventType, newlog);

            newlog.LogDetails = detailsAuditor.CreateLogDetails()
                .ToList();

            if (newlog.LogDetails.Any()) return newlog;

            return null;
        }
        private static object GetPrimaryKeyValuesOf(EntityEntry dbEntry)
        {
            var properties = dbEntry.Properties.Where((arg) => arg.Metadata.IsPrimaryKey());
            if (properties.Count() == 1)
                return dbEntry.GetDatabaseValues()
                    .GetValue<object>(
                        properties.Select(x => x.Metadata.Name)
                            .First());

            if (properties.Count() > 1)
            {
                var output = "[";

                output += string.Join(
                    ",",
                    properties.Select(
                        colName => dbEntry.GetDatabaseValues()
                            .GetValue<object>(colName.Metadata.Name)));

                output += "]";
                return output;
            }

            throw new KeyNotFoundException(
                "key not found for "
                + dbEntry.Entity.GetType()
                    .FullName);
        }
        private ChangeLogDetailsAuditor GetDetailsAuditor(EventType eventType, AuditLog newlog)
        {
            switch (eventType)
            {
                case EventType.Added:
                    return new AdditionLogDetailsAuditor(this._dbEntry, newlog);

                case EventType.Modified:
                    return new ChangeLogDetailsAuditor(this._dbEntry, newlog);

                default:
                    return null;
            }
        }

    }
}
