using System;

namespace Parse.Common.Internal
{
    [AttributeUsage(AttributeTargets.Assembly)]
    public class ParseModuleAttribute : Attribute
    {
        public ParseModuleAttribute(Type ModuleType) => this.ModuleType = ModuleType;

        public Type ModuleType { get; private set; }
    }
}