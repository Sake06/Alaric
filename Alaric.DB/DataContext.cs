using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Alaric.DB.Auditor;
using Alaric.DB.Models;
using Alaric.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.Options;

namespace Alaric.DB
{
    public class DataContext : DbContext
    {
        private readonly Dispatcher dispatcher;
        private readonly IOptions<BaseOptions> baseconfig;
        public bool DisableTrack { get; set; } = false;

        public DataContext(DbContextOptions<DataContext> options, Dispatcher dispatcher, IOptions<BaseOptions> baseconfig)
            : base(options)
        {
            this.dispatcher = dispatcher;
            this.baseconfig = baseconfig;
        }
        public DbSet<AuditLogDetail> AuditLogDetails { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<Trade> Trades { get; set; }

     

        public override int SaveChanges()
        {
            if (!DisableTrack)
            {
                this.AuditChanges();

                var addedEntries = this.GetAdditions();

                base.SaveChanges();

                this.AuditAdditions(addedEntries);

                return base.SaveChanges();
            }
            return base.SaveChanges();



        }
        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken)
        {

            if (cancellationToken.IsCancellationRequested) cancellationToken.ThrowIfCancellationRequested();

            if (!DisableTrack)
            {
                this.AuditChanges();
                var addedEntries = this.GetAdditions();
                var result = await base.SaveChangesAsync(cancellationToken);
                this.AuditAdditions(addedEntries);
                await base.SaveChangesAsync(cancellationToken);
                return result;
            }

            return await base.SaveChangesAsync(cancellationToken);


        }
        protected virtual void RaiseOnAuditLogGenerated(object sender, AuditLogGeneratedEventArgs e)
        {
            this.dispatcher.RaiseOnAuditLogGenerated(sender, e);
        }
        private EventType GetEventType(EntityEntry entry)
        {
            return EventType.Modified;
        }
        private IEnumerable<string> EntityTypeNames<TEntity>()
        {
            var entityType = typeof(TEntity);
            return
                typeof(TEntity).Assembly.GetTypes()
                    .Where(t => t.IsSubclassOf(entityType) || t.FullName == entityType.FullName)
                    .Select(m => m.FullName);
        }
        public IQueryable<AuditLog> GetLogs<TEntity>(IEnumerable<object> primaryKey)
        {
            var key = primaryKey.Select(o => o.ToString());
            var entityTypeNames = this.EntityTypeNames<TEntity>();

            return this.AuditLogs.Where(
                x => entityTypeNames.Contains(x.TypeFullName) && key.Contains(x.RecordId));
        }

        public IQueryable<AuditLog> GetLogs(string entityTypeName, object primaryKey)
        {
            var key = primaryKey.ToString();
            return this.AuditLogs.Where(x => x.TypeFullName == entityTypeName && x.RecordId == key);
        }
        public IEnumerable<EntityEntry> GetAdditions()
        {
            return this.ChangeTracker.Entries()
                .Where(p => p.State == EntityState.Added)
                .ToList();
        }
        public void AuditAdditions(IEnumerable<EntityEntry> addedEntries)
        {
            foreach (var ent in addedEntries)
            {
                if (ent.Entity.GetType().Equals(typeof(AuditLog)) || ent.Entity.GetType().Equals(typeof(AuditLogDetail)))
                {
                    continue;
                }
                using (var auditer = new LogAuditor(ent, this.baseconfig))
                {
                    var record = auditer.CreateLogRecord(EventType.Added);
                    if (record != null)
                    {
                        var arg = new AuditLogGeneratedEventArgs(record, ent.Entity);
                        this.RaiseOnAuditLogGenerated(this, arg);
                        this.AuditLogs.Add(record);
                    }
                }
            }
               
        }

        public void AuditChanges()
        {
            foreach (var ent in
                this.ChangeTracker.Entries()
                    .Where(p => p.State == EntityState.Modified))
            {
                if (ent.Entity.GetType().Equals(typeof(AuditLog)) || ent.Entity.GetType().Equals(typeof(AuditLogDetail)))
                {
                    continue;
                }
                using (var auditer = new LogAuditor(ent, this.baseconfig))
                {
                    var eventType = EventType.Modified;

                    var record = auditer.CreateLogRecord(eventType);

                    if (record != null)
                    {
                        var arg = new AuditLogGeneratedEventArgs(record, ent.Entity);
                        this.RaiseOnAuditLogGenerated(this, arg);
                        this.AuditLogs.Add(record);
                    }
                }
            }
              
            
        }
    }
}