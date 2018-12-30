using System;
using System.Collections.Generic;
using Parse.Common.Internal;

namespace Parse.Utilities
{
    public static class ConversionHelpers
    {
        static Dictionary<(Type, Type), Type> LookupCache { get; } = new Dictionary<(Type, Type), Type> { };

        public static T DowncastReference<T>(object value) where T : class => Downcast<T>(value) as T;

        public static T DowncastValue<T>(object value) => (T) Downcast<T>(value);

        internal static object Downcast<T>(object value)
        {
            if (value is T || value is null)
                return value;

            if (ReflectionHelpers.IsPrimitive(typeof(T)))
                return (T) Convert.ChangeType(value, typeof(T));

            if (ReflectionHelpers.IsConstructedGenericType(typeof(T)))
            {
                if (ReflectionHelpers.IsNullable(typeof(T)))
                {
                    Type innerType = ReflectionHelpers.GetGenericTypeArguments(typeof(T))[0];
                    if (ReflectionHelpers.IsPrimitive(innerType))
                        return (T) Convert.ChangeType(value, innerType);
                }
                
                if (GetInterfaceType(value.GetType(), typeof(IList<>)) is Type listType && typeof(T).GetGenericTypeDefinition() == typeof(IList<>))
                    return Activator.CreateInstance(typeof(FlexibleListWrapper<,>).MakeGenericType(ReflectionHelpers.GetGenericTypeArguments(typeof(T))[0], ReflectionHelpers.GetGenericTypeArguments(listType)[0]), value);

                if (GetInterfaceType(value.GetType(), typeof(IDictionary<,>)) is Type dictType && typeof(T).GetGenericTypeDefinition() == typeof(IDictionary<,>))
                    return Activator.CreateInstance(typeof(FlexibleDictionaryWrapper<,>).MakeGenericType(ReflectionHelpers.GetGenericTypeArguments(typeof(T))[1], ReflectionHelpers.GetGenericTypeArguments(dictType)[1]), value);
            }

            return value;

            Type GetInterfaceType(Type objType, Type genericInterfaceType)
            {
                (Type, Type) cacheKey = (objType, genericInterfaceType);

                if (LookupCache.ContainsKey(cacheKey))
                    return LookupCache[cacheKey];

                foreach (Type type in ReflectionHelpers.GetInterfaces(objType))
                    if (ReflectionHelpers.IsConstructedGenericType(type) && type.GetGenericTypeDefinition() == genericInterfaceType)
                        return LookupCache[cacheKey] = type;

                return null;
            }
        }
    }
}