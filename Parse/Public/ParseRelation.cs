using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Reflection;
using Parse.Common.Internal;
using Parse.Core.Internal;

namespace Parse
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public abstract class ParseRelationBase : IJsonConvertible
    {
        ParseObject Parent { get; set; }

        string Key { get; set; }

        internal ParseRelationBase(ParseObject parent, string key) => EnsureParentAndKey(parent, key);

        internal ParseRelationBase(ParseObject parent, string key, string targetClassName) : this(parent, key) => TargetClassName = targetClassName;

        internal static IObjectSubclassingController SubclassingController => ParseCorePlugins.Instance.SubclassingController;

        internal void EnsureParentAndKey(ParseObject parent, string key)
        {
            Parent = Parent ?? parent;
            Key = Key ?? key;
        }

        internal void Add(ParseObject obj)
        {
            ParseRelationOperation change = new ParseRelationOperation(new[] { obj }, null);
            Parent.PerformOperation(Key, change);
            TargetClassName = change.TargetClassName;
        }

        internal void Remove(ParseObject obj)
        {
            ParseRelationOperation change = new ParseRelationOperation(null, new[] { obj });
            Parent.PerformOperation(Key, change);
            TargetClassName = change.TargetClassName;
        }

        IDictionary<string, object> IJsonConvertible.ToJSON() => new Dictionary<string, object>
        {
            ["__type"] = "Relation",
            ["className"] = TargetClassName
        };

        internal ParseQuery<T> GetQuery<T>() where T : ParseObject => TargetClassName != null ? new ParseQuery<T>(TargetClassName).WhereRelatedTo(Parent, Key) : new ParseQuery<T>(Parent.ClassName).RedirectClassName(Key).WhereRelatedTo(Parent, Key);

        internal string TargetClassName { get; set; }

        internal static ParseRelationBase CreateRelation(ParseObject parent, string key, string targetClassName)
        {
            Type targetType = SubclassingController.GetType(targetClassName) ?? typeof(ParseObject);

            Expression<Func<ParseRelation<ParseObject>>> createRelationExpr = () => CreateRelation<ParseObject>(parent, key, targetClassName);
            MethodInfo createRelationMethod = ((MethodCallExpression) createRelationExpr.Body).Method.GetGenericMethodDefinition().MakeGenericMethod(targetType);
            return (ParseRelationBase) createRelationMethod.Invoke(null, new object[] { parent, key, targetClassName });
        }

        static ParseRelation<T> CreateRelation<T>(ParseObject parent, string key, string targetClassName) where T : ParseObject => new ParseRelation<T>(parent, key, targetClassName);
    }

    public sealed class ParseRelation<T> : ParseRelationBase where T : ParseObject
    {
        internal ParseRelation(ParseObject parent, string key) : base(parent, key) { }

        internal ParseRelation(ParseObject parent, string key, string targetClassName) : base(parent, key, targetClassName) { }

        public void Add(T obj) => base.Add(obj);

        public void Remove(T obj) => base.Remove(obj);

        public ParseQuery<T> Query => base.GetQuery<T>();
    }
}
