using System;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;

namespace CFC_Persistencia.Util
{
    public static class EFExtensions
    {
        public static IQueryable<T> NaoExcluidos<T>(this IQueryable<T> query)
        {
            return query.IsNull("data_exclusao");
        }

        public static IQueryable<T> IsNull<T>(this IQueryable<T> query, string propertyName)
        {
            try
            {
                ParameterExpression parameter = Expression.Parameter(typeof(T), "type");
                MemberExpression property = Expression.Property(parameter, propertyName);
                BinaryExpression expressao = Expression.Equal(property, Expression.Constant(null, property.Type));
                Expression<Func<T, bool>> predicate = Expression.Lambda<Func<T, bool>>(expressao, parameter);
                return query.Where(predicate);
            }
            catch (Exception e)
            {
                if (propertyName != "data_exclusao")
                    throw e;

                return query;
            }
        }

        public static IQueryable<T> WhereEquals<T>(this IQueryable<T> query, string propertyName, dynamic value)
        {
            try
            {
                ParameterExpression parameter = Expression.Parameter(typeof(T), "type");
                MemberExpression property = Expression.Property(parameter, propertyName);
                BinaryExpression expressao = Expression.Equal(property, Expression.Constant(value, value.GetType()));
                Expression<Func<T, bool>> predicate = Expression.Lambda<Func<T, bool>>(expressao, parameter);
                return query.Where(predicate);
            }
            catch (Exception)
            {
                return query;
            }
        }

        public static IQueryable<T> WhereNotEquals<T>(this IQueryable<T> query, string propertyName, dynamic value)
        {
            try
            {
                ParameterExpression parameter = Expression.Parameter(typeof(T), "type");
                MemberExpression property = Expression.Property(parameter, propertyName);
                BinaryExpression expressao = Expression.NotEqual(property, Expression.Constant(value, value.GetType()));
                Expression<Func<T, bool>> predicate = Expression.Lambda<Func<T, bool>>(expressao, parameter);
                return query.Where(predicate);
            }
            catch (Exception)
            {
                return query;
            }
        }

        public static E GetInstance<E>()
        {
            return (E)Activator.CreateInstance(typeof(E));
        }
        public static dynamic GetInstance(string type, bool camelCase = true, string referencedProject = "CFC_Entidades")
        {
            string fullName = "";
            if (camelCase)
            {
                fullName = $"{referencedProject}.{Regex.Replace(type, @"(?<!_|^)([A-Z])", "_$1").ToLower()}, {Regex.Replace(referencedProject, @"^([^.]*).*", "$1")}";
            }
            else
            {
                fullName = $"{referencedProject}.{type}, {Regex.Replace(referencedProject, @"^([^.]*).*", "$1")}";
            }

            return Activator.CreateInstance(Type.GetType(fullName));
        }

        public static dynamic EntityOf(this DbContext ctx, string classType, out Type entityType)
        {
            var instance = GetInstance(classType);
            Type targetType = entityType = instance.GetType();
            var model = ctx.GetType()
                .GetRuntimeProperties()
                .Where(o =>
                    o.PropertyType.IsGenericType &&
                    o.PropertyType.GetGenericTypeDefinition() == typeof(DbSet<>) &&
                    o.PropertyType.GenericTypeArguments.Contains(targetType))
                .FirstOrDefault();
            if (null != model)
                return model.GetValue(ctx);
            return null;
        }

        public static IQueryable<E> IncludeAll<E>(this IQueryable<E> query, params Expression<Func<E, object>>[] includeProperties)
        {
            foreach (Expression<Func<E, object>> includeProperty in includeProperties)
            {
                query = query.Include(includeProperty);
            }

            return query;
        }
    }
}
