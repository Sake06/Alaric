using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Alaric.Shared
{
    public static class TypeExtensions
    {
        #region Constants

         
        #endregion

        #region Methods


        public static object CastPropertyValue(PropertyDescriptor property, string value)
        {
            if (property == null || string.IsNullOrEmpty(value)) return null;
            if (property.PropertyType.IsEnum)
            {
                var enumType = property.PropertyType;
                if (Enum.IsDefined(enumType, value)) return Enum.Parse(enumType, value);
            }

            if (property.PropertyType == typeof(bool))
                return value == "1" || value == "true" || value == "on" || value == "checked";
            if (property.PropertyType == typeof(Uri)) return new Uri(Convert.ToString(value));
            return Convert.ChangeType(value, property.PropertyType);
        }


        public static object DefaultValue(this Type type)
        {
            if (type.IsValueType) return Activator.CreateInstance(type);

            return null;
        }
 
        public static KeyValuePair<string, string> GetKeyValuePair<TEntity>
        (
            this TEntity entity,
            Expression<Func<TEntity, object>> property)
        {
            return new KeyValuePair<string, string>(
                property.GetPropertyInfo()
                    .Name,
                GetPropertyValue(property, entity)
                    ?.ToString());
        }

    
        public static PropertyInfo GetPropertyInfo<TSource, TProperty>
        (
            this Expression<Func<TSource, TProperty>> propertyLambda)
        {
            var type = typeof(TSource);

            var member = GetMember(propertyLambda);

            var propInfo = member.Member as PropertyInfo;
            if (propInfo == null) throw new ArgumentException("Expression is not a valid property.");

            if (type != propInfo.ReflectedType && !type.IsSubclassOf(propInfo.ReflectedType))
                throw new ArgumentException("Expresion refers to a property that is not from type {type.Name}.");

            return propInfo;
        }

        public static object GetPropertyValue(this object entity, string propertyName)
        {
            return entity.GetType()
                .GetProperty(propertyName)
                .GetValue(entity, null);
        }
        public static bool IsNullable(this Type type)
        {
            return Nullable.GetUnderlyingType(type) != null;
        }

        public static bool IsNullable<T>(this Type type)
        {
            return Nullable.GetUnderlyingType(type) == typeof(T);
        }

        public static IDictionary<string, object> ToDictionary(this object obj)
        {
            IDictionary<string, object> result = new Dictionary<string, object>();
            var properties = TypeDescriptor.GetProperties(obj);
            foreach (PropertyDescriptor property in properties)
            {
                result.Add(property.Name, property.GetValue(obj));
            }

            return result;
        }

      

        private static MemberExpression GetMember<TSource, TProperty>
        (
            Expression<Func<TSource, TProperty>> propertyLambda)
        {
            if (propertyLambda.Body is MemberExpression) return (MemberExpression)propertyLambda.Body;

            if (propertyLambda.Body is UnaryExpression)
                return (MemberExpression)((UnaryExpression)propertyLambda.Body).Operand;

            throw new ArgumentException(
                "Expression '{propertyLambda.Name}' refers is not a member expression or unary expression.");
        }

        private static TValue GetPropertyValue<TEntity, TValue>
        (
            Expression<Func<TEntity, TValue>> property,
            TEntity entity)
        {
            return property.Compile()(entity);
        }

        #endregion
    }
}