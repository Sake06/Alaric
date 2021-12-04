using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Alaric.DB.Models;
using Alaric.Shared;
namespace Alaric.DB.Extensions
{
    public static class AuditLogExtensions
    {
        public static TEntity ToEntity<TEntity>(this IEnumerable<AuditLogDetail> logdetail)
        {
            var formDictionary = (TEntity)Activator.CreateInstance(typeof(TEntity));
            return logdetail.ToEntity(formDictionary);
        }

        public static TEntity ToEntity<TEntity>
            (this IEnumerable<AuditLogDetail> logdetail, TEntity src, bool direction = true)
        {
            var formDictionary = logdetail.ToList()
                .ToDictionary(
                    detail => detail.PropertyName,
                    detail => direction ? detail.NewValue : detail.OriginalValue);

            var properties = TypeDescriptor.GetProperties(src);
            foreach (PropertyDescriptor property in properties)
            {
                if (formDictionary.ContainsKey(property.Name))
                {
                    try
                    {
                        var val = TypeExtensions.CastPropertyValue(property, formDictionary[property.Name]);
                        property.SetValue(src, val);
                    }
                    catch { } 
                }
            }

            return src;
        }
        public static TEntity ToEntity<TEntity>(this AuditLog logdetail, TEntity src, bool direction = true)
        {
            return logdetail.LogDetails.ToEntity(src, direction);
        }
    }
}
