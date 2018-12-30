namespace Parse.Core.Internal
{
    public static class ParseRelationExtensions
    {
        public static ParseRelation<T> Create<T>(ParseObject parent, string childKey) where T : ParseObject => new ParseRelation<T>(parent, childKey);

        public static ParseRelation<T> Create<T>(ParseObject parent, string childKey, string targetClassName) where T : ParseObject => new ParseRelation<T>(parent, childKey, targetClassName);

        public static string GetTargetClassName<T>(this ParseRelation<T> relation) where T : ParseObject => relation.TargetClassName;
    }
}
