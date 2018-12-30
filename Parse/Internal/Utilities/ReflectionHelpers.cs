using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Parse.Common.Internal
{
    public static class ReflectionHelpers
    {
        public static IEnumerable<PropertyInfo> GetProperties(Type type) => type.GetRuntimeProperties();

        public static MethodInfo GetMethod(Type type, string name, Type[] parameters) => type.GetRuntimeMethod(name, parameters);

        public static bool IsPrimitive(Type type) => type.GetTypeInfo().IsPrimitive;

        public static IEnumerable<Type> GetInterfaces(Type type) => type.GetTypeInfo().ImplementedInterfaces;

        public static bool IsConstructedGenericType(Type type) => type.IsConstructedGenericType;

        public static IEnumerable<ConstructorInfo> GetConstructors(Type type) => type.GetTypeInfo().DeclaredConstructors.Where(c => (c.Attributes & MethodAttributes.Static) == 0);

        public static Type[] GetGenericTypeArguments(Type type) => type.GenericTypeArguments;

        public static PropertyInfo GetProperty(Type type, string name) => type.GetRuntimeProperty(name);

        public static ConstructorInfo FindConstructor(this Type self, params Type[] parameterTypes) => (from constructor in GetConstructors(self) let parameters = constructor.GetParameters() let types = from p in parameters select p.ParameterType where types.SequenceEqual(parameterTypes) select constructor).SingleOrDefault();

        public static bool IsNullable(Type t) => t.IsConstructedGenericType && t.GetGenericTypeDefinition().Equals(typeof(Nullable<>));

        public static IEnumerable<T> GetCustomAttributes<T>(this Assembly assembly) where T : Attribute => CustomAttributeExtensions.GetCustomAttributes<T>(assembly);
    }
}
